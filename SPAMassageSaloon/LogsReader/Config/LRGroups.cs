using LogsReader.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Utils;

namespace LogsReader.Config
{
	[Serializable]
	public class LRGroups
	{
		private LRGroupItem[] _groupItems; 
		
		[XmlIgnore] public Dictionary<string, IEnumerable<string>> Groups { get; private set; }

		[XmlElement("Group")]
		public LRGroupItem[] GroupItems
		{
			get => _groupItems;
			set
			{
				try
				{
					_groupItems = (value ?? _groupItems).OrderBy(x => x.GroupName).ToArray();
					Groups = GetGroups(_groupItems);
				}
				catch (ArgumentException ex)
				{
					throw new ArgumentException(Resources.Txt_ServerGroups_ErrUnique, ex);
				}
			}
		}

		public LRGroups() { }

		internal LRGroups(LRGroupItem[] groupItems)
		{
			GroupItems = groupItems;
		}

		static Dictionary<string, IEnumerable<string>> GetGroups(LRGroupItem[] items)
		{
			var result = items.ToDictionary(k => k.GroupName, v => v.Item[0].Value.Split(',')
				.GroupBy(p => p.Trim(), StringComparer.InvariantCultureIgnoreCase)
				.Select(x => x.Key), StringComparer.InvariantCultureIgnoreCase);

			if (result.Keys.Any(key => key.IsNullOrEmptyTrim()))
				throw new Exception(Resources.Txt_Forms_GroupNameIsIncorrect);

			if (result.Values.Any(key => key.Any(x => x.IsNullOrEmptyTrim())))
				throw new Exception(Resources.Txt_Forms_GroupChildItemIsIncorrect);

			return result;
		}
	}
}