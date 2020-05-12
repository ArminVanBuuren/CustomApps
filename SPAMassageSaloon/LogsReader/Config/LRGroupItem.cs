using System;
using System.Xml.Serialization;

namespace LogsReader.Config
{
	[Serializable, XmlRoot("Group")]
	public class LRGroupItem : LRItem
	{
		private string _groupName = "DEFAULT";

		[XmlAttribute("name")]
		public string GroupName
		{
			get => _groupName;
			set => _groupName = value.ToUpper();
		}

		public LRGroupItem() : base("item") { }

		internal LRGroupItem(string groupName, string items) : base(items)
		{
			GroupName = groupName;
		}
	}
}
