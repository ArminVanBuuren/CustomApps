﻿using System;
using System.Xml.Serialization;

namespace TFSAssist.Control.DataBase.Settings
{
    [Serializable, XmlRoot("Settings")]
	public class SettingsCollection
	{
	    private SettingValue<int> _interval = new SettingValue<int> { Value = 3 };
		private SettingBootRun _bootRun = new SettingBootRun();


        [XmlElement("Interval")]
		public SettingValue<int> Interval
		{
			get { return _interval; }
			set { _interval = value == null || value.Value < 1 ? _interval : value; }
		}

		[XmlElement("BootRun")]
		public SettingBootRun BootRun
		{
		    get
		    {
                return _bootRun;		        
		    }
		    set
		    {
		        if (value == null)
		        {
		            _bootRun.Value = false;
                    return;
		        }
		        _bootRun.Value = value.Value;
		    }
		}



		[XmlElement("MailOption")]
		public OptionMail MailOption { get; set; } = new OptionMail();

		[XmlElement("TFSOption")]
		public OptionTFS TFSOption { get; set; } = new OptionTFS();

	}

}
