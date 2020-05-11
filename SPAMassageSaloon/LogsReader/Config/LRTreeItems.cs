using System;
using System.Xml;
using System.Xml.Serialization;
using Utils;

namespace LogsReader.Config
{
	[Serializable, XmlRoot("Group")]
	public class LRGroupItem : LRItem
	{
		private string _groupName = "default";

		[XmlAttribute("name")]
		public string GroupName
		{
			get => _groupName;
			set => _groupName = value;
		}

		public LRGroupItem() : base("item") { }

		internal LRGroupItem(string groupName, string items) : base(items)
		{
			_groupName = groupName;
		}
	}

	[Serializable, XmlRoot("Folder")]
	public class LRFolder : LRItem
	{
		private bool _allDirectoriesSearch = true;

		[XmlAttribute("AllDirectoriesSearching")] 
		public bool AllDirectoriesSearching
		{
			get => _allDirectoriesSearch;
			set => _allDirectoriesSearch = value;
		}

		public LRFolder() { }

		internal LRFolder(string item, bool allDirSearching) : base(item)
		{
			_allDirectoriesSearch = allDirSearching;
		}
	}

	[Serializable]
	public class LRItem
	{
		private XmlNode[] _item = new XmlNode[] { new XmlDocument().CreateTextNode(string.Empty) };

		public LRItem() { }

		internal LRItem(string item)
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