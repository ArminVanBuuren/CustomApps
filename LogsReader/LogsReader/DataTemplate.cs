using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.WinForm.DataGridViewHelper;

namespace LogsReader
{
    public class DataTemplate
    {
        public bool IsMatched { get; set; }

        [DGVColumn(ColumnPosition.First, "Server")]
        public string Server { get; set; }

        [DGVColumn(ColumnPosition.After, "Date")]
        public string Date { get; set; }

        [DGVColumn(ColumnPosition.After, "FileName")]
        public string FileName { get; set; }

        [DGVColumn(ColumnPosition.After, "TraceType")]
        public string TraceType { get; set; }

        [DGVColumn(ColumnPosition.After, "Message", false)]
        public string Message { get; set; }
    }
}
