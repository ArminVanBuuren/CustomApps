﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LogsReader.Properties;

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
					var prevLogFolders = (value ?? _logsFolder).OrderBy(x => x.Item[0].Value).ToArray();
					var prevFolders = prevLogFolders.ToDictionary(x => x.Item[0].Value.Trim(), x => x.AllDirSearching, StringComparer.InvariantCultureIgnoreCase);

					_logsFolder = prevLogFolders;
					Folders = prevFolders;
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