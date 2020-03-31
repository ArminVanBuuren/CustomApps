using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using LogsReader.Properties;
using LogsReader.Reader;
using Utils;

namespace LogsReader.Config
{
    public delegate void ReportStatusHandler(string message, ReportStatusType type);

    [XmlRoot("Scheme")]
    public class LRSettingsScheme
    {
        private string _schemeName = "TEST";
        private string _servers = "localhost";
        private string _types = "test";
        private string _orderBy = "Date, File, FoundLineID";
        private int _maxTraceLines = 50;
        private int _maxThreads = -1;
        private int _rowsLimit = 9999;
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
                    _logsDirectory = @"C:\FORISLOG\MG";
                    _types = "crmcon,soapcon,smscon,ivrcon,emailcon,wcfhnd,dbcon,dispatcher";
                    _maxTraceLines = 50;
                    _maxThreads = -1;
                    _traceLinePattern = new LRTraceLinePattern(_schemeName);
                    break;
                case "SPA":
                    _schemeName = name;
                    _servers = "spa-bpm1,spa-bpm2,spa-bpm3,spa-bpm4,spa-bpm5,spa-bpm6,spa-sa1,spa-sa2,spa-sa3,spa-sa4,spa-sa5,spa-sa6";
                    _logsDirectory = @"C:\FORISLOG\SPA";
                    _types = "spa.bpm,bms,bsp,content,eir,am,scp,hlr,mca,mg,rbt,smsc";
                    _maxTraceLines = 1;
                    _maxThreads = -1;
                    _orderBy = "Date desc, ID";
                    _traceLinePattern = new LRTraceLinePattern(_schemeName);
                    break;
                case "MGA":
                    _schemeName = name;
                    _servers = "crm-mg1,crm-mg2,crm-mg3,crm-mg4,crm-mg5";
                    _logsDirectory = @"C:\FORISLOG\MGAdapter";
                    _types = "fast,slow,test";
                    _maxTraceLines = 20000;
                    _maxThreads = -1;
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

        [XmlAnyElement("RowsLimitComment")]
        public XmlComment RowsLimitComment
        {
            get => new XmlDocument().CreateComment(Resources.LRSettingsScheme_RowsLimitCommentComment);
            set { }
        }

        [XmlElement("RowsLimit")]
        public int RowsLimit
        {
            get => _rowsLimit;
            set => _rowsLimit = value <= 0 ? 9999 : value;
        }


        [XmlAnyElement("OrderByComment")]
        public XmlComment OrderByComment
        {
            get => new XmlDocument().CreateComment(Resources.LRSettingsScheme_OrderByComment);
            set { }
        }

        [XmlElement("OrderBy")]
        public string OrderBy
        {
            get => _orderBy;
            set => _orderBy = value.IsNullOrEmptyTrim() ? _orderBy : value;
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

        [XmlIgnore]
        public bool IsCorrect
        {
            get
            {
                if (!TraceLinePattern.IsCorrectRegex)
                {
                    ReportStatus?.Invoke($"Scheme '{Name}' has incorrect Regex pattern in 'TraceLinePattern' node. Please check.", ReportStatusType.Error);
                    return false;
                }

                return true;
            }
        }
    }
}
