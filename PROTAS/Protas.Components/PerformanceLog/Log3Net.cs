using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Protas.Components.Functions;
using Protas.Components.Types;
using Protas.Components.XPackage;

namespace Protas.Components.PerformanceLog
{
    public class Log3Net : ILog3NetMain, IDisposable
    {
        ~Log3Net()
        {
            Thread.Sleep(10);
            WriteLog();
        }
        ulong _logStep = 0;
        int _two = 15;
        int _three = 19;
        int _four = 15;
        int _tryLoad = 0;
        string _suffix = "log";
        string[] _filterByTrace;
        bool IsNewFileLog = false;
        int _writeLogRetry;
        string fullLogFilePath = string.Empty;
        static string _eventLogType = "Application";
        readonly Encoding _encoding = Encoding.UTF8;
        System.Timers.Timer _timerForWriteLog = new System.Timers.Timer();
        System.Timers.Timer _timerForSplitLogFile = new System.Timers.Timer();
        int SizeKbForSplit { get; set; } = 200;
        string SourceLogPath { get; set; }
        List<string> MemoryLog { get; } = new List<string>();
        List<IObjectStatistic> StatInstanceList { get; } = new List<IObjectStatistic>();
        /// <summary>
        /// текущий уровень логирования
        /// </summary>
        internal Log3NetSeverity LogSeverity { get;  set; } = Log3NetSeverity.Disable;
        /// <summary>
        /// путь к файлу записи лога
        /// </summary>
        public string FullPathLogWrite { get; private set; } = string.Empty;
        /// <summary>
        /// если успешно проинициалищирован
        /// </summary>
        public bool IsInitialize { get; private set; } = false;
        /// <summary>
        /// Значение в миллисекундах для сбора статистики, не на что не влияет просто какому классу необходим сбор статистики то он просто ориентируется по этому значению
        /// </summary>
        public int ReportTimeOutMsec { get; private set; } = 0;
        public Log3Net MainLog3Net => this;
        public string TraceMessagePrefix => string.Empty;
        public string TraceNamePostfix => string.Empty;

        public Log3Net(string configPath)
        {
            try
            {
                ReLoad(configPath);
            }
            catch(Exception ex)
            {
                AddEventLog(true, "Error When Loading Config:\r\n{0}\r\n{1}", ex.Message, ex.StackTrace);
            }
        }
        public Log3Net(XmlTransform config)
        {
            try
            {
                ReLoad(config);
            }
            catch (Exception ex)
            {
                AddEventLog(true, "Error When Loading Config:\r\n{0}\r\n{1}", ex.Message, ex.StackTrace);
            }
        }
        public Log3Net(XPack parentXpack)
        {
            try
            {
                ReLoad(parentXpack);
            }
            catch (Exception ex)
            {
                AddEventLog(true, "Error When Loading Config:\r\n{0}\r\n{1}", ex.Message, ex.StackTrace);
            }
        }
        /// <summary>
        /// Обработка конфиг файла, перезагрузка
        /// </summary>
        public bool ReLoad(string configPath)
        {
            if (string.IsNullOrEmpty(configPath))
            {
                AddEventLog(true, "Not Finded Config File. Input Path Is Empty");
                return false;
            }
            XmlTransform config = new XmlTransform(configPath, this);
            return ReLoad(config);
        }
        /// <summary>
        /// Обработка конфиг файла, перезагрузка
        /// </summary>
        public bool ReLoad(XmlTransform config)
        {
            if (!config.IsCorrect)
            {
                AddLog(Log3NetSeverity.Debug, "PerformanceLog", "Log3Net", "RELOAD", string.Format("Incorrect Config File.{0}{1}", (IsInitialize) ? " Prev Configuration In Work." : string.Empty, (!string.IsNullOrEmpty(config.FilePath)) ? " Path:" + config.FilePath : config.XmlDoc.OuterXml));
                return false;
            }
            _tryLoad = 0;
            Initate(config.XPackage);
            return true;
        }
        public bool ReLoad(XPack parentXpack)
        {
            _tryLoad = 0;
            Initate(parentXpack);
            return true;
        }


