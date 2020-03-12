﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
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
                MessageBox.Show($"Unable to save Settings to specified path=[{SettingsPath}].\r\n{ex.Message}");
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

                MessageBox.Show($"Settings from '{SettingsPath}' is incorrect! Moved to {IncorrectSettingsPath}.\r\n{ex.Message}");
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
        public XmlComment ServersComment { get => new XmlDocument().CreateComment("Сервера для поиска"); set { } }

        [XmlElement("Servers")]
        public string Servers
        {
            get => _servers;
            set => _servers = value ?? _servers;
        }

        [XmlAnyElement("TypesComment")]
        public XmlComment TypesComment { get => new XmlDocument().CreateComment("Типы файлов"); set { } }

        [XmlElement("Types")]
        public string Types
        {
            get => _types;
            set => _types = value ?? _types;
        }

        [XmlAnyElement("MaxThreadsComment")]
        public XmlComment MaxThreadsComment { get => new XmlDocument().CreateComment("Максимальное количество потоков. Для отключения опции установить значение -1"); set { } }

        [XmlElement("MaxThreads")]
        public int MaxThreads
        {
            get => _maxThreads;
            set => _maxThreads = value <= 0 ? -1 : value;
        }

        [XmlAnyElement("LogsDirectoryComment")]
        public XmlComment LogsDirectoryComment { get => new XmlDocument().CreateComment("Папка с логами"); set { } }

        [XmlElement("LogsDirectory")]
        public string LogsDirectory
        {
            get => _logsDirectory;
            set => _logsDirectory = value ?? _logsDirectory;
        }

        [XmlAnyElement("MaxTraceLinesComment")]
        public XmlComment MaxTraceLinesComment { get => new XmlDocument().CreateComment("Максимальный стек трейса"); set { } }

        [XmlElement("MaxTraceLines")]
        public int MaxTraceLines
        {
            get => _maxTraceLines;
            set => _maxTraceLines = value <= 0 ? 1 : value;
        }

        [XmlAnyElement("TraceLinePatternComment")]
        public XmlComment TraceLinePatternComment { get => new XmlDocument().CreateComment("Паттерн для считывания значений в найденном фрагменте лога. Учитывать что RegexOptions = Singleline. Использовать именованные группировки ?<Date> - дата; ?<TraceType> - тип трейса; ?<Message> - лог"); set { } }

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

    //public abstract class LRSettingsSerializable : IXmlSerializable
    //{
    //    public XmlSchema GetSchema()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public abstract void ReadXml(XmlReader reader);

    //    public void WriteXml(XmlWriter writer)
    //    {
    //        var tp = GetType();
    //        var props = tp.GetProperties(BindingFlags.Instance | BindingFlags.Public);
    //        foreach (var propertyInfo in props)
    //        {
    //            var attrs = propertyInfo.GetCustomAttributes(true);
    //            if (attrs.Length == 0 || attrs.Any(p => p is XmlIgnoreAttribute))
    //                continue;

    //            foreach (var attribute in attrs)
    //            {
    //                switch (attribute)
    //                {
    //                    case XmlCommentAttribute _:
    //                        writer.WriteComment(propertyInfo.GetCustomAttributes(typeof(XmlCommentAttribute), false).Cast<XmlCommentAttribute>().Single().Value);
    //                        break;
    //                    case XmlElementAttribute element:
    //                        if (propertyInfo.GetValue(this) is Dictionary<string, LRSettingsScheme>)
    //                        {
    //                            new XmlSerializer(typeof(LRSettingsScheme)).Serialize(stream, settings);
    //                        }
    //                        writer.WriteElementString(element.ElementName, propertyInfo.GetValue(this).ToString());
    //                        break;
    //                    case XmlAttributeAttribute element:
    //                        writer.WriteAttributeString(element.AttributeName, propertyInfo.GetValue(this).ToString());
    //                        break;
    //                }
    //            }
    //        }
    //    }
    //}

    //[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    //public class XmlCommentAttribute : Attribute
    //{
    //    public string Value { get; set; }
    //}

}
