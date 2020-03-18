﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        private LRSettingsScheme[] _schemes = new[] { new LRSettingsScheme("MG"), new LRSettingsScheme("SPA") };
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

                    var xml = new XmlSerializer(typeof(LRSettings));
                    using (StreamWriter sw = new StreamWriter(SettingsPath, false, new UTF8Encoding(false)))
                    {
                        xml.Serialize(sw, settings);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Resources.LRSettings_Serialize_Ex, SettingsPath, ex.Message), "Serialize Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    using (TextReader sr = new StringReader(stream.ReadToEnd().ReplaceUnacceptableUTFToCode()))
                        sett = new XmlSerializer(typeof(LRSettings)).Deserialize(sr) as LRSettings;
                }
            }
            catch (Exception ex)
            {
                var message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show(string.Format(Resources.LRSettings_Deserialize_Ex,
                        Path.GetFileName(SettingsPath),
                        Path.GetFileName(IncorrectSettingsPath),
                        message),
                    "Deserialize Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

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
        public event CatchWaringHandler CatchWaring;

        private string _schemeName = "TEST";
        private string _servers = "localhost";
        private string _types = "test";
        private int _maxThreads = -1;
        private int _maxTraceLines = 50;
        private string _logsDirectory = @"C:\TEST";
        private TraceLinePattern _traceLinePattern = new TraceLinePattern();

        public LRSettingsScheme()
        {

        }

        public LRSettingsScheme(string name)
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
            var maskMatch = IsMatch(message);
            if (maskMatch != null && maskMatch.Success)
            {
                result = new DataTemplate(fileLog,
                    maskMatch.Groups["ID"].Value,
                    maskMatch.Groups["Date"].Value,
                    maskMatch.Groups["TraceType"].Value,
                    maskMatch.Groups["Description"].Value,
                    maskMatch.Groups["Message"].Value,
                    message);
                return true;
            }

            result = new DataTemplate(fileLog, message);
            return false;
        }

        public Match IsMatch(string input)
        {
            foreach (var regexPatt in TraceLinePattern.RegexItem)
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
                    CatchWaring?.Invoke($"Some patterns is incorrect! Please check.", true);
                    return false;
                }

                if (TraceLinePattern.RegexItem.Count == 0)
                {
                    CatchWaring?.Invoke($"TraceLinePattern hasn't any regex patterns", true);
                    return false;
                }

                foreach (var rgPatt in TraceLinePattern.RegexItem)
                {
                    var groups = rgPatt.GetGroupNames();
                    if (groups.All(x => x != "Date"))
                    {
                        CatchWaring?.Invoke($"Scheme '{Name}' has incorrect '{rgPatt}' pattern. Not found group '?<Date>' in TraceLinePattern", true);
                        return false;
                    }
                    if (groups.All(x => x != "TraceType"))
                    {
                        CatchWaring?.Invoke($"Scheme '{Name}' has incorrect '{rgPatt}' pattern. Not found group '?<TraceType>' in TraceLinePattern", true);
                        return false;
                    }
                    if (groups.All(x => x != "Message"))
                    {
                        CatchWaring?.Invoke($"Scheme '{Name}' has incorrect '{rgPatt}' pattern. Not found group '?<Message>' in TraceLinePattern", true);
                        return false;
                    }
                }
                return true;
            }
        }
    }

    [XmlRoot("TraceLinePattern")]
    public class TraceLinePattern
    {
        private XmlNode[] _traceLinePattern = new XmlNode[] { };

        public TraceLinePattern()
        {
            
        }

        public TraceLinePattern(string name)
        {
            switch (name)
            {
                case "MG":
                    Items = new XmlNode[]
                    {
                        new XmlDocument().CreateCDataSection(@"^(?<Date>.+?)\s*(?<TraceType>\[.+?\])\s*(?<Description>.*?)(?<Message>\<.+\>).*$"),
                        new XmlDocument().CreateCDataSection(@"^(?<Date>.+?)\s*(?<TraceType>\[.+?\])\s*(?<Description>.+?)\s+(?<Message>.+)$"),
                        new XmlDocument().CreateCDataSection(@"^(?<Date>.+?)\s*(?<TraceType>\[.+?\])\s*(?<Message>.+)$")
                    };
                    break;
                case "SPA":
                    Items = new XmlNode[]
                    {
                        new XmlDocument().CreateCDataSection(@"(?<ID>\d+?)(?<TraceType>.+?)(?<Description>.+?)(?<Date>.+?)(?<Message>.*?)\d*")
                    };
                    break;
            }
        }

        [XmlIgnore]
        internal bool IsCorrectRegex { get; private set; } = false;

        [XmlElement("Item")]
        public XmlNode[] Items
        {
            get => _traceLinePattern;
            set
            {
                IsCorrectRegex = false;

                if (value != null)
                {
                    if (value.Length > 0)
                        _traceLinePattern = value;
                }

                if (_traceLinePattern.Length == 0)
                {
                    MessageBox.Show($"Some schemes have no patterns.", "TraceLinePattern Reader", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                foreach (var pattern in _traceLinePattern)
                {
                    if (pattern == null)
                    {
                        MessageBox.Show("One of patterns is null. Please check config.", "TraceLinePattern Reader", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var text = pattern.Value.ReplaceCodeToUTF();
                    if (!REGEX.Verify(text))
                    {
                        MessageBox.Show($"Pattern \"{text}\" is incorrect", "TraceLinePattern Reader", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    else
                    {
                        RegexItem.Add(new Regex(text, RegexOptions.Compiled | RegexOptions.Singleline));
                    }
                }

                IsCorrectRegex = true;
            }
        }

        [XmlIgnore]
        public List<Regex> RegexItem { get; private set; } = new List<Regex>();
    }
}
