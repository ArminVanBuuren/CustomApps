using System;
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
using SPAMassageSaloon.Common;
using Utils;

namespace LogsReader.Config
{
	[Serializable, XmlRoot("Settings")]
    public class LRSettings
    {
        private static object _sync = new object();
        private static bool _disableHintComments = false;

        private LRSettingsScheme[] _schemes = new[] { new LRSettingsScheme("MG"), new LRSettingsScheme("SPA"), new LRSettingsScheme("MGA") };

        CustomFunctions _customFunc = new CustomFunctions
        {
	        Assemblies = new CustomFunctionAssemblies
	        {
		        Childs = new[]
		        {
			        new XmlNodeValueText("System.dll")
		        }
	        },
	        Namespaces = new XmlNodeValueText("using System;"),
	        Functions = new CustomFunctionCode()
	        {
		        Function = new []
		        {
			        new XmlNodeCDATAText(@"
    public class custom_function_test : ICustomFunction 
    { 
        public string Invoke(string[] args) 
        { 
            return ""FUNC_TEST""; 
        }
    }".Trim())
		        }
	        }
        };

        private static string SettingsPath { get; }

        private static string FailedSettingsPath { get; }

        /// <summary>
        /// Функция используется для сичтывания стринговых аттрибутов, для подставления результата парсинга включая кастомные функции
        /// </summary>
        public static Func<string, Func<Match, string>> MatchCalculationFunc { get; private set; }

        [XmlIgnore] public Dictionary<string, LRSettingsScheme> Schemes { get; private set; }

        [XmlAnyElement(nameof(Resources.Txt_LRSettings_PreviousSearchComment))]
        public XmlComment SettingsComment
        {
            get => GetComment($"{Resources.Txt_LRSettings_UseRegexComment} {Resources.Txt_LRSettings_SettingsComment}");
            set { }
        }

        [XmlAttribute("disableHintComments")]
        public bool DisableHintCommentsString
        {
	        get => _disableHintComments;
	        set => _disableHintComments = value;
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
                    throw new ArgumentException(Resources.Txt_LRSettings_ErrUnique, ex);
                }
            }
        }

        [XmlAnyElement(nameof(Resources.Txt_LRSettings_CustomFunctionsComment))]
        public XmlComment CustomFunctionsComment
        {
	        get => GetComment(Resources.Txt_LRSettings_CustomFunctionsComment);
	        set { }
        }

        [XmlElement("CustomFunctions")]
        public CustomFunctions CustomFunctions
        {
	        get => _customFunc;
	        set
	        {
		        _customFunc = value;
		        if (_customFunc != null)
		        {
                    // если существуют кастомные функции, то при успешном паринге лога, будет производится вызов внутренних функций по шаблону для дальнейшей обработки записи
                    // функция GetValueByReplacement используется в качестве поиска группировок и подставления значений по шаблону
                    var compiler = new CustomFunctionsCompiler<ICustomFunction>(_customFunc);
			        if (compiler.Functions.Count > 0)
			        {
				        var functions = new Dictionary<string, Func<string[], string>>();
				        foreach (var (name, customFunction) in compiler.Functions)
					        functions.Add(name, customFunction.Invoke);

				        MatchCalculationFunc = (template) => CODE.Calculate<Match>(template, functions, REGEX.GetValueByReplacement);
				        return;
			        }
		        }

                MatchCalculationFunc = (template) => (match) => match.GetValueByReplacement(template);
            }
        }

        static LRSettings()
        {
	        SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{nameof(LogsReader)}.xml");
	        FailedSettingsPath = $"{SettingsPath}_failed.bak";
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
                    using (var sw = new StreamWriter(SettingsPath, false, new UTF8Encoding(false)))
                        xml.Serialize(sw, settings);
                }
            }
            catch (Exception ex)
            {
                ReportMessage.Show(string.Format(Resources.Txt_LRSettings_Serialize_Ex, SettingsPath, ex.Message), MessageBoxIcon.Error, Resources.Txt_LRSettings_ErrSerialize);
            }
        }

        public static LRSettings Deserialize()
        {
            LRSettings sett = null;

            lock (_sync)
            {
                try
                {
                    if (File.Exists(SettingsPath))
                    {
                        using (var stream = new StreamReader(SettingsPath, new UTF8Encoding(false)))
                        using (TextReader sr = new StringReader(XML.RemoveUnallowable(stream.ReadToEnd(), true)))
                            sett = new XmlSerializer(typeof(LRSettings)).Deserialize(sr) as LRSettings;
                    }
                }
                catch (Exception ex)
                {
                    var message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    ReportMessage.Show(string.Format(Resources.Txt_LRSettings_Deserialize_Ex,
                        Path.GetFileName(SettingsPath),
                        Path.GetFileName(FailedSettingsPath),
                        message), MessageBoxIcon.Error, Resources.Txt_LRSettings_ErrDeserialize);

                    try
                    {
                        if (File.Exists(FailedSettingsPath))
                        {
                            File.SetAttributes(FailedSettingsPath, FileAttributes.Normal);
                            File.Delete(FailedSettingsPath);
                        }

                        File.SetAttributes(SettingsPath, FileAttributes.Normal);
                        File.Copy(SettingsPath, FailedSettingsPath);
                        File.Delete(SettingsPath);
                    }
                    catch (Exception ex2)
                    {
                        ReportMessage.Show(ex2.ToString(), MessageBoxIcon.Error, Resources.Txt_LRSettings_ErrDeserialize);
                    }
                }
            }

            return sett ?? new LRSettings();
        }

        public static XmlComment GetComment(string value)
        {
	        if (_disableHintComments)
		        return null;
            return new XmlDocument().CreateComment(value);
        }
    }
}
