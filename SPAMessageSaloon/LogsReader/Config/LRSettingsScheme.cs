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
        private string _fileTypes = "test";
        private string _orderBy = "Date, File, FoundLineID";
        private int _maxLines = 50;
        private int _maxThreads = -1;
        private int _rowsLimit = 999;
        private string _logsFolder = @"C:\TEST";
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
                    _logsFolder = @"C:\FORISLOG\MG";
                    _fileTypes = "crmcon,soapcon,smscon,ivrcon,emailcon,wcfhnd,dbcon,dispatcher";
                    _maxLines = 50;
                    _maxThreads = -1;
                    _traceParce = new LRTraceParse(_schemeName);
                    break;
                case "SPA":
                    _schemeName = name;
                    _servers = "spa-bpm1,spa-bpm2,spa-bpm3,spa-bpm4,spa-bpm5,spa-bpm6,spa-sa1,spa-sa2,spa-sa3,spa-sa4,spa-sa5,spa-sa6";
                    _logsFolder = @"C:\FORISLOG\SPA";
                    _fileTypes = "spa.bpm,bms,bsp,content,eir,am,scp,hlr,mca,mg,rbt,smsc";
                    _maxLines = 1;
                    _maxThreads = -1;
                    _orderBy = "Date desc, ID desc";
                    _traceParce = new LRTraceParse(_schemeName);
                    break;
                case "MGA":
                    _schemeName = name;
                    _servers = "crm-mg1,crm-mg2,crm-mg3,crm-mg4,crm-mg5";
                    _logsFolder = @"C:\FORISLOG\MGAdapter";
                    _fileTypes = "fast,slow,test";
                    _maxLines = 20000;
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
                    ReportMessage.Show(ex.Message, MessageBoxIcon.Error, $"Scheme=\"{Name}\" - @encoding");
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
            get => new XmlDocument().CreateComment(Resources.Txt_LRSettingsScheme_Servers);
            set { }
        }

        [XmlElement("Servers")]
        public string Servers
        {
            get => _servers;
            set => _servers = value.IsNullOrEmptyTrim() ? _servers : value;
        }

        [XmlAnyElement("LogsFolderComment")]
        public XmlComment LogsFolderComment
        {
            get => new XmlDocument().CreateComment(Resources.Txt_LRSettingsScheme_LogsDirectory);
            set { }
        }

        [XmlElement("LogsFolder")]
        public string LogsFolder
        {
            get => _logsFolder;
            set => _logsFolder = value.IsNullOrEmptyTrim() ? _logsFolder : value;
        }

        [XmlAnyElement("FileTypesComment")]
        public XmlComment FileTypesComment
        {
            get => new XmlDocument().CreateComment(Resources.Txt_LRSettingsScheme_Types);
            set { }
        }

        [XmlElement("FileTypes")]
        public string FileTypes
        {
            get => _fileTypes;
            set => _fileTypes = value.IsNullOrEmptyTrim() ? _fileTypes : value;
        }

        [XmlAnyElement("MaxLinesComment")]
        public XmlComment MaxLinesComment
        {
            get => new XmlDocument().CreateComment(Resources.Txt_LRSettingsScheme_MaxTraceLines);
            set { }
        }

        [XmlElement("MaxLines")]
        public int MaxLines
        {
            get => _maxLines;
            set => _maxLines = value <= 0 ? 1 : value;
        }

        [XmlAnyElement("MaxThreadsComment")]
        public XmlComment MaxThreadsComment
        {
            get => new XmlDocument().CreateComment(Resources.Txt_LRSettingsScheme_MaxThreads);
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
            get => new XmlDocument().CreateComment(Resources.Txt_LRSettingsScheme_RowsLimit);
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
            get => new XmlDocument().CreateComment(Resources.Txt_LRSettingsScheme_OrderBy);
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
                        ReportMessage.Show(Properties.Resources.Txt_LRSettingsScheme_ErrUnique, MessageBoxIcon.Error, $"Scheme=\"{Name}\" - OrderBy");
                    else
                        ReportStatus.Invoke(Properties.Resources.Txt_LRSettingsScheme_ErrUnique, ReportStatusType.Error);
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
                    throw new Exception(string.Format(Properties.Resources.Txt_LRSettingsScheme_ErrOrderBy, orderItem));

                result.Add(orderItem2, isDescending);
            }

            return result;
        }

        [XmlAnyElement("TraceParseComment")]
        public XmlComment TraceParseComment
        {
            get => new XmlDocument().CreateComment(Resources.Txt_LRSettingsScheme_TraceParse);
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
                    ReportStatus?.Invoke(string.Format(Properties.Resources.Txt_LRSettingsScheme_ErrRegex, Name), ReportStatusType.Error);
                    return false;
                }

                return true;
            }
        }
    }
}
