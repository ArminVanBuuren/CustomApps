using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using LogsReader.Config;
using LogsReader.Properties;
using Utils;
using Utils.WinForm.DataGridViewHelper;

namespace LogsReader.Reader
{
	public class TransactionValue
	{
		public TransactionValue(bool foundByTrn, string trn)
		{
			FoundByTrn = foundByTrn;
			Trn = trn;
		}

		public bool FoundByTrn { get; }
		public string Trn { get; }
	}

	public class DataTemplate : ICloneable
	{
		public const string ReaderPriority = "Priority";
		public const string HeaderPrompt = "#";
		public const string HeaderDescription = "Description";
		public const string HeaderMessage = "Message";
		public const string HeaderTraceMessage = "Full Trace";

		private static DataTemplate _tmp;

		/// <summary>
		///     Темповый темплейт, во избежание ошибок. Если будут изменяться названия полей в <see cref="DataTemplate" /> то, они
		///     будут изменяться везде.
		/// </summary>
		internal static DataTemplate Tmp => _tmp ?? (_tmp = new DataTemplate());

		private readonly StringBuilder _traceMessage = new StringBuilder();
		private string _description;
		private string _traceName;
		private string _message = string.Empty;

		private readonly HashSet<DataTemplate> _trnBindings = new HashSet<DataTemplate>();

		private DataTemplate() => IsSuccess = true;

		internal DataTemplate(TraceReader traceReader, long foundLineID, TraceParseResult parseResult, string traceMessage, TransactionValue trn)
		{
			IsMatched = true;

			// проверка если нужно показать что трейс ошибочный
			if (parseResult.IsSuccess && traceReader.IsTraceError != null)
				IsSuccess = !traceReader.IsTraceError.IsMatch(traceMessage);
			else
				IsSuccess = parseResult.IsSuccess;
			FoundLineID = foundLineID;
			ParentReader = traceReader;
			ID = int.TryParse(parseResult.ID, out var id) ? id : -1;

			if (!parseResult.Date.IsNullOrWhiteSpace()
			 && traceReader.TryParseDate(parseResult.Date.Replace(",", "."), out var dateOfTrace, out var displayDate))
			{
				Date = dateOfTrace;
				DateString = displayDate;
			}
			else
			{
				DateString = string.Empty;
			}

			TraceName = parseResult.TraceName;
			Description = parseResult.Description;
			Message = parseResult.Message;
			TraceMessage = traceMessage;
			AddTransaction(trn);
		}

		internal DataTemplate(TraceReader traceReader, long foundLineID, string traceMessage, TransactionValue trn)
			: this(traceReader, foundLineID, trn)
			=> TraceMessage = traceMessage;

		internal DataTemplate(TraceReader traceReader, long foundLineID, TransactionValue trn)
		{
			IsMatched = false;
			IsSuccess = false;
			FoundLineID = foundLineID;
			ParentReader = traceReader;
			ID = -1;
			CountOfLines = 0;
			AddTransaction(trn);
		}

		internal DataTemplate(TraceReader traceReader, long foundLineID, string traceMessage, TransactionValue trn, Exception error)
		{
			IsMatched = false;
			IsSuccess = false;

			FoundLineID = foundLineID;
			ParentReader = traceReader;

			ID = -1;
			Date = DateTime.Now;
			DateString = string.Empty;

			Error = error;
			TraceName = error.GetType().ToString();
			Description = error.Message;

			if (error is RegexMatchTimeoutException errorRegex)
			{
				Message = string.Format(Resources.Txt_DataTemplate_ErrTimeout,
				                        errorRegex.Pattern,
				                        errorRegex.MatchTimeout.ToReadableString(),
				                        errorRegex.Input);
			}
			else
			{
				Message = error.ToString();
			}

			TraceMessage = traceMessage;
			AddTransaction(trn);
		}

		public TraceReader ParentReader { get; }

		public long FoundLineID { get; }

		public Dictionary<string, TransactionValue> Transactions { get; } = new Dictionary<string, TransactionValue>();

		public IEnumerable<DataTemplate> TransactionBindings => _trnBindings;

		public bool IsSelected { get; internal set; } = false;

		public Exception Error { get; }

		public bool IsMatched { get; }

		[DGVColumn(ColumnPosition.First, HeaderPrompt, false)]
		public object Prompt => null;

		[DGVColumn(ColumnPosition.After, nameof(Tmp.ID))]
		public int ID { get; internal set; }

