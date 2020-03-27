using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using LogsReader.Config;
using Utils;

namespace LogsReader.Reader
{
    public delegate void ReportProcessStatusHandler(int countMatches, int percentOfProgeress, int filesCompleted, int totalFiles);

    public class LogsReaderPerformer : IDisposable
    {
        private readonly IEnumerable<string> _servers;
        private readonly IEnumerable<string> _traces;
        private MTActionResult<TraceReader> _multiTaskingHandler = null;

        public event ReportProcessStatusHandler OnProcessReport;

        public Func<string, bool> IsMatchSearchPatternFunc { get; }

        /// <summary>
        /// Запрос на ожидание остановки выполнения поиска
        /// </summary>
        public bool IsStopPending { get; private set; } = false;

        /// <summary>
        /// Количество совпадений по критериям поиска
        /// </summary>
        public int CountMatches => KvpList?.Sum(x => x.CountMatches) ?? 0;

        public List<TraceReader> KvpList { get; private set; }

        /// <summary>
        /// Текущая схема настроек
        /// </summary>
        public LRSettingsScheme CurrentSettings { get; }

        public IEnumerable<DataTemplate> ResultsOfSuccess { get; private set; }
        public IEnumerable<DataTemplate> ResultsOfError { get; private set; }

        public LogsReaderPerformer(LRSettingsScheme settings, IEnumerable<string> servers, IEnumerable<string> traces, string findMessage, bool useRegex)
        {
            if (useRegex)
            {
                if (!REGEX.Verify(findMessage))
                    throw new ArgumentException($"Pattern \"{findMessage}\" is incorrect for use as regular expression! Please check.");

                var searchPattern = new Regex(findMessage, RegexOptions.Compiled | RegexOptions.IgnoreCase, new TimeSpan(0, 0, 5));
                IsMatchSearchPatternFunc = (input) => searchPattern.IsMatch(input);
            }
            else
            {
                IsMatchSearchPatternFunc = (input) => input.IndexOf(findMessage, StringComparison.InvariantCultureIgnoreCase) != -1;
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

            KvpList = await Task<List<TraceReader>>.Factory.StartNew(GetFileLogs);
            if (KvpList == null || KvpList.Count <= 0)
                throw new Exception(@"No files logs found");

            // ThreadPriority.Lowest - необходим чтобы не залипал основной поток и не мешал другим процессам
            var maxThreads = CurrentSettings.MaxThreads <= 0 ? KvpList.Count : CurrentSettings.MaxThreads;
            _multiTaskingHandler = new MTActionResult<TraceReader>(ReadData, KvpList, maxThreads, ThreadPriority.Lowest);
            new Action(CheckProgress).BeginInvoke(ProcessCompleted, null);
            await _multiTaskingHandler.StartAsync();


            ResultsOfSuccess = KvpList.SelectMany(x => x.FoundResults);
            ResultsOfError = _multiTaskingHandler.Result.CallBackList.Where(x => x.Error != null).Aggregate(new List<DataTemplate>(), (listErr, x) =>
            {
                listErr.Add(new DataTemplate(x.Source, -1, string.Empty, x.Error));
                return listErr;
            });
        }

        List<TraceReader> GetFileLogs()
        {
            var dirMatch = IO.CHECK_PATH.Match(CurrentSettings.LogsDirectory);
            var logsDirFormat = @"\\{0}\" + $"{dirMatch.Groups["DISC"]}${dirMatch.Groups["FULL"]}";
            var kvpList = new List<TraceReader>();

            Func<string, string, TraceReader> _getTraceReader;
            if (CurrentSettings.TraceLinePattern.StartLineWith != null)
                _getTraceReader = (server, filePath) => new TraceReaderStartWith(server, filePath, this);
            else
                _getTraceReader = (server, filePath) => new TraceReaderSimple(server, filePath, this);

            foreach (var serverName in _servers)
            {
                if (IsStopPending)
                    return kvpList;

                var serverDir = string.Format(logsDirFormat, serverName);
                if (!Directory.Exists(serverDir))
                    continue;

                var files = Directory.GetFiles(serverDir, "*", SearchOption.AllDirectories);
                foreach (var fileLog in files.Select(filePath => _getTraceReader(serverName, filePath)))
                {
                    if (IsStopPending)
                        return kvpList;

                    if (IsAllowedExtension(fileLog.FileName) && _traces.Any(x => fileLog.FileName.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) != -1))
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

        public void ReadData(TraceReader fileLog)
        {
            try
            {
                // FileShare должен быть ReadWrite. Иначе, если файл используется другим процессом то доступ к чтению файла будет запрещен.
                using (var inputStream = new FileStream(fileLog.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var inputReader = new StreamReader(inputStream, Encoding.GetEncoding("windows-1251")))
                {
                    string line;
                    while ((line = inputReader.ReadLine()) != null && !IsStopPending)
                    {
                        fileLog.ReadLine(line);
                    }
                }
            }
            finally
            {
                fileLog.Commit();
            }
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
                    OnProcessReport?.Invoke(CountMatches, _multiTaskingHandler.PercentOfComplete, _multiTaskingHandler.Result.Count, total);
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
                OnProcessReport?.Invoke(CountMatches, 100, _multiTaskingHandler.Result.Count, _multiTaskingHandler.Source.Count());
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        static bool IsAllowedExtension(string fileName)
        {
            switch (Path.GetExtension(fileName))
            {
                case ".aif": return false;
                case ".cda": return false;
                case ".mid": return false;
                case ".mp3": return false;
                case ".mpa": return false;
                case ".ogg": return false;
                case ".wav": return false;
                case ".wma": return false;
                case ".wpl": return false;
                case ".7z": return false;
                case ".arj": return false;
                case ".deb": return false;
                case ".pkg": return false;
                case ".rar": return false;
                case ".rpm": return false;
                case ".tar.gz": return false;
                case ".z": return false;
                case ".zip": return false;
                case ".bin": return false;
                case ".dmg": return false;
                case ".iso": return false;
                case ".toast": return false;
                case ".vcd": return false;
                case ".mdb": return false;
                case ".dat": return false;
                case ".exe": return false;
                case ".dll": return false;
                case ".ai": return false;
                case ".bmp": return false;
                case ".gif": return false;
                case ".ico": return false;
                case ".jpeg": return false;
                case ".png": return false;
                case ".ps": return false;
                case ".psd": return false;
                case ".svg": return false;
                case ".tif": return false;
                case ".ods": return false;
                case ".xls": return false;
                case ".xlsm": return false;
                case ".xlsx": return false;
                case ".bak": return false;
                case ".cab": return false;
                case ".cfg": return false;
                case ".cpl": return false;
                case ".cur": return false;
                case ".dmp": return false;
                case ".drv": return false;
                case ".icns": return false;
                case ".ini": return false;
                case ".lnk": return false;
                case ".msi": return false;
                case ".sys": return false;
                case ".tmp": return false;
                case ".3g2": return false;
                case ".3gp": return false;
                case ".avi": return false;
                case ".flv": return false;
                case ".h264": return false;
                case ".m4v": return false;
                case ".mkv": return false;
                case ".mov": return false;
                case ".mp4": return false;
                case ".mpg": return false;
                case ".rm": return false;
                case ".swf": return false;
                case ".vob": return false;
                case ".wmv": return false;
                case ".doc": return false;
                case ".odt": return false;
                case ".pdf": return false;
                case ".rtf": return false;
                case ".tex": return false;
                case ".txt": return false;
                case ".wpd": return false;
            }

            return true;
        }

        public void Dispose()
        {
            Reset();
        }
    }
}
