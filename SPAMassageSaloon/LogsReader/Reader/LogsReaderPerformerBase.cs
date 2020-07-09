using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LogsReader.Config;
using LogsReader.Properties;
using Utils;

namespace LogsReader.Reader
{
	public delegate V GetTransactionDelegate<in T, U, out V>(T input, out U output);

	public abstract class LogsReaderPerformerBase : IDisposable
	{
		private static long _seqBaseInstance = 0;
		private readonly long _instanceId = 0;

		private readonly LRSettingsScheme _currentSettings;
		private readonly ConcurrentDictionary<string, Regex> _transactionValues;

		/// <summary>
		/// Проверяет на совпадение по обычному поиску
		/// </summary>
		protected Func<string, bool> IsMatch { get; private set; }

		/// <summary>
		/// Проверяет на совпадение по найденным транзакциям
		/// </summary>
		protected GetTransactionDelegate<string, string, bool> IsMatchByTransactions { get; }

		/// <summary>
		/// Функция для получение определенного <see cref="TraceReader"/> согласно настройкам
		/// </summary>
		protected Func<(string server, string filePath, string originalFolder), TraceReader> GetTraceReader { get; }

		public string SchemeName => _currentSettings.Name;

		public Encoding Encoding => _currentSettings.Encoding;

		public int MaxTraceLines => _currentSettings.MaxLines;

		public int MaxThreads => _currentSettings.MaxThreads;

		public int RowsLimit => _currentSettings.RowsLimit;

		public (Func<string, TraceParseResult>, Func<string, bool>)? TraceParseCustomFunction { get; }

		public LRTraceParsePatternItem[] TraceParsePatterns => _currentSettings.TraceParse.Patterns;

		public LRTraceParseTransactionItem[] TransactionPatterns => _currentSettings.TraceParse.TransactionPatterns;

		public string DisplayDateFormat => _currentSettings.TraceParse.DisplayDateFormat;

		public Regex StartTraceLineWith => _currentSettings.TraceParse.StartTraceLineWith;

		public Regex EndTraceLineWith => _currentSettings.TraceParse.EndTraceLineWith;

		public Regex IsTraceError => _currentSettings.TraceParse.IsTraceError;

		public bool IsDisposed { get; private set; } = false;

		protected LogsReaderPerformerBase(LRSettingsScheme settings, string findMessage, bool useRegex)
		{
			_instanceId = ++_seqBaseInstance;
			_currentSettings = settings;

			ResetMatchFunc(findMessage, useRegex);

			_transactionValues = new ConcurrentDictionary<string, Regex>();
			IsMatchByTransactions = (string input, out string output) =>
			{
				output = null;

				foreach (var regex in _transactionValues.Values.ToList())
				{
					if (regex == null)
						continue;

					var match = regex.Match(input);
					if (!match.Success)
						continue;

					output = match.Value;
					return true;
				}

				return false;
			};

			if (_currentSettings.TraceParse.StartTraceLineWith != null && _currentSettings.TraceParse.EndTraceLineWith != null)
				GetTraceReader = (data) => new TraceReaderStartWithEndWith(this, data.server, data.filePath, data.originalFolder);
			else if (_currentSettings.TraceParse.StartTraceLineWith != null)
				GetTraceReader = (data) => new TraceReaderStartWith(this, data.server, data.filePath, data.originalFolder);
			else if (_currentSettings.TraceParse.EndTraceLineWith != null)
				GetTraceReader = (data) => new TraceReaderEndWith(this, data.server, data.filePath, data.originalFolder);
			else
				GetTraceReader = (data) => new TraceReaderSimple(this, data.server, data.filePath, data.originalFolder);

			if (_currentSettings.TraceParse.IsCorrectCustomFunction)
				TraceParseCustomFunction = _currentSettings.TraceParse.GetCustomFunction();
		}

		protected LogsReaderPerformerBase(LogsReaderPerformerBase control)
		{
			_currentSettings = control._currentSettings;
			IsMatch = control.IsMatch;
			_transactionValues = control._transactionValues;
			IsMatchByTransactions = control.IsMatchByTransactions;
			GetTraceReader = control.GetTraceReader;
			TraceParseCustomFunction = control.TraceParseCustomFunction;
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

		/// <summary>
		/// Добавить спарсенную транзакцию в общий список для всех лог файлов, текущей схемы
		/// </summary>
		/// <param name="trn"></param>
		protected bool AddTransactionValue(string trn)
		{
			if (trn.IsNullOrEmptyTrim())
				return false;

			if (_transactionValues.ContainsKey(trn))
				return false;

			_transactionValues.TryAdd(trn, new Regex(Regex.Escape(trn),
				RegexOptions.Compiled | RegexOptions.CultureInvariant,
				new TimeSpan(0, 0, 1)));

			if (_transactionValues.Count > 100)
				_transactionValues.TryRemove(_transactionValues.First().Key, out _);

			return true;
		}

		/// <summary>
		/// Сравниваются только базовые настройки и инстанс. Игнорируется изменения настроек поиска свойства IsMatch.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			var isEqual = false;
			if (obj is LogsReaderPerformerBase input)
				isEqual = _currentSettings == input._currentSettings && _instanceId == input._instanceId;
			return isEqual;
		}

		/// <summary>
		/// в качестве хэша должен быть только хэш базовых настроек
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
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
