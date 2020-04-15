using System;
using System.Text;
using System.Text.RegularExpressions;
using Utils;
using Utils.WinForm.DataGridViewHelper;

namespace LogsReader.Reader
{
    public class DataTemplate
    {
        private readonly StringBuilder _traceMessage = new StringBuilder();
        private string _date = null;
        private string _description = null;
        private string _traceName = null;

        internal DataTemplate(TraceReader traceReader, long foundLineID, string strID, string date, string traceName, string description, string message, string traceMessage)
        {
            IsMatched = true;
            FoundLineID = foundLineID;
            ParentReader = traceReader;

            ID = int.TryParse(strID, out var id) ? id : -1;

            if (DateTime.TryParse(date.Replace(",", "."), out var dateOfTrace))
            {
                Date = dateOfTrace;
                DateOfTrace = Date.Value.ToString("dd.MM.yyyy HH:mm:ss.fff");
            }
            else
            {
                DateOfTrace = date;
            }

            TraceName = traceName;
            Description = description;
            Message = message;
            TraceMessage = traceMessage;
        }

        internal DataTemplate(TraceReader traceReader, long foundLineID, string traceMessage)
        {
            IsMatched = false;
            FoundLineID = foundLineID;
            ParentReader = traceReader;

            ID = -1;
            TraceMessage = traceMessage;
        }

        internal DataTemplate(TraceReader traceReader, long foundLineID, string traceMessage, Exception error)
        {
            IsMatched = false;
            FoundLineID = foundLineID;
            ParentReader = traceReader;

            ID = -1;
            Date = DateTime.Now;
            DateOfTrace = Date.Value.ToString("dd.MM.yyyy HH:mm:ss.fff");

            Error = error;
            TraceName = error.GetType().ToString();
            Description = error.Message;
            if (error is RegexMatchTimeoutException errorRegex)
            {
                Message = $"Timeout while executing pattern: \"{errorRegex.Pattern}\".\r\nTimeout: {errorRegex.MatchTimeout.ToReadableString()}\r\nInput:\r\n{errorRegex.Input}";
            }
            else
            {
                Message = error.ToString();
            }
            TraceMessage = traceMessage;
        }

        public TraceReader ParentReader { get; }

        public long FoundLineID { get; }

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
            private set => _traceName = value?.Replace(Environment.NewLine, string.Empty).Trim();
        }

        [DGVColumn(ColumnPosition.After, "Date")]
        public string DateOfTrace
        {
            get => _date;
            private set => _date = value?.Replace(Environment.NewLine, string.Empty).Trim();
        }

        public DateTime? Date { get; }

        [DGVColumn(ColumnPosition.After, "File")]
        public string File => ParentReader.FileNamePartial;

        public string Description
        {
            get => $"FoundLineID:{FoundLineID}\r\n{_description}".Trim();
            private set => _description = value;
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

        public override string ToString()
        {
            return $"{File} | {TraceName}";
        }
    }
}
