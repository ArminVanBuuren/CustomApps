using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LogsReader.Config;
using LogsReader.Properties;
using Utils;

namespace LogsReader.Reader
{
	public delegate V GetTransactionDelegate<in T, U, out V>(T input, out U output);

	public abstract class LogsReaderControl : IDisposable
	{
		private static long _seqBaseInstance = 0;
		private readonly long _instanceId = 0;

		private readonly object trnSync = new object();
		private readonly LRSettingsScheme _currentSettings;
		private readonly Dictionary<string, string> _transactionValues;
		private readonly Crutch _findTrnPattern;

		/// <summary>
		/// Проверяет на совпадение по обычному поиску
		/// </summary>
		protected Func<string, bool> IsMatch { get; private set; }

		/// <summary>
		/// Проверяет на совпадение по найденным транзакциям
		/// </summary>
		protected GetTransactionDelegate<string, string, bool> IsMatchByTransactions { get; }

		protected Func<(string server, string filePath, string originalFolder), TraceReader> GetTraceReader { get; }

		public Encoding Encoding => _currentSettings.Encoding;

		public int MaxTraceLines => _currentSettings.MaxLines;

		public int MaxThreads => _currentSettings.MaxThreads;

		public int RowsLimit => _currentSettings.RowsLimit;

		public LRTraceParsePatternItem[] TraceParsePatterns => _currentSettings.TraceParse.Patterns;

		public LRTraceParseTransactionItem[] TransactionPatterns => _currentSettings.TraceParse.TransactionPatterns;

		public string OutputDateFormat => _currentSettings.TraceParse.OutputDateFormat;

		public Regex StartTraceWith => _currentSettings.TraceParse.StartTraceWith;

		public Regex EndTraceWith => _currentSettings.TraceParse.EndTraceWith;

		public bool IsDisposed { get; private set; } = false;

		protected LogsReaderControl(LRSettingsScheme settings, string findMessage, bool useRegex)
		{
			_instanceId = ++_seqBaseInstance;

			_currentSettings = settings;

			ResetMatchFunc(findMessage, useRegex);

			_transactionValues = new Dictionary<string, string>();
			_findTrnPattern = new Crutch();
			IsMatchByTransactions = (string input, out string output) =>
			{
				output = null;
				if (_findTrnPattern.Value == null)
					return false;

				var match = _findTrnPattern.Value.Match(input);
				output = match.Value;
				return match.Success;
			};

			if (_currentSettings.TraceParse.StartTraceWith != null && _currentSettings.TraceParse.EndTraceWith != null)
				GetTraceReader = (data) => new TraceReaderStartWithEndWith(this, data.server, data.filePath, data.originalFolder);
			else if (_currentSettings.TraceParse.StartTraceWith != null)
				GetTraceReader = (data) => new TraceReaderStartWith(this, data.server, data.filePath, data.originalFolder);
			else if (_currentSettings.TraceParse.EndTraceWith != null)
				GetTraceReader = (data) => new TraceReaderEndWith(this, data.server, data.filePath, data.originalFolder);
			else
				GetTraceReader = (data) => new TraceReaderSimple(this, data.server, data.filePath, data.originalFolder);
		}

		protected LogsReaderControl(LogsReaderControl control)
		{
			_currentSettings = control._currentSettings;
			IsMatch = control.IsMatch;
			_transactionValues = control._transactionValues;
			_findTrnPattern = control._findTrnPattern;
			IsMatchByTransactions = control.IsMatchByTransactions;
			GetTraceReader = control.GetTraceReader;
		}

		class Crutch
		{
			public Regex Value { get; set; }
		}

		protected void ResetMatchFunc(string findMessage, bool useRegex)
		{
			if (useRegex)
			{
				if (!REGEX.Verify(findMessage))
					throw new ArgumentException(string.Format(Resources.Txt_LogsReaderPerformer_IncorrectSearchPattern, findMessage));

				var searchPattern = new Regex(findMessage, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, new TimeSpan(0, 0, 1));
				IsMatch = input => searchPattern.IsMatch(input);
			}
			else
			{
				IsMatch = input => input.IndexOf(findMessage, StringComparison.InvariantCultureIgnoreCase) != -1;
			}
		}

		protected void AddTransactionValue(string trn)
		{
			if(trn.IsNullOrEmptyTrim())
				return;

			lock (trnSync)
			{
				if (_transactionValues.ContainsKey(trn))
					return;
				_transactionValues.Add(trn, Regex.Escape(trn));

				if (_transactionValues.Count > 100)
					_transactionValues.Remove(_transactionValues.First().Key);

				var trnPattern = new StringBuilder(_transactionValues.Values.Sum(x => x.Length) + _transactionValues.Count);
				foreach (var value in _transactionValues.Values)
				{
					trnPattern.Append(value);
					trnPattern.Append("|");
				}

				_findTrnPattern.Value = new Regex(
					trnPattern.ToString().Trim('|'), 
					RegexOptions.Compiled | RegexOptions.CultureInvariant, 
					new TimeSpan(0, 0, _transactionValues.Count > 10 ? 10 : _transactionValues.Count));
			}
		}

		public override bool Equals(object obj)
		{
			var isEqual = false;
			if (obj is LogsReaderControl input)
				isEqual = _currentSettings == input._currentSettings && _instanceId == input._instanceId;
			return isEqual;
		}

		public override int GetHashCode()
		{
			// в качестве хэша должен быть только хэш настроек
			var hash = _currentSettings.GetHashCode() + 13;
			return hash;
		}

		public virtual void Dispose()
		{
			_transactionValues.Clear();
			IsDisposed = true;
		}
	}
}
