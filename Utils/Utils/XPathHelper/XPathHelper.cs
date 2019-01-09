using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Utils.XPathHelper
{
    public class XPathHelper
    {
        public static XPathResultCollection Execute(XPathNavigator navigator, string xpath)
        {
            if (navigator == null)
                return null;
            XPathExpression expression = XPathExpression.Compile(xpath);
            XmlNamespaceManager manager = new XmlNamespaceManager(navigator.NameTable);
            //manager.AddNamespace("bk", "http://www.contoso.com/books");
            manager.AddNamespace(string.Empty, "urn:samples");
            expression.SetContext(manager);
            switch (expression.ReturnType)
            {
                case XPathResultType.NodeSet:
                    XPathNodeIterator nodes = navigator.Select(expression);
                    if (nodes.Count > 0)
                    {
                        XPathResultCollection strOut = new XPathResultCollection();
                        int i = 0;
                        while (nodes.MoveNext())
                        {
                            XPathNavigator current = nodes.Current;
                            if (current == null)
                                continue;
                            XPathResult res = new XPathResult
                            {
                                ID = i + 1,
                                NodeType = current.NodeType.ToString(),
                                NodeName = current.Name,
                                Value = current.Value
                            };
                            IHasXmlNode node = current as IHasXmlNode;
                            if (node != null)
                                res.Node = node.GetNode();
                            strOut.Add(res);
                            i++;
                        }
                        return strOut;
                    }
                    return null;
                default:
                    object o = navigator.Evaluate(expression);
                    if (o != null)
                    {
                        XPathResultCollection res = new XPathResultCollection
                        {
                            new XPathResult
                            {
                                ID = 0,
                                NodeType = expression.ReturnType.ToString(),
                                NodeName = "Empty",
                                Value = o.ToString()
                            }
                        };
                        IHasXmlNode node = o as IHasXmlNode;
                        if (node != null)
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
