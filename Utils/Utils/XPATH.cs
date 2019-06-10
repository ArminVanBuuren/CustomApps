﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Utils
{
    public class XPATH
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
                            if (current is IHasXmlNode node)
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
