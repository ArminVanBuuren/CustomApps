using System;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using LogsReader.Reader;

namespace LogsReader.Config
{
	[Serializable, XmlRoot("Pattern")]
	public class LRTraceParsePatternItem : LRTraceParseItem
	{
		public LRTraceParsePatternItem() { }

		internal LRTraceParsePatternItem(string regexPattern) : base(regexPattern) { }

		[XmlAttribute(nameof(DataTemplate.Tmp.ID))]
		public string ID { get; set; } = string.Empty;

		[XmlAttribute(nameof(DataTemplate.Tmp.Date))] 
		public string Date { get; set; } = string.Empty;

		[XmlAttribute(nameof(DataTemplate.Tmp.TraceName))] 
		public string TraceName { get; set; } = string.Empty;

		[XmlAttribute(DataTemplate.HeaderDescription)] 
		public string Description { get; set; } = string.Empty;

		[XmlAttribute(DataTemplate.HeaderMessage)] 
		public string Message { get; set; } = string.Empty;

		public TraceParseResult GetParsingResult(Match match) => new TraceParsePatternResult(this, match);

		public class TraceParsePatternResult : TraceParseResult
		{
			public LRTraceParsePatternItem Parent { get; }
			public Func<string, Func<Match, string>> MatchCalculationFunc { get; }

			internal TraceParsePatternResult(LRTraceParsePatternItem parent, Match match)
			{
				Parent = parent;
				MatchCalculationFunc = LRSettings.MatchCalculationFunc;

				ID = MatchCalculationFunc.Invoke(Parent.ID).Invoke(match);
				Date = MatchCalculationFunc.Invoke(Parent.Date).Invoke(match);
				TraceName = MatchCalculationFunc.Invoke(Parent.TraceName).Invoke(match);
				Description = MatchCalculationFunc.Invoke(Parent.Description).Invoke(match);
				Message = MatchCalculationFunc.Invoke(Parent.Message).Invoke(match);
			}
		}
	}
}