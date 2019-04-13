using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace XPackage
{
    public class XPack : XPackGetValue, IDisposable
    {
        internal const string DEFAULT_MAIN_NAME = "###HEADER###";
        const string EMPTY_NAME = "###EMPTY###";
        /// <summary>
        /// Дочерние элементы
        /// </summary>
        public CollectionXPack ChildPacks { get; set; }
        /// <summary>
        /// Аттрибуты пакета
        /// </summary>
        public XPackAttributeCollection Attributes { get; set; }
        private XmlNode CurrentNode { get; set; }
        /// <summary>
        /// Имя пака
        /// </summary>
        public string Name { get; private set; }
        protected XPack() : this(DEFAULT_MAIN_NAME, string.Empty)
        {

        }
        XPack(string name, string value) : base(value)
        {
            Name = string.IsNullOrEmpty(name) ? EMPTY_NAME : name;
            Parent = null;
            TypePack = XType.String;
            ChildPacks = new CollectionXPack();
            Attributes = new XPackAttributeCollection(this, StringComparison.CurrentCultureIgnoreCase);
        }

        protected XPack(XPack parentPack, XmlNode node)
        {
            CurrentNode = node;
            Parent = parentPack;
            Name = node.Name;

            TypePack = XType.String;
            ChildPacks = new CollectionXPack();
            Attributes = new XPackAttributeCollection(this, StringComparison.CurrentCultureIgnoreCase);

            if (node.Attributes == null)
                return;

            foreach (XmlAttribute attribute in node.Attributes)
            {
                Attributes.Add(attribute.Name, attribute.Value);
            }
        }

        protected void PerformChild(XmlNode node)
        {
            foreach (XmlNode node2 in node.ChildNodes)
            {
                if (string.Equals(node2.Name, "#text", StringComparison.CurrentCultureIgnoreCase) || string.Equals(node2.Name, "#cdata-section", StringComparison.CurrentCultureIgnoreCase))
                {
                    SourceValue = SourceValue + Functions.ReplaceXmlSpecSymbls(node2.OuterXml.Trim(), 2);
                    continue;
                }

                if (node2.NodeType == XmlNodeType.Element)
                {
                    XPack childObj = GetNewXPack(this, node2);
                    if(childObj == null)
                        continue;
                    ChildPacks.Add(childObj);
                    childObj.PerformChild(node2);
                }
            }
        }
        public XPack GetNewXPack(XmlNode node)
        {
            return GetNewXPack(null, node);
        }
        public virtual XPack GetNewXPack(XPack parentPack, XmlNode node)
        {
            return new XPack(parentPack, node);
        }










        public XType TypePack { get; private set; }


        /// <summary>
        /// Индекс дочернего пакета. Если текущий пакет дочерний, то он больше -1
        /// </summary>
        public int CurrentUniqueIndex
        {
            get
            {
                int myIndex = -1;
                if (Parent != null)
                {
                    foreach (XPack xp in Parent.ChildPacks)
                    {
                        if (string.Equals(xp.Name, Name, StringComparison.CurrentCultureIgnoreCase))
                        {
                            myIndex++;
                            if (xp == this)
                            {
                                break;
                            }
                        }
                    }
                }
                return myIndex;
            }
        }


        public List<XPack> this[string path]
        {
            get
            {
                List<XPack> result;
                TryGetPacks(path, out result);
                return result;
            }
        }
        public List<XPack> this[string path, FindMode mode]
        {
            get
            {
                List<XPack> result;
                GetXPacksByExpression(path, mode, out result);
                return result;
            }
        }

        public bool TryGetPacks(string path, out List<XPack> result)
        {
            bool finded = false;
            finded = GetXPacksByExpression(path, FindMode.Down, out result);
            if (result.Count == 0)
                finded = GetXPacksByExpression(path, FindMode.Up, out result);
            return finded;
        }

        /// <summary>
        /// Найти дочернии пакеты используя XPack выражения
        /// </summary>
        /// <param name="path">путь который нужно найти</param>
        /// <param name="mode"></param>
        /// <param name="result">результат выполнения</param>
        /// <returns></returns>
        bool GetXPacksByExpression(string path, FindMode mode, out List<XPack> result)
        {
            result = new List<XPack>();
            if (string.IsNullOrEmpty(path))
            {
                result.Add(this);
                return true;
            }

            if (path[0] == '/')
                path = path.Substring(1, path.Length - 1);

            if (string.Equals(path, Name, StringComparison.CurrentCultureIgnoreCase))
            {
                result.Add(this);
                return true;
            }
            
            if (path.Split('/').Length == 1)
            {
                path = string.Format("/{0}", path);
            }

            List<XPackPath> xpathCond = new List<XPackPath>();
            foreach (string node in path.Split('/'))
            {
                if (node.IndexOf(DEFAULT_MAIN_NAME, StringComparison.CurrentCultureIgnoreCase) != -1)
                    continue;
                xpathCond.Add(new XPackPath(node));
            }
            FindByXPack(-1, result, mode, this, xpathCond);
            string strResult = string.Join<XPack>(";", result.ToArray());
            return (result.Count > 0);
        }


        void FindByXPack(int id, List<XPack> mainResult, FindMode mode, XPack pack, List<XPackPath> xPath)
        {
            id++;
            List<XPack> result;
            bool getAnySatisfying = false;
            while (xPath[id].XType == XPackPathType.EmptyFindNext)
            {
                getAnySatisfying = true;
                id++;
                if (id >= xPath.Count)
                    break;
            }

            if (getAnySatisfying && mode == FindMode.Down)
                xPath[id].GetAnyChildSatisfying(pack, out result);
            else if (getAnySatisfying && mode == FindMode.Up)
                xPath[id].GetAnyParentSatisfying(pack, out result);
            else
                xPath[id].GetXPack(pack, out result);

            if (result == null)
                return;

            if (id + 1 == xPath.Count)
            {
                mainResult.AddRange(result);
                return;
            }

            if (result.Any())
            {
                foreach (XPack childPck in result)
                {
                    FindByXPack(id, mainResult, mode, childPck, xPath);
                }
            }
        }

        public string SysXPath => GetSysPath();

        string GetSysPath()
        {
            string path = string.Empty;
            XPack sys = this;

            while (sys != null)
            {
                if (sys.Name.Equals(DEFAULT_MAIN_NAME))
                    break;
                path = string.Format("{0}/{1}", sys.Name, path);
                sys = sys.Parent;
            }
            
            return string.Format("/{0}", path.TrimEnd('/'));
        }

        //public string StringView => Shape(ShapeText.Original, ShapeType.String);
        //public string VxmlView => Shape(ShapeText.Original, ShapeType.VXml);
        public string XmlView => Shape(ShapeText.Original, ShapeType.Xml);

        /// <summary>
        /// Вернуть сформированный текущий объект в разных синтаксисах
        /// </summary>
        /// <returns></returns>
        public string Shape(ShapeText texttype, ShapeType type)
        {
            string result;
            if (Name.Equals(DEFAULT_MAIN_NAME) && ChildPacks.Count > 0)
                result = ChildPacks[0].ShapeByType(texttype, type);
            else
                result = ShapeByType(texttype, type);
            return result;
        }

        string ShapeByType(ShapeText texttype, ShapeType type)
        {
            switch (type)
            {
                case ShapeType.String:
                    return ShapeToString();
                case ShapeType.VXml:
                    return ShapeToVXml(0, texttype);
                case ShapeType.Xml:
                    return ShapeToXml(0, texttype);
                default:
                    return string.Empty;
            }
        }

        string ShapeToString()
        {
            string _out = string.Empty;
            if (Name.IndexOf('#') == -1)
                _out = Name;
            _out = string.Format("{0}{1}", _out, Value);
            foreach (XPackAttribute a in Attributes)
            {
                _out = string.Format("{0}{1}{2}", _out, a.Key, a.Value);
            }
            foreach (XPack n in ChildPacks)
            {
                _out = string.Format("{0}{1}", _out, n.ShapeToString());
            }
            return _out;
        }

        string ShapeToVXml(int numSubGroup, ShapeText texttype)
        {
            string separators = new string(' ', numSubGroup);
            string _out = string.Empty;
            string nodeName = string.Empty;
            if (Name.IndexOf('#') == -1)
            {
                nodeName = (texttype != ShapeText.Original) ? (texttype == ShapeText.LowerXName) ? Name.ToLower() : Name.ToUpper() : Name;
                _out = string.Format("{1}[{0}", nodeName, separators);
            }
            if (Attributes != null && !string.IsNullOrEmpty(nodeName))
            {
                foreach (XPackAttribute att in Attributes)
                {
                    _out = string.Format("{0} {1}={2}", _out, (texttype != ShapeText.Original) ? (texttype == ShapeText.LowerXName) ? att.Key.ToLower() : att.Key.ToUpper() : att.Key, att.Value);
                }
            }
            if (ChildPacks.Count == 0)
            {
                if (!string.IsNullOrEmpty(Value) && string.IsNullOrEmpty(nodeName))
                    _out = string.Format("[{0}]", Value);
                else if (!string.IsNullOrEmpty(Value))
                    _out = string.Format("{0}={1}]", _out, Value);
                else
                    _out = string.Format("{0}]", _out);
            }
            else
            {
                if (!string.IsNullOrEmpty(Value) && string.IsNullOrEmpty(nodeName))
                    _out = string.Format("[{0}]", Value);
                else if (!string.IsNullOrEmpty(Value))
                    _out = string.Format("{0}[{1}]", _out, Value);
                else
                    _out = string.Format("{0}", _out);
                int childGroupNum = numSubGroup + 1;
                int i = 0;
                foreach (XPack n in ChildPacks)
                {
                    if (n.Name.IndexOf('#') == -1)
                    {
                        i++;
                        _out = string.Format("{0}{2}{1}", _out, n.ShapeToVXml(childGroupNum, texttype), Environment.NewLine);
                    }
                    else
                        _out = string.Format("{0}{1}", _out, n.ShapeToVXml(childGroupNum, texttype));
                }
                _out = i > 0 ? string.Format("{0}{2}{1}]", _out, separators, Environment.NewLine) : string.Format("{0}]", _out);
            }
            return _out;
        }

        string ShapeToXml(int numSubGroup, ShapeText texttype)
        {
            string separators = new string('	', numSubGroup);
            string _out = string.Empty;
            string nodeName = string.Empty;
            if (Name.IndexOf('#') == -1)
            {
                nodeName = (texttype != ShapeText.Original) ? (texttype == ShapeText.LowerXName) ? Name.ToLower() : Name.ToUpper() : Name;
                _out = string.Format("{0}<{1}", separators, nodeName);
            }
            if (Attributes != null && !string.IsNullOrEmpty(nodeName))
            {
                foreach (XPackAttribute att in Attributes)
                {
                    _out = string.Format("{0} {1}=\"{2}\"", _out, (texttype != ShapeText.Original) ? (texttype == ShapeText.LowerXName) ? att.Key.ToLower() : att.Key.ToUpper() : att.Key, att.Value);
                }
            }
            if (ChildPacks.Count == 0)
            {
                if (!string.IsNullOrEmpty(Value) && string.IsNullOrEmpty(nodeName))
                    _out = string.Format("{2}<![CDATA[{0}]]>{1}", Value, Environment.NewLine, separators);
                else if (!string.IsNullOrEmpty(Value))
                    _out = string.Format("{0}>{1}</{2}>{3}", _out, Value, nodeName, Environment.NewLine);
                else
                    _out = string.Format("{0}/>{1}", _out, Environment.NewLine);
            }
            else
            {
                if (!string.IsNullOrEmpty(Value))
                    _out = string.Format("{0}><![CDATA[{1}]]>{2}", _out, Value, Environment.NewLine);
                else
                    _out = string.Format("{0}>{1}", _out, Environment.NewLine);
                int childGroupNum = numSubGroup + 1;
                foreach (XPack n in ChildPacks)
                {
                    _out = string.Format("{0}{1}", _out, n.ShapeToXml(childGroupNum, texttype));
                }
                _out = string.Format("{0}{1}</{2}>{3}", _out, separators, nodeName, Environment.NewLine);
            }
            return _out;
        }

        /// <summary>
        /// сравнивает текущий экземпляр  XPack с входным. Если равны то true, иначе false
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool CompareXPack(XPack input)
        {
            if (input == null)
                return false;
            if (this == input)
                return true;
            if (!string.Equals(Name, input.Name, StringComparison.CurrentCultureIgnoreCase) || Value != input.Value)
                return false;

            if (ChildPacks?.Count != input.ChildPacks?.Count)
                return false;
            if (ChildPacks?.Count > 0)
            {
                for (int i = 0; i < ChildPacks.Count; i++)
                {
                    if (!ChildPacks[i].CompareXPack(input.ChildPacks[i]))
                        return false;
                }
            }
            if (Attributes?.Count != input.Attributes?.Count)
                return false;
            if (Attributes?.Count == 0)
                return true;
            foreach(XPackAttribute attr in Attributes)
            {
                if (attr.Value != input.Attributes[attr.Key])
                    return false;
            }
            //for (int i = 0; i < Attributes?.Count; i++)
            //{
            //    if (Attributes[i].Name != input.Attributes[i].Name
            //        || Attributes[i].Value != input.Attributes[i].Value)
            //        return false;
            //}
            return true;
        }




        /// <summary>
        /// Создать идентичную копию XPack (только без связки с родительским пакетом) со всеми новыми аттрибутами и дочерними пакетами
        /// </summary>
        /// <param name="copy"></param>
        /// <param name="fromWhichCreateCopy">экземпляр из которого надо создать копию</param>
        /// <returns></returns>
        public void CreateXPackCopy(ref XPack copy, XPack fromWhichCreateCopy)
        {
            if (fromWhichCreateCopy == null)
                return;
            if (copy == null)
            {
                UsurpProtectedReadyPack(out copy, fromWhichCreateCopy);
            }
            else
                UsurpReadyChildValues(ref copy, fromWhichCreateCopy);
        }

        /// <summary>
        /// Создать идентичную копию XPack (только без связки с родительским пакетом) со всеми новыми аттрибутами и дочерними пакетами
        /// </summary>
        /// <param name="fromWhichCreateCopy">экземпляр из которого надо создать копию</param>
        /// <returns></returns>
        public XPack CreateXPackCopy(XPack fromWhichCreateCopy)
        {
            if (fromWhichCreateCopy == null)
                return null;
            XPack copy;
            UsurpProtectedReadyPack(out copy, fromWhichCreateCopy);
            return copy;
        }
        void UsurpProtectedReadyPack(out XPack copy, XPack original)
        {
            copy = null;
            if (original == null)
                return;

            copy = GetNewXPack(original.CurrentNode);
            DynamicFunction = original.DynamicFunction;
            UsurpReadyChildValues(ref copy, original);
        }

        public void UsurpReadyChildValues(ref XPack copy, XPack original)
        {
            if (original == null)
                return;
            foreach (XPack childOriginal in original.ChildPacks)
            {
                XPack childCopy;
                UsurpProtectedReadyPack(out childCopy, childOriginal);
                copy.ChildPacks.Add(childCopy);
            }
            foreach (XPackAttribute attr in original.Attributes)
            {
                copy.Attributes.Add(attr.Key, attr.Value);
            }
        }

        public List<XPack> UsurpReadyChild(CollectionXPack childsCollectionOriginal)
        {
            if (childsCollectionOriginal == null)
                return null;
            return UsurpReadyChild(childsCollectionOriginal.ToList());
        }
        public List<XPack> UsurpReadyChild(List<XPack> childsCollectionOriginal)
        {
            if (childsCollectionOriginal == null)
                return null;
            List<XPack> childsCollectionCopy = new List<XPack>();
            foreach (XPack childOriginal in childsCollectionOriginal)
            {
                XPack childCopy;
                UsurpProtectedReadyPack(out childCopy, childOriginal);
                childsCollectionCopy.Add(childCopy);
            }
            return childsCollectionCopy;
        }

        public override string ToString()
        {
            return string.Format("<{0} {1} />", Name, Attributes);
        }

        public void Dispose()
        {
            Attributes?.Clear();
            ChildPacks?.Dispose();
        }
    }
}