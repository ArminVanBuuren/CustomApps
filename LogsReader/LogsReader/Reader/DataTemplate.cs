using System;
using System.Text;
using System.Text.RegularExpressions;
using Utils;
using Utils.WinForm.DataGridViewHelper;

namespace LogsReader.Reader
{
    public class DataTemplate
    {
        private readonly TraceReader _fileLog;
        private readonly StringBuilder _traceMessage = new StringBuilder();
        private string _date = null;
        private string _traceName = null;
        private string _message = string.Empty;

        internal DataTemplate(TraceReader fileLog, int foundLineID, string strID, string date, string traceName, string description, string message, string traceMessage)
        {
            FoundLineID = foundLineID;
            IsMatched = true;
            _fileLog = fileLog;

            ID = int.TryParse(strID, out var id) ? id : -1;

            if (DateTime.TryParse(date.Replace(",", "."), out var dateOfTrace))
            {
                DateOfTrace = dateOfTrace;
                Date = DateOfTrace.Value.ToString("dd.MM.yyyy HH:mm:ss.fff");
            }
            else
            {
                Date = date;
            }

            TraceName = traceName;
            Description = description.Trim();
            Message = message;
            TraceMessage = traceMessage;
        }

        internal DataTemplate(TraceReader fileLog, int foundLineID, string traceMessage)
        {
            FoundLineID = foundLineID;
            IsMatched = false;
            _fileLog = fileLog;

            ID = -1;
            TraceMessage = traceMessage;
        }

        internal DataTemplate(TraceReader fileLog, int foundLineID, string traceMessage, Exception error)
        {
            FoundLineID = foundLineID;
            IsMatched = false;
            _fileLog = fileLog;

            ID = -1;
            DateOfTrace = DateTime.Now;
            Date = DateOfTrace.Value.ToString("dd.MM.yyyy HH:mm:ss.fff");

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

        public int FoundLineID { get; }

        [DGVColumn(ColumnPosition.Last, "PrivateID", false)]
        public int PrivateID { get; internal set; }

        [DGVColumn(ColumnPosition.Last, "IsMatched", false)]
        public bool IsMatched { get; private set; }

        [DGVColumn(ColumnPosition.After, "ID")]
        public int ID { get; internal set; }

        [DGVColumn(ColumnPosition.After, "Server")]
        public string Server => _fileLog.Server;

        [DGVColumn(ColumnPosition.After, "Trace name")]
        public string TraceName
        {
            get => _traceName;
            private set => _traceName = value?.Replace(Environment.NewLine, string.Empty).Trim();
        }

        [DGVColumn(ColumnPosition.After, "Date")]
        public string Date
        {
            get => _date;
            private set => _date = value?.Replace(Environment.NewLine, string.Empty).Trim();
        }

        [DGVColumn(ColumnPosition.After, "File")]
        public string File => _fileLog.FileNamePartial;

        public DateTime? DateOfTrace { get; }

        public string Description { get; private set; }

        public string Message
        {
            get => _message.IsNullOrEmpty() ? TraceMessage : _message;
            private set => _message = value;
        }

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
    }
}
