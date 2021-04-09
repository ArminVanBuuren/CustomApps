using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LogsReader.Properties;

namespace LogsReader.Config
{
	[Serializable]
	public class LRGroups
	{
		private LRGroupItem[] _groupItems = { new LRGroupItem(string.Empty, 0, string.Empty) };

		[XmlIgnore]
		public Dictionary<string, (int, IEnumerable<string>)> Groups { get; private set; }

		[XmlElement("Group")]
		public LRGroupItem[] GroupItems
		{
			get => _groupItems;
			set
			{
				try
				{
					var prevVal = (value ?? _groupItems).OrderBy(x => x.Priority).ThenBy(x => x.GroupName).ToArray();
					var prevGroups = GetGroups(prevVal);
					_groupItems = prevVal;
					Groups = prevGroups;
				}
				catch (ArgumentException ex)
				{
					throw new ArgumentException(Resources.Txt_ServerGroups_ErrUnique, ex);
				}
			}
		}

		public LRGroups()
		{
		}

		internal LRGroups(LRGroupItem[] groupItems) => GroupItems = groupItems;

		private static Dictionary<string, (int, IEnumerable<string>)> GetGroups(IEnumerable<LRGroupItem> items)
			=> items.ToDictionary(k => k.GroupName,
			                      v => (v.PriorityInternal,
			                            v.Item[0].Value?.Split(',').GroupBy(p => p.Trim(), StringComparer.InvariantCultureIgnoreCase).Select(x => x.Key)),
			                      StringComparer.InvariantCultureIgnoreCase);
	}
}