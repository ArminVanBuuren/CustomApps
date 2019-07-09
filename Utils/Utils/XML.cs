using System;
using System.IO;
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

    public enum XMLValueEncoder
    {
        /// <summary>
        /// из имени объекта превращает в символ
        /// </summary>
        Decode = 0,

        /// <summary>
        /// из символа превращает в имя объекта
        /// </summary>
        Encode = 1,

        /// <summary>
        /// из символа превращает в имя объекта для аттрибутов
        /// </summary>
        EncodeAttribute = 2
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
            return getNode != null ? getNode.InnerText : string.Empty;
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

        public static bool IsFileXml(string filePath, out XmlDocument xmldoc)
        {
            return IsXml(IO.SafeReadFile(filePath), out xmldoc);
        }

        public static bool IsXml(this string xmlSource, out XmlDocument xmldoc)
        {
            xmldoc = LoadXmlCommon(xmlSource);
            return xmldoc != null;
        }

        public static XmlDocument LoadXml(string path, bool convertToLower = false)
        {
            var contextSource = IO.SafeReadFile(path, convertToLower);
            return LoadXmlCommon(contextSource);
        }

        static XmlDocument LoadXmlCommon(string contextSource)
        {
            if (string.IsNullOrEmpty(contextSource) || !contextSource.TrimStart().StartsWith("<"))
                return null;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(contextSource);
                return xmlDoc;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool ValidateXmlDocument(string docPath)
        {
            var schemas = new XmlSchemaSet();
            var doc = XDocument.Load(docPath);
            var msg = "";
            doc.Validate(schemas, (o, e) => { msg += e.Message + Environment.NewLine; });
            return msg.IsNullOrEmpty();
        }

        public static string NormalizeXmlValue(string xmlStringValue, XMLValueEncoder type = XMLValueEncoder.Decode)
        {
            var regex = type == XMLValueEncoder.Decode ? new Regex(@"\&(.+?)\;") : new Regex(@"(.+?)");
            var xf = new XmlEntityNames(type);

            MatchEvaluator evaluator = (xf.Replace);
            var strOut = regex.Replace(xmlStringValue, evaluator);
            return strOut;
        }

        public static string NormalizeXmlValueFast(string xmlStingValue, XMLValueEncoder type = XMLValueEncoder.Decode)
        {
            var builder = new StringBuilder();

            switch (type)
            {
                case XMLValueEncoder.Decode:
                    var isOpen = 0;
                    var charName = new StringBuilder();

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
                                builder.Append(charName);
                                charName.Clear();
                                continue;
                            }
                        }

                        if (isOpen > 0 && ch == ';')
                        {
                            isOpen--;
                            if (XmlEntityNames.GetCharByName(charName.ToString(), out var res))
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
                                builder.Append(charName);
                                charName.Clear();
                            }
                        }

                        if (isOpen > 0 && (charName.Length >= 6 || char.IsWhiteSpace(ch)))
                        {
                            isOpen--;
                            builder.Append('&');
                            builder.Append(charName);
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
                        builder.Append(charName);
                        charName.Clear();
                    }
                    break;

                case XMLValueEncoder.Encode:
                    foreach (var ch in xmlStingValue)
                    {
                        if (XmlEntityNames.GetNameByChar(ch, out var res))
                        {
                            builder.Append(res);
                            continue;
                        }

                        builder.Append(ch);
                    }
                    break;

                case XMLValueEncoder.EncodeAttribute:
                    foreach (var ch in xmlStingValue)
                    {
                        switch (ch)
                        {
                            case '"':
                                builder.Append("&quot;");
                                break;
                            case '<':
                                builder.Append("&lt;");
                                break;
                            default:
                                builder.Append(ch);
                                break;
                        }
                    }
                    break;
            }

            return builder.ToString();
        }



        class XmlEntityNames
        {
            public static bool GetCharByName(string charName, out char result)
            {
                result = char.MinValue;
                switch (charName)
                {
                    case "amp": result = '&'; break;
                    case "quot": result = '\"'; break;
                    case "lt": result = '<'; break;
                    case "gt": result = '>'; break;
                    case "apos": result = '\''; break;
                    case "circ": result = 'ˆ'; break;
                    case "tilde": result = '˜'; break;
                    case "ndash": result = '–'; break;
                    case "mdash": result = '—'; break;
                    case "lsquo": result = '‘'; break;
                    case "rsquo": result = '’'; break;
                    case "lsaquo": result = '‹'; break;
                    case "rsaquo": result = '›'; break;
                    case "#xD": result = '\r'; break;
                    case "#xA": result = '\n'; break;
                    default: return false;
                }

                return true;
            }

            public static bool GetNameByChar(char symbol, out string result)
            {
                result = null;
                switch (symbol)
                {
                    case '&': result = "&amp;"; break;
                    case '\"': result = "&quot;"; break;
                    case '<': result = "&lt;"; break;
                    case '>': result = "&gt;"; break;
                    case '\'': result = "&apos;"; break;
                    case 'ˆ': result = "&circ;"; break;
                    case '˜': result = "&tilde;"; break;
                    case '–': result = "&ndash;"; break;
                    case '—': result = "&mdash;"; break;
                    case '‘': result = "&lsquo;"; break;
                    case '’': result = "&rsquo;"; break;
                    case '‹': result = "&lsaquo;"; break;
                    case '›': result = "&rsaquo;"; break;
                    case '\r': result = "#xD"; break;
                    case '\n': result = "#xA"; break;
                    default: return false;
                }

                return true;
            }

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
                        if (GetCharByName(find, out var result))
                            return result.ToString();
                        break;
                    }
                    case XMLValueEncoder.Encode:
                    {
                        if (GetNameByChar(find[0], out var result))
                            return result;
                        break;
                    }
                }

                return find;
            }
        }

        public static string PrintXml(string xmlString)
        {
            var xml = new XmlDocument();
            xml.LoadXml(xmlString);
            return xml.PrintXml();
        }

        public static string PrintXml(this XmlDocument xml)
        {
            var source = new StringBuilder();
            ProcessXmlInnerText(xml.DocumentElement, source, 0);
            return source.ToString();
        }

        static bool ProcessXmlInnerText(XmlNode node, StringBuilder source, int nested)
        {
            var whiteSpaces = new string(' ', nested);

            if (node.Attributes == null)
            {
                switch (node.Name)
                {
                    case "#text":
                        var trimStr = node.OuterXml.Trim('\r', '\n');
                        var strLines = trimStr.Split('\r', '\n');

                        if (strLines.Length == 1)
                        {
                            source.Append(trimStr);
                            return false;
                        }
                        else
                        {
                            var builder = new StringBuilder();

                            foreach (var line in strLines)
                            {
                                if (string.IsNullOrWhiteSpace(line))
                                    continue;

                                builder.Append(Environment.NewLine);
                                builder.Append(whiteSpaces);
                                builder.Append(line.Trim());
                            }

                            source.Append(builder);
                            return true;
                        }

                    case "#comment":
                    case "#cdata-section":
                        source.Append(Environment.NewLine);
                        source.Append(whiteSpaces);
                        source.Append(node.OuterXml.Trim()); break;
                    default:
                        source.Append(Environment.NewLine);
                        source.Append(whiteSpaces);
                        source.Append(node.InnerText.TrimEnd()); break;
                }
            }
            else
            {
                var newLine = (nested != 0) ? Environment.NewLine : string.Empty;
                var xmlAttributes = new StringBuilder();

                foreach (XmlAttribute attribute in node.Attributes)
                {
                    xmlAttributes.Append(' ');
                    xmlAttributes.Append(attribute.Name);
                    xmlAttributes.Append('=');
                    xmlAttributes.Append('"');
                    xmlAttributes.Append(NormalizeXmlValueFast(attribute.InnerXml, XMLValueEncoder.EncodeAttribute));
                    xmlAttributes.Append('"');
                }

                source.Append(newLine);
                source.Append(whiteSpaces);
                source.Append('<');
                source.Append(node.Name);

                if (xmlAttributes.Length >= 0)
                    source.Append(xmlAttributes);

                if (node.ChildNodes.Count <= 0 && node.InnerText.IsNullOrEmpty())
                    source.Append(" />");
                else
                {
                    source.Append('>');

                    nested += 3;
                    bool addNewLine = true;
                    foreach (XmlNode node2 in node.ChildNodes)
                    {
                        addNewLine = ProcessXmlInnerText(node2, source, nested);
                    }

                    if (node.ChildNodes.Count == 1 && !addNewLine)
                    {
                        source.Append("</");
                        source.Append(node.Name);
                        source.Append('>');
                    }
                    else
                    {
                        source.Append(Environment.NewLine);
                        source.Append(whiteSpaces);
                        source.Append("</");
                        source.Append(node.Name);
                        source.Append('>');
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Получить точно позицию ноды в неотформатированном тексте XML
        /// </summary>
        /// <param name="sourceXmlText">Неотформатированный текст XML</param>
        /// <param name="xmlDocument"></param>
        /// <param name="find">ноду которую необходимо найти</param>
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
                source.Append(NormalizeXmlValueFast(node.OuterXml));
            }
            else
            {
                if ((node.ChildNodes.Count <= 0) && string.IsNullOrEmpty(node.InnerText))
                {
                    if (node.Attributes.Count > 0)
                    {
                        var attrBuilder = new StringBuilder();
                        if (IsXmlAttribute(node.Attributes, attrBuilder, ref targetText, findNode))
                        {
                            source.Append('<');
                            source.Append(node.Name);
                            source.Append(attrBuilder);
                            return XMlType.Attribute;
                        }
                    }

                    source.Append(NormalizeXmlValueFast(node.OuterXml));
                }
                else
                {
                    if (node.Attributes.Count > 0)
                    {
                        source.Append('<');
                        source.Append(node.Name);

                        if(IsXmlAttribute(node.Attributes, source, ref targetText, findNode))
                            return XMlType.Attribute;

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
                        var type = GetXmlPosition(node2, source, ref targetText, findNode);
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

        static bool IsXmlAttribute(XmlAttributeCollection attributes, StringBuilder source, ref string targetText, XmlNode findNode)
        {
            foreach (XmlAttribute attribute in attributes)
            {
                var prevIndexStart = source.Length;
                source.Append(' ');
                source.Append(attribute.Name);
                source.Append('=');
                source.Append('"');
                source.Append(NormalizeXmlValueFast(attribute.InnerXml));
                source.Append('"');
                if (attribute.Equals(findNode))
                {
                    targetText = source.ToString(prevIndexStart + 1, source.Length - prevIndexStart - 1);
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Немного колхоз, но работает точно корректно. Также учитывает отступы по спецсимволам.
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
            var indexStart = -1;
            var indexEnd = -1;

            var isOpen = 0;
            var symbolsIdents = 0;
            var charName = new StringBuilder();
            for (int i = 0; i < sourceText.Length; i++)
            {
                char ch = sourceText[i];

                if (isOpen > 0)
                {
                    if (ch == ';')
                    {
                        isOpen--;
                        if (XmlEntityNames.GetCharByName(charName.ToString(), out var res))
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

                if (char.IsWhiteSpace(ch))
                    continue;

                if (j == indexStartEscapeWhiteSpace + symbolsIdents && indexStart == -1)
                {
                    indexStart = i;
                }

                j++;

                if (j == indexEndEscapeWhiteSpace + symbolsIdents && indexEnd == -1)
                {
                    indexEnd = i;
                    break;
                }

                //sourceTextBld.Append(ch);
            }

            if (indexStart == -1 || indexEnd == -1 || (indexStart > indexEnd))
                return null;

            indexEnd++;

            //string res1 = xmlTextBld.ToString();
            //string res2 = sourceTextBld.ToString();
            //string res3 = sourceText.Substring(indexStart, indexEnd - indexStart);

            return new XmlNodeResult(indexStart, indexEnd, indexEnd - indexStart, type);
        }


        /// <summary>
        /// Creates validated XmlDocument object using given schema for validation.
        /// </summary>
        /// <param name="schemaStream"> The stream to read xml schema from. </param>
        /// <param name="source"> The stream to read xml data from. </param>
        /// <returns>The validated XML document</returns>
        public static XmlDocument ReadAndValidateXML(Stream schemaStream, XmlReader source)
        {
            var veh = new ValidationEventHandler(OnValidationEventHandler);
            var schema = XmlSchema.Read(schemaStream, veh);
            var settings = new XmlReaderSettings();
            settings.Schemas.Add(schema);

            var resolver = new XmlUrlResolver
            {
                Credentials = System.Net.CredentialCache.DefaultCredentials
            };

            settings.XmlResolver = resolver;
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationEventHandler += OnValidationEventHandler;

            var doc = new XmlDocument();
            doc.Load(XmlReader.Create(source, settings));

            return doc;
        }

        /// <summary>
        /// Creates validated XmlDocument object using given schema for validation.
        /// </summary>
        /// <param name="schemaStream"> The stream to read xml schema from. </param>
        /// <param name="xmlString"> The XML document string representation. </param>
        /// <returns>The validated XML document</returns>
        public static XmlDocument ReadAndValidateXML(Stream schemaStream, string xmlString)
        {
            var veh = new ValidationEventHandler(OnValidationEventHandler);
            var schema = XmlSchema.Read(schemaStream, veh);
            var settings = new XmlReaderSettings();
            settings.Schemas.Add(schema);

            var resolver = new XmlUrlResolver
            {
                Credentials = System.Net.CredentialCache.DefaultCredentials
            };

            settings.XmlResolver = resolver;
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationEventHandler += OnValidationEventHandler;

            var reader = XmlReader.Create(new StringReader(xmlString), settings);
            var doc = new XmlDocument();
            doc.Load(reader);

            return doc;
        }

        /// <summary>
        /// Creates validated XmlDocument object using given schema for validation.
        /// </summary>
        /// <param name="xsdString"> The XML schema string representation. </param>
        /// <param name="xmlString"> The XML document string representation. </param>
        /// <returns>The validated XML document</returns>
        public static XmlDocument ReadAndValidateXML(string xsdString, string xmlString)
        {
            var veh = new ValidationEventHandler(OnValidationEventHandler);
            var schemaReader = new XmlTextReader(new StringReader(xsdString));
            var schema = XmlSchema.Read(schemaReader, veh);
            var settings = new XmlReaderSettings();
            settings.Schemas.Add(schema);

            var resolver = new XmlUrlResolver
            {
                Credentials = System.Net.CredentialCache.DefaultCredentials
            };

            settings.XmlResolver = resolver;
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationEventHandler += OnValidationEventHandler;

            var reader = XmlReader.Create(new StringReader(xmlString), settings);
            var doc = new XmlDocument();
            doc.Load(reader);

            return doc;
        }

        /// <summary>
        /// Validates XML node name candidate
        /// </summary>
        /// <param name="nodeName">Node name</param>
        public static void ValidateXMLNodeName(string nodeName)
        {
            new XmlDocument().LoadXml($"<{nodeName} />");
        }

        private static void OnValidationEventHandler(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Error)
            {
                throw args.Exception;
            }
        }

        public static string FormatXmlString(string xml, bool exceptional = true)
        {
            if (string.IsNullOrEmpty(xml))
            {
                if (!exceptional) return string.Empty;
                throw new ArgumentException("Can not be null or empty", nameof(xml));
            }

            try
            {
                return XDocument.Parse(xml).ToString().Replace("&#xD;&#xA;", Environment.NewLine).Replace("&#xA;", Environment.NewLine);
            }
            catch (Exception)
            {
                if (!exceptional)
                    return xml;
                throw;
            }
        }
    }
}