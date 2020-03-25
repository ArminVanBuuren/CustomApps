using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LogsReader.Config;
using Utils;

namespace LogsReader.Reader
{
    public abstract class TraceReader
    {
        private readonly LogsReader _mainReader;


        public string Server { get; }
        public string FileName { get; }
        public string FilePath { get; }

        protected abstract Queue<string> PastTraceLines { get; }

        protected abstract int MaxTraceLines { get; }

        protected int FoundStackLines { get; set; } = 0;

        protected DataTemplate Found { get; set; }

        public Func<string, bool> IsMatchSearchPatternFunc => _mainReader.IsMatchSearchPatternFunc;

        /// <summary>
        /// Текущая схема настроек
        /// </summary>
        public LRSettingsScheme CurrentSettings => _mainReader.CurrentSettings;

        public bool IsStopPending => _mainReader.IsStopPending;

        public LRTraceLinePatternItem[] PatternItems => CurrentSettings.TraceLinePattern.Items;

        public Regex StartLineWith => CurrentSettings.TraceLinePattern.StartLineWith;

        public Regex EndLineWith => CurrentSettings.TraceLinePattern.EndLineWith;

        /// <summary>
        /// Количество совпадений по критериям поиска
        /// </summary>
        public int NumberOfFound { get; protected set; } = 0;

        public List<DataTemplate> FoundResults { get; }

        protected TraceReader(string server, string filePath, LogsReader mainReader)
        {
            _mainReader = mainReader;
            FoundResults = new List<DataTemplate>();
            Server = server;
            FilePath = filePath;
            FileName = Path.GetFileName(FilePath);
        }

        public abstract void ReadLine(string line);

        public void Commit()
        {
            if (Found != null && !Found.IsMatched)
            {
                FoundResults.Add(Found);
            }
        }

        protected bool IsLineMatch(string input, out DataTemplate result)
        {
            // замена \r чинит баг с некорректным парсингом
            var message = input.Replace("\r", string.Empty);
            foreach (var item in PatternItems)
            {
                var match = item.RegexItem.Match(message);
                if (match.Success && match.Value.Length == message.Length)
                {
                    result = new DataTemplate(this,
                        match.GetValueByReplacement(item.ID),
                        match.GetValueByReplacement(item.Date),
                        match.GetValueByReplacement(item.Trace),
                        match.GetValueByReplacement(item.Description),
                        match.GetValueByReplacement(item.Message),
                        message);
                    return true;
                }
            }

            result = new DataTemplate(this, message);
            return false;
        }


        protected Match IsMatch(string input)
        {
            // замена \r чинит баг с некорректным парсингом
            var message = input.Replace("\r", string.Empty);
            foreach (var regexPatt in PatternItems.Select(x => x.RegexItem))
            {
                var match = regexPatt.Match(message);
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
