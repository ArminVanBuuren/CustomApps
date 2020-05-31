using System;
using System.Xml.Serialization;

namespace LogsReader.Config
{
	[Serializable, XmlRoot("TransactionPattern")]
	public class LRTraceParseTransactionItem : LRTraceParseItem
	{
		public LRTraceParseTransactionItem() { }

		internal LRTraceParseTransactionItem(string regexPattern) : base(regexPattern) { }

		[XmlAttribute] public string Trn { get; set; } = string.Empty;
	}
}