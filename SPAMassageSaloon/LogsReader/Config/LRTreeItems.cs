using System;
using System.Xml;
using System.Xml.Serialization;
using Utils;

namespace LogsReader.Config
{
	[Serializable, XmlRoot("FileTypeGroup")]
	public class LRGroupItem : LRFolder
	{
		private string _groupName = "default";

		public LRGroupItem() : base("item") { }

		internal LRGroupItem(string groupName, string items) : base(items)
		{
			_groupName = groupName;
		}

		[XmlAttribute("name")]
		public string GroupName
		{
			get => _groupName;
			set => _groupName = value;
		}
	}

	[Serializable, XmlRoot("Folder")]
	public class LRFolder
	{
		private XmlNode[] _item = new XmlNode[] { new XmlDocument().CreateTextNode(@"C:\TEST") };

		public LRFolder() { }

		internal LRFolder(string item)
		{
			_item = new XmlNode[] { new XmlDocument().CreateTextNode(item) };
		}

		[XmlText]
		public XmlNode[] Item
		{
			get => _item;
			set
			{
				if (value == null)
					return;
				if (value.Length > 0)
					_item = new XmlNode[] { new XmlDocument().CreateTextNode(value[0].Value.ReplaceUTFCodeToSymbol()) };
			}
		}
	}
}