using System;
using System.Xml.Serialization;
using Utils;

namespace LogsReader.Config
{
	[Serializable, XmlRoot("Group")]
	public class LRGroupItem : LRItem
	{
		private string _groupName = string.Empty;

		[XmlAttribute("name")]
		public string GroupName
		{
			get => _groupName;
			set => _groupName = value;
		}

		[XmlIgnore] internal int PriorityInternal { get; set; } = 0;

		[XmlAttribute("priority")]
		public string Priority
		{
			get => PriorityInternal.ToString();
			set
			{
				if (!value.IsNullOrWhiteSpace() && int.TryParse(value, out var result) && result >= 0)
				{
					PriorityInternal = result;
					return;
				}

				PriorityInternal = 0;
			}
		}

		public LRGroupItem() : base(string.Empty) { }

		internal LRGroupItem(string groupName, int priority, string items) : base(items)
		{
			GroupName = groupName;
			Priority = priority.ToString();
		}
	}
}
