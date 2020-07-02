using System;
using System.Xml;
using System.Xml.Serialization;

namespace Utils
{
	[Serializable]
	public class CustomFunctions
	{
		[XmlElement("Assemblies")]
		public CustomFunctionAssemblies Assemblies { get; set; }

		[XmlElement("Namespaces")]
		public XmlNodeValueText Namespaces { get; set; }

		[XmlElement("Code")]
		public CustomFunctionCode Functions { get; set; }
	}

	[Serializable, XmlRoot("Assemblies")]
	public class CustomFunctionAssemblies
	{
		[XmlElement("Assembly")]
		public XmlNodeValueText[] Childs { get; set; }
	}

	[Serializable, XmlRoot("Code")]
	public class CustomFunctionCode
	{
		[XmlElement("Function")]
		public XmlNodeCDATAText[] Function { get; set; }
	}

	[Serializable]
	public class XmlNodeValueText : XmlNodeValue
	{
		public XmlNodeValueText() : base((item) => new XmlNode[] { new XmlDocument().CreateTextNode(item) }, string.Empty) { }
		public XmlNodeValueText(string value) : base((item) => new XmlNode[] { new XmlDocument().CreateTextNode(item) }, value) { }
	}

	[Serializable]
	public class XmlNodeCDATAText : XmlNodeValue
	{
		public XmlNodeCDATAText() : base((item) => new XmlNode[] { new XmlDocument().CreateCDataSection(item) }, string.Empty) { }

		public XmlNodeCDATAText(string value) : base((item) => new XmlNode[] { new XmlDocument().CreateCDataSection(item) }, value) { }
	}

	[Serializable]
	public abstract class XmlNodeValue
	{
		private XmlNode[] _item;
		private Func<string, XmlNode[]> _getValue;

		protected XmlNodeValue(Func<string, XmlNode[]> getValue, string value)
		{
			_getValue = getValue;
			_item = GetValue(value);
		}

		[XmlText]
		public virtual XmlNode[] Item
		{
			get => _item;
			set
			{
				if (value == null || value.Length == 0)
				{
					_item = GetValue(string.Empty);
					return;
				}
				_item = GetValue(value[0].Value);
			}
		}

		protected XmlNode[] GetValue(string value)
		{
			return _getValue.Invoke(value);
		}
	}
}
