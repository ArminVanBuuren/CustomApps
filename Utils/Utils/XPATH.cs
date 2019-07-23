using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace Utils
{
    public class XPathResult
    {
        public int ID { get; set; }
        public string NodeType { get; set; }
        public string NodeName { get; set; }
        public string Value { get; set; }
        public XmlNode Node { get; set; }
    }

    public class XPathResultCollection : List<XPathResult>
    {

    }

    public class XPATH
    {
        public static XPathResultCollection Execute(XPathNavigator navigator, string xpath)
        {
            if (navigator == null)
                return null;
            var expression = XPathExpression.Compile(xpath);
            var manager = new XmlNamespaceManager(navigator.NameTable);
            //manager.AddNamespace("bk", "http://www.contoso.com/books");
            manager.AddNamespace(string.Empty, "urn:samples");
            expression.SetContext(manager);
            switch (expression.ReturnType)
            {
                case XPathResultType.NodeSet:
                    var nodes = navigator.Select(expression);
                    if (nodes.Count > 0)
                    {
                        var strOut = new XPathResultCollection();
                        var i = 0;
                        while (nodes.MoveNext())
                        {
                            var current = nodes.Current;
                            if (current == null)
                                continue;
                            var res = new XPathResult
                            {
                                ID = i + 1,
                                NodeType = current.NodeType.ToString(),
                                NodeName = current.Name,
                                Value = current.Value
                            };
                            if (current is IHasXmlNode node)
                                res.Node = node.GetNode();
                            strOut.Add(res);
                            i++;
                        }
                        return strOut;
                    }
                    return null;
                default:
                    var o = navigator.Evaluate(expression);
                    if (o != null)
                    {
                        var res = new XPathResultCollection
                        {
                            new XPathResult
                            {
                                ID = 0,
                                NodeType = expression.ReturnType.ToString(),
                                NodeName = "Empty",
                                Value = o.ToString()
                            }
                        };
                        if (o is IHasXmlNode node)
                        {
                            res[0].Node = node.GetNode();
                            res[0].NodeName = res[0].Node.NodeType.ToString();
                        }
                        return res;
                    }
                    return null;
            }
        }
    }
}
