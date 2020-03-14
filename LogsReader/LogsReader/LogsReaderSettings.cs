using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using LogsReader.Properties;
using Utils;
using static Utils.ASSEMBLY;

namespace LogsReader
{
    public delegate void CatchWaringHandler(string message, bool isError);
    [Serializable, XmlRoot("Settings")]
    public class LRSettings
    {
        private static object _sync = new object();
        private LRSettingsScheme[] _schemes = new[] { new LRSettingsScheme() };
        static string SettingsPath => $"{ApplicationFilePath}.xml";
        static string IncorrectSettingsPath => $"{SettingsPath}_incorrect.bak";
        private bool _useRegex = true;
        XmlNode[] _previousSearch =
        {
            new XmlDocument().CreateCDataSection(string.Empty)
        };

        [XmlAnyElement(nameof(Resources.LRSettings_PreviousSearchComment))]
        public XmlComment PreviousSearchComment { get => new XmlDocument().CreateComment(Resources.LRSettings_PreviousSearchComment + Resources.LRSettings_UseRegexComment); set { } }

        [XmlElement("PreviousSearch")]
        public XmlNode[] PreviousSearch
        {
            get => _previousSearch;
            set
            {
                if (value != null && value.Length > 0)
                    _previousSearch = value;
            }
        }

        [XmlAttribute("UseRegex")]
        public bool UseRegex
        {
            get => _useRegex;
            set => _useRegex = value;
        }

        [XmlIgnore]
        public Dictionary<string, LRSettingsScheme> Schemes { get; private set; }

        [XmlElement("Scheme", IsNullable = false)]
        public LRSettingsScheme[] SchemeList
        {
            get => _schemes;
            set
            {
                _schemes = value ?? _schemes;
                Schemes = _schemes.Length > 0 ? _schemes.ToDictionary(k => k.Name, v => v) : new Dictionary<string, LRSettingsScheme>();
            }
        }

        public LRSettings()
        {
            Schemes = SchemeList.Length > 0 ? SchemeList.ToDictionary(k => k.Name, v => v) : new Dictionary<string, LRSettingsScheme>();
        }

        public static async Task SerializeAsync(LRSettings settings)
        {
            await Task.Factory.StartNew((input) => Serialize((LRSettings)input), settings);
        }

        public static void Serialize(LRSettings settings)
        {
            try
            {
                lock (_sync)
                {
                    if (File.Exists(SettingsPath))
                        File.SetAttributes(SettingsPath, FileAttributes.Normal);
                    using (var stream = new FileStream(SettingsPath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        new XmlSerializer(typeof(LRSettings)).Serialize(stream, settings);
                    }
                }
            }
            catch (Exception ex)
            {
                
                MessageBox.Show(string.Format(Resources.LRSettings_Serialize_Ex, SettingsPath, ex.Message));
            }
        }

        public static LRSettings Deserialize()
        {
            LRSettings sett = null;

            try
            {
                if (File.Exists(SettingsPath))
                {
                    using (var stream = new FileStream(SettingsPath, FileMode.Open, FileAccess.Read))
                    {
                        sett = new XmlSerializer(typeof(LRSettings)).Deserialize(stream) as LRSettings;
                    }
                }
            }
            catch (Exception ex)
            {
                if (File.Exists(IncorrectSettingsPath))
                {
                    File.SetAttributes(IncorrectSettingsPath, FileAttributes.Normal);
                    File.Delete(IncorrectSettingsPath);
                }

                File.SetAttributes(SettingsPath, FileAttributes.Normal);
                File.Copy(SettingsPath, IncorrectSettingsPath);
                File.Delete(SettingsPath);

                MessageBox.Show(string.Format(Resources.LRSettings_Deserialize_Ex, SettingsPath, IncorrectSettingsPath, ex.Message));
            }

            return sett ?? new LRSettings();
        }
    }

