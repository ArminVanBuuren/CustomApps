using System;
using System.Xml.Serialization;

namespace LogsReader.Config
{
	[Serializable, XmlRoot("Folder")]
	public class LRFolder : LRItem
	{
		private bool _allDirSearching = true;

		[XmlAttribute("allDirSearching")]
		public bool AllDirSearching
		{
			get => _allDirSearching;
			set => _allDirSearching = value;
		}

		public LRFolder() : base(@"C:\TEST") { }

		internal LRFolder(string item, bool allDirSearching) : base(item)
		{
			_allDirSearching = allDirSearching;
		}
	}
}
