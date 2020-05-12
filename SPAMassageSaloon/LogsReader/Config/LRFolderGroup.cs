using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LogsReader.Properties;
using Utils;

namespace LogsReader.Config
{
	[Serializable, XmlRoot("LogsFolderGroup")]
	public class LRFolderGroup
	{
		private LRFolder[] _logsFolder = new LRFolder[] { new LRFolder() };

		[XmlIgnore] public Dictionary<string, bool> Folders { get; private set; }

		[XmlElement("Folder")]
		public LRFolder[] LogsFolder
		{
			get => _logsFolder;
			set
			{
				try
				{
					_logsFolder = (value ?? _logsFolder).OrderBy(x => x.Item[0].Value).ToArray();
					Folders = _logsFolder.ToDictionary(x => x.Item[0].Value.Trim(), x => x.AllDirSearching, StringComparer.InvariantCultureIgnoreCase);

					var incorrectFolder = Folders.Keys.FirstOrDefault(key => key.IsNullOrEmptyTrim() || !IO.CHECK_PATH.IsMatch(key));
					if (incorrectFolder != null)
						throw new Exception(string.Format(Resources.Txt_Forms_FolderIsIncorrect, incorrectFolder));
				}
				catch (ArgumentException ex)
				{
					throw new ArgumentException(Resources.Txt_Folders_ErrUnique, ex);
				}
			}
		}

		public LRFolderGroup() { }

		internal LRFolderGroup(LRFolder[] folders)
		{
			LogsFolder = folders;
		}
	}
}