        public void AddStatisticInstance(IObjectStatistic instance)
        {
            if (instance != null)
                StatInstanceList.Add(instance);
        }
        void Initate(XPack parentXpack)
        {
            try
            {
                XPack log3NetNode = parentXpack["//log3net"]?[0];
                if (log3NetNode == null)
                    return;
                SourceLogPath = log3NetNode.Attributes["path"];
                FullPathLogWrite = GetFullPathAndCreate(SourceLogPath);
                if (string.IsNullOrEmpty(FullPathLogWrite))
                    return;
                foreach (XPack xpack in log3NetNode.ChildPacks)
                {
                    if (xpack.Name.Equals("item", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (xpack.Attributes["name"].Equals("severity", StringComparison.CurrentCultureIgnoreCase))
                            LogSeverity = CheckSeverity(xpack.Attributes["value"]);
                        else if (xpack.Attributes["name"].Equals("filter", StringComparison.CurrentCultureIgnoreCase))
                            GetTraceFilter(xpack.Attributes["value"]);
                        else if (xpack.Attributes["name"].Equals("ReportTimeout", StringComparison.CurrentCultureIgnoreCase))
                        {
                            string value = xpack.Attributes["value"];
                            if (GetTypeEx.IsNumber(value))
                            {
                                ReportTimeOutMsec = int.Parse(value.Trim()) * 1000 * 60;
                            }
                        }
                        else if(xpack.Attributes["name"].Equals("SplitLogFile", StringComparison.CurrentCultureIgnoreCase))
                        {
                            double splitPeriod = 0;
                            double.TryParse(xpack.Attributes["periodSec"], out splitPeriod);
                            if(splitPeriod > 0)
                            {
                                _timerForSplitLogFile = new System.Timers.Timer();
                                _timerForSplitLogFile.Interval = splitPeriod * 60 * 1000;
                                _timerForSplitLogFile.Elapsed += StartWriteNewLog;

                                int sizeKb = 0;
                                int.TryParse(xpack.Attributes["size"], out sizeKb);
                                if (sizeKb > 0)
                                    SizeKbForSplit = sizeKb;

                            }
                        }
                    }
                }
                if (LogSeverity != Log3NetSeverity.Disable)
                {
                    StartWriteNewLog(this, null);
                    if (_timerForSplitLogFile != null)
                        _timerForSplitLogFile.Enabled = true;
                    _timerForWriteLog = new System.Timers.Timer { Interval = 3000 };
                    _timerForWriteLog.Elapsed += (TimerElapsed);
                    _timerForWriteLog.Enabled = true;
                }
            }
            catch(Exception ex)
            {
                IsInitialize = false;
                AddEventLog(true, "Exception:{0}\r\n{1}", ex.Message, ex.Data);
            }
            finally
            {
                IsInitialize = true;
                MemoryLog.Clear();
            }
        }

        
        void StartWriteNewLog(object sender, System.Timers.ElapsedEventArgs e)
        {
            _logStep = 0;
            IsNewFileLog = true;
        }



        void GetTraceFilter(string tracefilter)
        {
            if (!string.IsNullOrEmpty(tracefilter))
                _filterByTrace = tracefilter.Split(';');
        }
        static Log3NetSeverity CheckSeverity(string str)
        {
            string sourceTraceSeverity = str.ToLower().Trim(' ', '\r', '\n').Trim();
            switch (sourceTraceSeverity)
            {
                case "max":
                case "maximum": return Log3NetSeverity.Max;
                case "m":
                case "maxerr":
                case "maxerror": return Log3NetSeverity.MaxErr;
                case "debug": return Log3NetSeverity.Debug;
                case "w":
                case "warn":
                case "warning": return Log3NetSeverity.Warning;
                case "n":
                case "nrm":
                case "norm":
                case "normal": return Log3NetSeverity.Normal;
                case "r":
                case "rep":
                case "rprt":
                case "report": return Log3NetSeverity.Report;
                case "e":
                case "er":
                case "err":
                case "error": return Log3NetSeverity.Error;
                case "f":
                case "fat":
                case "fatal": return Log3NetSeverity.Fatal;
                default: return Log3NetSeverity.Disable;
            }
        }

        /// <summary>
        /// Проверка указанного пути для записи лога, создание дериктории если в указанном пути он не создан
        /// </summary>
        /// <param name="inPath"></param>
        /// <returns></returns>
        string GetFullPathAndCreate(string inPath)
        {
            PathProperty pathProp;
            if (string.IsNullOrEmpty(inPath))
            {
                string defaultPath = string.Format("{0}_%date%_%id%", Path.Combine(FStatic.LocalPath, FStatic.ModuleName));
                pathProp = FormatPath(defaultPath);
                //File.Create(pp.FullPath);
            }
            else
            {
                pathProp = FormatPath(inPath);
                if (string.IsNullOrEmpty(pathProp.FileName) && !string.IsNullOrEmpty(pathProp.FolderPath))
                {
                    if (!Directory.Exists(pathProp.FolderPath))
                        Directory.CreateDirectory(pathProp.FolderPath);
                    //string temp = pp.FolderPath + FStatic.ModuleName + _defaultExtension;
                    //if (!File.Exists(temp))
                       //File.Create(temp);
                }
                if (!Directory.Exists(pathProp.FolderPath) && !string.IsNullOrEmpty(pathProp.FolderPath))
                    Directory.CreateDirectory(pathProp.FolderPath);
                //if (!File.Exists(pp.FullPath))
                    //File.Create(pp.FullPath, 100 ,FileOptions.None);
            }
            return pathProp?.FullPath;
        }

        PathProperty FormatPath(string input)
        {
            string path = input;
            path = Regex.Replace(path, @"%yyyy%", DateTime.Now.Year.ToString(), RegexOptions.IgnoreCase);
            path = Regex.Replace(path, @"%mm%", DateTime.Now.Month.ToString("00"), RegexOptions.IgnoreCase);
            path = Regex.Replace(path, @"%dd%", DateTime.Now.Day.ToString("00"), RegexOptions.IgnoreCase);
            path = Regex.Replace(path, @"%hh%", DateTime.Now.Hour.ToString("00"), RegexOptions.IgnoreCase);
            path = Regex.Replace(path, @"%mi%", DateTime.Now.Minute.ToString("00"), RegexOptions.IgnoreCase);
            path = Regex.Replace(path, @"%ss%", DateTime.Now.Second.ToString("00"), RegexOptions.IgnoreCase);
            path = Regex.Replace(path, @"%ff%", DateTime.Now.Millisecond.ToString("000"), RegexOptions.IgnoreCase);
            path = Regex.Replace(path, @"%date%|%data%", DateTime.Now.ToString("yyyy.MM.dd"), RegexOptions.IgnoreCase);
            path = Regex.Replace(path, @"%datetime%", DateTime.Now.ToString("yyyy.MM.dd_HH.mm"), RegexOptions.IgnoreCase);
            path = Regex.Replace(path, string.Format(@"(\.{0})$", _suffix), string.Empty, RegexOptions.IgnoreCase);
            PathProperty pp = new PathProperty(path);
            if (Regex.IsMatch(pp.FullPath, "%id%", RegexOptions.IgnoreCase))
            {
                string pathWithId = string.Empty;
                int id = -1;
                while (true)
                {
                    id++;
                    pathWithId = Regex.Replace(pp.FullPath, @"%id%", id.ToString(), RegexOptions.IgnoreCase);
                    if (!File.Exists(string.Format(@"{0}.{1}", pathWithId, _suffix)))
                        break;
                }
                pp = new PathProperty(pathWithId);
                return pp;
            }
            return pp;
        }
        
        void TimerElapsed(object sender, EventArgs e)
        {
            WriteLog();
        }

        
        void WriteLog()
        {
            //если текущий экземпляр создан и спарсился путь записи лога в fullLogFilePath
            if (MemoryLog.Count > 0)
            {
                lock (MemoryLog)
                {
                    try
                    {
                        if (IsNewFileLog)
                        {
                            WriteHeader();
                            IsNewFileLog = false;
                        }
                        using (StreamWriter file = new StreamWriter(fullLogFilePath, true, _encoding))
                        {
                            foreach (string str in MemoryLog)
                            {
                                file.WriteLine(str);
                            }
                        }
                        _writeLogRetry = 0;
                        MemoryLog.Clear();
                    }
                    catch
                    {
                        _writeLogRetry++;
                        if (_writeLogRetry < 10)
                        {
                            Thread.Sleep(1000);
                            WriteLog();
                        }
                        else
                        {
                            MemoryLog.Clear();
                        }
                    }
                }
            }
        }

        
        void WriteHeader()
        {
            _tryLoad++;
            if (_tryLoad > 1)
            {
                if (File.Exists(fullLogFilePath) && (new FileInfo(fullLogFilePath).Length / 1024) <= SizeKbForSplit)
                    return;
                fullLogFilePath = GetFullPathAndCreate(string.Format("{0}{1}", SourceLogPath, (SourceLogPath.IndexOf("%id%", StringComparison.CurrentCultureIgnoreCase) == -1 && !string.IsNullOrEmpty(SourceLogPath)) ? "_%id%" : string.Empty));
            }
            else
            {
                fullLogFilePath = FullPathLogWrite;
            }
            string logHeader = string.Format(LogFormat("RootTrace", "TraceName", "Method"), "ID", "DateEvent", "Severity", "RootTrace", "TraceName", "Method", "Message");
            fullLogFilePath = string.Format("{0}.{1}", fullLogFilePath, _suffix);
            using (StreamWriter file = new StreamWriter(fullLogFilePath, File.Exists(fullLogFilePath), _encoding))
            {
                file.WriteLine(new string('=', 200));
                file.WriteLine(logHeader);
                file.WriteLine(new string('=', 200));
            }
        }

        string LogFormat(string root, string Base, string sub)
        {
            int lengthRoot = root.Length, lenghTraceName = Base.Length, lengthSubTrace = sub.Length;
            if (_two < lengthRoot) _two = lengthRoot;
            if (_three < lenghTraceName) _three = lenghTraceName;
            if (_four < lengthSubTrace) _four = lengthSubTrace;
            string result = "{0,-5} || {1,-26} || {2,-8} || {3,-%RootTrace%} || {4,-%TraceNameLength%} || {5,-%SubTraceNameLength%} || {6}"
                .Replace("%RootTrace%", (_two).ToString())
                .Replace("%TraceNameLength%", (_three).ToString())
                .Replace("%SubTraceNameLength%", (_four).ToString());
            return result;
        }
        bool CheckTraceName(string trace)
        {
            foreach (string s in _filterByTrace)
            {
                if (trace.IndexOf(s, StringComparison.CurrentCultureIgnoreCase) != -1)
                    return true;
            }
            return false;
        }

        public void AddLog(Log3NetSeverity severity, string rootTrace, string traceName, string subTraceName, object message)
        {
            if (_filterByTrace != null)
            {
                if (!CheckTraceName(string.Format("{0}.{1}", rootTrace, traceName)))
                    AddLog(severity.ToString("g"), DateTime.Now, rootTrace, traceName, subTraceName, ReturnTrimString(message.ToString()));
            }
            else
                AddLog(severity.ToString("g"), DateTime.Now, rootTrace, traceName, subTraceName, ReturnTrimString(message.ToString()));
        }
        string ReturnTrimString(string str)
        {
            return ProtasFunk.TrimString(str.Replace("\r", "").Replace("\n", ""));
        }

        void AddLog(string severityLog, DateTime dateEvent, string rootTrace, string traceName, string subTraceName, string message)
        {
            _logStep++;
            if (_logStep == ulong.MaxValue)
                _logStep = 0;
            string log = string.Format(LogFormat(rootTrace, traceName, subTraceName),
                        _logStep,
                        dateEvent.ToString("dd.MM.yyyy HH:mm:ss.ffffff"),
                        severityLog,
                        rootTrace,
                        traceName,
                        subTraceName,
                        message);
            lock (MemoryLog)
            {
                MemoryLog.Add(log);
            }
        }

       
        public static void AddEventLog(bool isCritical, string stringFormat, params object[] objects)
        {
            try
            {
                AddEventLog(isCritical, string.Format(stringFormat, objects));
            }
            catch(FormatException)
            {
                AddEventLog(isCritical, stringFormat + objects);
            }
        }
        public static void AddEventLog(bool isCritical, Exception ex)
        {
            AddEventLog(isCritical, string.Format("Catched Exception:\r\n{0}\r\n{1}\r\n{2}\r\n{3}\r\n{4}", ex.Message, ex.Source, ex.StackTrace, ex.InnerException, ex.Data));
        }
        
        public static void AddEventLog(bool isCritical, string log)
        {
            if (!EventLog.SourceExists(_eventLogType))
                EventLog.CreateEventSource(_eventLogType, "LogData");
            EventLog myLog = new EventLog
            {
                Source = _eventLogType,
                Log = _eventLogType
            };
            string evntLog = string.Format("[{0}] {1}", FStatic.ProgramName, log);
            myLog.WriteEntry(evntLog, EventLogEntryType.Error, 101, 0);
            if (isCritical)
                throw new Exception(log);
        }

        public void Dispose()
        {
           
        }


    }
}