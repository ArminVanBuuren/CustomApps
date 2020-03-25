using System;
using System.Text;
using Utils.WinForm.DataGridViewHelper;

namespace LogsReader.Reader
{
    public class DataTemplate
    {
        private readonly TraceReader _fileLog;
        private readonly StringBuilder _message = new StringBuilder();
        private readonly StringBuilder _entireMessage = new StringBuilder();
        private string _date = null;
        private string _trace = null;

        public DataTemplate(TraceReader fileLog, string strID, string date, string trace, string description, string message, string entireMessage)
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
            _message.Append(message);
            _entireMessage.Append(entireMessage);
        }

        public DataTemplate(TraceReader fileLog, string message)
        {
            IsMatched = false;
            _fileLog = fileLog;

            ID = -1;
            _message.Append(message);
            _entireMessage.Append(message);
        }

        public DataTemplate(TraceReader fileLog, Exception error)
        {
            IsMatched = false;
            _fileLog = fileLog;

            ID = -1;
            DateOfTrace = DateTime.Now;
            Date = DateOfTrace.Value.ToString("dd.MM.yyyy HH:mm:ss.fff");
            Trace = error.GetType().ToString();
            _message.Append(error);
            _entireMessage.Append(error);
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

        public string Message => _message.ToString();

        public string EntireMessage
        {
            get
            {
                if (!IsMatched && _entireMessage.Length == 0)
                    return Message;
                return _entireMessage.ToString();
            }
        }

        public void AppendMessageBefore(string str)
        {
            _message.Insert(0, str);
            _entireMessage.Insert(0, str);
        }

        public void AppendMessageAfter(string str)
        {
            _message.Append(str);
            _entireMessage.Append(str);
        }

        public void MergeDataTemplates(DataTemplate input)
        {
            _message.Clear();
            _message.Append(input.Message);
            _entireMessage.Clear();
            _entireMessage.Append(input.EntireMessage);
        }
    }
}
