using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Utils;
using Utils.WinForm.DataGridViewHelper;

namespace LogsReader.Reader
{
	public enum TraceReaderSearchType
	{
		ByCustomFunctions = 0,
		ByRegexPatterns = 1,
		ByBoth = 3
	}

	public delegate void FoundDataTemplate(DataTemplate item);

	public abstract class TraceReader : LogsReaderPerformerBase
	{
		private bool _searchByTransaction;

		private Func<string, bool> _IsMatchedFunc;

		/// <summary>
		///     Сохраненные данный которые не спарсились по основному поиску и по транзакциям
		/// </summary>
		protected Queue<string> PastTraceLines { get; set; }

		/// <summary>
		///     Прошлый успешный результат, который возможно будет дополняться
		/// </summary>
		protected DataTemplate Found { get; set; }

		/// <summary>
		///     Текущая транзакция, используется для DataTempalte
		/// </summary>
		protected TransactionValue CurrentTransactionValue { get; private set; }

		/// <summary>
		///     Разрешить поиск по транзакциям
		/// </summary>
		protected bool SearchByTransaction
		{
			get => _searchByTransaction;
			set
			{
				_searchByTransaction = value;

				if (_searchByTransaction)
				{
					_IsMatchedFunc = input =>
					{
						CurrentTransactionValue = null;
						var result = IsMatch.Invoke(input);

						if (IsMatchByTransactions.Invoke(input, out var _trnValue))
						{
							CurrentTransactionValue = new TransactionValue(!result, _trnValue);
							result = true;
						}

						return result;
					};
				}
				else
				{
					CurrentTransactionValue = null;
					_IsMatchedFunc = input => IsMatch.Invoke(input);
				}
			}
		}

		/// <summary>
		///     Генерирует событие при найденном трейсе
		/// </summary>
		public event FoundDataTemplate OnFound;

		[DGVColumn(ColumnPosition.After, "ID")]
		public int ID { get; internal set; }

		[DGVColumn(ColumnPosition.After, "PrivateID", false)]
		public int PrivateID { get; internal set; }

		public TraceReaderStatus Status { get; private set; } = TraceReaderStatus.Waiting;

		[DGVColumn(ColumnPosition.After, "ThreadID")]
		public string ThreadId { get; private set; } = string.Empty;

		/// <summary>
		///     Количество совпадений по критериям поиска
		/// </summary>
		[DGVColumn(ColumnPosition.After, "Matches")]
		public int CountMatches { get; private set; }

		/// <summary>
		///     Количество ошибочных трейсов
		/// </summary>
		[DGVColumn(ColumnPosition.After, "Errors")]
		public int CountErrors { get; private set; }

		public string Server { get; }

		/// <summary>
		///     Отсекается часть пути из первичных настроек (OriginalFolder) от основного пути к файлу (FilePath)
		///     Пример - отсекается "\\LOCALHOST\C$\TEST" от "\\LOCALHOST\C$\TEST\soapcon.log" - получается soapcon.log
		/// </summary>
		public string FileNamePartial { get; }

		/// <summary>
		///     Содержит в себе полный путь к файлу, включая сервер "\\LOCALHOST\C$\TEST\soapcon.log"
		/// </summary>
		[DGVColumn(ColumnPosition.After, "File")]
		public string FilePath { get; }

		[DGVColumn(ColumnPosition.After, "CreationTime")]
		public DateTime CreationTime => File.CreationTime;

		[DGVColumn(ColumnPosition.After, "LastWriteTime")]
		public DateTime LastWriteTime => File.LastWriteTime;

		[DGVColumn(ColumnPosition.After, "Size")]
		public double Size => Math.Round(((double)File.Length) / 1048576, 3, MidpointRounding.AwayFromZero);

		/// <summary>
		///     Содержит в себе исходную настройку пути к директории "C:\TEST"
		/// </summary>
		public string OriginalFolder { get; }

		public FileInfo File { get; }

		[DGVColumn(ColumnPosition.After, "Priority")]
		public int Priority { get; internal set; }

		public long Lines { get; protected set; }

