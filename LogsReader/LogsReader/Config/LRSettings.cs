using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using LogsReader.Properties;
using Utils;

namespace LogsReader.Config
{
    [Serializable, XmlRoot("Settings")]
    public class LRSettings
    {
        private static object _sync = new object();
        static string SettingsPath => $"{ASSEMBLY.ApplicationFilePath}.xml";
        static string IncorrectSettingsPath => $"{SettingsPath}_incorrect.bak";

        private LRSettingsScheme[] _schemes = new[] { new LRSettingsScheme("MG"), new LRSettingsScheme("SPA") };

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
                Utils.MessageShow(string.Format(Resources.LRSettings_Serialize_Ex, SettingsPath, ex.Message), "Serialize Error");
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
                Utils.MessageShow(string.Format(Resources.LRSettings_Deserialize_Ex,
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
}
