using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Utils
{
	public class XPathNavigator2
	{
		private readonly StringBuilder _internalXml;
		private readonly Dictionary<XmlNode, XPathNavigatorResult> _resultCollection;

        internal Dictionary<int, int> Indexes_Source_Internal { get; private set; }

        public string Xml { get; }

		public XmlDocument Document { get; }

		public string XPath { get; }

		public IEnumerable<XPathNavigatorResult> Result => _resultCollection.Values;

		internal XPathNavigator2(string sourceXml, string xpath, bool onlyFirst)
	    {
		    Xml = sourceXml;
		    _internalXml = new StringBuilder(Xml.Length);
		    _resultCollection = new Dictionary<XmlNode, XPathNavigatorResult>();
            Document = new XmlDocument();
		    Document.LoadXml(Xml);
		    XPath = xpath;

		    if (onlyFirst)
		    {
			    if (Document.SelectFirst(XPath, out var result))
			    {
				    var navigator = (XPathNavigatorResult)result;
				    navigator.Parent = this;
				    _resultCollection.Add(navigator.Node, navigator);
			    }
		    }
		    else
		    {
			    if (Document.Select(XPath, out var result))
			    {
				    foreach (XPathNavigatorResult navigator in result)
				    {
					    navigator.Parent = this;
					    _resultCollection.Add(navigator.Node, navigator);
				    }
			    }
		    }

		    if (_resultCollection.Count > 0)
		    {
			    foreach (XmlNode child in Document.ChildNodes)
			    {
				    GetXmlPosition(child);
                }
			    GetPositionInSourceText();
		    }
	    }

        void GetXmlPosition(XmlNode node)
        {
            var inputSourceLength = _internalXml.Length;

            if (node.Attributes == null)
            {
	            _internalXml.Append(Regex.Replace(XML.NormalizeXmlValueFast(node.OuterXml), @"\s+", ""));
            }
            else
            {
                if (node.Attributes.Count > 0)
                {
                    _internalXml.Append('<');
                    _internalXml.Append(node.Name);

                    foreach (XmlAttribute attribute in node.Attributes)
                    {
                        var prevIndexStart = _internalXml.Length;
                        _internalXml.Append(attribute.Name);
                        _internalXml.Append('=');
                        _internalXml.Append('"');
                        _internalXml.Append(Regex.Replace(XML.NormalizeXmlValueFast(attribute.InnerXml), @"\s+", ""));
                        _internalXml.Append('"');

                        if (_resultCollection.TryGetValue(attribute, out var result))
                        {
                            result.InternalStart = prevIndexStart;
                            result.InternalEnd = prevIndexStart + _internalXml.Length - prevIndexStart - 1;
                            result.InternalText = _internalXml.ToString(prevIndexStart, _internalXml.Length - prevIndexStart);
                            result.Type = XMlType.Attribute;
                        }
                    }
                }
                else
                {
                    _internalXml.Append('<');
                    _internalXml.Append(node.Name);
                }

                if (node.ChildNodes.Count <= 0 && node.InnerText.IsNullOrEmpty())
                {
                    _internalXml.Append("/>");
                }
                else
                {
                    _internalXml.Append('>');

                    foreach (XmlNode node2 in node.ChildNodes)
                    {
                        GetXmlPosition(node2);
                    }

                    _internalXml.Append("</");
                    _internalXml.Append(node.Name);
                    _internalXml.Append('>');
                }
            }

            if (_resultCollection.TryGetValue(node, out var result2))
            {
                result2.InternalStart = inputSourceLength;
                result2.InternalEnd = inputSourceLength + _internalXml.Length - inputSourceLength - 1;
                result2.InternalText = _internalXml.ToString(inputSourceLength, _internalXml.Length - inputSourceLength);
                result2.Type = XMlType.Node;
            }
        }

        /// <summary>
        /// Немного колхоз, но работает точно корректно. Также учитывает отступы по спецсимволам.
        /// </summary>
        void GetPositionInSourceText()
        {
	        Indexes_Source_Internal = new Dictionary<int, int>();
            
	        var j = -1;
            for (var i = 0; i < Xml.Length; i++)
            {
                var ch = Xml[i];
                if (char.IsWhiteSpace(ch))
                    continue;

                if (XML.IsSymbolName(Xml, i, out var symbolName, out var symbolResult))
                {
                    ch = symbolResult;
                    i = i + symbolName.Length - 1;
                }

                j++;
                var outerCh = _internalXml[j];
                while (char.IsWhiteSpace(outerCh) && _internalXml.Length > j + 1)
                {
                    j++;
                    outerCh = _internalXml[j];
                }

                if (ch == outerCh || ((ch == '\'' || ch == '"') && (outerCh == '\'' || outerCh == '"')))
                {
                    Indexes_Source_Internal.Add(j, i);
                }
                else
                {
                    // ошибка считывания если последнее условие не выполнилось
                    while (ch != outerCh)
                    {
                        i++;
                        if (i > Xml.Length)
                            break;

                        ch = Xml[i];
                    }
                    Indexes_Source_Internal.Add(j, i);
                }
            }
        }
	}

	public enum XMlType
	{
		Unknown = 0,
		Attribute = 1,
		Node = 2
	}

    public class XPathNavigatorResult : XPathResult
	{
		internal XPathNavigator2 Parent { get; set; }

		internal int InternalStart { get; set; } = -1;
		internal int InternalEnd { get; set; } = -1;
		internal string InternalText { get; set; }

        public int Start => GetSourceIndex(InternalStart);

        public int End => GetSourceIndex(InternalEnd) + 1;

        public string Text
        {
	        get
	        {
		        var start = Start;
		        var end = End;
		        return start >= 0 && end >= 0 && Parent.Xml.Length >= end ? Parent.Xml.Substring(start, end - start) : null;
	        }
        }

        public XMlType Type { get; internal set; } = XMlType.Unknown;

		public XPathNavigatorResult(int id, string nodeType, string nodeName, string value, XmlNode node)
			: base(id, nodeType, nodeName, value, node)
		{

		}

		int GetSourceIndex(int internalIndex)
		{
			if (Parent?.Indexes_Source_Internal != null && internalIndex >= 0 && Parent.Indexes_Source_Internal.TryGetValue(internalIndex, out var result))
				return result;
			return -1;
        }

		public override string ToString()
		{
			return Parent == null ? base.ToString() : $"{Type:G} | Index - {(InternalStart >= 0 && InternalEnd >= 0 ? "Found" : "Not Found")}";
		}
    }

    public class XPathResult
    {
        public int ID { get; }
        public string NodeType { get; }
        public string NodeName { get; }
        public string Value { get; }
        public XmlNode Node { get; }

        public XPathResult() { }

        internal XPathResult(int id, string nodeType, string nodeName, string value, XmlNode node)
        {
	        ID = id;
	        NodeType = nodeType;
	        NodeName = nodeName;
	        Value = value;
	        Node = node;
        }

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
	    public static XPathNavigator2 SelectFirst(string sourceXml, string xpath)
	    {
		    return new XPathNavigator2(sourceXml, xpath, true);
	    }

        public static XPathNavigator2 Select(string sourceXml, string xpath)
	    {
		    return new XPathNavigator2(sourceXml, xpath, false);
        }

        public static bool SelectFirst(this XmlDocument document, string xpath, out XPathResult result)
        {
            return SelectFirst(document.CreateNavigator(), xpath, out result);
        }

        public static bool SelectFirst(this XPathNavigator navigator, string xpath, out XPathResult result)
        {
            result = null;
            var collection = Select(navigator, xpath, true);
            if (collection == null || !collection.Any())
                return false;

            result = collection.First();
            return true;
        }

        public static bool Select(this XmlDocument document, string xpath, out IList<XPathResult> result)
        {
            return Select(document.CreateNavigator(), xpath, out result);
        }

        public static bool Select(this XPathNavigator navigator, string xpath, out IList<XPathResult> result)
        {
            result = Select(navigator, xpath);
            return result != null;
        }

        public static IList<XPathResult> Select(XmlDocument document, string xpath, bool getFirst = false)
        {
            return Select(document.CreateNavigator(), xpath, getFirst);
        }

        public static IList<XPathResult> Select(XPathNavigator navigator, string xpath, bool getFirst = false)
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

	                    var xpathRes = new XPathNavigatorResult(
		                    i++,
		                    current.NodeType.ToString(),
		                    current.Name,
		                    current.Value,
		                    current is IHasXmlNode node1 ? node1.GetNode() : null);

	                    nodeSetResult.Add(xpathRes);

	                    if (getFirst)
		                    return new ReadOnlyCollection<XPathResult>(nodeSetResult);
                    }

                    return new ReadOnlyCollection<XPathResult>(nodeSetResult);
                default:
                    var obj = navigator.Evaluate(expression);
                    if (obj == null)
                        return null;

                    var nodeName = "Empty";
                    XmlNode node = null;
                    if (obj is IHasXmlNode node2)
                    {
	                    node = node2.GetNode();
	                    nodeName = node.NodeType.ToString();
                    }

                    var result = new XPathNavigatorResult(
	                    0,
	                    expression.ReturnType.ToString(),
	                    nodeName,
	                    obj.ToString(),
	                    node);

                    
                    return new ReadOnlyCollection<XPathResult>(new XPathResult[] { result });
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