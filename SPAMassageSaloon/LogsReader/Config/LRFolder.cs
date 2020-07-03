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

		public LRFolder() : base(string.Empty) { }

		internal LRFolder(string folderPath, bool allDirSearching) : base(folderPath)
		{
			_allDirSearching = allDirSearching;
		}
	}
}
