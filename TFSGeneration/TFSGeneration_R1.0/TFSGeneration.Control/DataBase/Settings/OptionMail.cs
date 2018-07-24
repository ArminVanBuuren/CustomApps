using System;
using System.Xml;
using System.Xml.Serialization;
using Utils;

namespace TFSAssist.Control.DataBase.Settings
{
	[XmlRoot("MailOption", IsNullable = false)]
	public class OptionMail
	{
		private SettingValue<string> _exchangeUri = new SettingValue<string> {Value = "https://e-mail.mts.ru/ews/exchange.asmx"};
		private SettingValue<string> _authorizationTimeout = new SettingValue<string> {Value = "15"};
		private SettingValue<bool> _debugLogging = new SettingValue<bool> {Value = false};
	    private SettingValue<string> _startDate = new SettingValue<string> { Value = DateTime.Now.ToString("G") };
	    private SettingValue<string> _filterMailFrom = new SettingValue<string> { Value = "jira@mts.by" };
	    private SettingValue<string> _sourceFolder = new SettingValue<string> { Value = string.Empty };
	    private SettingValue<string> _filterSubject = new SettingValue<string> { Value = "jira" };

        XmlNode[] _getRegexParceBody =
		{
			new XmlDocument().CreateCDataSection(
				@".+На\s*вашу\s*группу\s*\[.+?\]\s*назначена\s*новая\s*заявка\.\s*(?<TITLE>.+?)\s*Код:\s*(?<ITHD>.+?)\s*URL:\s*(?<URL>.+?)\s*Проект:\s*(?<PROJECT>.+?)\s*Тип\s*запроса:\s*(?<REQTYPE>.+?)\s*$.+")
		};

		XmlNode[] _getRegexParceSubject =
		{
			new XmlDocument().CreateCDataSection(@".+Приоритет:\s*\[(?<SEVERITY>.+?)\].+")
		};

	    [XmlIgnore]
	    public SettingValue<string> Address { get; set; } = new SettingValue<string>();

	    [XmlIgnore]
	    public SettingValue<string> UserName { get; set; } = new SettingValue<string>();

	    [XmlIgnore]
	    public SettingValue<string> Password { get; set; } = new SettingValue<string>();


        [XmlElement("ExchangeUri")]
	    public SettingValue<string> ExchangeUri
	    {
	        get { return _exchangeUri; }
	        set { _exchangeUri = value ?? _exchangeUri; }
	    }

	    [XmlElement("AuthorizationTimeout")]
	    public SettingValue<string> AuthorizationTimeout
        {
	        get { return _authorizationTimeout; }
	        set { _authorizationTimeout = value ?? _authorizationTimeout; }
	    }

        [XmlElement("StartDate")]
	    public SettingValue<string> StartDate
	    {
	        get
	        {
	            return _startDate;
	        }
	        set
	        {
	            _startDate = value == null || value.Value.IsNullOrEmpty()
	                ? _startDate
	                : new SettingValue<string>
	                {
	                    Value = DateTime.Parse(value.Value).ToString("G")
	                };
	        }
	    }

	    [XmlElement("SourceFolder")]
	    public SettingValue<string> SourceFolder
	    {
	        get { return _sourceFolder; }
	        set { _sourceFolder = value ?? _sourceFolder; }
	    }

	    [XmlElement("FilterMailFrom")]
	    public SettingValue<string> FilterMailFrom
	    {
	        get { return _filterMailFrom; }
	        set { _filterMailFrom = value ?? _filterMailFrom; }
	    }

	    [XmlElement("FilterSubject")]
	    public SettingValue<string> FilterSubject
	    {
	        get { return _filterSubject; }
	        set { _filterSubject = value ?? _filterSubject; }
	    }


	    [XmlElement("DebugLogging")]
	    public SettingValue<bool> DebugLogging
	    {
	        get { return _debugLogging; }
	        set { _debugLogging = value ?? _debugLogging; }
	    }

        [XmlElement]
		public XmlNode[] ParceBody
		{
			get { return _getRegexParceBody; }
			set { _getRegexParceBody = value ?? _getRegexParceBody; }
		}

		[XmlElement]
		public XmlNode[] ParceSubject
		{
			get { return _getRegexParceSubject; }
			set { _getRegexParceSubject = value ?? _getRegexParceSubject; }
		}
	}
}