		[DGVColumn(ColumnPosition.After, nameof(Tmp.Server))]
		public string Server => ParentReader.Server;

		[DGVColumn(ColumnPosition.After, nameof(Tmp.TraceName))]
		public string TraceName
		{
			get => _traceName;
			private set => _traceName = value?.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim() ?? string.Empty;
		}

		[DGVColumn(ColumnPosition.After, nameof(Tmp.Date))]
		public string DateString { get; }

		public DateTime? Date { get; }

		[DGVColumn(ColumnPosition.After, nameof(Tmp.ElapsedSec))]
		public string ElapsedSecString => ElapsedSec > 0 ? ElapsedSec.ToString("0.000") : ElapsedSec == 0 ? "0" : string.Empty;

		public string ElapsedSecDescription
			=> ElapsedSecTotal > 0
				? $"Elapsed current=[{ElapsedSec:0.###} sec], processing=[{ElapsedSecFromFirst:0.###} sec], total=[{ElapsedSecTotal:0.###} sec]"
				: ElapsedSecTotal == 0
					? "Elapsed 0 sec"
					: string.Empty;

		public double ElapsedSec { get; internal set; } = -1;

		public double ElapsedSecFromFirst { get; internal set; } = -1;

		public double ElapsedSecTotal { get; internal set; } = -1;

		[DGVColumn(ColumnPosition.Last, nameof(Tmp.SchemeName), false)]
		public string SchemeName => ParentReader.SchemeName;

		[DGVColumn(ColumnPosition.Last, nameof(Tmp.PrivateID), false)]
		public int PrivateID { get; internal set; }

		[DGVColumn(ColumnPosition.Last, nameof(Tmp.IsSuccess), false)]
		public bool IsSuccess { get; }

		/// <summary>
		///     Свойство всегда false, он меняется только в DataGridView
		/// </summary>
		[DGVColumn(ColumnPosition.Last, nameof(Tmp.IsFiltered), false)]
		public bool IsFiltered => false;

		[DGVColumn(ColumnPosition.Last, nameof(Tmp.File))]
		public string FileNamePartial => ParentReader.FileNamePartial;

		public string File => ParentReader.FilePath;

		public string Description
		{
			get => _description;
			private set => _description = value.Trim().Replace("\0", "");
		}

		public string Message
		{
			get => _message;
			private set => _message = value.Replace("\0", "");
		}

		public string TraceMessage
		{
			get => _traceMessage.ToString();
			private set
			{
				_traceMessage.Clear();
				_traceMessage.Append(value.Replace("\0", ""));
				CountOfLines = value.Split('\n').Length;
			}
		}

		public int CountOfLines { get; private set; } = 1;

		internal void AppendPastLine(string line)
		{
			if (CountOfLines == 0)
				_traceMessage.Insert(0, line.Replace("\0", ""));
			else
				_traceMessage.Insert(0, line.Replace("\0", "") + Environment.NewLine);
			CountOfLines++;
		}

		internal void AppendNextLine(string line)
		{
			_traceMessage.Append(Environment.NewLine + line.Replace("\0", ""));
			CountOfLines++;
		}

		internal void MergeDataTemplates(DataTemplate input)
		{
			Message = input.Message;
			TraceMessage = input.TraceMessage;
		}

		internal void AddTransaction(TransactionValue trnValue)
		{
			if (trnValue == null)
				return;

			if (Transactions.TryGetValue(trnValue.Trn, out var exist))
			{
				if (trnValue.FoundByTrn && !exist.FoundByTrn)
				{
					Transactions.Remove(exist.Trn);
					Transactions.Add(trnValue.Trn, trnValue);
				}
			}
			else
			{
				Transactions.Add(trnValue.Trn, trnValue);
			}
		}

		internal void AddTransactionBindingList(IList<DataTemplate> list) => _trnBindings.AddRange(list);

		public override bool Equals(object obj)
		{
			var isEqual = false;
			if (obj is DataTemplate input)
				isEqual = FoundLineID == input.FoundLineID && Date == input.Date && ParentReader.Equals(input.ParentReader);
			return isEqual;
		}

		/// <summary>
		///     хэш найденной позиции в файле и хэш ридера
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			unchecked
			{
				var hash = 67;
				hash = hash * 7 + FoundLineID.GetHashCode();
				hash = hash * 7 + ParentReader.GetHashCode();
				return hash;
			}
		}

		public object Clone() => MemberwiseClone();

		public override string ToString() => ParentReader.ToString();
	}
}