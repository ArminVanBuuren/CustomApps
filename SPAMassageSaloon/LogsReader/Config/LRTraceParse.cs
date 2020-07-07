using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using Utils;

namespace LogsReader.Config
{
	[Serializable, XmlRoot("TraceParse")]
	public class LRTraceParse : TraceParse
	{
		private CustomFunctions _customFunc;
		private LRTraceParsePatternItem[] _traceParsePatterns;
		private LRTraceParseTransactionItem[] _transactionParsePatterns;
		private XmlNode[] _startTraceWith;
		private XmlNode[] _endTraceWith;
		private XmlNode[] _isError;

		public LRTraceParse()
		{

		}

		internal LRTraceParse(string name)
		{
			switch (name)
			{
				case "MG":
					Patterns = new[]
					{
						new LRTraceParsePatternItem(@"(.+?)\s*\[\s*(.+?)\s*\]\s*(.*?)(\<.+\>)(.*)") {Date = "$1", TraceName = "$2", Description = "$3$5", Message = "$4"},
						new LRTraceParsePatternItem(@"(.+?)\s*\[\s*(.+?)\s*\]\s*(.+?)\s+(.+)") {Date = "$1", TraceName = "$2", Description = "$3", Message = "$4"},
						new LRTraceParsePatternItem(@"(.+?)\s*\[\s*(.+?)\s*\]\s*(.+)") {Date = "$1", TraceName = "$2", Message = "$3"}
					};
					TransactionPatterns = new[]
					{
						new LRTraceParseTransactionItem(@"\s+id=\""(.+?)\""") {Trn = "$1"},
						new LRTraceParseTransactionItem(@"\(trn=(\d+)\)") {Trn = "$1"}
					};
					StartWith = new XmlNode[] {new XmlDocument().CreateCDataSection(@"^\d+[.]\d+[.]\d+\s+\d+[:]\d+[:]\d+\.\d+\s+\[")};
					IsError = new XmlNode[] {new XmlDocument().CreateCDataSection(@"dbresult=\""(\-|\d{2,})|Exception") };
					break;
				case "SPA":
					Patterns = new[]
					{
						new LRTraceParsePatternItem(@"(\d+?)\u0001(.+?)\u0001(.+?)\u0001(.+?)\u0001(.*?)\u0001(\d*)")
							{ID = "$1", TraceName = "$2", Description = "$3", Date = "$4.$6", Message = "$5"}
					};
					TransactionPatterns = new[]
					{
						new LRTraceParseTransactionItem(@"OriginalID\s*\>(.+?)\<") {Trn = "$1"}
					};
					IsError = new XmlNode[] {new XmlDocument().CreateCDataSection(@"FAILURE|NoPrintout|Exception")};
					break;
				case "MGA":
					Patterns = new[]
					{
						new LRTraceParsePatternItem(@"(\d+[-]\d+[-]\d+\s+\d+[:]\d+[:]\d+[,]\d+)\s+\((\w+)\).*?[-]{49,}(.+)")
							{Date = "$1", TraceName = "$2", Message = "$3"}
					};
					StartWith = new XmlNode[] {new XmlDocument().CreateCDataSection(@"^(\d+[-]\d+[-]\d+\s+\d+[:]\d+[:]\d+[,]\d+)\s+\((\w+)\)")};
					IsError = new XmlNode[] {new XmlDocument().CreateCDataSection(@"dbresult=\""(\-|\d{2,})|Exception") };
					break;
			}
		}

		[XmlIgnore]
		internal override bool IsCorrect
		{
			get => IsCorrectPatterns || IsCorrectCustomFunction;
			set { }
		}

		[XmlIgnore]
		internal bool IsCorrectPatterns => _traceParsePatterns != null && _traceParsePatterns.Length > 0 && _traceParsePatterns.All(x => x.IsCorrect);

		/// <summary>
		/// Если кастомная функция валидно компилится
		/// </summary>
		[XmlIgnore] internal bool IsCorrectCustomFunction { get; private set; } = false;

		[XmlAttribute("displayDateFormat")] public string DisplayDateFormat { get; set; } = "dd.MM.yyyy HH:mm:ss.fff";

		[XmlElement("Pattern")]
		public LRTraceParsePatternItem[] Patterns
		{
			get => _traceParsePatterns;
			set
			{
				if (value == null)
					return;
				if (value.Length > 0)
					_traceParsePatterns = value;
			}
		}

		[XmlElement("Custom")]
		public CustomFunctions Custom
		{
			get => _customFunc;
			set
			{
				_customFunc = value;
				// проверяем на валидность кастомной функции
				if (_customFunc?.Assemblies?.Childs?.Length > 0 && _customFunc?.Namespaces?.Item?.Length > 0 && _customFunc.Functions?.Function?.Length > 0)
				{
					var res = GetCustomFunction();
					if (res != null)
						IsCorrectCustomFunction = true;
					return;
				}

				IsCorrectCustomFunction = false;
			}
		}

		[XmlElement("TransactionPattern")]
		public LRTraceParseTransactionItem[] TransactionPatterns
		{
			get => _transactionParsePatterns;
			set => _transactionParsePatterns = value;
		}

		[XmlElement("StartTraceWith")]
		public XmlNode[] StartWith
		{
			get => _startTraceWith;
			set => StartTraceWith = GetCDataNode(value, false, out _startTraceWith);
		}

		[XmlIgnore] internal Regex StartTraceWith { get; private set; }

		[XmlElement("EndTraceWith")]
		public XmlNode[] EndWith
		{
			get => _endTraceWith;
			set => EndTraceWith = GetCDataNode(value, false, out _endTraceWith);
		}

		[XmlIgnore] internal Regex EndTraceWith { get; private set; }

		[XmlElement("IsError")]
		public XmlNode[] IsError
		{
			get => _isError;
			set => IsTraceError = GetCDataNode(value, false, out _isError, RegexOptions.IgnoreCase);
		}

		[XmlIgnore] internal Regex IsTraceError { get; private set; }

		public (Func<string, TraceParseResult>, Func<string, bool>)? GetCustomFunction()
		{
			var compiler = new CustomFunctionsCompiler<ICustomTraceParse>(Custom);
			if (compiler.Functions.Count == 0)
				return null;

			var functions = new Dictionary<string, (Func<string, TraceParseResult>, Func<string, bool>)>();
			foreach (var (name, customFunction) in compiler.Functions)
				functions.Add(name, (customFunction.IsTraceMatch, customFunction.IsLineMatch));

			TraceParseResult IsTraceMatch(string input)
			{
				foreach (var func in functions.Values)
				{
					var result = func.Item1.Invoke(input);
					if (result.IsMatched) 
						return result;
				}

				return null;
			}

			bool IsLineMatch(string input)
			{
				foreach (var func in functions.Values)
					return func.Item2.Invoke(input);
				return false;
			}

			return (IsTraceMatch, IsLineMatch);
		}
	}
}