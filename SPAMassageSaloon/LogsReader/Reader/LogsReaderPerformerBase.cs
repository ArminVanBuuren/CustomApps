using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LogsReader.Config;
using LogsReader.Properties;
using Utils;
using Utils.WinForm.DataGridViewHelper;

namespace LogsReader.Reader
{
	public delegate V OutFuncDelegate<in T, U, out V>(T input, out U output);
	public delegate V DoubleOutFuncDelegate<in T, U, S, out V>(T input, out U output1, out S output2);

	public abstract class LogsReaderPerformerBase : IDisposable
	{
		private static long _seqBaseInstance = 0;
		private readonly long _instanceId = 0;
		private readonly object _syncTrn = new object();

		private readonly LRSettingsScheme _currentSettings;
		private readonly Dictionary<string, Regex> _transactionValues;

		/// <summary>
		/// Проверяет на совпадение по обычному поиску
		/// </summary>
		protected Func<string, bool> IsMatch { get; private set; }

		/// <summary>
		/// Проверяет на совпадение по найденным транзакциям
		/// </summary>
		internal OutFuncDelegate<string, string, bool> IsMatchByTransactions { get; }

		/// <summary>
		/// Функция для получение определенного <see cref="TraceReader"/> согласно настройкам
		/// </summary>
		protected Func<(string server, string filePath, string originalFolder), TraceReader> GetTraceReader { get; }

		[DGVColumn(ColumnPosition.After, "SchemeName")]
		public string SchemeName => _currentSettings.Name;

		public Encoding Encoding => _currentSettings.Encoding;

		public int MaxTraceLines => _currentSettings.MaxLines;

		public int MaxThreads => _currentSettings.MaxThreads;

		public int RowsLimit => _currentSettings.RowsLimit;

		public (Func<string, TraceParseResult>, Func<string, bool>)? TraceParseCustomFunction { get; }

		public LRTraceParsePatternItem[] TraceParsePatterns => _currentSettings.TraceParse.Patterns;

		public LRTraceParseTransactionItem[] TransactionPatterns => _currentSettings.TraceParse.TransactionPatterns;

		public DoubleOutFuncDelegate<string, DateTime, string, bool> TryParseDate { get; }

		public Regex StartTraceLineWith => _currentSettings.TraceParse.StartTraceLineWith;

		public Regex EndTraceLineWith => _currentSettings.TraceParse.EndTraceLineWith;

		public Regex IsTraceError => _currentSettings.TraceParse.IsTraceError;

		public bool IsDisposed { get; private set; } = false;

		protected LogsReaderPerformerBase(LRSettingsScheme settings, string findMessage, bool useRegex)
		{
			_instanceId = ++_seqBaseInstance;
			_currentSettings = settings;

			ResetMatchFunc(findMessage, useRegex);

			Func<DateTime, string> getDisplayDate;
			if (_currentSettings.TraceParse.DisplayCulture != null)
				getDisplayDate = (date) => date.ToString(_currentSettings.TraceParse.DisplayDateFormat, _currentSettings.TraceParse.DisplayCulture);
			else
				getDisplayDate = (date) => date.ToString(_currentSettings.TraceParse.DisplayDateFormat);

			if (_currentSettings.CultureList.Count > 0)
			{
				TryParseDate = (string dateValue, out DateTime result, out string displayDate) =>
				{
					foreach (var cultureInfo in _currentSettings.CultureList)
					{
						if (DateTime.TryParse(dateValue, cultureInfo, DateTimeStyles.AllowWhiteSpaces, out result))
						{
							displayDate = getDisplayDate(result);
							return true;
						}
					}

					displayDate = string.Empty;
					result = DateTime.MinValue;
					return false;
				};
			}
			else
			{
				TryParseDate = (string dateValue, out DateTime result, out string displayDate) =>
				{
					if (DateTime.TryParse(dateValue, out result))
					{
						displayDate = getDisplayDate(result);
						return true;
					}

					displayDate = string.Empty;
					return false;
				};
			}

			_transactionValues = new Dictionary<string, Regex>();
			IsMatchByTransactions = (string input, out string output) =>
			{
				output = null;

				List<Regex> trnList;
				lock(_syncTrn)
					trnList = _transactionValues.Values.ToList();

				foreach (var regex in trnList.Skip(Math.Max(0, RowsLimit == 0 ? trnList.Count : trnList.Count - RowsLimit / 2)))
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
			TryParseDate = control.TryParseDate;
			_syncTrn = control._syncTrn;
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
			lock (_syncTrn)
				if (_transactionValues.ContainsKey(trn))
					return false;

			var regex = new Regex(Regex.Escape(trn),
				RegexOptions.Compiled | RegexOptions.CultureInvariant,
				new TimeSpan(0, 0, 1));

			lock (_syncTrn)
				_transactionValues.Add(trn, regex);

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

		public virtual void Clear()
		{
			lock (_syncTrn)
				_transactionValues.Clear();
		}

		public virtual void Dispose()
		{
			_transactionValues.Clear();
			IsDisposed = true;
		}
	}
}
