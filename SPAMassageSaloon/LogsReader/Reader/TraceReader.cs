using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LogsReader.Config;
using Utils;

namespace LogsReader.Reader
{
    public delegate void FoundDataTemplate(DataTemplate item);

    public abstract class TraceReader
    {
        private readonly LogsReaderControl _settings;

        protected Queue<string> PastTraceLines { get; }

        protected DataTemplate Found { get; set; }

        public event FoundDataTemplate OnFound;

        public Func<string, bool> IsMatchSearchPatternFunc => _settings.IsMatchSearchPatternFunc;

        /// <summary>
        /// Текущая схема настроек
        /// </summary>
        public LRSettingsScheme CurrentSettings => _settings.CurrentSettings;

        protected int MaxTraceLines => CurrentSettings.MaxLines;

        protected LRTraceParseItem[] TraceParsePatterns => CurrentSettings.TraceParse.Patterns;

        public string Server { get; }
        public string FileNamePartial { get; }
        public string FilePath { get; }
        public FileInfo File { get; }

        public long Lines { get; protected set; } = 0;

        protected TraceReader(string server, string filePath, string originalFolder, LogsReaderControl settings)
        {
            _settings = settings;

            PastTraceLines = new Queue<string>(MaxTraceLines);

            Server = server;
            FilePath = filePath;
            File = new FileInfo(filePath);
            FileNamePartial = IO.GetPartialPath(FilePath, originalFolder);
        }

        public abstract void ReadLine(string line);

        public void Commit()
        {
            if (Found == null)
                return;

            try
            {
                if (!Found.IsMatched)
                {
                    if (IsTraceMatch(Found.TraceMessage, out var result, Found, true))
                        AddResult(result);
                    else
                        AddResult(Found);
                }
            }
            catch (Exception ex)
            {
                if (!Found.IsMatched)
                    AddResult(new DataTemplate(this, Lines, Found.TraceMessage, ex));
                throw;
            }
            finally
            {
                Found = null;
            }
        }

        protected void AddResult(DataTemplate item)
        {
            OnFound?.Invoke(item);
        }

        protected bool IsTraceMatch(string input, out DataTemplate result, DataTemplate failed = null, bool throwException = false)
        {
            // замена '\r' чинит баг с некорректным парсингом
            var traceMessage = input.Replace("\r", string.Empty);
            foreach (var item in TraceParsePatterns)
            {
                Match match;
                try
                {
	                match = item.RegexItem.Match(traceMessage);
                }
                catch (Exception ex)
                {
	                if (throwException)
		                throw;


	                result = new DataTemplate(this, failed?.FoundLineID ?? Lines, input, ex);
	                return false;
                }

                if (match.Success && match.Value.Length == traceMessage.Length)
                {
                    result = new DataTemplate(this,
                        failed?.FoundLineID ?? Lines,
                        match.GetValueByReplacement(item.ID),
                        match.GetValueByReplacement(item.Date),
                        match.GetValueByReplacement(item.TraceName),
                        match.GetValueByReplacement(item.Description),
                        match.GetValueByReplacement(item.Message),
                        traceMessage);
                    return true;
                }
            }

            result = new DataTemplate(this, Lines, traceMessage);
            return false;
        }


        protected Match IsLineMatch(string input)
        {
            // замена '\r' чинит баг с некорректным парсингом
            var traceMessage = input.Replace("\r", string.Empty);
            foreach (var regexPatt in TraceParsePatterns.Select(x => x.RegexItem))
            {
                Match match;
                try
                {
                    match = regexPatt.Match(traceMessage);
                }
                catch (Exception)
                {
                    return null;
                }
                
                if (match.Success && match.Value.Length == traceMessage.Length)
                    return match;
            }

            return null;
        }

        public override string ToString()
        {
            return FilePath;
        }
    }
}