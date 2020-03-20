﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utils.WinForm.DataGridViewHelper;

namespace LogsReader
{
    public class FileLog
    {
        public FileLog(string server, string filePath)
        {
            Server = server;
            FilePath = filePath;
            FileName = Path.GetFileName(FilePath);
        }
        public string Server { get; }
        public string FileName { get; }
        public string FilePath { get; }

        public override string ToString()
        {
            return $"\\{Server}\\{FilePath}";
        }
    }

    public class DataTemplateCollection : IEnumerable<DataTemplate>, IDisposable
    {
        int _seqPrivateID = 0;
        int _seqID = 0;
        private readonly Dictionary<int, DataTemplate> _values;

        public DataTemplateCollection(IEnumerable<DataTemplate> list)
        {
            _values = new Dictionary<int, DataTemplate>(list.Count());

            // Сортируем по дате, если она спарсилась корректно. Потому что стрингов
            if (list.All(x => x.DateOfTrace != null))
                AddRange(list.OrderBy(p => p.DateOfTrace).ThenBy(p => p.FileName));
            else
                AddRange(list.OrderBy(p => p.Date).ThenBy(p => p.FileName));
        }

        public void AddRange(IEnumerable<DataTemplate> list)
        {
            foreach (var template in list)
            {
                template.PrivateID = ++_seqPrivateID;
                if (template.ID == -1)
                    template.ID = ++_seqID;

                _values.Add(template.PrivateID, template);
            }
        }

        public int Count => _values.Count;

        public DataTemplate this[int privateID] => _values[privateID];

        public void Clear()
        {
            _values.Clear();
        }

        public IEnumerator<DataTemplate> GetEnumerator()
        {
            return _values.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.Values.GetEnumerator();
        }

        public void Dispose()
        {
            Clear();
        }
    }

    public class DataTemplate
    {
        private readonly FileLog _fileLog;
        private readonly StringBuilder _message = new StringBuilder();
        private readonly StringBuilder _entireMessage = new StringBuilder();
        private string _date = null;
        private string _description = null;
        private string _traceType = null;

        public DataTemplate(FileLog fileLog, string strID, string date, string traceType, string description, string message, string entireMessage)
        {
            IsMatched = true;
            _fileLog = fileLog;

            ID = int.TryParse(strID, out var id) ? id : -1;

            Date = date;
            if (DateTime.TryParse(Date, out var dateOfTrace))
                DateOfTrace = dateOfTrace;

            TraceType = traceType;
            Description = description;
            _message.Append(message);
            _entireMessage.Append(entireMessage);
        }

        public DataTemplate(FileLog fileLog, string message)
        {
            IsMatched = false;
            _fileLog = fileLog;

            ID = -1;
            _message.Append(message);
            _entireMessage.Append(message);
        }

        public DataTemplate(FileLog fileLog, Exception error)
        {
            IsMatched = false;
            _fileLog = fileLog;

            ID = -1;
            DateOfTrace = DateTime.Now;
            Date = DateOfTrace.Value.ToString("dd.MM.yyyy HH:mm:ss.fff");
            TraceType = error.GetType().ToString();
            _message.Append(error);
            _entireMessage.Append(error);
        }

        [DGVColumn(ColumnPosition.First, "PrivateID", false)]
        public int PrivateID { get; internal set; }

        [DGVColumn(ColumnPosition.After, "ID")]
        public int ID { get; internal set; }

        [DGVColumn(ColumnPosition.After, "Server")]
        public string Server => _fileLog.Server;

        [DGVColumn(ColumnPosition.After, "Date")]
        public string Date
        {
            get => _date;
            private set => _date = value?.Replace(Environment.NewLine, string.Empty).Trim();
        }

        [DGVColumn(ColumnPosition.After, "FileName")]
        public string FileName => _fileLog.FileName;

        public DateTime? DateOfTrace { get; }

        [DGVColumn(ColumnPosition.After, "TraceType")]
        public string TraceType
        {
            get => _traceType;
            private set => _traceType = value?.Replace(Environment.NewLine, string.Empty).Trim();
        }

        [DGVColumn(ColumnPosition.After, "Description")]
        public string Description
        {
            get => _description;
            private set => _description = value?.Replace(Environment.NewLine, string.Empty).Trim();
        }

        [DGVColumn(ColumnPosition.Last, "IsMatched", false)]
        public bool IsMatched { get; private set; }

        public string Message
        {
            get => _message.ToString();
        }

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
    }
}
