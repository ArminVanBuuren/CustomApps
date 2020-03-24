using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using LogsReader.Config;
using LogsReader.Data;
using Utils;

namespace LogsReader
{
    public delegate void ReportProcessStatusHandler(int numberOfFound, int percentOfProgeress, int filesCompleted, int totalFiles);

    public class LogsReader : IDisposable
    {
        private readonly object _syncRootFinded = new object();
        private readonly IEnumerable<string> _servers;
        private readonly IEnumerable<string> _traces;
        
        private int _found = 0;
        private MTFuncResult<FileLog, List<DataTemplate>> _multiTaskingHandler = null;
        private readonly Func<string, bool> _isMatchSearchPatternFunc = null;

        public event ReportProcessStatusHandler OnProcessReport;

        /// <summary>
        /// Запрос на ожидание остановки выполнения поиска
        /// </summary>
        public bool IsStopPending { get; private set; } = false;

        /// <summary>
        /// Количество совпадений по критериям поиска
        /// </summary>
        public int Found
        {
            get
            {
                lock (_syncRootFinded)
                    return _found;
            }
            private set
            {
                lock (_syncRootFinded)
                    _found = value;
            }
        }

        public List<FileLog> KvpList { get; private set; }

        /// <summary>
        /// Текущая схема настроек
        /// </summary>
        public LRSettingsScheme CurrentSettings { get; }

        public IEnumerable<DataTemplate> ResultsOfSuccess { get; private set; }
        public IEnumerable<DataTemplate> ResultsOfError { get; private set; }

