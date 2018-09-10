using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ProcessFilter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new ProcessFilterForm());

            ProcessFilterForm mainControl = null;
            if (File.Exists(ProcessFilterForm.SerializationDataPath))
            {
                try
                {
                    using (Stream stream = new FileStream(ProcessFilterForm.SerializationDataPath, FileMode.Open, FileAccess.Read))
                    {
                        mainControl = new BinaryFormatter().Deserialize(stream) as ProcessFilterForm;
                    }
                }
                catch (Exception ex)
                {
                    File.Delete(ProcessFilterForm.SerializationDataPath);
                }
            }

            Application.Run(mainControl ?? new ProcessFilterForm());
        }

        public static string TrimEnd(this string input, string suffixToRemove, StringComparison comparisonType = StringComparison.CurrentCultureIgnoreCase)
        {
            if (input != null && suffixToRemove != null && input.EndsWith(suffixToRemove, comparisonType))
            {
                return input.Substring(0, input.Length - suffixToRemove.Length);
            }
            else return input;
        }

        public static void GetLastNameInPath(this string[] collectionPath)
        {
            int i = 0;
            foreach (string path in collectionPath)
            {
                collectionPath[i] = path.GetLastNameInPath();
                i++;
            }
        }

        public static string GetLastNameInPath(this string path)
        {
            string[] spltStr = path.Split('\\');
            return spltStr[spltStr.Length - 1].TrimEnd(".xml");
        }

        public static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var cnt = new Dictionary<T, int>();
            foreach (T s in list1)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else
                {
                    cnt.Add(s, 1);
                }
            }
            foreach (T s in list2)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else
                {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }

        public static XmlDocument LoadXml(string path, bool toLower = false)
        {
            if (File.Exists(path))
            {
                string context;
                using (StreamReader sr = new StreamReader(path))
                {
                    context = toLower ? sr.ReadToEnd().ToLower() : sr.ReadToEnd();
                }

                if (!string.IsNullOrEmpty(context) && context.TrimStart().StartsWith("<"))
                {
                    try
                    {
                        XmlDocument xmlSetting = new XmlDocument();
                        xmlSetting.LoadXml(context);
                        return xmlSetting;
                    }
                    catch (Exception ex)
                    {
                        //null
                    }
                }
            }
            return null;
        }

        

        public static bool IsXml(string path, out XmlDocument xmldoc, out string source)
        {
            xmldoc = null;
            source = null;
            if (File.Exists(path))
            {
                string context;
                using (StreamReader sr = new StreamReader(path))
                {
                    context = sr.ReadToEnd();
                }
                if (IsXml(context, out xmldoc))
                {
                    source = context;
                    return true;
                }
            }
            return false;
        }

        public static bool IsXml(string source, out XmlDocument xmldoc)
        {
            xmldoc = null;
            if (!string.IsNullOrEmpty(source) && source.TrimStart().StartsWith("<"))
            {
                try
                {
                    xmldoc = new XmlDocument();
                    xmldoc.LoadXml(source);
                    return true;
                }
                catch (Exception ex)
                {
                    //null
                }
            }
            return false;
        }
    }
}
