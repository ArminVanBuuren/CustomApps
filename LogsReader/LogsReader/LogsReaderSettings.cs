using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using static Utils.ASSEMBLY;

namespace LogsReader
{
    [Serializable, XmlRoot("Settings")]
    public class LogsReaderSettings
    {
        private string _servers = "mg1,mg2,mg3,mg4";
        private string _types = "crm,soap,sms,ivr,db,dispatcher";
        private string _logsDirectory = @"C$\FORISLOG\MG";
        XmlNode[] _traceLinePattern =
        {
            new XmlDocument().CreateCDataSection(@"(?<Date>.+)(?<TraceType>\[.+\])(?<Message>.+)")
        };

        public static string SettingsPath => $"{ApplicationFilePath}.xml";

        [XmlElement("Servers")]
        public string Servers
        {
            get => _servers;
            set => _servers = value ?? _servers;
        }

        [XmlElement("Types")]
        public string Types
        {
            get => _types;
            set => _types = value ?? _types;
        }

        [XmlElement("LogsDirectory")]
        public string LogsDirectory
        {
            get => _types;
            set => _types = value ?? _types;
        }

        [XmlElement]
        public XmlNode[] TraceLinePattern
        {
            get => _traceLinePattern;
            set => _traceLinePattern = value ?? _traceLinePattern;
        }

        public static LogsReaderSettings Load()
        {
            var sett = DeserializeSettings(SettingsPath);

            if (sett != null)
                return sett;

            var fileInfo = new FileInfo(SettingsPath);
            if (fileInfo.Exists)
            {
                var bakFileName = $"{SettingsPath}_incorrect.bak";
                if (File.Exists(bakFileName))
                    File.Delete(bakFileName);

                File.Copy(SettingsPath, bakFileName);
            }

            return new LogsReaderSettings();
        }

        static LogsReaderSettings DeserializeSettings(string settPath)
        {
            if (!File.Exists(settPath))
                return null;

            try
            {
                using (var stream = new FileStream(settPath, FileMode.Open, FileAccess.Read))
                {
                    return new XmlSerializer(typeof(LogsReaderSettings)).Deserialize(stream) as LogsReaderSettings;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Settings is incorrect!\r\n{ex.Message}");
            }
            return null;
        }
    }
}
