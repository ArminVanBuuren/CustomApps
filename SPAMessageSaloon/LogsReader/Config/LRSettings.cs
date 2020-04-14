using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using LogsReader.Properties;
using SPAMessageSaloon.Common;
using Utils;

namespace LogsReader.Config
{
    [Serializable, XmlRoot("Settings")]
    public class LRSettings
    {
        private static object _sync = new object();
        private static string SettingsPath { get; }
        private static string FailedSettingsPath { get; }

        static LRSettings()
        {
            SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{nameof(LogsReader)}.xml");
            FailedSettingsPath = $"{SettingsPath}_failed.bak";
        }

        private LRSettingsScheme[] _schemes = new[] { new LRSettingsScheme("MG"), new LRSettingsScheme("SPA"), new LRSettingsScheme("MGA") };

        [XmlIgnore] public Dictionary<string, LRSettingsScheme> Schemes { get; private set; }

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
                        ? _schemes.ToDictionary(k => k.Name, v => v, StringComparer.InvariantCultureIgnoreCase)
                        : new Dictionary<string, LRSettingsScheme>(StringComparer.InvariantCultureIgnoreCase);
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
                ? SchemeList.ToDictionary(k => k.Name, v => v, StringComparer.InvariantCultureIgnoreCase)
                : new Dictionary<string, LRSettingsScheme>(StringComparer.InvariantCultureIgnoreCase);
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
                        IO.SetAllAccessPermissions(SettingsPath);

                    var xml = new XmlSerializer(typeof(LRSettings));
                    using (StreamWriter sw = new StreamWriter(SettingsPath, false, new UTF8Encoding(false)))
                    {
                        xml.Serialize(sw, settings);
                    }
                }
            }
            catch (Exception ex)
            {
                ReportMessage.Show(string.Format(Resources.LRSettings_Serialize_Ex, SettingsPath, ex.Message), MessageBoxIcon.Error, "Serialize Error");
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
                ReportMessage.Show(string.Format(Resources.LRSettings_Deserialize_Ex,
                    Path.GetFileName(SettingsPath),
                    Path.GetFileName(FailedSettingsPath),
                    message), MessageBoxIcon.Error, "Deserialize Error");

                if (File.Exists(FailedSettingsPath))
                {
                    File.SetAttributes(FailedSettingsPath, FileAttributes.Normal);
                    File.Delete(FailedSettingsPath);
                }

                File.SetAttributes(SettingsPath, FileAttributes.Normal);
                File.Copy(SettingsPath, FailedSettingsPath);
                File.Delete(SettingsPath);
            }

            return sett ?? new LRSettings();
        }
    }
}
