using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using Script.Control.Handlers.Arguments;
using Script.Control.Handlers.GetValue;
using Script.Control.Handlers.GetValue.Based;
using Script.Control.Handlers.SysObj.Based;
using XPackage;

namespace Script.Control.Handlers
{
    [IdentifierClass(typeof(GetValueByXPathHandler), "Выполняет поиск по файлу по XPath выражению")]
    public class GetValueByXPathHandler : GetValueBase
    {
        [Identifier("Matches", "XPath выражение", "Обязательный")]
        public string XPathMatches { get; }

        [Identifier("Replacement", "Шаблон текста для замены. Изменить можно только занения в аттрибутах. Для это необходимо XPath выражение которое указывает на аттрибут, например - '//objectlis/object/@value' ")]
        public string XPathReplace { get; }

        XPathExpression Expression { get; }
        ExecutionBasedByParentType _exec;
        GetOrSetXmlNode _getOrSetXmlNode;
        public GetValueByXPathHandler(XPack parentPack, XmlNode node, LogFill logFill) : base(parentPack, node, logFill)
        {
            //xpath выражение
            XPathMatches = Attributes[GetXMLAttributeName(nameof(XPathMatches))];
            if (string.IsNullOrEmpty(XPathMatches))
                throw new HandlerInitializationException(GetIdentifier(nameof(XPathMatches)), true);

            Expression = XPathExpression.Compile(XPathMatches);
            if (Expression.ReturnType == XPathResultType.Error)
                throw new HandlerInitializationException(GetIdentifier(nameof(XPathMatches)), false);


            //замена значения аттрибута
            XPathReplace = Attributes[GetXMLAttributeName(nameof(XPathReplace))];
            if (XPathReplace != null)
            {
                _getOrSetXmlNode = delegate (XmlNode nodeGet, ref string logContent)
                {
                    var attribute = (XmlAttribute)nodeGet;
                    var oldAttribute = attribute.OuterXml;
                    attribute.Value = string.Format(XPathReplace, attribute.Value);
                    logContent = logContent + string.Format("Set [XmlAttribute] NewAttribute=[{1}]; OldAttribute=[{0}];\r\n", oldAttribute, attribute.OuterXml);
                    return true;
                };
            }
            else
            {
                _getOrSetXmlNode = delegate (XmlNode nodeGet, ref string logContent)
                {
                    logContent = logContent + string.Format("Get [Xml{0}] Value=[{1}]\r\n", nodeGet.NodeType, nodeGet.OuterXml);
                    return false;
                };
            }

            var _parent = Parent as FindBase;
            if (_parent == null)
                throw new HandlerInitializationException(this);

            _exec = ExecutionBySysObj;
        }

        public override void Execute()
        {
            _exec.Invoke();
        }
        void ExecutionBySysObj()
        {
            var _parent = (FindBase)Parent;
            foreach (var match in _parent.Matches.Where(x => x.SysObjType == FindType.Files))
            {
                //string logContent = string.Format("XPath=[{0}]; XPathExpression.Type=[{1}]\r\n", xpathMatches, expression.ReturnType);
                var logContent = string.Empty;
                try
                {
                    var xmlSetting = new XmlDocument();
                    xmlSetting.Load(match.FullPath);
                    var _values = new List<string>();

                    var matchesCount = 0;
                    var isAttrChanged = false;
                    if (Expression.ReturnType == XPathResultType.NodeSet)
                    {
                        foreach (XmlNode xm in xmlSetting.SelectNodes(XPathMatches))
                        {
                            isAttrChanged = _getOrSetXmlNode(xm, ref logContent);
                            //добавляем в коллекцию найденное значение согласно XPath
                            _values.Add(xm.OuterXml);
                            matchesCount++;
                        }

                        if (isAttrChanged)
                            xmlSetting.Save(match.FullPath);
                    }
                    //если xpath выражение имеет булевое значение то просто добавляем его в коллекцию найденных
                    else if (Expression.ReturnType == XPathResultType.Boolean)
                    {
                        var temp_isMatched = false;
                        if (bool.TryParse(xmlSetting.CreateNavigator().Evaluate(Expression).ToString(), out temp_isMatched) && temp_isMatched)
                        {
                            //добавляем в коллекцию найденное значение согласно XPath
                            _values.Add(bool.TrueString);
                            logContent = logContent + "Get [XmlBoolean] Value; Value=[true]\r\n";
                            matchesCount++;
                        }
                    }
                    //все что совпао по xpath выражению то добавляется в список коллекции найденных
                    if (matchesCount > 0)
                    {
                        var newInnerMatch = new InnerTextMatch(_parent, match.SubDirectoryPathWithCurrentName, match.SysObjType);
                        newInnerMatch.Values.AddRange(_values);
                        Matches.Add(newInnerMatch);
                        AddLog(LogType.Success, this, "{1}; Source=[{0}]", match.FullPath, logContent.Trim());
                    }
                }
                catch (Exception ex)
                {
                    AddLog(ex, this, "{1}Source=[{0}]", match.FullPath, logContent);
                }
            }
        }


    }
}
