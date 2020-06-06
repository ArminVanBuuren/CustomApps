using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using LogsReader.Properties;
using Utils;
using Utils.WinForm.DataGridViewHelper;
using static LogsReader.Config.LRTraceParsePatternItem;

namespace LogsReader.Reader
{
    public class DataTemplate
    {
	    private readonly StringBuilder _traceMessage = new StringBuilder();
        private string _description;
        private string _traceName;

        internal DataTemplate(
	        TraceReader traceReader, 
	        long foundLineID,
	        TraceParsePatternResult parseResult, 
	        string traceMessage,
	        string trn)
        {
            IsMatched = true;
            FoundLineID = foundLineID;
            ParentReader = traceReader;

            ID = int.TryParse(parseResult.ID, out var id) ? id : -1;

            var dateValue = parseResult.Date ?? string.Empty;
            if (TIME.TryParseAnyDate(dateValue.Replace(",", "."), DateTimeStyles.AllowWhiteSpaces, out var dateOfTrace))
            {
                Date = dateOfTrace;
                DateOfTrace = Date.Value.ToString(traceReader.DisplayDateFormat);
            }
            else
            {
                DateOfTrace = dateValue;
            }

            TraceName = parseResult.TraceName;
            Description = parseResult.Description;
            Message = parseResult.Message;
            TraceMessage = traceMessage;
            TransactionValue = trn;
        }

        internal DataTemplate(TraceReader traceReader, long foundLineID, string traceMessage, string trn)
        {
            IsMatched = false;
            FoundLineID = foundLineID;
            ParentReader = traceReader;

            ID = -1;
            TraceMessage = traceMessage;
            TransactionValue = trn;
        }

        internal DataTemplate(TraceReader traceReader, long foundLineID, string traceMessage, string trn, Exception error)
        {
            IsMatched = false;
            FoundLineID = foundLineID;
            ParentReader = traceReader;

            ID = -1;
            Date = DateTime.Now;
            DateOfTrace = Date.Value.ToString(traceReader.DisplayDateFormat);

            Error = error;
            TraceName = error.GetType().ToString();
            Description = error.Message;
            if (error is RegexMatchTimeoutException errorRegex)
            {
                Message = string.Format(Resources.Txt_DataTemplate_ErrTimeout, errorRegex.Pattern, errorRegex.MatchTimeout.ToReadableString(), errorRegex.Input);
            }
            else
            {
                Message = error.ToString();
            }
            TraceMessage = traceMessage;
            TransactionValue = trn;
        }

        public TraceReader ParentReader { get; }

        public long FoundLineID { get; }

        public string TransactionValue { get; }

        public Exception Error { get; }

        [DGVColumn(ColumnPosition.Last, "PrivateID", false)]
        public int PrivateID { get; internal set; }

        [DGVColumn(ColumnPosition.Last, "IsMatched", false)]
        public bool IsMatched { get; private set; }

        [DGVColumn(ColumnPosition.After, "ID")]
        public int ID { get; internal set; }

        [DGVColumn(ColumnPosition.After, "Server")]
        public string Server => ParentReader.Server;

        [DGVColumn(ColumnPosition.After, "TraceName")]
        public string TraceName
        {
            get => _traceName;
            private set => _traceName = value?.Replace("\r", string.Empty).Replace("\n", string.Empty).TrimWhiteSpaces() ?? string.Empty;
        }

        [DGVColumn(ColumnPosition.After, "Date")]
        public string DateOfTrace { get; private set; }

        public DateTime? Date { get; }

        [DGVColumn(ColumnPosition.After, "File")]
        public string File => ParentReader.FileNamePartial;

        public string Description
        {
            get => _description;
            private set => _description = value.Trim();
        }

        public string Message { get; private set; } = string.Empty;

        public string TraceMessage
        {
            get => _traceMessage.ToString();
            private set
            {
                _traceMessage.Clear();
                _traceMessage.Append(value);
                CountOfLines = value.Split('\n').Length;
            }
        }

        public int CountOfLines { get; private set; } = 1;

        internal void AppendPastLine(string line)
        {
            _traceMessage.Insert(0, line + Environment.NewLine);
            CountOfLines++;
        }

        internal void AppendNextLine(string line)
        {
            _traceMessage.Append(Environment.NewLine + line);
            CountOfLines++;
        }

        internal void MergeDataTemplates(DataTemplate input)
        {
            Message = input.Message;
            TraceMessage = input.TraceMessage;
        }

        public override bool Equals(object obj)
        {
	        var isEqual = false;
	        if (obj is DataTemplate input)
		        isEqual = FoundLineID == input.FoundLineID && ParentReader.Equals(input.ParentReader);
	        return isEqual;
        }

        public override int GetHashCode()
        {
	        // найденная позиция в файле и хэш ридера
            var hash = FoundLineID.GetHashCode() + ParentReader.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            return $"{ParentReader} | {nameof(FoundLineID)} = {FoundLineID}";
        }
    }
}
