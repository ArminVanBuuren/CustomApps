using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using static Utils.ASSEMBLY;

namespace LogsReader
{
    [Serializable, XmlRoot("Settings")]
    public class LRSettings : LRSettingsSerializable
    {
        private LRSettingsScheme[] _schemes = new[] { new LRSettingsScheme() };
        static string SettingsPath => $"{ApplicationFilePath}.xml";
        static string IncorrectSettingsPath => $"{SettingsPath}_incorrect.bak";

        [XmlElement("Scheme", IsNullable = false)]
        public LRSettingsScheme[] Schemes
        {
            get => _schemes;
            set => _schemes = value ?? _schemes;
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

                MessageBox.Show($"Settings from '{SettingsPath}' is incorrect! Moved to {IncorrectSettingsPath}.\r\n{ex.Message}");
            }

            return sett ?? new LRSettings();
        }
    }

    [XmlRoot("Scheme")]
    public class LRSettingsScheme : LRSettingsSerializable
    {
        private string _schemeName = "MG";
        private string _servers = "mg1,mg2,mg3,mg4";
        private string _types = "crm,soap,sms,ivr,db,dispatcher";
        private int _maxThreads = -1;
        private int _maxTraceLines = 10;
        private string _logsDirectory = @"C$\FORISLOG\MG";
        XmlNode[] _traceLinePattern =
        {
            new XmlDocument().CreateCDataSection(@"(?<Date>.+)(?<TraceType>\[.+\])(?<Message>.+)")
        };

        [XmlAttribute]
        public string Name
        {
            get => _schemeName;
            set => _schemeName = value ?? _schemeName;
        }

        [XmlComment(Value = "Сервера для поиска")]
        [XmlElement("Servers")]
        public string Servers
        {
            get => _servers;
            set => _servers = value ?? _servers;
        }

        [XmlComment(Value = "Типы файлов")]
        [XmlElement("Types")]
        public string Types
        {
            get => _types;
            set => _types = value ?? _types;
        }

        [XmlComment(Value = "Максимальное количество потоков. Для отключения опции установить значение -1")]
        [XmlElement("MaxThreads")]
        public int MaxThreads
        {
            get => _maxThreads;
            set => _maxThreads = value <= 0 ? -1 : value;
        }

        [XmlComment(Value = "Папка с логами")]
        [XmlElement("LogsDirectory")]
        public string LogsDirectory
        {
            get => _logsDirectory;
            set => _logsDirectory = value ?? _logsDirectory;
        }

        [XmlComment(Value = "Максимальный стек трейса.")]
        [XmlElement("MaxTraceLines")]
        public int MaxTraceLines
        {
            get => _maxTraceLines;
            set => _maxTraceLines = value <= 0 ? 1 : value;
        }

        [XmlComment(Value = "Паттерн для считывания значений в найденном фрагменте лога. Учитывать что RegexOptions = Singleline. Использовать именованные группировки ?<Date> - дата; ?<TraceType> - тип трейса; ?<Message> - лог")]
        [XmlElement]
        public XmlNode[] TraceLinePattern
        {
            get => _traceLinePattern;
            set => _traceLinePattern = value ?? _traceLinePattern;
        }
    }

    public abstract class LRSettingsSerializable : IXmlSerializable
    {
        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteXml(XmlWriter writer)
        {
            var properties = GetType().GetProperties();

            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.IsDefined(typeof(XmlCommentAttribute), false))
                {
                    writer.WriteComment(propertyInfo.GetCustomAttributes(typeof(XmlCommentAttribute), false).Cast<XmlCommentAttribute>().Single().Value);
                }

                writer.WriteElementString(propertyInfo.Name, propertyInfo.GetValue(this, null).ToString());
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class XmlCommentAttribute : Attribute
    {
        public string Value { get; set; }
    }
}
