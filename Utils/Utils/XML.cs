using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Utils.CollectionHelper;

namespace Utils
{
    public enum XMlType
    {
        Unknown = 0,
        Attribute = 1,
        Node = 2
    }

    public class XmlNodeResult
    {
        //public string InnerText { get; set; }
        //public string FindedText { get; set; }

        public int IndexStart { get; }
        public int IndexEnd { get; }
        public int Length { get; }
        public XMlType Type { get; }

        public XmlNodeResult(int start, int end, int length, XMlType type)
        {
            IndexStart = start;
            IndexEnd = end;
            Length = length;
            Type = type;
        }
    }

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
            doc.Validate(schemas, (o, e) => { msg += e.Message + Environment.NewLine; });
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
                {"amp", '&'},
                {"quot", '\"'},
                {"lt", '<'},
                {"gt", '>'},
                {"apos", '\''},
                {"circ", 'ˆ'},
                {"tilde", '˜'},
                {"ndash", '–'},
                {"mdash", '—'},
                {"lsquo", '‘'},
                {"rsquo", '’'},
                {"lsaquo", '‹'},
                {"rsaquo", '›'},
                {"#xD", '\r'},
                {"#xA", '\n'}
            };

            public static readonly Dictionary<char, string> CHAR_NAME = new Dictionary<char, string>
            {
                {'&', "&amp;"},
                {'\"', "&quot;"},
                {'<', "&lt;"},
                {'>', "&gt;"},
                {'\'', "&apos;"},
                {'ˆ', "&circ;"},
                {'˜', "&tilde;"},
                {'–', "&ndash;"},
                {'—', "&mdash;"},
                {'‘', "&lsquo;"},
                {'’', "&rsquo;"},
                {'‹', "&lsaquo;"},
                {'›', "&rsaquo;"},
                {'\r', "#xD"},
                {'\n', "#xA"}
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceXmlText"></param>
        /// <param name="xmlDocument"></param>
        /// <param name="find"></param>
        /// <returns></returns>
        public static XmlNodeResult GetPositionByXmlNode(string sourceXmlText, XmlDocument xmlDocument, XmlNode find)
        {
            var formattedXML = new StringBuilder();
            var targetText = string.Empty;
            var type = XMlType.Unknown;

            foreach (XmlNode child in xmlDocument.ChildNodes)
            {
                var resType = GetXmlPosition(child, formattedXML, ref targetText,  find);

                if (resType != XMlType.Unknown)
                {
                    type = resType;
                    break;
                }
            }

            return type != XMlType.Unknown ? GetPositionInSourceText(formattedXML.ToString(), targetText, type, sourceXmlText) : null;
        }

        static XMlType GetXmlPosition(XmlNode node, StringBuilder source, ref string targetText, XmlNode findNode)
        {
            var inputSourceLength = source.Length;

            if (node.Attributes == null)
            {
                source.Append(XML.NormalizeXmlValueFast(node.OuterXml));
            }
            else
            {
                if ((node.ChildNodes.Count <= 0) && string.IsNullOrEmpty(node.InnerText))
                {
                    if (node.Attributes.Count > 0)
                    {
                        string attributes = string.Empty;
                        foreach (XmlAttribute attribute in node.Attributes)
                        {
                            attributes = attributes + $" {attribute.Name}=\"{XML.NormalizeXmlValueFast(attribute.InnerXml)}\"";
                            if (attribute.Equals(findNode))
                            {
                                source.Append('<');
                                source.Append(node.Name);
                                source.Append(attributes);
                                targetText = $"{attribute.Name}=\"{XML.NormalizeXmlValueFast(attribute.InnerXml)}\"";
                                return XMlType.Attribute;
                            }
                        }
                    }

                    source.Append(XML.NormalizeXmlValueFast(node.OuterXml));
                }
                else
                {
                    if (node.Attributes.Count > 0)
                    {
                        string attributes = string.Empty;
                        foreach (XmlAttribute attribute in node.Attributes)
                        {
                            attributes = attributes + $" {attribute.Name}=\"{XML.NormalizeXmlValueFast(attribute.InnerXml)}\"";
                            if (attribute.Equals(findNode))
                            {
                                source.Append('<');
                                source.Append(node.Name);
                                source.Append(attributes);
                                targetText = $"{attribute.Name}=\"{XML.NormalizeXmlValueFast(attribute.InnerXml)}\"";
                                return XMlType.Attribute;
                            }
                        }

                        source.Append('<');
                        source.Append(node.Name);
                        source.Append(attributes);
                        source.Append('>');
                    }
                    else
                    {
                        source.Append('<');
                        source.Append(node.Name);
                        source.Append('>');
                    }

                    foreach (XmlNode node2 in node.ChildNodes)
                    {
                        XMlType type = GetXmlPosition(node2, source, ref targetText, findNode);
                        if (type != XMlType.Unknown)
                            return type;
                    }

                    source.Append("</");
                    source.Append(node.Name);
                    source.Append('>');
                }
            }

            if (node.Equals(findNode))
            {
                targetText = source.ToString(inputSourceLength, source.Length - inputSourceLength);
                return XMlType.Node;
            }

            return XMlType.Unknown;
        }


        /// <summary>
        /// Тоже колхоз немного, но работает точно корректно. Учитывает отступы по спецсимволам.
        /// </summary>
        static XmlNodeResult GetPositionInSourceText(string innerText, string targetText, XMlType type, string sourceText)
        {
            //StringBuilder xmlTextBld = new StringBuilder();
            //StringBuilder sourceTextBld = new StringBuilder();

            int indexStartEscapeWhiteSpace = -1;
            int indexEndEscapeWhiteSpace = -1;

            int docIndex = innerText.Length - targetText.TrimStart().Length;
            int findedRange = innerText.Length;

            int j = 0;
            for (int i = 0; i < innerText.Length; i++)
            {
                if (i == docIndex)
                    indexStartEscapeWhiteSpace = j;

                char ch = innerText[i];
                if (char.IsWhiteSpace(ch))
                    continue;

                j++;
                //xmlTextBld.Append(ch);
            }


            indexEndEscapeWhiteSpace = j;
            if (indexStartEscapeWhiteSpace == -1 || indexEndEscapeWhiteSpace == -1 || (indexStartEscapeWhiteSpace > indexEndEscapeWhiteSpace))
                return null;

            j = 0;
            int indexStart = -1;
            int indexEnd = -1;

            int isOpen = 0;
            int symbolsIdents = 0;
            StringBuilder charName = new StringBuilder();
            for (int i = 0; i < sourceText.Length; i++)
            {
                char ch = sourceText[i];

                if (isOpen > 0)
                {
                    if (ch == ';')
                    {
                        isOpen--;
                        if (XmlEntityNames.NAME_CHAR.TryGetValue(charName.ToString(), out var res))
                        {
                            symbolsIdents += charName.Length + 1 + (char.IsWhiteSpace(res) ? 1 : 0);
                        }
                        charName.Clear();
                    }
                    else if (ch == '&')
                    {
                        charName.Clear();
                    }
                    else if (charName.Length >= 6 || char.IsWhiteSpace(ch))
                    {
                        isOpen--;
                        charName.Clear();
                    }
                    else
                    {
                        charName.Append(ch);
                    }
                }
                else if (ch == '&')
                {
                    isOpen++;
                }


                if (j == indexEndEscapeWhiteSpace + symbolsIdents && indexEnd == -1)
                {
                    indexEnd = i;
                    break;
                }

                if (char.IsWhiteSpace(ch))
                    continue;

                if (j == indexStartEscapeWhiteSpace + symbolsIdents && indexStart == -1)
                {
                    indexStart = i;
                }

                j++;
                //sourceTextBld.Append(ch);
            }

            if (indexStart == -1 || indexEnd == -1 || (indexStart > indexEnd))
                return null;

            //string res1 = xmlTextBld.ToString();
            //string res2 = sourceTextBld.ToString();

            return new XmlNodeResult(indexStart, indexEnd, indexEnd - indexStart, type);
        }

        ///// <summary>
        ///// Колхозно, но зато работает корректно.
        ///// Тут проблема в том что XmlDocument обрезает лишние пробелы, а в исходном тексте чтобы выделить ноду нужно правильно подобрать позиции, поэтому нужно считать все пропуски в исходном тексте, тот что на экране и найти правильную позицию учитывая. Единственное отлчичие XmlDocument от исходного текста так это пропуски, их мы и вычленяем в данном методе.
        ///// </summary>
        ///// <param name="findedObj"></param>
        ///// <param name="sourceText"></param>
        ///// <returns></returns>
        //static bool IsCorrectXmlNodeIndex2(XmlNodeResult findedObj, string sourceText)
        //{
        //    bool finished = false;
        //    int i = -1;
        //    int j = -1;
        //    int findedIndexStart = findedObj.InnerText.Length - findedObj.FindedText.TrimStart().Length;
        //    int correctfindedIndexStart = -1;

        //    while (true)
        //    {
        //        i++;
        //        j++;

        //        if (j >= findedIndexStart && correctfindedIndexStart == -1)
        //            correctfindedIndexStart = i - 1;

        //        if (j > findedObj.InnerText.Length - 1)
        //            break;

        //        while (char.IsWhiteSpace(findedObj.InnerText[j]))
        //        {
        //            j++;
        //            if (j > findedObj.InnerText.Length - 1)
        //            {
        //                finished = true;
        //                break;
        //            }
        //        }

        //        if (i > sourceText.Length - 1)
        //        {
        //            i = -1;
        //            break;
        //        }

        //        while (char.IsWhiteSpace(sourceText[i]))
        //        {
        //            i++;
        //            if (i > sourceText.Length - 1)
        //            {
        //                i = -1;
        //                break;
        //            }
        //        }

        //        if (finished)
        //            break;
        //    }

        //    if (i == -1 || correctfindedIndexStart == -1)
        //        return false;
        //    findedObj.InnerText = sourceText.Substring(0, i);
        //    findedObj.FindedText = sourceText.Substring(correctfindedIndexStart, i - correctfindedIndexStart);
        //    return true;
        //}
    }
}