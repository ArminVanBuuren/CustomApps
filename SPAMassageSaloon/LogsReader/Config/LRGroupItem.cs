using System;
using System.Xml.Serialization;
using Utils;

namespace LogsReader.Config
{
	[Serializable, XmlRoot("Group")]
	public class LRGroupItem : LRItem
	{
		[XmlAttribute("name")]
		public string GroupName { get; set; } = string.Empty;

		[XmlIgnore]
		internal int PriorityInternal { get; set; } = 0;

		[XmlAttribute("priority")]
		public string Priority
		{
			get => PriorityInternal.ToString();
			set
			{
				if (!value.IsNullOrWhiteSpace() && int.TryParse(value, out var result))
				{
					if (result >= 0 && result <= 99)
						PriorityInternal = result;
					else if (result > 99)
						PriorityInternal = 99;
					else
						PriorityInternal = 0;
					return;
				}

				PriorityInternal = 0;
			}
		}

		public LRGroupItem() : base(string.Empty)
		{
		}

		internal LRGroupItem(string groupName, int priority, string items) : base(items)
		{
			GroupName = groupName;
			Priority = priority.ToString();
		}
	}
}