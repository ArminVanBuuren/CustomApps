using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using LogsReader.Data;
using LogsReader.Properties;
using Utils;

namespace LogsReader.Config
{
    [XmlRoot("Scheme")]
    public class LRSettingsScheme
    {
        private string _schemeName = "TEST";
        private string _servers = "localhost";
        private string _types = "test";
        private int _maxThreads = -1;
        private int _maxTraceLines = 50;
        private string _logsDirectory = @"C:\TEST";
        private LRTraceLinePattern _traceLinePattern = new LRTraceLinePattern();

        public event ReportStatusHandler ReportStatus;

        public LRSettingsScheme()
        {

        }

        internal LRSettingsScheme(string name)
        {
            switch (name)
            {
                case "MG":
                    _schemeName = name;
                    _servers = "mg1,mg2,mg3,mg4,mg5";
                    _types = "crm,soap,sms,ivr,email,wcf,dispatcher";
                    _maxThreads = -1;
                    _maxTraceLines = 50;
                    _logsDirectory = @"C:\FORISLOG\MG";
                    _traceLinePattern = new LRTraceLinePattern(_schemeName);
                    break;
                case "SPA":
                    _schemeName = name;
                    _servers = "spa-bpm1,spa-bpm2,spa-bpm3,spa-bpm4,spa-bpm5,spa-bpm6,spa-sa1,spa-sa2,spa-sa3,spa-sa4,spa-sa5,spa-sa6";
                    _types = "spa.bpm,bms,bsp,content,eir,am,scp,hlr,mca,mg,rbt,smsc";
                    _maxThreads = -1;
                    _maxTraceLines = 20;
                    _logsDirectory = @"C:\FORISLOG\SPA";
                    _traceLinePattern = new LRTraceLinePattern(_schemeName);
                    break;
                case "MGA":
                    _schemeName = name;
                    _servers = "crm-mg1,crm-mg2,crm-mg3,crm-mg4,crm-mg5";
                    _types = "fast,slow,test";
                    _maxThreads = -1;
                    _maxTraceLines = 300;
                    _logsDirectory = @"C:\FORISLOG\MGAdapter";
                    _traceLinePattern = new LRTraceLinePattern(_schemeName);
                    break;
            }
        }

        [XmlAttribute]
        public string Name
        {
            get => _schemeName;
            set => _schemeName = value.IsNullOrEmptyTrim()
                ? _schemeName
                : Regex.Replace(value, @"\s+", "");
        }

        [XmlAnyElement("ServersComment")]
        public XmlComment ServersComment
        {
            get => new XmlDocument().CreateComment(Resources.LRSettingsScheme_ServersComment);
            set { }
        }

        [XmlElement("Servers")]
        public string Servers
        {
            get => _servers;
            set => _servers = value.IsNullOrEmptyTrim() ? _servers : value;
        }

        [XmlAnyElement("LogsDirectoryComment")]
        public XmlComment LogsDirectoryComment
        {
            get => new XmlDocument().CreateComment(Resources.LRSettingsScheme_LogsDirectoryComment);
            set { }
        }

        [XmlElement("LogsDirectory")]
        public string LogsDirectory
        {
            get => _logsDirectory;
            set => _logsDirectory = value.IsNullOrEmptyTrim() ? _logsDirectory : value;
        }

        [XmlAnyElement("TypesComment")]
        public XmlComment TypesComment
        {
            get => new XmlDocument().CreateComment(Resources.LRSettingsScheme_TypesComment);
            set { }
        }

        [XmlElement("Types")]
        public string Types
        {
            get => _types;
            set => _types = value.IsNullOrEmptyTrim() ? _types : value;
        }

        [XmlAnyElement("MaxTraceLinesComment")]
        public XmlComment MaxTraceLinesComment
        {
            get => new XmlDocument().CreateComment(Resources.LRSettingsScheme_MaxTraceLinesComment);
            set { }
        }

        [XmlElement("MaxTraceLines")]
        public int MaxTraceLines
        {
            get => _maxTraceLines;
            set => _maxTraceLines = value <= 0 ? 1 : value;
        }

        [XmlAnyElement("MaxThreadsComment")]
        public XmlComment MaxThreadsComment
        {
            get => new XmlDocument().CreateComment(Resources.LRSettingsScheme_MaxThreadsComment);
            set { }
        }

        [XmlElement("MaxThreads")]
        public int MaxThreads
        {
            get => _maxThreads;
            set => _maxThreads = value <= 0 ? -1 : value;
        }

        [XmlAnyElement("TraceLinePatternComment")]
        public XmlComment TraceLinePatternComment
        {
            get => new XmlDocument().CreateComment(Resources.LRSettingsScheme_TraceLinePatternComment);
            set { }
        }

        [XmlElement]
        public LRTraceLinePattern TraceLinePattern
        {
            get => _traceLinePattern;
            set
            {
                if (value != null)
                {
                    if (value.Items.Length > 0)
                        _traceLinePattern = value;
                }
            }
        }

        public bool IsLineMatch(string input, FileLog fileLog, out DataTemplate result)
        {
            // замена \r чинит баг с корректным парсингом
            var message = input.Replace("\r", string.Empty);
            foreach (var item in TraceLinePattern.Items)
            {
                var match = item.RegexItem.Match(message);
                if (match.Success && match.Value.Length == message.Length)
                {
                    result = new DataTemplate(fileLog,
                        match.GetValueByReplacement(item.ID),
                        match.GetValueByReplacement(item.Date),
                        match.GetValueByReplacement(item.Trace),
                        match.GetValueByReplacement(item.Description),
                        match.GetValueByReplacement(item.Message),
                        message);
                    return true;
                }
            }

            result = new DataTemplate(fileLog, message);
            return false;
        }


        public Match IsMatch(string input)
        {
            // замена \r чинит баг с корректным парсингом
            var message = input.Replace("\r", string.Empty);
            foreach (var regexPatt in TraceLinePattern.Items.Select(x => x.RegexItem))
            {
                var match = regexPatt.Match(message);
                if (match.Success && match.Value.Length == message.Length)
                    return match;
            }

            return null;
        }

        [XmlIgnore]
        public bool IsCorrect
        {
            get
            {
                if (!TraceLinePattern.IsCorrectRegex)
                {
                    ReportStatus?.Invoke($"Scheme '{Name}' has incorrect Regex pattern in 'TraceLinePattern' node. Please check.", true);
                    return false;
                }

                return true;
            }
        }
    }
}
