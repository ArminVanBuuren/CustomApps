using System;
using Utils;
using Utils.WinForm.DataGridViewHelper;

namespace LogsReader
{
    public class DataTemplate
    {
        private string _entireMessage = null;
        private string _date = null;
        private string _traceType = null;

        [DGVColumn(ColumnPosition.First, "ID")]
        public int ID { get; set; }

        [DGVColumn(ColumnPosition.First, "Server")]
        public string Server { get; set; }

        [DGVColumn(ColumnPosition.After, "Date")]
        public string Date
        {
            get => _date;
            set => _date = value.Replace(Environment.NewLine, string.Empty);
        }

        [DGVColumn(ColumnPosition.After, "TraceType")]
        public string TraceType
        {
            get => _traceType;
            set => _traceType = value.Replace(Environment.NewLine, string.Empty);
        }

        [DGVColumn(ColumnPosition.After, "FileName")]
        public string FileName { get; set; }

        [DGVColumn(ColumnPosition.After, "Message", false)]
        public string Message { get; set; }

        [DGVColumn(ColumnPosition.Last, "IsMatched", false)]
        public bool IsMatched { get; set; }

        [DGVColumn(ColumnPosition.Last, "EntireMessage", false)]
        public string EntireMessage
        {
            get
            {
                if (!IsMatched && _entireMessage.IsNullOrEmpty())
                    return Message;
                return _entireMessage;
            }
            set => _entireMessage = value;
        }
    }
}
