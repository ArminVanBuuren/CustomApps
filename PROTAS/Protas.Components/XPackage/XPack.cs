using System;
using System.Collections.Generic;
using System.Linq;

namespace Protas.Components.XPackage
{
    public enum ShapeType
    {
        String = 0,
        VXml = 1,
        Xml = 2
    }
    public enum ShapeText
    {
        Original = 0,
        LowerXName = 1,
        UpperXName = 2
    }
    public enum XType
    {
        //Null = 0,
        Function = 1,
        //Bool = 2,
        //Number = 3,
        String = 4
    }

    public class XPackAttributes : Dictionary<string, string>
    {
        public XPackAttributes(IEqualityComparer<string> comparer) : base(comparer)
        {

        }
        public new string this[string key]
        {
            get
            {
                string value;
                if (TryGetValue(key, out value))
                    return value;
                else
                    return null;
            }
            set
            {
                string getterValue;
                if (TryGetValue(key, out getterValue))
                    this[key] = value;
                else
                    Add(key, value);
            }
        }
    }

    public class XPack
    {
        public static readonly string DefaultName = "Empty";
        public XPack Parent { get; internal set; }
        string _value = string.Empty;
        Func<object> _function;
        /// <summary>
        /// Дочерние элементы
        /// </summary>
        public CollectionXPack ChildPacks { get; private set; }
        /// <summary>
        /// Аттрибуты пакета
        /// </summary>
        public XPackAttributes Attributes { get; } = new XPackAttributes(StringComparer.CurrentCultureIgnoreCase);


        delegate string GetValue();
        GetValue _getValue;
        delegate void SetValue(string value);
        SetValue _setValue;
        public XPack()
        {
            DefaultInitialization(DefaultName, string.Empty);
        }
        public XPack(string name)
        {
            DefaultInitialization((string.IsNullOrEmpty(name)) ? DefaultName : name, string.Empty);
        }
        public XPack(string name, string value)
        {
            DefaultInitialization((string.IsNullOrEmpty(name)) ? DefaultName : name, value);
        }

        void DefaultInitialization(string name, string value)
        {
            Name = name;
            ChangeValue(value);
            TypePack = XType.String;
            _getValue = GetDefaultValue;
            _setValue = ChangeValue;
            ChildPacks = new CollectionXPack(this);
        }

        public XPack(string name, Func<object> functionValue)
        {
            Name = (string.IsNullOrEmpty(name)) ? DefaultName : name;
            TypePack = XType.Function;
            _function = functionValue;
            _getValue = GetFunkValue;
            ChildPacks = new CollectionXPack(this);
        }


        public string Name { get; private set; }
        public XType TypePack { get; private set; }
        public virtual string Value
        {
            get
            {
                return _getValue.Invoke();
            }
            set
            {
                _setValue?.Invoke(value);
            }
        }
        string GetDefaultValue()
        {
            return _value;
        }
        string GetFunkValue()
        {
            return _function().ToString();
        }
        void ChangeValue(string value)
        {
            _value = value;
            //TypeParam t = GetTypeEx.GetType(value);
            //switch (t)
            //{
            //    case TypeParam.Null: TypePack = XType.Null; break;
            //    case TypeParam.Bool: TypePack = XType.Bool; break;
            //    case TypeParam.Number: TypePack = XType.Number; break;
            //    default: TypePack = XType.String; break;
            //}
        }
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

        /// <summary>
        /// Добавить аттрибут пакета
        /// </summary>
        /// <param name="name">Имя аттрибута</param>
        /// <param name="value">Значение</param>
        internal void AddAttribute(string name, string value)
        {
            string valueAttr;
            //если атрибут с похожим название уже существует то не добавляем аттрибут
            if (!Attributes.TryGetValue(name, out valueAttr))
            {
                Attributes.Add(name, value);
                //AddLogForm(Log3NetSeverity.Max, "Name:{0}, Value:{1}", name, value);
            }
            
        }

        /// <summary>
        /// Добавить дочерние элементы
        /// </summary>
        /// <param name="child">дочерний пакет</param>
        internal void AddChildNodes(XPack child)
        {
            ChildPacks.Add(child);
        }


        public List<XPack> this[string path]
        {
            get
            {
                List<XPack> result;
                GetXPacksByExpression(path, out result);
                return result;
            }
        }

        public bool TryGetPacks(string path, out List<XPack> result)
        {
            return GetXPacksByExpression(path, out result);
        }

