using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Utils.CollectionHelper;

namespace Utils
{
    public static class XML
    {
        public static string GetText(this XmlNode node, string nodeName)
        {
            XmlNode getNode = node[nodeName];
            if (getNode != null)
                return getNode.InnerText;
            return string.Empty;
        }

        public static string GetTextLike(this XmlNode node, string nodeName)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name.Like(nodeName))
                {
                    return child.InnerText;
                }
            }

            return string.Empty;
        }

        public static DuplicateDictionary<string, string> GetChildNodes(this XmlNode node, StringComparer type = null)
        {
            var dictionary = new DuplicateDictionary<string, string>(type ?? StringComparer.CurrentCulture);

            foreach (XmlNode child in node.ChildNodes)
            {
                dictionary.Add(child.Name, child.InnerText);
            }

            return dictionary;
        }

        public static XmlDocument LoadXml(string path, bool convertToLower = false)
        {
            string context = IO.SafeReadFile(path, convertToLower);
            if (!string.IsNullOrEmpty(context) && context.TrimStart().StartsWith("<"))
            {
                try
                {
                    var xmlSetting = new XmlDocument();
                    xmlSetting.LoadXml(context);
                    return xmlSetting;
                }
                catch (Exception)
                {
                    //null
                }
            }

            return null;
        }

        public static bool IsXml(string path, out XmlDocument xmldoc, out string source)
        {
            xmldoc = null;
            source = IO.SafeReadFile(path);

            if (!IsXml(source, out xmldoc))
                return false;

            return true;
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
                catch (Exception)
                {
                    //null
                }
            }
            return false;
        }

        public static bool ValidateXmlDocument(string docPath)
        {
            XmlSchemaSet schemas = new XmlSchemaSet();
            XDocument doc = XDocument.Load(docPath);
            string msg = "";
            doc.Validate(schemas, (o, e) =>
            {
                msg += e.Message + Environment.NewLine;
            });
            return (msg == string.Empty);
        }

        public static string NormalizeXmlValue(string xmlStingValue, XMLValueEncoder type = XMLValueEncoder.Decode)
        {
            var regex = type == XMLValueEncoder.Decode ? new Regex(@"\&(.+?)\;", RegexOptions.IgnoreCase) : new Regex(@"(.+?)", RegexOptions.IgnoreCase);
            var xf = new XmlEntityNames(type);

            MatchEvaluator evaluator = (xf.Replace);
            string strOut = regex.Replace(xmlStingValue, evaluator);
            return strOut;
        }

        public enum XMLValueEncoder
        {
            /// <summary>
            /// из имени объекта превращает в символ
            /// </summary>
            Decode = 0,

            /// <summary>
            /// из символа превращает в имя объекта
            /// </summary>
            Encode = 1
        }

        class XmlEntityNames
        {
            static readonly Dictionary<string, string> NAME_CHAR = new Dictionary<string, string>
            {
                { "amp","&"},
                { "quot","\""},
                { "lt","<"},
                { "gt",">"},
                { "apos","'"},
                { "circ","ˆ"},
                { "tilde","˜"},
                { "ndash","–"},
                { "mdash","—"},
                { "lsquo","‘"},
                { "rsquo","’"},
                { "lsaquo","‹"},
                { "rsaquo","›"}
            };
            static readonly Dictionary<string, string> CHAR_NAME = new Dictionary<string, string>
            {
                { "&", "&amp;"},
                { "\"", "&quot;"},
                { "<", "&lt;"},
                { ">", "&gt;"},
                { "'", "&apos;"},
                { "ˆ", "&circ;"},
                { "˜", "&tilde;"},
                { "–", "&ndash;"},
                { "—", "&mdash;"},
                { "‘", "&lsquo;"},
                { "’", "&rsquo;"},
                { "‹", "&lsaquo;"},
                { "›", "&rsaquo;"}
            };

            private XMLValueEncoder Type { get; }

            public XmlEntityNames(XMLValueEncoder type)
            {
                Type = type;
            }

            public string Replace(Match m)
            {
                var find = m.Groups[1].ToString();

                switch (Type)
                {
                    case XMLValueEncoder.Decode:
                        {
                            if (NAME_CHAR.TryGetValue(find, out var result))
                                return result;
                            break;
                        }
                    case XMLValueEncoder.Encode:
                        {
                            if (CHAR_NAME.TryGetValue(find, out var result))
                                return result;
                            break;
                        }
                }

                return find;
            }
        }
    }
}
