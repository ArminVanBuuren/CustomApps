using System.Xml;
using System.Xml.Serialization;

namespace TFSAssist.Control.DataBase.Settings
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

	    [XmlIgnore]
        public SettingValue<string> TFSUserName { get; set; } = new SettingValue<string>();

        [XmlIgnore]
        public SettingValue<string> TFSUserPassword { get; set; } = new SettingValue<string>();


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