		protected void AddLine(string input)
		{
			PastTraceLines.Enqueue(input);
			if (PastTraceLines.Count > MaxTraceLines)
				PastTraceLines.Dequeue();
			Lines++;
		}

		public TraceReaderSearchType SearchType { get; }

		protected TraceReader(LogsReaderPerformerBase control, string server, string filePath, string originalFolder)
			: base(control)
		{
			PastTraceLines = new Queue<string>(MaxTraceLines);
			Server = server;
			FilePath = filePath;
			File = new FileInfo(filePath);
			OriginalFolder = originalFolder;
			FileNamePartial = IO.GetPartialPath(FilePath, OriginalFolder);
			
			if (TraceParsePatterns != null && TraceParsePatterns.Length > 0 && TraceParseCustomFunction != null)
				SearchType = TraceReaderSearchType.ByBoth;
			else if (TraceParsePatterns != null && TraceParsePatterns.Length > 0)
				SearchType = TraceReaderSearchType.ByRegexPatterns;
			else if (TraceParseCustomFunction != null)
				SearchType = TraceReaderSearchType.ByCustomFunctions;
			
			SearchByTransaction = TransactionPatterns != null && TransactionPatterns.Length > 0;
		}

		public void Start()
		{
			try
			{
				ThreadId = Thread.CurrentThread.ManagedThreadId.ToString();

				bool IsStopStatus()
				{
					if (HasOutOfMemoryException)
					{
						Status = TraceReaderStatus.Failed;
						return true;
					}

					if (IsStopPending || Status == TraceReaderStatus.Aborted)
					{
						Status = TraceReaderStatus.Aborted;
						return true;
					}

					return false;
				}

				if (IsStopStatus())
					return;

				if (!File.Exists)
				{
					Status = TraceReaderStatus.Finished;
					return;
				}

				// FileShare должен быть ReadWrite. Иначе, если файл используется другим процессом то доступ к чтению файла будет запрещен.
				using (var inputStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					if (IsStopStatus())
						return;

					using (var streamReader = new StreamReader(inputStream, Encoding))
					{
						if (Status != TraceReaderStatus.OnPause)
							Status = TraceReaderStatus.Processing;
						string line;

						while ((line = streamReader.ReadLine()) != null)
						{
							if (Status == TraceReaderStatus.OnPause)
								while (Status == TraceReaderStatus.OnPause && !IsStopPending && !HasOutOfMemoryException)
									Thread.Sleep(50);
							if (IsStopStatus())
								return;

							ReadLine(line);
						}
					}
				}

				Status = TraceReaderStatus.Finished;
			}
			catch (OutOfMemoryException)
			{
				Status = TraceReaderStatus.Failed;
				HasOutOfMemoryException = true;
			}
			catch (Exception)
			{
				Status = TraceReaderStatus.Failed;
				throw;
			}
			finally
			{
				try
				{
					if (HasOutOfMemoryException)
					{
						Clear();
					}
					else
					{
						Commit();
						Clear();
					}
				}
				catch
				{
					// ignored
				}
			}
		}

		public override void Pause()
		{
			if (Status == TraceReaderStatus.Waiting || Status == TraceReaderStatus.Processing)
				Status = TraceReaderStatus.OnPause;
		}

		public override void Resume()
		{
			if (Status == TraceReaderStatus.OnPause)
				Status = ThreadId.IsNullOrEmpty() ? TraceReaderStatus.Waiting : TraceReaderStatus.Processing;
		}

		public override void Abort()
		{
			if (Status == TraceReaderStatus.Waiting || Status == TraceReaderStatus.Processing || Status == TraceReaderStatus.OnPause)
				Status = TraceReaderStatus.Aborted;
		}

		public abstract void ReadLine(string line);

		protected bool IsMatched(string input) => _IsMatchedFunc.Invoke(input);

