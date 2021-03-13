using System;
using System.Xml;
using System.Xml.Serialization;
using Utils;

namespace LogsReader.Config
{
	[Serializable]
	public class LRItem
	{
		private XmlNode[] _item = new XmlNode[]
		{
			new XmlDocument().CreateTextNode(string.Empty)
		};

		public LRItem()
		{
		}

		internal LRItem(string item)
			=> Item = new XmlNode[]
			{
				new XmlDocument().CreateTextNode(item)
			};

		[XmlText]
		public XmlNode[] Item
		{
			get => _item;
			set
			{
				if (value == null || value.Length == 0)
					return;
				Value = value[0].Value?.ReplaceUTFCodeToSymbol();
				_item = new XmlNode[]
				{
					new XmlDocument().CreateTextNode(Value)
				};
			}
		}

		internal string Value { get; private set; }
	}
}