        public LogsReader(LRSettingsScheme settings, IEnumerable<string> servers, IEnumerable<string> traces, string findMessage, bool useRegex = true)
        {
            if (useRegex)
            {
                if (!REGEX.Verify(findMessage))
                    throw new ArgumentException($"Pattern \"{findMessage}\" is incorrect for use as regular expression! Please check.");

                var searchPattern = new Regex(findMessage, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                _isMatchSearchPatternFunc = (input) => searchPattern.IsMatch(input);
            }
            else
            {
                _isMatchSearchPatternFunc = (input) => input.IndexOf(findMessage, StringComparison.OrdinalIgnoreCase) != -1;
            }

            CurrentSettings = settings;
            _servers = servers;
            _traces = traces;
            Reset();
        }

        public async Task StartAsync()
        {
            if (IsStopPending)
                throw new Exception(@"Session was stopped");
            if (_multiTaskingHandler != null && _multiTaskingHandler.IsCompleted)
                throw new Exception(@"Current session already completed!");

            KvpList = await Task<List<FileLog>>.Factory.StartNew(GetFileLogs);
            if (KvpList == null || KvpList.Count <= 0)
                throw new Exception(@"No files logs found");


            var maxThreads = CurrentSettings.MaxThreads <= 0 ? KvpList.Count : CurrentSettings.MaxThreads;
            // ThreadPriority.Lowest - необходим чтобы не залипал основной поток и не мешал другим процессам
            _multiTaskingHandler = new MTFuncResult<FileLog, List<DataTemplate>>(ReadData, KvpList, maxThreads, ThreadPriority.Lowest);
            new Action(CheckProgress).BeginInvoke(ProcessCompleted, null);
            await _multiTaskingHandler.StartAsync();


            ResultsOfSuccess = _multiTaskingHandler.Result.CallBackList.Where(x => x.Result != null).SelectMany(x => x.Result);
            ResultsOfError = _multiTaskingHandler.Result.CallBackList.Where(x => x.Error != null).Aggregate(new List<DataTemplate>(), (listErr, x) =>
            {
                listErr.Add(new DataTemplate(x.Source, x.Error));
                return listErr;
            });
        }

        List<FileLog> GetFileLogs()
        {
            var dirMatch = IO.CHECK_PATH.Match(CurrentSettings.LogsDirectory);
            var logsDirFormat = @"\\{0}\" + $"{dirMatch.Groups["DISC"]}${dirMatch.Groups["FULL"]}";
            var kvpList = new List<FileLog>();

            foreach (var serverName in _servers)
            {
                if (IsStopPending)
                    return kvpList;

                var serverDir = string.Format(logsDirFormat, serverName);
                if (!Directory.Exists(serverDir))
                    continue;

                var files = Directory.GetFiles(serverDir, "*", SearchOption.AllDirectories);
                foreach (var fileLog in files.Select(x => new FileLog(serverName, x)))
                {
                    if (IsStopPending)
                        return kvpList;

                    if (_traces.Any(x => fileLog.FileName.IndexOf(x, StringComparison.CurrentCultureIgnoreCase) != -1))
                        kvpList.Add(fileLog);
                }
            }

            //var stop = new Stopwatch();
            //stop.Start();
            //var getCountLines = new MTFuncResult<FileLog, long>((input) => IO.CountLinesReader(input.FilePath), kvpList, kvpList.Count, ThreadPriority.Lowest);
            //getCountLines.Start();
            //var lines = getCountLines.Result.Values.Select(x => x.Result).Sum(x => x);
            //stop.Stop();

            return kvpList;
        }

        public void Stop()
        {
            IsStopPending = true;
            _multiTaskingHandler?.Stop();
        }

        public void Reset()
        {
            IsStopPending = false;
            KvpList?.Clear();
            _multiTaskingHandler?.Stop();
            _multiTaskingHandler = null;
            ResultsOfSuccess = null;
            ResultsOfError = null;
        }

        void CheckProgress()
        {
            try
            {
                var total = _multiTaskingHandler.Source.Count;
                while (_multiTaskingHandler != null && !_multiTaskingHandler.IsCompleted)
                {
                    OnProcessReport?.Invoke(Found, _multiTaskingHandler.PercentOfComplete, _multiTaskingHandler.Result.Count, total);
                    Thread.Sleep(10);
                }
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        void ProcessCompleted(IAsyncResult asyncResult)
        {
            try
            {
                OnProcessReport?.Invoke(Found, 100, _multiTaskingHandler.Result.Count, _multiTaskingHandler.Source.Count());
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        private List<DataTemplate> ReadData(FileLog fileLog)
        {
            var beforeTraceLines = new Queue<string>(CurrentSettings.MaxTraceLines);
            var listResult = new List<DataTemplate>();

            // FileShare должен быть ReadWrite. Иначе, если файл используется другим процессом то доступ к чтению файла будет запрещен.
            using (var inputStream = new FileStream(fileLog.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var inputReader = new StreamReader(inputStream, Encoding.GetEncoding("windows-1251")))
            {
                var stackLines = 0;
                string line;
                DataTemplate lastResult = null;
                while ((line = inputReader.ReadLine()) != null && !IsStopPending)
                {
                    if (lastResult != null)
                    {
                        // если стек лога превышает допустимый размер, то лог больше не дополняется
                        if (stackLines >= CurrentSettings.MaxTraceLines)
                        {
                            if (!lastResult.IsMatched)
                                listResult.Add(lastResult);
                            stackLines = 0;
                            lastResult = null;
                        }
                        else
                        {
                            if (lastResult.IsMatched)
                            {
                                var appendedToEntireMessage = lastResult.EntireMessage + Environment.NewLine + line;
                                // Eсли строка не совпадает с паттерном строки, то текущая строка лога относится к предыдущему успешно спарсеному.
                                // Иначе строка относится к другому логу и завершается дополнение
                                if (CurrentSettings.IsMatch(line) == null && CurrentSettings.IsLineMatch(appendedToEntireMessage, fileLog, out var newResult))
                                {
                                    stackLines++;
                                    lastResult.MergeDataTemplates(newResult);
                                    continue;
                                }
                                else
                                {
                                    stackLines = 0;
                                    lastResult = null;
                                }
                            }
                            else if (!lastResult.IsMatched)
                            {
                                // Если предыдущий фрагмент лога не спарсился удачано, то выполняются новые попытки спарсить лог
                                stackLines++;
                                lastResult.AppendMessageAfter(Environment.NewLine + line);
                                if (CurrentSettings.IsLineMatch(lastResult.Message, fileLog, out var afterSuccessResult))
                                {
                                    // Паттерн успешно сработал и тепмлейт заменяется. И дальше продолжается проврерка на дополнение строк
                                    listResult.Add(afterSuccessResult);
                                    lastResult = afterSuccessResult;
                                    beforeTraceLines.Clear();
                                    continue;
                                }
                            }
                        }
                    }

                    if (!_isMatchSearchPatternFunc.Invoke(line))
                    {
                        beforeTraceLines.Enqueue(line);
                        if (beforeTraceLines.Count > CurrentSettings.MaxTraceLines)
                            beforeTraceLines.Dequeue();
                        continue;
                    }
                    else
                    {
                        ++Found;
                        stackLines = 1;

                        if (lastResult != null && !lastResult.IsMatched)
                        {
                            listResult.Add(lastResult);
                        }
                    }

                    if (CurrentSettings.IsLineMatch(line, fileLog, out lastResult))
                    {
                        listResult.Add(lastResult);
                    }
                    else
                    {
                        // Попытки спарсить текущую строку вместе с сохраненными предыдущими строками лога
                        var reverceBeforeTraceLines = new Queue<string>(beforeTraceLines.Reverse());
                        while (stackLines < CurrentSettings.MaxTraceLines && reverceBeforeTraceLines.Count > 0)
                        {
                            stackLines++;
                            lastResult.AppendMessageBefore(reverceBeforeTraceLines.Dequeue() + Environment.NewLine);

                            if (CurrentSettings.IsLineMatch(lastResult.Message, fileLog, out var beforeResult))
                            {
                                lastResult = beforeResult;
                                listResult.Add(lastResult);
                                break;
                            }
                        }
                    }

                    beforeTraceLines.Clear();
                }

                if (lastResult != null && !lastResult.IsMatched)
                {
                    listResult.Add(lastResult);
                }
            }

            return listResult;
        }

        public void Dispose()
        {
            Reset();
        }
    }
}