		public void Commit()
		{
			if (Found == null)
				return;

			try
			{
				if (!Found.IsMatched)
				{
					if (IsTraceMatch(Found.TraceMessage, out var result, Found, true))
					{
						AddResult(result);
					}
					else
					{
						TransactionsSearch(Found.TraceMessage, Found);
						AddResult(Found);
					}
				}
			}
			catch (Exception ex)
			{
				if (!Found.IsMatched)
				{
					Found = new DataTemplate(this, Found.FoundLineID, Found.TraceMessage, CurrentTransactionValue, ex);
					TransactionsSearch(Found.TraceMessage, Found);
					AddResult(Found);
				}

				throw;
			}
			finally
			{
				Found = null;
			}
		}

		protected void AddResult(DataTemplate item)
		{
			if (Filter != null && !Filter.IsAllowed(item))
				return;

			if (item.Error == null)
				++CountMatches;
			if (!item.IsSuccess)
				++CountErrors;
			OnFound?.Invoke(item);
		}

		protected bool IsTraceMatch(string input, out DataTemplate result, DataTemplate failed = null, bool throwException = false)
		{
			// замена '\r' чинит баг с некорректным парсингом
			var traceMessage = input.Replace("\r", string.Empty);
			var length = traceMessage.Split('\n').Length;
			var foundLineId = Lines - length + 1;

			switch (SearchType)
			{
				case TraceReaderSearchType.ByRegexPatterns:
					if (IsTraceMatchByRegexPatterns(traceMessage, foundLineId, out result, throwException))
						return true;
					break;

				case TraceReaderSearchType.ByCustomFunctions:
					if (IsTraceMatchByCustomFunction(traceMessage, foundLineId, out result))
						return true;
					break;

				case TraceReaderSearchType.ByBoth:
					if (IsTraceMatchByRegexPatterns(traceMessage, foundLineId, out result, throwException))
						return true;
					if (IsTraceMatchByCustomFunction(traceMessage, foundLineId, out result))
						return true;
					break;

				default:
					throw new NotSupportedException();
			}

			result = new DataTemplate(this, foundLineId, traceMessage, CurrentTransactionValue);
			return false;
		}

		private bool IsTraceMatchByCustomFunction(string traceMessage, long foundLineId, out DataTemplate result)
		{
			var traceParceResult = TraceParseCustomFunction.Value.Item1.Invoke(traceMessage);

			if (traceParceResult != null)
			{
				var current = new DataTemplate(this, foundLineId, traceParceResult, traceMessage, CurrentTransactionValue);
				TransactionsSearch(traceMessage, current);
				result = current;
				return true;
			}

			result = null;
			return false;
		}

		private bool IsTraceMatchByRegexPatterns(string traceMessage, long foundLineId, out DataTemplate result, bool throwException)
		{
			foreach (var traceParsePattern in TraceParsePatterns)
			{
				Match match;

				try
				{
					match = traceParsePattern.RegexItem.Match(traceMessage);
				}
				catch (Exception ex)
				{
					if (throwException)
						throw;

					result = new DataTemplate(this, foundLineId, traceMessage, CurrentTransactionValue, ex);
					return false;
				}

				if (!match.Success || match.Value.Length != traceMessage.Length)
					continue;

				var current = new DataTemplate(this, foundLineId, traceParsePattern.GetParsingResult(match), traceMessage, CurrentTransactionValue);
				TransactionsSearch(traceMessage, current);
				result = current;
				return true;
			}

			result = null;
			return false;
		}

