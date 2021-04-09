using System;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace LogsReader.Config
{
	[Serializable]
	[XmlRoot("TransactionPattern")]
	public class LRTraceParseTransactionItem : LRTraceParseItem
	{
		public LRTraceParseTransactionItem()
		{
		}

		internal LRTraceParseTransactionItem(string regexPattern)
			: base(regexPattern)
		{
		}

		[XmlAttribute]
		public string Trn { get; set; } = string.Empty;

		public TraceParseTransactionResult GetParsingResult(Match match) => new TraceParseTransactionResult(this, match);

		public class TraceParseTransactionResult
		{
			public LRTraceParseTransactionItem Parent { get; }
			public Func<string, Func<Match, string>> MatchCalculationFunc { get; }

			public string Trn { get; }

			internal TraceParseTransactionResult(LRTraceParseTransactionItem parent, Match match)
			{
				Parent = parent;
				MatchCalculationFunc = LRSettings.MatchCalculationFunc;
				Trn = MatchCalculationFunc.Invoke(Parent.Trn).Invoke(match);
			}
		}
	}
}