using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using LogsReader.Properties;
using Utils;
using Utils.Handles;
using static Utils.ASSEMBLY;

namespace LogsReader
{
    [Serializable]
    public class UserSettings : IDisposable
    {
        private RegeditControl parentRegistry;

        public UserSettings(string schemeName)
        {
            try
            {
                Scheme = schemeName;
                var userName = Environment.UserName.Replace(Environment.NewLine, string.Empty).Replace(" ", string.Empty);
                UserName = Path.GetInvalidFileNameChars().Aggregate(userName, (current, ch) => current.Replace(ch, '\0'));
                parentRegistry = new RegeditControl(ApplicationName);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public string Scheme { get; }
        public string UserName { get; }

        public string PreviousSearch
        {
            get => GetValue(nameof(PreviousSearch));
            set => SetValue(nameof(PreviousSearch), value);
        }

        public bool UseRegex
        {
            get
            {
                var resStr = GetValue(nameof(UseRegex));
                if (resStr.IsNullOrEmptyTrim() || !bool.TryParse(resStr, out var res))
                    return true;
                return res;
            }
            set => SetValue(nameof(UseRegex), value);
        }

        public string TraceLike
        {
            get => GetValue(nameof(TraceLike));
            set => SetValue(nameof(TraceLike), value);
        }

        public string TraceNotLike
        {
            get => GetValue(nameof(TraceNotLike));
            set => SetValue(nameof(TraceNotLike), value);
        }

        public string Message
        {
            get => GetValue(nameof(Message));
            set => SetValue(nameof(Message), value);
        }

        string GetValue(string name)
        {
            if (parentRegistry == null)
                return string.Empty;

            try
            {
                using (var schemeControl = new RegeditControl(Scheme, parentRegistry))
                {
                    return (string) schemeControl[$"{UserName}_{name}"] ?? string.Empty;
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        void SetValue(string name, object value)
        {
            if (parentRegistry == null)
                return;

            try
            {
                using (var schemeControl = new RegeditControl(Scheme, parentRegistry))
                {
                    schemeControl[$"{UserName}_{name}"] = value;
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void Dispose()
        {
            parentRegistry?.Dispose();
        }
    }

    [Serializable, XmlRoot("Settings")]
    public class LRSettings
    {
        private static object _sync = new object();
        static string SettingsPath => $"{ApplicationFilePath}.xml";
        static string IncorrectSettingsPath => $"{SettingsPath}_incorrect.bak";

        private LRSettingsScheme[] _schemes = new[] {new LRSettingsScheme("MG"), new LRSettingsScheme("SPA")};

        [XmlIgnore]
        public Dictionary<string, LRSettingsScheme> Schemes { get; private set; }

        [XmlAnyElement(nameof(Resources.LRSettings_PreviousSearchComment))]
        public XmlComment PreviousSearchComment
        {
            get => new XmlDocument().CreateComment(Resources.LRSettings_UseRegexComment);
            set { }
        }


        [XmlElement("Scheme", IsNullable = false)]
        public LRSettingsScheme[] SchemeList
        {
            get => _schemes;
            set
            {
                try
                {
                    _schemes = value ?? _schemes;
                    Schemes = _schemes != null && _schemes.Length > 0
                        ? _schemes.ToDictionary(k => k.Name, v => v, StringComparer.CurrentCultureIgnoreCase)
                        : new Dictionary<string, LRSettingsScheme>(StringComparer.CurrentCultureIgnoreCase);
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException("Schemes names must be unique!", ex);
                }
            }
        }

        public LRSettings()
        {
            Schemes = SchemeList != null && SchemeList.Length > 0
                ? SchemeList.ToDictionary(k => k.Name, v => v, StringComparer.CurrentCultureIgnoreCase)
                : new Dictionary<string, LRSettingsScheme>(StringComparer.CurrentCultureIgnoreCase);
        }

        public static async Task SerializeAsync(LRSettings settings)
        {
            await Task.Factory.StartNew((input) => Serialize((LRSettings) input), settings);
        }

        public static void Serialize(LRSettings settings)
        {
            try
            {
                lock (_sync)
                {
                    if (File.Exists(SettingsPath))
                        File.SetAttributes(SettingsPath, FileAttributes.Normal);

                    var xml = new XmlSerializer(typeof(LRSettings));
                    using (StreamWriter sw = new StreamWriter(SettingsPath, false, new UTF8Encoding(false)))
                    {
                        xml.Serialize(sw, settings);
                    }
                }
            }
            catch (Exception ex)
            {
                Program.MessageShow(string.Format(Resources.LRSettings_Serialize_Ex, SettingsPath, ex.Message), "Serialize Error");
            }
        }

        public static LRSettings Deserialize()
        {
            LRSettings sett = null;

            try
            {
                if (File.Exists(SettingsPath))
                {
                    using (StreamReader stream = new StreamReader(SettingsPath, new UTF8Encoding(false)))
                    using (TextReader sr = new StringReader(XML.RemoveUnallowable(stream.ReadToEnd(), true)))
                        sett = new XmlSerializer(typeof(LRSettings)).Deserialize(sr) as LRSettings;
                }
            }
            catch (Exception ex)
            {
                var message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Program.MessageShow(string.Format(Resources.LRSettings_Deserialize_Ex,
                    Path.GetFileName(SettingsPath),
                    Path.GetFileName(IncorrectSettingsPath),
                    message), "Deserialize Error");

                if (File.Exists(IncorrectSettingsPath))
                {
                    File.SetAttributes(IncorrectSettingsPath, FileAttributes.Normal);
                    File.Delete(IncorrectSettingsPath);
                }

                File.SetAttributes(SettingsPath, FileAttributes.Normal);
                File.Copy(SettingsPath, IncorrectSettingsPath);
                File.Delete(SettingsPath);
            }

            return sett ?? new LRSettings();
        }
    }

    [XmlRoot("Scheme")]
    public class LRSettingsScheme
    {
        private string _schemeName = "TEST";
        private string _servers = "localhost";
        private string _types = "test";
        private int _maxThreads = -1;
        private int _maxTraceLines = 50;
        private string _logsDirectory = @"C:\TEST";
        private TraceLinePattern _traceLinePattern = new TraceLinePattern();

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
                    _traceLinePattern = new TraceLinePattern(_schemeName);
                    break;
                case "SPA":
                    _schemeName = name;
                    _servers = "spa-bpm1,spa-bpm2,spa-bpm3,spa-bpm4,spa-bpm5,spa-bpm6,spa-sa1,spa-sa2,spa-sa3,spa-sa4,spa-sa5,spa-sa6";
                    _types = "spa.bpm,bms,bsp,content,eir,am,scp,hlr,mca,mg,rbt,smsc";
                    _maxThreads = -1;
                    _maxTraceLines = 50;
                    _logsDirectory = @"C:\FORISLOG\SPA";
                    _traceLinePattern = new TraceLinePattern(_schemeName);
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
        public TraceLinePattern TraceLinePattern
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

        public bool IsLineMatch(string message, FileLog fileLog, out DataTemplate result)
        {
            foreach (var item in TraceLinePattern.Items)
            {
                var match = item.RegexItem.Match(message);
                if (match.Success)
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
            foreach (var regexPatt in TraceLinePattern.Items.Select(x => x.RegexItem))
            {
                var match = regexPatt.Match(input);
                if (match.Success)
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

    [XmlRoot("TraceLinePattern")]
    public class TraceLinePattern
    {
        private TraceLinePatternItem[] _traceLinePattern = new TraceLinePatternItem[] { new TraceLinePatternItem() };

        public TraceLinePattern()
        {

        }

        internal TraceLinePattern(string name)
        {
            switch (name)
            {
                case "MG":
                    Items = new[]
                    {
                        new TraceLinePatternItem(@"(.+?)\s*(\[.+?\])\s*(.*?)(\<.+\>).*") {Date = "$1", Trace = "$2", Description = "$3", Message = "$4"},
                        new TraceLinePatternItem(@"(.+?)\s*(\[.+?\])\s*(.+?)\s+(.+)") {Date = "$1", Trace = "$2", Description = "$3", Message = "$4"},
                        new TraceLinePatternItem(@"(.+?)\s*(\[.+?\])\s*(.+)") {Date = "$1", Trace = "$2", Message = "$3"}
                    };
                    break;
                case "SPA":
                    Items = new[]
                    {
                        new TraceLinePatternItem(@"(\d+?)\u0001(.+?)\u0001(.+?)\u0001(.+?)\u0001(.*?)\u0001(\d*)")
                            {ID = "$1", Trace = "$2", Description = "$3", Date = "$4.$6", Message = "$5"}
                    };
                    break;
            }
        }

        [XmlIgnore]
        internal bool IsCorrectRegex => _traceLinePattern != null && _traceLinePattern.Length > 0 && _traceLinePattern.All(x => x.IsCorrectRegex);

        [XmlElement("Item")]
        public TraceLinePatternItem[] Items
        {
            get => _traceLinePattern;
            set
            {
                if (value != null)
                {
                    if (value.Length > 0)
                        _traceLinePattern = value;
                }
            }
        }
    }

    [XmlRoot("Item")]
    public class TraceLinePatternItem
    {
        private XmlNode[] _item = new XmlNode[] { new XmlDocument().CreateCDataSection("(.+)") };

        public TraceLinePatternItem()
        {
            CDataContent = _item;
        }

        internal TraceLinePatternItem(string regexPattern)
        {
            CDataContent = new XmlNode[] {new XmlDocument().CreateCDataSection(regexPattern)};
        }


        [XmlText]
        public XmlNode[] CDataContent
        {
            get => _item;
            set
            {
                IsCorrectRegex = false;

                if (value != null)
                {
                    if (value.Length > 0)
                    {
                        if (value[0].NodeType == XmlNodeType.CDATA)
                            _item = value;
                        else
                            _item = new XmlNode[] {new XmlDocument().CreateCDataSection(value[0].Value)};
                    }
                }

                if (_item == null || _item.Length == 0)
                    return;

                var text = _item[0].Value.ReplaceUTFCodeToSymbol();
                if (!REGEX.Verify(text))
                {
                    Program.MessageShow($"Pattern \"{text}\" is incorrect", "TraceLinePattern Reader");
                    return;
                }
                else
                {
                    RegexItem = new Regex(text, RegexOptions.Compiled | RegexOptions.Singleline);
                    GroupNames = RegexItem.GetGroupNames();
                }

                IsCorrectRegex = true;
            }
        }

        [XmlIgnore] internal bool IsCorrectRegex { get; set; }

        [XmlIgnore]
        public Regex RegexItem { get; private set; }

        [XmlIgnore]
        public string[] GroupNames { get; private set; }

        [XmlAttribute] public string ID { get; set; } = string.Empty;
        [XmlAttribute] public string Date { get; set; } = string.Empty;
        [XmlAttribute] public string Trace { get; set; } = string.Empty;
        [XmlAttribute] public string Description { get; set; } = string.Empty;
        [XmlAttribute] public string Message { get; set; } = string.Empty;
    }
}