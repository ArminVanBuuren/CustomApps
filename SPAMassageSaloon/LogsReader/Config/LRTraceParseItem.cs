using System;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace LogsReader.Config
{
	[Serializable]
	public class LRTraceParseItem : TraceParse
	{
		private XmlNode[] _cdataItem = new XmlNode[] { new XmlDocument().CreateCDataSection("(.+)") };

		public LRTraceParseItem()
		{
			CDataItem = _cdataItem;
		}

		internal LRTraceParseItem(string regexPattern)
		{
			CDataItem = new XmlNode[] { new XmlDocument().CreateCDataSection(regexPattern) };
		}


		[XmlText]
		public XmlNode[] CDataItem
		{
			get => _cdataItem;
			set
			{
				RegexItem = GetCDataNode(value, true, out _cdataItem);
				if (RegexItem != null)
				{
					IsCorrectRegex = true;
					return;
				}

				IsCorrectRegex = false;
			}
		}

		[XmlIgnore] internal override bool IsCorrectRegex { get; set; }
		[XmlIgnore] internal Regex RegexItem { get; private set; }
	}
}