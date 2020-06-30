﻿using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using LogsReader.Properties;
using Utils;
using Utils.WinForm.DataGridViewHelper;
using static LogsReader.Config.LRTraceParsePatternItem;

namespace LogsReader.Reader
{
    public class DataTemplate : ICloneable
    {
        // Названия столбцов. Менять не рекомендуется, но влияния нет.
	    public const string HeaderID = "ID";
        public const string HeaderServer = "Server";
        public const string HeaderTraceName = "TraceName";
        public const string HeaderDate = "Date";
        public const string HeaderFile = "File";
        public const string HeaderDescription = "Description";
        public const string HeaderMessage = "Message";
        public const string HeaderTraceMessage = "Full Trace";

        private static DataTemplate _tmp;
        /// <summary>
        /// Темповый темплейт, во избежание ошибок. Если будут изменяться названия полей в <see cref="DataTemplate"/> то, они будут изменяться везде.
        /// </summary>
        internal static DataTemplate Tmp => _tmp ?? (_tmp = new DataTemplate(null, -1, null, null));

	    private readonly StringBuilder _traceMessage = new StringBuilder();
        private string _description;
        private string _traceName;
        private string _message = string.Empty;

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

        [DGVColumn(ColumnPosition.Last, nameof(DataTemplate.Tmp.SchemeName), false)]
        public string SchemeName => ParentReader.SchemeName;

        [DGVColumn(ColumnPosition.Last, nameof(DataTemplate.Tmp.PrivateID), false)]
        public int PrivateID { get; internal set; }

        public bool IsMatched { get; }

        [DGVColumn(ColumnPosition.After, HeaderID)]
        public int ID { get; internal set; }

        [DGVColumn(ColumnPosition.After, HeaderServer)]
        public string Server => ParentReader.Server;

        [DGVColumn(ColumnPosition.After, HeaderTraceName)]
        public string TraceName
        {
            get => _traceName;
            private set => _traceName = value?.Replace("\r", string.Empty).Replace("\n", string.Empty).TrimWhiteSpaces() ?? string.Empty;
        }

        [DGVColumn(ColumnPosition.After, HeaderDate)]
        public string DateOfTrace { get; }

        public DateTime? Date { get; }

        [DGVColumn(ColumnPosition.After, HeaderFile)]
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

        public override bool Equals(object obj)
        {
	        var isEqual = false;
	        if (obj is DataTemplate input)
		        isEqual = FoundLineID == input.FoundLineID && ParentReader.Equals(input.ParentReader);
	        return isEqual;
        }

        /// <summary>
        /// хэш найденной позиции в файле и хэш ридера
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
	        var hash = FoundLineID.GetHashCode() + ParentReader.GetHashCode();
            return hash;
        }

        public object Clone()
        {
	        return this.MemberwiseClone();
        }

        public override string ToString()
        {
            return ParentReader.ToString();
        }
    }
}
