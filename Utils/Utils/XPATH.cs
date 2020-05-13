﻿using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
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
        public static bool SelectFirst(this XmlDocument document, string xpath, out XPathResult result)
        {
            return SelectFirst(document.CreateNavigator(), xpath, out result);
        }

        public static bool SelectFirst(this XPathNavigator navigator, string xpath, out XPathResult result)
        {
            result = null;
            var collection = Select(navigator, xpath, true);
            if (collection == null || collection.Count == 0)
                return false;

            result = collection.First();
            return true;
        }

        public static bool Select(this XmlDocument document, string xpath, out List<XPathResult> result)
        {
            return Select(document.CreateNavigator(), xpath, out result);
        }

        public static bool Select(this XPathNavigator navigator, string xpath, out List<XPathResult> result)
        {
            result = Select(navigator, xpath);
            return result != null;
        }

        public static List<XPathResult> Select(XmlDocument document, string xpath, bool getFirst = false)
        {
            return Select(document.CreateNavigator(), xpath, getFirst);
        }

        public static List<XPathResult> Select(XPathNavigator navigator, string xpath, bool getFirst = false)
        {
            if (navigator == null || xpath == null)
                return null;

            var manager = new XmlNamespaceManager(navigator.NameTable);
            while (navigator.MoveToFollowing(XPathNodeType.Element))
            {
                var localNamespaces = navigator.GetNamespacesInScope(XmlNamespaceScope.Local);
                if (localNamespaces == null || localNamespaces.Count == 0)
                    continue;
                foreach (var localNamespace in localNamespaces)
                {
                    var prefix = localNamespace.Key;
                    if (string.IsNullOrEmpty(prefix))
                        continue;

                    manager.AddNamespace(prefix, localNamespace.Value);
                }
            }

            var expression = XPathExpression.Compile(xpath);
            expression.SetContext(manager);

            switch (expression.ReturnType)
            {
                case XPathResultType.NodeSet:
                    var nodes = navigator.Select(expression);
                    if (nodes.Count == 0)
                        return null;

                    var nodeSetResult = new List<XPathResult>();
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
		                    Value = current.Value,
		                    Node = current is IHasXmlNode node1 ? node1.GetNode() : null
	                    };

	                    nodeSetResult.Add(xpathRes);

	                    if (getFirst)
		                    return nodeSetResult;
                    }

                    return nodeSetResult;
                default:
                    var obj = navigator.Evaluate(expression);
                    if (obj == null)
                        return null;

                    var defaultResult = new List<XPathResult>
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
                        defaultResult[0].Node = node2.GetNode();
                        defaultResult[0].NodeName = defaultResult[0].Node.NodeType.ToString();
                    }

                    return defaultResult;
            }
        }

        /// <summary>
        /// Implemented based on interface, not part of algorithm
        /// </summary>
        /// <param name="xmlDocument"></param>
        /// <returns></returns>
        public static XElement RemoveAllNamespaces(string xmlDocument)
        {
            return RemoveAllNamespaces(XElement.Parse(xmlDocument));
        }

        /// <summary>
        /// Core recursion function
        /// </summary>
        /// <param name="xmlDocument"></param>
        /// <returns></returns>
        public static XElement RemoveAllNamespaces(XElement xmlDocument)
        {
            if (!xmlDocument.HasElements)
            {
                var xElement = new XElement(xmlDocument.Name.LocalName)
                {
                    Value = xmlDocument.Value
                };

                foreach (var attribute in xmlDocument.Attributes())
                    xElement.Add(attribute);

                return xElement;
            }
            return new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(RemoveAllNamespaces));
        }
    }
}