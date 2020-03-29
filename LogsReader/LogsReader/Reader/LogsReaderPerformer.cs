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
        private DateTime _startDate;
        private DateTime _endDate;

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

        public LogsReaderPerformer(LRSettingsScheme settings, IEnumerable<string> servers, IEnumerable<string> traces, string findMessage, bool useRegex, DateTime startDate, DateTime endDate)
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
            _startDate = startDate;
            _endDate = endDate;
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

                    if(!IsAllowedExtension(fileLog.File.Name))
                        continue;

                    // если фильтр даты начала больше даты последней записи в файл, то пропускаем
                    if (DateTime.Compare(_startDate, fileLog.File.LastAccessTime) > 0)
                        continue;

                    // если фильтр даты конца меньше даты создания файла, то пропускаем
                    if (DateTime.Compare(_endDate, fileLog.File.CreationTime) < 0)
                        continue;

                    if (_traces.Any(x => fileLog.File.Name.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) != -1))
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
                case ".aif":
                case ".cda":
                case ".mid":
                case ".mp3":
                case ".mpa":
                case ".ogg":
                case ".wav":
                case ".wma":
                case ".wpl":
                case ".7z":
                case ".arj":
                case ".deb":
                case ".pkg":
                case ".rar":
                case ".rpm":
                case ".tar.gz":
                case ".z":
                case ".zip":
                case ".bin":
                case ".dmg":
                case ".iso":
                case ".toast":
                case ".vcd":
                case ".mdb":
                case ".dat":
                case ".exe":
                case ".dll":
                case ".ai":
                case ".bmp":
                case ".gif":
                case ".ico":
                case ".jpeg":
                case ".png":
                case ".ps":
                case ".psd":
                case ".svg":
                case ".tif":
                case ".ods":
                case ".xls":
                case ".xlsm":
                case ".xlsx":
                case ".bak":
                case ".cab":
                case ".cfg":
                case ".cpl":
                case ".cur":
                case ".dmp":
                case ".drv":
                case ".icns":
                case ".ini":
                case ".lnk":
                case ".msi":
                case ".sys":
                case ".tmp":
                case ".3g2":
                case ".3gp":
                case ".avi":
                case ".flv":
                case ".h264":
                case ".m4v":
                case ".mkv":
                case ".mov":
                case ".mp4":
                case ".mpg":
                case ".rm":
                case ".swf":
                case ".vob":
                case ".wmv":
                case ".doc":
                case ".odt":
                case ".pdf":
                case ".rtf":
                case ".tex":
                case ".txt":
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
