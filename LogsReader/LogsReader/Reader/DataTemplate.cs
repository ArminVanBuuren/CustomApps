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
        private readonly StringBuilder _entireMessage = new StringBuilder();
        private string _date = null;
        private string _trace = null;
        private string _message = string.Empty;

        internal DataTemplate(TraceReader fileLog, string strID, string date, string trace, string description, string message, string entireTrace)
        {
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

            Trace = trace;
            Description = description.Trim();
            Message = message;
            EntireTrace = entireTrace;
        }

        internal DataTemplate(TraceReader fileLog, string entireTrace)
        {
            IsMatched = false;
            _fileLog = fileLog;

            ID = -1;
            EntireTrace = entireTrace;
        }

        internal DataTemplate(TraceReader fileLog, string entireTrace, Exception error)
        {
            IsMatched = false;
            _fileLog = fileLog;

            ID = -1;
            DateOfTrace = DateTime.Now;
            Date = DateOfTrace.Value.ToString("dd.MM.yyyy HH:mm:ss.fff");

            Trace = error.GetType().ToString();
            Description = error.Message;
            if (error is RegexMatchTimeoutException errorRegex)
            {
                Message = $"Timeout while executing pattern: \"{errorRegex.Pattern}\".\r\nTimeout: {errorRegex.MatchTimeout.ToReadableString()}\r\nInput:\r\n{errorRegex.Input}";
            }
            else
            {
                Message = error.ToString();
            }
            EntireTrace = entireTrace;
        }

        [DGVColumn(ColumnPosition.Last, "PrivateID", false)]
        public int PrivateID { get; internal set; }

        [DGVColumn(ColumnPosition.Last, "IsMatched", false)]
        public bool IsMatched { get; private set; }

        [DGVColumn(ColumnPosition.After, "ID")]
        public int ID { get; internal set; }

        [DGVColumn(ColumnPosition.After, "Server")]
        public string Server => _fileLog.Server;

        [DGVColumn(ColumnPosition.After, "Trace")]
        public string Trace
        {
            get => _trace;
            private set => _trace = value?.Replace(Environment.NewLine, string.Empty).Trim();
        }

        [DGVColumn(ColumnPosition.After, "Date")]
        public string Date
        {
            get => _date;
            private set => _date = value?.Replace(Environment.NewLine, string.Empty).Trim();
        }

        [DGVColumn(ColumnPosition.After, "FileName")]
        public string FileName => _fileLog.FileName;

        public DateTime? DateOfTrace { get; }

        public string Description { get; private set; }

        public string Message
        {
            get => _message.IsNullOrEmpty() ? EntireTrace : _message;
            private set => _message = value;
        }

        public string EntireTrace
        {
            get => _entireMessage.ToString();
            private set
            {
                _entireMessage.Clear();
                _entireMessage.Append(value);
                CountOfLines = value.Split('\n').Length;
            }
        }

        public int CountOfLines { get; private set; } = 1;

        internal void AppendPastLine(string line)
        {
            _entireMessage.Insert(0, line + Environment.NewLine);
            CountOfLines++;
        }

        internal void AppendNextLine(string line)
        {
            _entireMessage.Append(Environment.NewLine + line);
            CountOfLines++;
        }

        internal void MergeDataTemplates(DataTemplate input)
        {
            Message = input.Message;
            EntireTrace = input.EntireTrace;
        }
    }
}
