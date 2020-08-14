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

	public interface IReaderPerformer
	{
		LRSettingsScheme Settings { get; }

		bool IsStopPending { get; }

		bool HasOutOfMemoryException { get; }
	}

	internal class ReaderPerformer : IReaderPerformer
	{
		public LogsReaderPerformerBase Base { get; set; }

		/// <summary>
		/// Основные настройки
		/// </summary>
		public LRSettingsScheme Settings { get; set; }

		/// <summary>
		/// Запрос на ожидание остановки выполнения поиска
		/// </summary>
		public bool IsStopPending { get; set; }

		/// <summary>
		/// Если возникла ошибка переолнения памяти
		/// </summary>
		public bool HasOutOfMemoryException { get; set; } = false;
	}

	public abstract class LogsReaderPerformerBase : IReaderPerformer, IDisposable
	{
		private static long _seqBaseInstance = 0;
		private readonly long _instanceId = 0;
		private readonly object _syncTrn = new object();
		private readonly Dictionary<string, Regex> _transactionValues;

		private ReaderPerformer Parent { get; }

		/// <summary>
		/// Проверяет на совпадение по найденным транзакциям
		/// </summary>
		internal OutFuncDelegate<string, string, bool> IsMatchByTransactions { get; }

		/// <summary>
		/// Проверяет на совпадение по обычному поиску
		/// </summary>
		protected Func<string, bool> IsMatch { get; private set; }

		/// <summary>
		/// Функция для получение определенного <see cref="TraceReader"/> согласно настройкам
		/// </summary>
		protected Func<(string server, string filePath, string originalFolder), TraceReader> GetTraceReader { get; }

		public LRSettingsScheme Settings => Parent.Settings;

		public bool IsStopPending
		{
			get => Parent.IsStopPending;
			protected set => Parent.IsStopPending = value;
		}

		public bool HasOutOfMemoryException
		{
			get => Parent.HasOutOfMemoryException;
			protected set => Parent.HasOutOfMemoryException = value;
		}

		[DGVColumn(ColumnPosition.After, "SchemeName")]
		public string SchemeName => Settings.Name;

		public Encoding Encoding => Settings.Encoding;

		public int MaxTraceLines => Settings.MaxLines;

		public int MaxThreads => Settings.MaxThreads;

		public int RowsLimit => Settings.RowsLimit;

		public (Func<string, TraceParseResult>, Func<string, bool>)? TraceParseCustomFunction { get; }

		public LRTraceParsePatternItem[] TraceParsePatterns => Settings.TraceParse.Patterns;

		public LRTraceParseTransactionItem[] TransactionPatterns => Settings.TraceParse.TransactionPatterns;

		public DoubleOutFuncDelegate<string, DateTime, string, bool> TryParseDate { get; }

		public Regex StartTraceLineWith => Settings.TraceParse.StartTraceLineWith;

		public Regex EndTraceLineWith => Settings.TraceParse.EndTraceLineWith;

		public Regex IsTraceError => Settings.TraceParse.IsTraceError;

		protected LogsReaderPerformerBase(LRSettingsScheme settings, string findMessage, bool useRegex)
		{
			Parent = new ReaderPerformer
			{
				Base = this,
				Settings = settings
			};

			_instanceId = ++_seqBaseInstance;
			
			ResetMatchFunc(findMessage, useRegex);

			Func<DateTime, string> getDisplayDate;
			if (Settings.TraceParse.DisplayCulture != null)
				getDisplayDate = (date) => date.ToString(Settings.TraceParse.DisplayDateFormat, Settings.TraceParse.DisplayCulture);
			else
				getDisplayDate = (date) => date.ToString(Settings.TraceParse.DisplayDateFormat);

			if (Settings.CultureList.Count > 0)
			{
				TryParseDate = (string dateValue, out DateTime result, out string displayDate) =>
				{
					foreach (var cultureInfo in Settings.CultureList)
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
				lock (_syncTrn)
					trnList = _transactionValues.Values.ToList();

				var trnsLimit = trnList.Skip(Math.Max(0, RowsLimit == 0 ? trnList.Count : trnList.Count - RowsLimit / 2));
				foreach (var regex in trnsLimit)
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

			if (Settings.TraceParse.StartTraceLineWith != null && Settings.TraceParse.EndTraceLineWith != null)
				GetTraceReader = (data) => new TraceReaderStartWithEndWith(this, data.server, data.filePath, data.originalFolder);
			else if (Settings.TraceParse.StartTraceLineWith != null)
				GetTraceReader = (data) => new TraceReaderStartWith(this, data.server, data.filePath, data.originalFolder);
			else if (Settings.TraceParse.EndTraceLineWith != null)
				GetTraceReader = (data) => new TraceReaderEndWith(this, data.server, data.filePath, data.originalFolder);
			else
				GetTraceReader = (data) => new TraceReaderSimple(this, data.server, data.filePath, data.originalFolder);

			if (Settings.TraceParse.IsCorrectCustomFunction)
				TraceParseCustomFunction = Settings.TraceParse.GetCustomFunction();
		}

		protected LogsReaderPerformerBase(LogsReaderPerformerBase control)
		{
			Parent = control.Parent;
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
			try
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
			catch (Exception)
			{
				return false; // есть вероятность что тот же элемен может добавится меду двумя локами (для разных файлов), а два раза делать ContainsKey увеличит время выполнения поиска
			}
		}

		public abstract void Pause();

		public abstract void Resume();

		public abstract void Abort();

		/// <summary>
		/// Сравниваются только базовые настройки и инстанс. Игнорируется изменения настроек поиска свойства IsMatch.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			var isEqual = false;
			if (obj is LogsReaderPerformerBase input)
				isEqual = Settings == input.Settings && _instanceId == input._instanceId;
			return isEqual;
		}

		/// <summary>
		/// в качестве хэша должен быть только хэш базовых настроек
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			var hash = Settings.GetHashCode() + 13;
			return hash;
		}

		public virtual void Clear()
		{
			lock (_syncTrn)
				_transactionValues.Clear();
		}

		public abstract void Dispose();
	}
}
