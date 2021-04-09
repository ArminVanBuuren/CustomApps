using System;
using System.Xml.Serialization;

namespace LogsReader.Config
{
	[Serializable]
	[XmlRoot("Folder")]
	public class LRFolder : LRItem
	{
		[XmlAttribute("allDirSearching")]
		public bool AllDirSearching { get; set; } = true;

		public LRFolder()
			: base(string.Empty)
		{
		}

		internal LRFolder(string folderPath, bool allDirSearching)
			: base(folderPath)
			=> AllDirSearching = allDirSearching;
	}
}