    [XmlRoot("Scheme")]
    public class LRSettingsScheme
    {
        public event CatchWaringHandler CatchWaring;
        private string _schemeName = "MG";
        private string _servers = "mg1,mg2,mg3,mg4";
        private string _types = "crm,soap,sms,ivr,db,dispatcher";
        private int _maxThreads = -1;
        private int _maxTraceLines = 10;
        private string _logsDirectory = @"C$\FORISLOG\MG";
        XmlNode[] _traceLinePattern =
        {
            new XmlDocument().CreateCDataSection(@"(?<Date>.+?)\s*(?<TraceType>\[.+?\])\s*(?<Message>.+)")
        };

        [XmlAttribute]
        public string Name
        {
            get => _schemeName;
            set => _schemeName = value ?? _schemeName;
        }

        [XmlAnyElement("ServersComment")]
        public XmlComment ServersComment { get => new XmlDocument().CreateComment(Resources.LRSettingsScheme_ServersComment); set { } }

        [XmlElement("Servers")]
        public string Servers
        {
            get => _servers;
            set => _servers = value ?? _servers;
        }

        [XmlAnyElement("TypesComment")]
        public XmlComment TypesComment { get => new XmlDocument().CreateComment(Resources.LRSettingsScheme_TypesComment); set { } }

        [XmlElement("Types")]
        public string Types
        {
            get => _types;
            set => _types = value ?? _types;
        }

        [XmlAnyElement("MaxThreadsComment")]
        public XmlComment MaxThreadsComment { get => new XmlDocument().CreateComment(Resources.LRSettingsScheme_MaxThreadsComment); set { } }

        [XmlElement("MaxThreads")]
        public int MaxThreads
        {
            get => _maxThreads;
            set => _maxThreads = value <= 0 ? -1 : value;
        }

        [XmlAnyElement("LogsDirectoryComment")]
        public XmlComment LogsDirectoryComment { get => new XmlDocument().CreateComment(Resources.LRSettingsScheme_LogsDirectoryComment); set { } }

        [XmlElement("LogsDirectory")]
        public string LogsDirectory
        {
            get => _logsDirectory;
            set => _logsDirectory = value ?? _logsDirectory;
        }

        [XmlAnyElement("MaxTraceLinesComment")]
        public XmlComment MaxTraceLinesComment { get => new XmlDocument().CreateComment(Resources.LRSettingsScheme_MaxTraceLinesComment); set { } }

        [XmlElement("MaxTraceLines")]
        public int MaxTraceLines
        {
            get => _maxTraceLines;
            set => _maxTraceLines = value <= 0 ? 1 : value;
        }

        [XmlAnyElement("TraceLinePatternComment")]
        public XmlComment TraceLinePatternComment { get => new XmlDocument().CreateComment(Resources.LRSettingsScheme_TraceLinePatternComment); set { } }

        [XmlElement]
        public XmlNode[] TraceLinePattern
        {
            get => _traceLinePattern;
            set
            {
                if (value != null && value.Length > 0 && REGEX.Verify(value[0].Value))
                    _traceLinePattern = value;

                TraceLinePatternRegex = new Regex(_traceLinePattern[0].Value, RegexOptions.Compiled | RegexOptions.Singleline);   
            }
        }

        [XmlIgnore]
        public Regex TraceLinePatternRegex { get; private set; }

        [XmlIgnore]
        public bool IsCorrect
        {
            get
            {
                if (TraceLinePatternRegex == null)
                    return false;

                var groups = TraceLinePatternRegex.GetGroupNames();
                if (groups.All(x => x != "Date"))
                {
                    CatchWaring?.Invoke($"Scheme '{Name}' is incorrect. Not found group '?<Date>' in TraceLinePattern", true);
                    return false;
                }
                if (groups.All(x => x != "TraceType"))
                {
                    CatchWaring?.Invoke($"Scheme '{Name}' is incorrect. Not found group '?<TraceType>' in TraceLinePattern", true);
                    return false;
                }
                if (groups.All(x => x != "Message"))
                {
                    CatchWaring?.Invoke($"Scheme '{Name}' is incorrect. Not found group '?<Message>' in TraceLinePattern", true);
                    return false;
                }

                return true;
            }
        }

        public LRSettingsScheme()
        {
            TraceLinePatternRegex = new Regex(_traceLinePattern[0].Value, RegexOptions.Compiled | RegexOptions.Singleline);
        }
    }
}
