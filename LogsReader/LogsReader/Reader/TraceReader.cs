using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LogsReader.Config;
using Utils;

namespace LogsReader.Reader
{
    public abstract class TraceReader
    {
        private readonly LogsReaderPerformer _mainReader;

        protected Queue<string> PastTraceLines { get; }

        protected DataTemplate Found { get; set; }

        public Func<string, bool> IsMatchSearchPatternFunc => _mainReader.IsMatchSearchPatternFunc;

        /// <summary>
        /// Текущая схема настроек
        /// </summary>
        public LRSettingsScheme CurrentSettings => _mainReader.CurrentSettings;

        protected int MaxTraceLines => CurrentSettings.MaxTraceLines;

        protected LRTraceLinePatternItem[] PatternItems => CurrentSettings.TraceLinePattern.Items;

        public string Server { get; }
        public string FileName { get; }
        public string FilePath { get; }

        /// <summary>
        /// Количество совпадений по критериям поиска
        /// </summary>
        public int CountMatches { get; protected set; } = 0;

        public int Lines { get; protected set; } = 0;

        public List<DataTemplate> FoundResults { get; }

        protected TraceReader(string server, string filePath, LogsReaderPerformer mainReader)
        {
            _mainReader = mainReader;

            FoundResults = new List<DataTemplate>();
            PastTraceLines = new Queue<string>(MaxTraceLines);

            Server = server;
            FilePath = filePath;

            var logsPathWithoutRoot = CurrentSettings.LogsDirectory.Replace(Path.GetPathRoot(CurrentSettings.LogsDirectory), string.Empty, StringComparison.InvariantCultureIgnoreCase);
            var filePathWithoutRoot = FilePath.Replace(Path.GetPathRoot(FilePath), string.Empty, StringComparison.InvariantCultureIgnoreCase).Trim('\\');
            FileName = filePathWithoutRoot.Substring(logsPathWithoutRoot.Length, filePathWithoutRoot.Length - logsPathWithoutRoot.Length).Trim('\\');
        }

        public abstract void ReadLine(string line);

        public void Commit()
        {
            if (Found == null)
                return;

            try
            {
                if (IsTraceMatch(Found.EntireTrace, out var result, Found, true))
                    AddResult(result);
                else
                    AddResult(Found);
            }
            catch (Exception ex)
            {
                AddResult(new DataTemplate(this, Lines, Found.EntireTrace, ex));
            }
            finally
            {
                Found = null;
            }
        }

        protected void AddResult(DataTemplate item)
        {
            FoundResults.Add(item);
        }

        protected bool IsTraceMatch(string input, out DataTemplate result, DataTemplate failed = null, bool throwException = false)
        {
            // замена \r чинит баг с некорректным парсингом
            var message = input.Replace("\r", string.Empty);
            foreach (var item in PatternItems)
            {
                Match match;
                try
                {
                    match = item.RegexItem.Match(message);
                }
                catch (Exception ex)
                {
                    if (throwException)
                    {
                        throw;
                    }
                    else
                    {
                        result = new DataTemplate(this, failed?.FoundLineID ?? Lines, input, ex);
                        return false;
                    }
                }
                
                if (match.Success && match.Value.Length == message.Length)
                {
                    result = new DataTemplate(this,
                        failed?.FoundLineID ?? Lines,
                        match.GetValueByReplacement(item.ID),
                        match.GetValueByReplacement(item.Date),
                        match.GetValueByReplacement(item.Trace),
                        match.GetValueByReplacement(item.Description),
                        match.GetValueByReplacement(item.Message),
                        message);
                    return true;
                }
            }

            result = new DataTemplate(this, Lines, message);
            return false;
        }


        protected Match IsLineMatch(string input)
        {
            // замена \r чинит баг с некорректным парсингом
            var message = input.Replace("\r", string.Empty);
            foreach (var regexPatt in PatternItems.Select(x => x.RegexItem))
            {
                Match match;
                try
                {
                    match = regexPatt.Match(message);
                }
                catch (Exception)
                {
                    return null;
                }
                
                if (match.Success && match.Value.Length == message.Length)
                    return match;
            }

            return null;
        }

        public override string ToString()
        {
            return $"\\{Server}\\{FilePath}";
        }
    }
}