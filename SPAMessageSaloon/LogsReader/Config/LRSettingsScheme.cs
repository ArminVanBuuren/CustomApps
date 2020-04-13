using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using LogsReader.Properties;
using LogsReader.Reader;
using SPAMessageSaloon.Common;
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
        private int _rowsLimit = 999;
        private string _logsDirectory = @"C:\TEST";
        private LRTraceParse _traceParce = new LRTraceParse();

        public event ReportStatusHandler ReportStatus;

        public LRSettingsScheme() { }

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
                    _traceParce = new LRTraceParse(_schemeName);
                    break;
                case "SPA":
                    _schemeName = name;
                    _servers = "spa-bpm1,spa-bpm2,spa-bpm3,spa-bpm4,spa-bpm5,spa-bpm6,spa-sa1,spa-sa2,spa-sa3,spa-sa4,spa-sa5,spa-sa6";
                    _logsDirectory = @"C:\FORISLOG\SPA";
                    _types = "spa.bpm,bms,bsp,content,eir,am,scp,hlr,mca,mg,rbt,smsc";
                    _maxTraceLines = 1;
                    _maxThreads = -1;
                    _orderBy = "Date desc, ID desc";
                    _traceParce = new LRTraceParse(_schemeName);
                    break;
                case "MGA":
                    _schemeName = name;
                    _servers = "crm-mg1,crm-mg2,crm-mg3,crm-mg4,crm-mg5";
                    _logsDirectory = @"C:\FORISLOG\MGAdapter";
                    _types = "fast,slow,test";
                    _maxTraceLines = 20000;
                    _maxThreads = -1;
                    _traceParce = new LRTraceParse(_schemeName);
                    break;
            }
        }

        [XmlAttribute("name")]
        public string Name
        {
            get => _schemeName;
            set => _schemeName = value.IsNullOrEmptyTrim()
                ? _schemeName
                : Regex.Replace(value, @"\s+", "");
        }

        [XmlAttribute("encoding")]
        public string EncodingName
        {
            get => Encoding.HeaderName;
            set
            {
                if(value.IsNullOrEmptyTrim())
                    return;

                Encoding enc = null;
                try
                {
                    enc = Encoding.GetEncoding(value);
                }
                catch (Exception ex)
                {
                    ReportMessage.Show(ex.Message, MessageBoxIcon.Error, $"Scheme=\"{Name}\" - encoding");
                    return;
                }

                Encoding = enc;
            }
        }

        [XmlIgnore]
        public Encoding Encoding { get; private set; } = Encoding.GetEncoding("windows-1251");

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
            set
            {
                if (value.IsNullOrEmptyTrim())
                    return;

                Dictionary<string, bool> result = null;
                try
                {
                    result = CheckOrderByItem(value);
                }
                catch (ArgumentException)
                {
                    if (ReportStatus == null)
                        ReportMessage.Show("Columns must be unique!", MessageBoxIcon.Error, $"Scheme=\"{Name}\" - OrderBy");
                    else
                        ReportStatus.Invoke("Columns must be unique!", ReportStatusType.Error);
                    return;
                }
                catch (Exception ex)
                {
                    if (ReportStatus == null)
                        ReportMessage.Show(ex.Message, MessageBoxIcon.Error, $"Scheme=\"{Name}\" - OrderBy");
                    else
                        ReportStatus.Invoke(ex.Message, ReportStatusType.Error);
                    return;
                }

                OrderByItems = result;
                _orderBy = string.Join(", ", OrderByItems.Select(x => $"{x.Key}" + (x.Value ? " desc" : "")));
            }
        }

        [XmlIgnore]
        public Dictionary<string, bool> OrderByItems { get; private set; }

        internal static Dictionary<string, bool> CheckOrderByItem(string value)
        {
            var result = new Dictionary<string, bool>();
            foreach (var orderItem in value.Split(',').Where(x => !x.IsNullOrEmptyTrim()).Select(x => x.Trim()))
            {
                var orderStatement = orderItem.Split(' ');
                var isDescending = orderStatement.Length > 1 && orderStatement[1].Length > 0 && (orderStatement[1].LikeAny("desc", "descending"));
                
                if (!orderStatement[0].LikeAny(out var orderItem2, "FoundLineID", "ID", "Server", "TraceName", "Date", "File"))
                    throw new Exception($"OrderBy item '{orderItem}' is incorrect! Please check.");

                result.Add(orderItem2, isDescending);
            }

            return result;
        }

        [XmlAnyElement("TraceParseComment")]
        public XmlComment TraceParseComment
        {
            get => new XmlDocument().CreateComment(Resources.LRSettingsScheme_TraceParseComment);
            set { }
        }

        [XmlElement]
        public LRTraceParse TraceParse
        {
            get => _traceParce;
            set
            {
                if (value != null)
                {
                    if (value.Patterns.Length > 0)
                        _traceParce = value;
                }
            }
        }

        [XmlIgnore]
        public bool IsCorrect
        {
            get
            {
                if (!TraceParse.IsCorrectRegex)
                {
                    ReportStatus?.Invoke($"Scheme '{Name}' has incorrect Regex patterns in 'TraceParse' node. Please check.", ReportStatusType.Error);
                    return false;
                }

                return true;
            }
        }
    }
}
