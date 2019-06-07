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

        public static string NormalizeXmlValue(string xmlStringValue, XMLValueEncoder type = XMLValueEncoder.Decode)
        {
            var regex = type == XMLValueEncoder.Decode ? new Regex(@"\&(.+?)\;") : new Regex(@"(.+?)");
            var xf = new XmlEntityNames(type);

            MatchEvaluator evaluator = (xf.Replace);
            string strOut = regex.Replace(xmlStringValue, evaluator);
            return strOut;
        }

        public static string NormalizeXmlValueFast(string xmlStingValue, XMLValueEncoder type = XMLValueEncoder.Decode)
        {
            StringBuilder builder = new StringBuilder();
            
            if (type == XMLValueEncoder.Decode)
            {
                int isOpen = 0;
                StringBuilder charName = new StringBuilder();
                
                foreach (var ch in xmlStingValue)
                {
                    if (ch == '&')
                    {
                        if (isOpen == 0)
                        {
                            isOpen++;
                            continue;
                        }
                        else
                        {
                            builder.Append('&');
                            builder.Append(charName.ToString());
                            charName.Clear();
                            continue;
                        }
                    }

                    if (isOpen > 0 && ch == ';')
                    {
                        isOpen--;
                        if (XmlEntityNames.NAME_CHAR.TryGetValue(charName.ToString(), out var res))
                        {
                            charName.Clear();
                            if (res == '&')
                                isOpen++;
                            else
                                builder.Append(res);

                            continue;
                        }
                        else
                        {
                            builder.Append('&');
                            builder.Append(charName.ToString());
                            charName.Clear();
                        }
                    }

                    if (isOpen > 0 && (charName.Length >= 6 || char.IsWhiteSpace(ch)))
                    {
                        isOpen--;
                        builder.Append('&');
                        builder.Append(charName.ToString());
                        builder.Append(ch);
                        charName.Clear();
                        continue;
                    }

                    if (isOpen > 0)
                    {
                        charName.Append(ch);
                        continue;
                    }

                    builder.Append(ch);
                }

                if (isOpen > 0)
                {
                    builder.Append('&');
                    builder.Append(charName.ToString());
                    charName.Clear();
                }
            }
            else
            {
                foreach (var ch in xmlStingValue)
                {
                    if (XmlEntityNames.CHAR_NAME.TryGetValue(ch, out var res))
                    {
                        builder.Append(res);
                        continue;
                    }
                    builder.Append(ch);
                }
            }

            return builder.ToString();
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
            public static readonly Dictionary<string, char> NAME_CHAR = new Dictionary<string, char>
            {
                { "amp",'&'},
                { "quot",'\"'},
                { "lt",'<'},
                { "gt",'>'},
                { "apos",'\''},
                { "circ",'ˆ'},
                { "tilde",'˜'},
                { "ndash",'–'},
                { "mdash",'—'},
                { "lsquo",'‘'},
                { "rsquo",'’'},
                { "lsaquo",'‹'},
                { "rsaquo",'›'},
                { "#xD",'\r'},
                { "#xA",'\n'}
            };
            public static readonly Dictionary<char, string> CHAR_NAME = new Dictionary<char, string>
            {
                { '&', "&amp;"},
                { '\"', "&quot;"},
                { '<', "&lt;"},
                { '>', "&gt;"},
                { '\'', "&apos;"},
                { 'ˆ', "&circ;"},
                { '˜', "&tilde;"},
                { '–', "&ndash;"},
                { '—', "&mdash;"},
                { '‘', "&lsquo;"},
                { '’', "&rsquo;"},
                { '‹', "&lsaquo;"},
                { '›', "&rsaquo;"},
                { '\r', "#xD"},
                { '\n', "#xA"}
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
                                return result.ToString();
                            break;
                        }
                    case XMLValueEncoder.Encode:
                        {
                            if (CHAR_NAME.TryGetValue(find[0], out var result))
                                return result;
                            break;
                        }
                }

                return find;
            }
        }
    }
}
