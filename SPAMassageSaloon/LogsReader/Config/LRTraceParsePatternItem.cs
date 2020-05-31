using System;
using System.Xml.Serialization;

namespace LogsReader.Config
{
	[Serializable, XmlRoot("Pattern")]
	public class LRTraceParsePatternItem : LRTraceParseItem
	{
		public LRTraceParsePatternItem() { }

		internal LRTraceParsePatternItem(string regexPattern) : base(regexPattern) { }

		[XmlAttribute] public string ID { get; set; } = string.Empty;
		[XmlAttribute] public string Date { get; set; } = string.Empty;
		[XmlAttribute] public string TraceName { get; set; } = string.Empty;
		[XmlAttribute] public string Description { get; set; } = string.Empty;
		[XmlAttribute] public string Message { get; set; } = string.Empty;
	}
}