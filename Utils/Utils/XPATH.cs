using System.Collections.Generic;
using System.Linq;
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

        public override string ToString()
        {
            if (!NodeName.IsNullOrEmpty() && !Value.IsNullOrEmpty())
                return $"{NodeName}=[{Value}]";
            else if (!NodeName.IsNullOrEmpty())
                return NodeName;
            else if (Node != null)
                return Node.ToString();
            else
                return "[Null]";
        }
    }

    public static class XPATH
    {
        public static bool SelectFirst(this XPathNavigator navigator, string xpath, out XPathResult result)
        {
            result = null;
            var collection = Select(navigator, xpath, true);
            if (collection == null)
                return false;

            result = collection.First();
            return true;
        }

        public static bool Select(this XPathNavigator navigator, string xpath, out List<XPathResult> result)
        {
            result = Select(navigator, xpath);
            return result != null;
        }

        public static List<XPathResult> Select(XPathNavigator navigator, string xpath, bool getSingle = false)
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
                    if (nodes.Count == 0)
                        return null;

                    var res1 = new List<XPathResult>();
                    var i = 0;

                    while (nodes.MoveNext())
                    {
                        var current = nodes.Current;
                        if (current == null)
                            continue;

                        var xpathRes = new XPathResult
                        {
                            ID = i++,
                            NodeType = current.NodeType.ToString(),
                            NodeName = current.Name,
                            Value = current.Value
                        };

                        if (current is IHasXmlNode node1)
                            xpathRes.Node = node1.GetNode();

                        res1.Add(xpathRes);

                        if (getSingle)
                            return res1;
                    }

                    return res1;
                default:
                    var obj = navigator.Evaluate(expression);
                    if (obj == null)
                        return null;

                    var res2 = new List<XPathResult>
                    {
                        new XPathResult
                        {
                            ID = 0,
                            NodeType = expression.ReturnType.ToString(),
                            NodeName = "Empty",
                            Value = obj.ToString()
                        }
                    };

                    if (obj is IHasXmlNode node2)
                    {
                        res2[0].Node = node2.GetNode();
                        res2[0].NodeName = res2[0].Node.NodeType.ToString();
                    }

                    return res2;
            }
        }
    }
}