        /// <summary>
        /// Найти дочернии пакеты используя XPack выражения
        /// </summary>
        /// <param name="path">путь который нужно найти</param>
        /// <param name="result">результат выполнения</param>
        /// <returns></returns>
        bool GetXPacksByExpression(string path, out List<XPack> result)
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
                if (node.IndexOf(XmlTransform.HeaderName, StringComparison.CurrentCultureIgnoreCase) != -1)
                    continue;
                xpathCond.Add(new XPackPath(node));
            }
            FindByXPack(-1, result, this, xpathCond);
            string strResult = string.Join<XPack>(";", result.ToArray());
            return (result.Count > 0);
        }


        void FindByXPack(int id, List<XPack> mainResult, XPack pack, List<XPackPath> xPath)
        {
            id++;
            List<XPack> result;
            bool getAnySatisfying = false;
            while (xPath[id].XType == XPackPathType.EmptyFindNext)
            {
                getAnySatisfying = true;
                id++;
                if (id >= xPath.Count)
                    return;
            }

            if (getAnySatisfying)
                xPath[id].GetAnyChildSatisfying(pack, out result);
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
                    FindByXPack(id, mainResult, childPck, xPath);
                }
            }
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
            //header$$$ системная нода означает родительнску всех элементов, выводить её не надо
            if (Name == XmlTransform.HeaderName)
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
            foreach (KeyValuePair<string, string> a in Attributes)
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
                foreach (KeyValuePair<string, string> att in Attributes)
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
                foreach (KeyValuePair<string, string> att in Attributes)
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
            foreach(KeyValuePair<string, string> attr in Attributes)
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

        public override string ToString()
        {
            return string.Format("{0}", Name, (!string.IsNullOrEmpty(Value)) ? string.Format("Name=\"{0}\" Value=\"{1}\"", Name, Value) : Name);
        }

        public void Dispose()
        {
            Value = null;
            Attributes?.Clear();
            ChildPacks?.Dispose();
        }


        /// <summary>
        /// Создать идентичную копию XPack (только без связки с родительским пакетом) со всеми новыми аттрибутами и дочерними пакетами
        /// </summary>
        /// <param name="copy"></param>
        /// <param name="fromWhichCreateCopy">экземпляр из которого надо создать копию</param>
        /// <returns></returns>
        public static void CreateXPackCopy(ref XPack copy, XPack fromWhichCreateCopy)
        {
            if (fromWhichCreateCopy == null)
                return;
            if (copy == null)
            {
                copy = new XPack();
                UsurpProtectedReadyPack(ref copy, fromWhichCreateCopy);
            }
            else
                UsurpReadyChildValues(ref copy, fromWhichCreateCopy);
        }

        /// <summary>
        /// Создать идентичную копию XPack (только без связки с родительским пакетом) со всеми новыми аттрибутами и дочерними пакетами
        /// </summary>
        /// <param name="fromWhichCreateCopy">экземпляр из которого надо создать копию</param>
        /// <returns></returns>
        public static XPack CreateXPackCopy(XPack fromWhichCreateCopy)
        {
            if (fromWhichCreateCopy == null)
                return null;
            XPack copy = new XPack();
            UsurpProtectedReadyPack(ref copy, fromWhichCreateCopy);
            return copy;
        }
        static void UsurpProtectedReadyPack(ref XPack copy, XPack original)
        {
            if (original == null)
                return;
            copy.Name = original.Name;
            if (original._function != null)
            {
                copy._function = original._function;
                copy._getValue = copy.GetFunkValue;
            }
            else
            {
                copy.Value = original.Value;
                copy._getValue = copy.GetDefaultValue;
            }
            UsurpReadyChildValues(ref copy, original);
        }

        public static void UsurpReadyChildValues(ref XPack copy, XPack original)
        {
            if (original == null)
                return;
            foreach (XPack childOriginal in original.ChildPacks)
            {
                XPack childCopy = new XPack();
                UsurpProtectedReadyPack(ref childCopy, childOriginal);
                copy.ChildPacks.Add(childCopy);
            }
            foreach (KeyValuePair<string, string> attr in original.Attributes)
            {
                copy.Attributes.Add(attr.Key, attr.Value);
            }
        }

        public static List<XPack> UsurpReadyChild(CollectionXPack childsCollectionOriginal)
        {
            if (childsCollectionOriginal == null)
                return null;
            return UsurpReadyChild(childsCollectionOriginal.ToList());
        }
        public static List<XPack> UsurpReadyChild(List<XPack> childsCollectionOriginal)
        {
            if (childsCollectionOriginal == null)
                return null;
            List<XPack> childsCollectionCopy = new List<XPack>();
            foreach (XPack childOriginal in childsCollectionOriginal)
            {
                XPack childCopy = new XPack();
                UsurpProtectedReadyPack(ref childCopy, childOriginal);
                childsCollectionCopy.Add(childCopy);
            }
            return childsCollectionCopy;
        }
    }
}