		/// <summary>
		///     поиск по транзакциям
		/// </summary>
		/// <param name="traceMessage">Успешно спарсенный трейс</param>
		/// <param name="current">Успешно созданный темплейт</param>
		private void TransactionsSearch(string traceMessage, DataTemplate current)
		{
			if (!SearchByTransaction || TransactionPatterns == null || TransactionPatterns.Length <= 0)
				return;

			try
			{
				foreach (var transactionParsePattern in TransactionPatterns)
				{
					// пытаемся из результата найти транзакцию
					var trnMatch = transactionParsePattern.RegexItem.Match(traceMessage);
					if (!trnMatch.Success)
						continue;

					// Текущая транзакция. Подставляется из группировок regex replace mode
					var trnValue = transactionParsePattern.GetParsingResult(trnMatch).Trn;

					// ищем дальше, если результатам replace mode значение пустое
					if (trnValue.IsNullOrWhiteSpace())
						continue;

					// указываем спарсенную транзакцию
					current.AddTransaction(new TransactionValue(false, trnValue));

					// добавляем новую транзакцию в общую коллекцию спарсенных транзакций
					if (!AddTransactionValue(trnValue))
						break; // если транзакция найдена,, или транзакция уже была в списках, то завершаем поиск транзакций

					// если сохранились предыдущие строки, то ищем текущую транзакцию в предыдущих строках
					if (PastTraceLines.Count > 0)
					{
						// создаем внутренний ридер, для считывания предыдущих записей для поиска текущей транзакции
						var innerReader = GetTraceReader((Server, FilePath, OriginalFolder));
						innerReader.SearchByTransaction = false; // отменить повторную внутреннюю проверку по транзакциям предыдущих записей
						innerReader.CurrentTransactionValue = new TransactionValue(true, trnValue);
						innerReader.Lines = Lines - PastTraceLines.Count - current.CountOfLines - 1; // возвращаемся обратно к первой сохраненной строке
						innerReader.ResetMatchFunc(Regex.Escape(trnValue), true);

						void OnPastFound(DataTemplate pastItem)
						{
							if (pastItem.IsMatched && !pastItem.Equals(current))
							{
								AddResult(pastItem);
							}
						}

						innerReader.OnFound += OnPastFound;

						foreach (var pastLine in PastTraceLines)
						{
							innerReader.ReadLine(pastLine);
						}

						try
						{
							innerReader.Commit();
						}
						catch (Exception)
						{
							// ignored
						}

						innerReader.OnFound -= OnPastFound;

						// очищаем предыдущие данные, т.к. трейс успешно был спарсен в текущем контексте
						// и также в дочернем вызове был произведен поиск по транзакциям
						PastTraceLines.Clear();
					}

					break;
				}
			}
			catch (Exception)
			{
				// ignored
			}
		}

		protected bool IsLineMatch(string input)
		{
			// замена '\r' чинит баг с некорректным парсингом
			var traceMessage = input.Replace("\r", string.Empty);

			switch (SearchType)
			{
				case TraceReaderSearchType.ByRegexPatterns:
					return IsLineMatchByRegexPatterns(traceMessage);

				case TraceReaderSearchType.ByCustomFunctions:
					return IsLineMatchByCustomFunction(traceMessage);

				case TraceReaderSearchType.ByBoth:
					if (IsLineMatchByRegexPatterns(traceMessage))
						return true;
					if (IsLineMatchByCustomFunction(traceMessage))
						return true;
					break;

				default:
					throw new NotSupportedException();
			}

			return false;
		}

		protected bool IsLineMatchByCustomFunction(string traceMessage) => TraceParseCustomFunction.Value.Item2.Invoke(traceMessage);

		protected bool IsLineMatchByRegexPatterns(string traceMessage)
		{
			foreach (var traceParsePattern in TraceParsePatterns.Select(x => x.RegexItem))
			{
				Match match;

				try
				{
					match = traceParsePattern.Match(traceMessage);
				}
				catch (Exception)
				{
					return false;
				}

				if (match.Success && match.Value.Length == traceMessage.Length)
					return true;
			}

			return false;
		}

		/// <summary>
		///     Останавливаем и очищаем
		/// </summary>
		public override void Clear()
		{
			Found = null;
			Abort();
			PastTraceLines.Clear();
		}

		public override bool Equals(object obj)
		{
			var isEqual = false;
			if (obj is TraceReader input)
				isEqual = FilePath == input.FilePath && base.Equals(input);
			return isEqual;
		}

		/// <summary>
		///     хэш только полного пути к файлу и базовый хэш
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			unchecked
			{
				var hash = 30;
				hash = hash * 14 + FilePath.GetHashCode();
				hash = hash * 14 + base.GetHashCode();
				return hash;
			}
		}

		public override string ToString() => FilePath;

		public override void Dispose() => Clear();
	}
}