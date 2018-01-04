using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using TFSGeneration.Control.ConditionEx;

namespace TFSGeneration.Control.DataBase.Settings
{
	[XmlRoot("TFSOption")]
	public class OptionTFS
	{
		private SettingValue<string> _tFSUri = new SettingValue<string> { Value = "https://tfs.bss.nvision-group.com" };

		XmlNode[] _cdataGetDublicateTFS =
		{
			new XmlDocument().CreateCDataSection(
				"SELECT [System.Id] \r\nFROM WorkItems \r\nWHERE [System.TeamProject] = 'Support' \r\nAND [System.Title] Contains '%ParceBody_ITHD%'")
		};

		[XmlElement("TFSUri")]
		public SettingValue<string> TFSUri
		{
			get { return _tFSUri; }
			set { _tFSUri = value ?? _tFSUri; }
		}

		[XmlElement("GetDublicateTFS")]
		public XmlNode[] GetDublicateTFS
		{
			get { return _cdataGetDublicateTFS; }
			set { _cdataGetDublicateTFS = value ?? _cdataGetDublicateTFS; }
		}

		[XmlElement("CreateTFS")]
		public CreateTFS TFSCreate { get; set; } = new CreateTFS();
	}

}
