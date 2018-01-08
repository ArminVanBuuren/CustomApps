using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace XPackage
{
    public class Functions
    {
        public static string AssemblyFile
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                return Uri.UnescapeDataString(uri.Path);
                //return Path.GetDirectoryName(path);
            }
        }
        public static Encoding Enc => Encoding.UTF8;
        /// <summary>
        /// Клонировать любой объект.
        /// типо такого MyObject myObj = GetMyObj(); // Create and fill a new object
        ///             MyObject newObj = myObj.Clone();
        /// Perform a deep Copy of the object.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T Clone<T>(T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", nameof(source));
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }


        public static bool IsPossiblyFile(string path)
        {
            return Regex.IsMatch(path, @"[^\\]+\.[A-z]{1,}$") && (File.Exists(path) ||!Directory.Exists(path));
        }

        public static string LocalPath => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        public static RegexOptions GetRegexOptions(string option)
        {
            RegexOptions defaultOption = RegexOptions.None;
            if (option == null)
                return defaultOption;

            foreach (string opt in option.Split('|'))
            {
                RegexOptions retrnOpt = GetRegexOptionsSeparatly(opt);
                if(retrnOpt == RegexOptions.None)
                    continue;

                defaultOption |= retrnOpt;
            }
            return defaultOption;
        }

        static RegexOptions GetRegexOptionsSeparatly(string option)
        {
            StringComparison comparer = StringComparison.InvariantCultureIgnoreCase;
            string newOption = option.Trim();

            if (newOption.Equals("Compiled", comparer))
                return RegexOptions.Compiled;
            if (newOption.Equals("CultureInvariant", comparer))
                return RegexOptions.CultureInvariant;
            if (newOption.Equals("ECMAScript", comparer))
                return RegexOptions.ECMAScript;
            if (newOption.Equals("ExplicitCapture", comparer))
                return RegexOptions.ExplicitCapture;
            if (newOption.Equals("IgnoreCase", comparer))
                return RegexOptions.IgnoreCase;
            if (newOption.Equals("IgnorePatternWhitespace", comparer))
                return RegexOptions.IgnorePatternWhitespace;
            if (newOption.Equals("Multiline", comparer))
                return RegexOptions.Multiline;
            if (newOption.Equals("RightToLeft", comparer))
                return RegexOptions.RightToLeft;
            if (newOption.Equals("Singleline", comparer))
                return RegexOptions.Singleline;

            return RegexOptions.None;
        }

        public static StringComparison GetStringOptions(string option)
        {
            StringComparison comparer = StringComparison.InvariantCultureIgnoreCase;
            if (option == null)
                return StringComparison.InvariantCulture;

            string newOption = option.Trim();

            if (newOption.Equals("InvariantCultureIgnoreCase", comparer))
                return comparer;
            if (newOption.Equals("CurrentCulture", comparer))
                return StringComparison.CurrentCulture;
            if (newOption.Equals("CurrentCultureIgnoreCase", comparer))
                return StringComparison.CurrentCultureIgnoreCase;
            if (newOption.Equals("Ordinal", comparer))
                return StringComparison.Ordinal;
            if (newOption.Equals("OrdinalIgnoreCase", comparer))
                return StringComparison.OrdinalIgnoreCase;

            return StringComparison.InvariantCulture;
        }
        public static string Evaluate(string filepath, string xpath)
        {
            StreamReader stream = new StreamReader(filepath, Enc);
            var source = ReplaceXmlVersion(stream.ReadToEnd());
            XmlDocument xmlSetting = new XmlDocument();
            xmlSetting.LoadXml(source);
            return Evaluate(xmlSetting.CreateNavigator(), xpath);
        }
        public static string ReplaceXmlVersion(string source)
        {
            string sourcexml2 = new Regex(@"\<\?xml(.+?)\?\>").Replace(source, "");

            if (!new Regex(@"\<\?xml(.+?)\?\>").IsMatch(sourcexml2))
            {
                sourcexml2 = "<?xml version=\"1.0\" encoding=\"windows-1251\"?>\n" + sourcexml2;
            }
            return sourcexml2;
        }
        static string Evaluate(XPathNavigator navigator, string xPath)
        {
            XmlNamespaceManager manager = new XmlNamespaceManager(navigator.NameTable);
            XPathExpression expression = XPathExpression.Compile(xPath);
            manager.AddNamespace("bk", "http://www.contoso.com/books");
            expression.SetContext(manager);
            string Out = string.Empty;
            switch (expression.ReturnType)
            {
                case XPathResultType.NodeSet:
                    XPathNodeIterator nodes = navigator.Select(expression);
                    while (nodes.MoveNext())
                    {
                        Out = Out + nodes.Current;
                    }
                    return Out;
                default: return navigator.Evaluate(expression).ToString();
            }
        }


        public static string ReplaceXmlSpecSymbls(string str, int direction)
        {
            Regex reg;
            XmlFunc xf;
            if (direction == 0)
            {
                reg = new Regex(@"\&(.+?)\;", RegexOptions.IgnoreCase);
                xf = new XmlFunc(0);
            }
            else
            {
                reg = new Regex(@"(.+?)", RegexOptions.IgnoreCase);
                xf = new XmlFunc(direction);
            }
            MatchEvaluator evaluator = (xf.Replace);
            string strOut = reg.Replace(str, evaluator);
            return strOut;
        }
        internal class XmlFunc : IDisposable
        {
            int _direction = 0;
            static List<Simbol> _xmlSimbl = new List<Simbol>
            {
                new Simbol {Param = "amp", Value = "&"},
                new Simbol {Param = "quot", Value = "\""},
                new Simbol {Param = "lt", Value = "<"},
                new Simbol {Param = "gt", Value = ">"},
                new Simbol {Param = "apos", Value = "'"},
                new Simbol {Param = "circ", Value = "ˆ"},
                new Simbol {Param = "tilde", Value = "˜"},
                new Simbol {Param = "ndash", Value = "–"},
                new Simbol {Param = "mdash", Value = "—"},
                new Simbol {Param = "lsquo", Value = "‘"},
                new Simbol {Param = "rsquo", Value = "’"},
                new Simbol {Param = "lsaquo", Value = "‹"},
                new Simbol {Param = "rsaquo", Value = "‹"}
            };

            public XmlFunc(int direction)
            {
                _direction = direction;
            }

            public void Dispose()
            {

            }

            public string Replace(Match m)
            {
                string find = m.Groups[1].ToString();
                if (_direction == 0)
                {
                    foreach (Simbol sm in _xmlSimbl)
                    {
                        if (sm.Param == find)
                            return sm.Value;
                    }
                }
                else if (_direction == 2)
                {
                    foreach (Simbol sm in _xmlSimbl)
                    {
                        if (sm.Value == find && sm.Value != "\'")
                            return "&" + sm.Param + ";";
                    }
                }
                else
                {
                    foreach (Simbol sm in _xmlSimbl)
                    {
                        if (sm.Value == find)
                            return "&" + sm.Param + ";";
                    }
                }
                return find;
            }
            class Simbol
            {
                public string Param { get; set; }
                public string Value { get; set; }
            }
        }

    }
}
