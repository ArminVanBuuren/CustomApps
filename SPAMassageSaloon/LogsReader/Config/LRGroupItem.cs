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
			set => _groupName = value.ToUpper();
		}

		[XmlIgnore] internal int InternalPriority { get; set; } = 0;

		[XmlAttribute("priority")]
		public string Priority
		{
			get => InternalPriority.ToString();
			set
			{
				if (!value.IsNullOrEmptyTrim() && int.TryParse(value, out var result) && result >= 0)
				{
					InternalPriority = result;
					return;
				}

				InternalPriority = 0;
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
