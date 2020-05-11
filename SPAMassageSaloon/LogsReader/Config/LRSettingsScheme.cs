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
using SPAMassageSaloon.Common;
using Utils;

namespace LogsReader.Config
{
    public delegate void ReportStatusHandler(string message, ReportStatusType type);

    [XmlRoot("Scheme")]
    public class LRSettingsScheme
    {
	    private LRGroupItem[] _servers = new LRGroupItem[] { new LRGroupItem("local", "localhost") };
	    private LRGroupItem[] _fileTypes = new LRGroupItem[] {new LRGroupItem("type", "log") };
	    private LRFolder[] _logsFolder = new LRFolder[] { new LRFolder() };
        
	    private string _schemeName = "TEST";
        private string _orderBy = "Date, File, FoundLineID";
        private int _maxLines = 50;
        private int _maxThreads = -1;
        private int _rowsLimit = 999;
        
        private LRTraceParse _traceParce = new LRTraceParse();

        public event ReportStatusHandler ReportStatus;

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

                Encoding enc;
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
        public LRGroupItem[] Servers
        {
	        get => _servers;
	        set
	        {
		        try
		        {
			        _servers = value ?? _servers;
			        ServerGroups = GetGroups(_servers);
                }
		        catch (ArgumentException ex)
		        {
			        throw new ArgumentException(Resources.Txt_ServerGroups_ErrUnique, ex);
		        }
	        }
        }

        [XmlAnyElement("FileTypesComment")]
        public XmlComment FileTypesComment
        {
            get => new XmlDocument().CreateComment(Resources.Txt_LRSettingsScheme_Types);
            set { }
        }

        [XmlElement("FileTypes")]
        public LRGroupItem[] FileTypes
        {
	        get => _fileTypes;
	        set
	        {
		        try
		        {
			        _fileTypes = value ?? _fileTypes;
			        FileTypesGroups = GetGroups(_fileTypes);
                }
		        catch (ArgumentException ex)
		        {
			        throw new ArgumentException(Resources.Txt_FileTypesGroups_ErrUnique, ex);
		        }
	        }
        }

        static Dictionary<string, IEnumerable<string>> GetGroups(LRGroupItem[] items)
        {
	        return items.ToDictionary(k => k.GroupName, v => v.Item[0].Value.Split(',')
		        .GroupBy(p => p.Trim(), StringComparer.InvariantCultureIgnoreCase)
		        .OrderBy(p => p.Key)
		        .Select(x => x.Key), StringComparer.InvariantCultureIgnoreCase);
        }

        [XmlAnyElement("LogsFolderComment")]
        public XmlComment LogsFolderComment
        {
	        get => new XmlDocument().CreateComment(Resources.Txt_LRSettingsScheme_LogsDirectory);
	        set { }
        }

        [XmlElement("LogsFolderGroup")]
        public LRFolder[] LogsFolder
        {
	        get => _logsFolder;
	        set
	        {
		        try
		        {
			        _logsFolder = value ?? _logsFolder;
			        Folders = _logsFolder.ToDictionary(x => x.Item[0].Value.Trim(), x => x.AllDirectoriesSearching, StringComparer.InvariantCultureIgnoreCase);
                }
		        catch (ArgumentException ex)
		        {
			        throw new ArgumentException(Resources.Txt_Folders_ErrUnique, ex);
		        }
	        }
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

                Dictionary<string, bool> result;
                try
                {
                    result = CheckOrderByItem(value);
                }
                catch (ArgumentException)
                {
                    if (ReportStatus == null)
                        ReportMessage.Show(Resources.Txt_LRSettingsScheme_ErrUnique, MessageBoxIcon.Error, $"Scheme=\"{Name}\" - OrderBy");
                    else
                        ReportStatus.Invoke(Resources.Txt_LRSettingsScheme_ErrUnique, ReportStatusType.Error);
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
                    throw new Exception(string.Format(Resources.Txt_LRSettingsScheme_ErrOrderBy, orderItem));

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
                    ReportStatus?.Invoke(string.Format(Resources.Txt_LRSettingsScheme_ErrRegex, Name), ReportStatusType.Error);
                    return false;
                }

                return true;
            }
        }

        public LRSettingsScheme() { }

        [XmlIgnore] public Dictionary<string, IEnumerable<string>> ServerGroups { get; private set; }
        [XmlIgnore] public Dictionary<string, IEnumerable<string>> FileTypesGroups { get; private set; }
        [XmlIgnore] public Dictionary<string, bool> Folders { get; private set; }

        internal LRSettingsScheme(string schemeName)
        {
	        switch (schemeName)
	        {
		        case "MG":
			        _schemeName = schemeName;
			        _servers = new[] { new LRGroupItem("UZ", "mg1, mg2, mg3, mg4, mg5") };
			        _fileTypes = new[] { new LRGroupItem("default", "crmcon, soapcon, smscon, ivrcon, emailcon, wcfhnd, dbcon, dispatcher") };
			        _logsFolder = new[] { new LRFolder(@"C:\FORISLOG\MG", true) };
			        _maxLines = 100;
			        _maxThreads = -1;
			        _traceParce = new LRTraceParse(_schemeName);
			        break;
		        case "SPA":
			        _schemeName = schemeName;
			        _servers = new[] { new LRGroupItem("UZ", "spa-bpm1, spa-bpm2, spa-bpm3, spa-bpm4, spa-bpm5, spa-bpm6, spa-sa1, spa-sa2, spa-sa3, spa-sa4, spa-sa5, spa-sa6") };
			        _fileTypes = new[] { new LRGroupItem("default", "spa.bpm, bms, bsp, content, eir, am, scp, hlr, mca, mg, rbt, smsc") };
			        _logsFolder = new[] { new LRFolder(@"C:\FORISLOG\SPA", true) };
			        _maxLines = 1;
			        _maxThreads = -1;
			        _orderBy = "Date desc, ID desc";
			        _traceParce = new LRTraceParse(_schemeName);
			        break;
		        case "MGA":
			        _schemeName = schemeName;
			        _servers = new[] { new LRGroupItem("UZ", "crm-mg1, crm-mg2, crm-mg3, crm-mg4, crm-mg5") };
			        _fileTypes = new[] { new LRGroupItem("default", "fast, slow, test") };
			        _logsFolder = new[] { new LRFolder(@"C:\FORISLOG\MGAdapter", true) };
			        _maxLines = 20000;
			        _maxThreads = -1;
			        _traceParce = new LRTraceParse(_schemeName);
			        break;
	        }
        }

        void Load()
        {

        }

        public override string ToString()
        {
            return Name;
        }
    }
}
