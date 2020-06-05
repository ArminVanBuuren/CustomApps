using System;
using System.Text.RegularExpressions;
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

		public TraceParsePatternResult GetParsingResult(Match match)
		{
			return new TraceParsePatternResult(this, match);
		}

		public class TraceParsePatternResult
		{
			public LRTraceParsePatternItem Parent { get; }
			public Func<string, Func<Match, string>> MatchCalculationFunc { get; }

			public string ID { get; }
			public string Date { get; }
			public string TraceName { get; }
			public string Description { get; }
			public string Message { get; }

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