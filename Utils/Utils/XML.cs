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
        EncodeFull = 1,

        /// <summary>
        /// из символа превращает в имя объекта
        /// </summary>
        EncodeLight = 2,

        /// <summary>
        /// из символа превращает в имя объекта для аттрибутов
        /// </summary>
        EncodeAttribute = 4
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

        public static DoubleDictionary<string, string> GetChildNodes(this XmlNode node, StringComparer type = null)
        {
            var dictionary = new DoubleDictionary<string, string>(type ?? StringComparer.CurrentCulture, node.ChildNodes.Count);

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
            var msg = string.Empty;
            doc.Validate(schemas, (o, e) => msg += e.Message + Environment.NewLine);
            return msg.IsNullOrEmpty();
        }

        public static string NormalizeXmlValueSlow(string xmlStringValue, XMLValueEncoder type = XMLValueEncoder.Decode)
        {
            var regex = type == XMLValueEncoder.Decode ? new Regex(@"\&(.+?)\;") : new Regex(@"(.+?)");
            var xf = new XmlEntityNames(type);

            MatchEvaluator evaluator = (xf.Replace);
            var strOut = regex.Replace(xmlStringValue, evaluator);
            return strOut;
        }

        public static string NormalizeXmlValueFast(string xmlStingValue, XMLValueEncoder type = XMLValueEncoder.Decode)
        {
            var builder = new StringBuilder(xmlStingValue.Length + 100);

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

                case XMLValueEncoder.EncodeFull:
                    foreach (var ch in xmlStingValue)
                    {
                        if (XmlEntityNames.GetNameByCharFull(ch, out var res))
                        {
                            builder.Append(res);
                            continue;
                        }

                        builder.Append(ch);
                    }
                    break;

                case XMLValueEncoder.EncodeLight:
                    foreach (var ch in xmlStingValue)
                    {
                        if (XmlEntityNames.GetNameByCharLight(ch, out var res))
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


        #region XML-Mnemonic Decoder\Encoder
        class XmlEntityNames
        {
            public static bool GetCharByName(string charName, out char result)
            {
                result = char.MinValue;
                switch (charName)
                {
                    case "#160": case "nbsp": result = ' '; break;
                    case "#161": case "iexcl": result = '¡'; break;
                    case "#162": case "cent": result = '¢'; break;
                    case "#163": case "pound": result = '£'; break;
                    case "#164": case "curren": result = '¤'; break;
                    case "#165": case "yen": result = '¥'; break;
                    case "#166": case "brvbar": result = '¦'; break;
                    case "#167": case "sect": result = '§'; break;
                    case "#168": case "uml": result = '¨'; break;
                    case "#169": case "copy": result = '©'; break;
                    case "#170": case "ordf": result = 'ª'; break;
                    case "#171": case "laquo": result = '«'; break;
                    case "#187": case "raquo": result = '»'; break;
                    case "#172": case "not": result = '¬'; break;
                    case "#174": case "reg": result = '®'; break;
                    case "#175": case "macr": result = '¯'; break;
                    case "#176": case "deg": result = '°'; break;
                    case "#177": case "plusmn": result = '±'; break;
                    case "#178": case "sup2": result = '²'; break;
                    case "#179": case "sup3": result = '³'; break;
                    case "#180": case "acute": result = '´'; break;
                    case "#181": case "micro": result = 'µ'; break;
                    case "#182": case "para": result = '¶'; break;
                    case "#183": case "middot": result = '·'; break;
                    case "#184": case "cedil": result = '¸'; break;
                    case "#185": case "sup1": result = '¹'; break;
                    case "#186": case "ordm": result = 'º'; break;
                    case "#188": case "frac14": result = '¼'; break;
                    case "#189": case "frac12": result = '½'; break;
                    case "#190": case "frac34": result = '¾'; break;
                    case "#191": case "iquest": result = '¿'; break;
                    case "#192": case "Agrave": result = 'À'; break;
                    case "#193": case "Aacute": result = 'Á'; break;
                    case "#194": case "Acirc": result = 'Â'; break;
                    case "#195": case "Atilde": result = 'Ã'; break;
                    case "#196": case "Auml": result = 'Ä'; break;
                    case "#197": case "Aring": result = 'Å'; break;
                    case "#198": case "AElig": result = 'Æ'; break;
                    case "#199": case "Ccedil": result = 'Ç'; break;
                    case "#200": case "Egrave": result = 'È'; break;
                    case "#201": case "Eacute": result = 'É'; break;
                    case "#202": case "Ecirc": result = 'Ê'; break;
                    case "#203": case "Euml": result = 'Ë'; break;
                    case "#204": case "Igrave": result = 'Ì'; break;
                    case "#205": case "Iacute": result = 'Í'; break;
                    case "#206": case "Icirc": result = 'Î'; break;
                    case "#207": case "Iuml": result = 'Ï'; break;
                    case "#208": case "ETH": result = 'Ð'; break;
                    case "#209": case "Ntilde": result = 'Ñ'; break;
                    case "#210": case "Ograve": result = 'Ò'; break;
                    case "#211": case "Oacute": result = 'Ó'; break;
                    case "#212": case "Ocirc": result = 'Ô'; break;
                    case "#213": case "Otilde": result = 'Õ'; break;
                    case "#214": case "Ouml": result = 'Ö'; break;
                    case "#215": case "times": result = '×'; break;
                    case "#216": case "Oslash": result = 'Ø'; break;
                    case "#217": case "Ugrave": result = 'Ù'; break;
                    case "#218": case "Uacute": result = 'Ú'; break;
                    case "#219": case "Ucirc": result = 'Û'; break;
                    case "#220": case "Uuml": result = 'Ü'; break;
                    case "#221": case "Yacute": result = 'Ý'; break;
                    case "#222": case "THORN": result = 'Þ'; break;
                    case "#223": case "szlig": result = 'ß'; break;
                    case "#224": case "agrave": result = 'à'; break;
                    case "#225": case "aacute": result = 'á'; break;
                    case "#226": case "acirc": result = 'â'; break;
                    case "#227": case "atilde": result = 'ã'; break;
                    case "#228": case "auml": result = 'ä'; break;
                    case "#229": case "aring": result = 'å'; break;
                    case "#230": case "aelig": result = 'æ'; break;
                    case "#231": case "ccedil": result = 'ç'; break;
                    case "#232": case "egrave": result = 'è'; break;
                    case "#233": case "eacute": result = 'é'; break;
                    case "#234": case "ecirc": result = 'ê'; break;
                    case "#235": case "euml": result = 'ë'; break;
                    case "#236": case "igrave": result = 'ì'; break;
                    case "#237": case "iacute": result = 'í'; break;
                    case "#238": case "icirc": result = 'î'; break;
                    case "#239": case "iuml": result = 'ï'; break;
                    case "#240": case "eth": result = 'ð'; break;
                    case "#241": case "ntilde": result = 'ñ'; break;
                    case "#242": case "ograve": result = 'ò'; break;
                    case "#243": case "oacute": result = 'ó'; break;
                    case "#244": case "ocirc": result = 'ô'; break;
                    case "#245": case "otilde": result = 'õ'; break;
                    case "#246": case "ouml": result = 'ö'; break;
                    case "#247": case "divide": result = '÷'; break;
                    case "#248": case "oslash": result = 'ø'; break;
                    case "#249": case "ugrave": result = 'ù'; break;
                    case "#250": case "uacute": result = 'ú'; break;
                    case "#251": case "ucirc": result = 'û'; break;
                    case "#252": case "uuml": result = 'ü'; break;
                    case "#253": case "yacute": result = 'ý'; break;
                    case "#254": case "thorn": result = 'þ'; break;
                    case "#255": case "yuml": result = 'ÿ'; break;
                    case "#402": case "fnof": result = 'ƒ'; break;
                    case "#913": case "Alpha": result = 'Α'; break;
                    case "#914": case "Beta": result = 'Β'; break;
                    case "#915": case "Gamma": result = 'Γ'; break;
                    case "#916": case "Delta": result = 'Δ'; break;
                    case "#917": case "Epsilon": result = 'Ε'; break;
                    case "#918": case "Zeta": result = 'Ζ'; break;
                    case "#919": case "Eta": result = 'Η'; break;
                    case "#920": case "Theta": result = 'Θ'; break;
                    case "#921": case "Iota": result = 'Ι'; break;
                    case "#922": case "Kappa": result = 'Κ'; break;
                    case "#923": case "Lambda": result = 'Λ'; break;
                    case "#924": case "Mu": result = 'Μ'; break;
                    case "#925": case "Nu": result = 'Ν'; break;
                    case "#926": case "Xi": result = 'Ξ'; break;
                    case "#927": case "Omicron": result = 'Ο'; break;
                    case "#928": case "Pi": result = 'Π'; break;
                    case "#929": case "Rho": result = 'Ρ'; break;
                    case "#931": case "Sigma": result = 'Σ'; break;
                    case "#932": case "Tau": result = 'Τ'; break;
                    case "#933": case "Upsilon": result = 'Υ'; break;
                    case "#934": case "Phi": result = 'Φ'; break;
                    case "#935": case "Chi": result = 'Χ'; break;
                    case "#936": case "Psi": result = 'Ψ'; break;
                    case "#937": case "Omega": result = 'Ω'; break;
                    case "#945": case "alpha": result = 'α'; break;
                    case "#946": case "beta": result = 'β'; break;
                    case "#947": case "gamma": result = 'γ'; break;
                    case "#948": case "delta": result = 'δ'; break;
                    case "#949": case "epsilon": result = 'ε'; break;
                    case "#950": case "zeta": result = 'ζ'; break;
                    case "#951": case "eta": result = 'η'; break;
                    case "#952": case "theta": result = 'θ'; break;
                    case "#953": case "iota": result = 'ι'; break;
                    case "#954": case "kappa": result = 'κ'; break;
                    case "#955": case "lambda": result = 'λ'; break;
                    case "#956": case "mu": result = 'μ'; break;
                    case "#957": case "nu": result = 'ν'; break;
                    case "#958": case "xi": result = 'ξ'; break;
                    case "#959": case "omicron": result = 'ο'; break;
                    case "#960": case "pi": result = 'π'; break;
                    case "#961": case "rho": result = 'ρ'; break;
                    case "#962": case "sigmaf": result = 'ς'; break;
                    case "#963": case "sigma": result = 'σ'; break;
                    case "#964": case "tau": result = 'τ'; break;
                    case "#965": case "upsilon": result = 'υ'; break;
                    case "#966": case "phi": result = 'φ'; break;
                    case "#967": case "chi": result = 'χ'; break;
                    case "#968": case "psi": result = 'ψ'; break;
                    case "#969": case "omega": result = 'ω'; break;
                    case "#977": case "thetasym": result = 'ϑ'; break;
                    case "#978": case "upsih": result = 'ϒ'; break;
                    case "#982": case "piv": result = 'ϖ'; break;
                    case "#8226": case "bull": result = '•'; break;
                    case "#8230": case "hellip": result = '…'; break;
                    case "#8242": case "prime": result = '′'; break;
                    case "#8243": case "Prime": result = '″'; break;
                    case "#8254": case "oline": result = '‾'; break;
                    case "#8260": case "frasl": result = '⁄'; break;
                    case "#8472": case "weierp": result = '℘'; break;
                    case "#8465": case "image": result = 'ℑ'; break;
                    case "#8476": case "real": result = 'ℜ'; break;
                    case "#8482": case "trade": result = '™'; break;
                    case "#8501": case "alefsym": result = 'ℵ'; break;
                    case "#8592": case "larr": result = '←'; break;
                    case "#8593": case "uarr": result = '↑'; break;
                    case "#8594": case "rarr": result = '→'; break;
                    case "#8595": case "darr": result = '↓'; break;
                    case "#8596": case "harr": result = '↔'; break;
                    case "#8629": case "crarr": result = '↵'; break;
                    case "#8656": case "lArr": result = '⇐'; break;
                    case "#8657": case "uArr": result = '⇑'; break;
                    case "#8658": case "rArr": result = '⇒'; break;
                    case "#8659": case "dArr": result = '⇓'; break;
                    case "#8660": case "hArr": result = '⇔'; break;
                    case "#8704": case "forall": result = '∀'; break;
                    case "#8706": case "part": result = '∂'; break;
                    case "#8707": case "exist": result = '∃'; break;
                    case "#8709": case "empty": result = '∅'; break;
                    case "#8711": case "nabla": result = '∇'; break;
                    case "#8712": case "isin": result = '∈'; break;
                    case "#8713": case "notin": result = '∉'; break;
                    case "#8715": case "ni": result = '∋'; break;
                    case "#8719": case "prod": result = '∏'; break;
                    case "#8721": case "sum": result = '∑'; break;
                    case "#8722": case "minus": result = '−'; break;
                    case "#8727": case "lowast": result = '∗'; break;
                    case "#8730": case "radic": result = '√'; break;
                    case "#8733": case "prop": result = '∝'; break;
                    case "#8734": case "infin": result = '∞'; break;
                    case "#8736": case "ang": result = '∠'; break;
                    case "#8743": case "and": result = '∧'; break;
                    case "#8744": case "or": result = '∨'; break;
                    case "#8745": case "cap": result = '∩'; break;
                    case "#8746": case "cup": result = '∪'; break;
                    case "#8747": case "int": result = '∫'; break;
                    case "#8756": case "there4": result = '∴'; break;
                    case "#8764": case "sim": result = '∼'; break;
                    case "#8773": case "cong": result = '≅'; break;
                    case "#8776": case "asymp": result = '≈'; break;
                    case "#8800": case "ne": result = '≠'; break;
                    case "#8801": case "equiv": result = '≡'; break;
                    case "#8804": case "le": result = '≤'; break;
                    case "#8805": case "ge": result = '≥'; break;
                    case "#8834": case "sub": result = '⊂'; break;
                    case "#8835": case "sup": result = '⊃'; break;
                    case "#8836": case "nsub": result = '⊄'; break;
                    case "#8838": case "sube": result = '⊆'; break;
                    case "#8839": case "supe": result = '⊇'; break;
                    case "#8853": case "oplus": result = '⊕'; break;
                    case "#8855": case "otimes": result = '⊗'; break;
                    case "#8869": case "perp": result = '⊥'; break;
                    case "#8901": case "sdot": result = '⋅'; break;
                    case "#8968": case "lceil": result = '⌈'; break;
                    case "#8969": case "rceil": result = '⌉'; break;
                    case "#8970": case "lfloor": result = '⌊'; break;
                    case "#8971": case "rfloor": result = '⌋'; break;
                    case "#9001": case "lang": result = '〈'; break;
                    case "#9002": case "rang": result = '〉'; break;
                    case "#9674": case "loz": result = '◊'; break;
                    case "#9824": case "spades": result = '♠'; break;
                    case "#9827": case "clubs": result = '♣'; break;
                    case "#9829": case "hearts": result = '♥'; break;
                    case "#9830": case "diams": result = '♦'; break;
                    case "#39": case "apos": result = '\''; break;
                    case "#34": case "quot": result = '"'; break;
                    case "#38": case "amp": result = '&'; break;
                    case "#60": case "lt": result = '<'; break;
                    case "#62": case "gt": result = '>'; break;
                    case "#338": case "OElig": result = 'Œ'; break;
                    case "#339": case "oelig": result = 'œ'; break;
                    case "#352": case "Scaron": result = 'Š'; break;
                    case "#353": case "scaron": result = 'š'; break;
                    case "#376": case "Yuml": result = 'Ÿ'; break;
                    case "#710": case "circ": result = 'ˆ'; break;
                    case "#732": case "tilde": result = '˜'; break;
                    case "#8194": case "ensp": result = ' '; break;
                    case "#8195": case "emsp": result = ' '; break;
                    case "#8201": case "thinsp": result = ' '; break;
                    case "#8204": case "zwnj": result = '‌'; break;
                    case "#8205": case "zwj": result = '‍'; break;
                    case "#8206": case "lrm": result = 'l'; break;
                    case "#8207": case "rlm": result = '‏'; break;
                    case "#8211": case "ndash": result = '–'; break;
                    case "#8212": case "mdash": result = '—'; break;
                    case "#8216": case "lsquo": result = '‘'; break;
                    case "#8217": case "rsquo": result = '’'; break;
                    case "#8218": case "sbquo": result = '‚'; break;
                    case "#8220": case "ldquo": result = '“'; break;
                    case "#8221": case "rdquo": result = '”'; break;
                    case "#8222": case "bdquo": result = '„'; break;
                    case "#8224": case "dagger": result = '†'; break;
                    case "#8225": case "Dagger": result = '‡'; break;
                    case "#8240": case "permil": result = '‰'; break;
                    case "#8249": case "lsaquo": result = '‹'; break;
                    case "#8250": case "rsaquo": result = '›'; break;
                    case "#8364": case "euro": result = '€'; break;

                    case "#173": case "shy": result = '	'; break;
                    case "#8531": result = '⅓'; break;
                    case "#9668": result = '◄'; break;
                    case "#9650": result = '▲'; break;
                    case "#9658": result = '►'; break;
                    case "#9660": result = '▼'; break;
                    case "#91": result = '['; break;
                    case "#93": result = ']'; break;
                    case "#47": result = '/'; break;
                    case "#92": result = '\\'; break;
                    case "#8209": result = '‑'; break;
                    case "#8381": result = '₽'; break;
                    case "#8470": result = '№'; break;
                    case "#769": result = '́'; break;

                    case "#xD": result = '\r'; break;
                    case "#xA": result = '\n'; break;
                    default: return false;
                }

                return true;
            }

            public static bool GetNameByCharLight(char symbol, out string result)
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

            public static bool GetNameByCharFull(char symbol, out string result)
            {
                result = null;
                switch (symbol)
                {
                    case ' ': result = "&nbsp;"; break;
                    case '¡': result = "&iexcl;"; break;
                    case '¢': result = "&cent;"; break;
                    case '£': result = "&pound;"; break;
                    case '¤': result = "&curren;"; break;
                    case '¥': result = "&yen;"; break;
                    case '¦': result = "&brvbar;"; break;
                    case '§': result = "&sect;"; break;
                    case '¨': result = "&uml;"; break;
                    case '©': result = "&copy;"; break;
                    case 'ª': result = "&ordf;"; break;
                    case '«': result = "&laquo;"; break;
                    case '»': result = "&raquo;"; break;
                    case '¬': result = "&not;"; break;
                    case '®': result = "&reg;"; break;
                    case '¯': result = "&macr;"; break;
                    case '°': result = "&deg;"; break;
                    case '±': result = "&plusmn;"; break;
                    case '²': result = "&sup2;"; break;
                    case '³': result = "&sup3;"; break;
                    case '´': result = "&acute;"; break;
                    case 'µ': result = "&micro;"; break;
                    case '¶': result = "&para;"; break;
                    case '·': result = "&middot;"; break;
                    case '¸': result = "&cedil;"; break;
                    case '¹': result = "&sup1;"; break;
                    case 'º': result = "&ordm;"; break;
                    case '¼': result = "&frac14;"; break;
                    case '½': result = "&frac12;"; break;
                    case '¾': result = "&frac34;"; break;
                    case '¿': result = "&iquest;"; break;
                    case 'À': result = "&Agrave;"; break;
                    case 'Á': result = "&Aacute;"; break;
                    case 'Â': result = "&Acirc;"; break;
                    case 'Ã': result = "&Atilde;"; break;
                    case 'Ä': result = "&Auml;"; break;
                    case 'Å': result = "&Aring;"; break;
                    case 'Æ': result = "&AElig;"; break;
                    case 'Ç': result = "&Ccedil;"; break;
                    case 'È': result = "&Egrave;"; break;
                    case 'É': result = "&Eacute;"; break;
                    case 'Ê': result = "&Ecirc;"; break;
                    case 'Ë': result = "&Euml;"; break;
                    case 'Ì': result = "&Igrave;"; break;
                    case 'Í': result = "&Iacute;"; break;
                    case 'Î': result = "&Icirc;"; break;
                    case 'Ï': result = "&Iuml;"; break;
                    case 'Ð': result = "&ETH;"; break;
                    case 'Ñ': result = "&Ntilde;"; break;
                    case 'Ò': result = "&Ograve;"; break;
                    case 'Ó': result = "&Oacute;"; break;
                    case 'Ô': result = "&Ocirc;"; break;
                    case 'Õ': result = "&Otilde;"; break;
                    case 'Ö': result = "&Ouml;"; break;
                    case '×': result = "&times;"; break;
                    case 'Ø': result = "&Oslash;"; break;
                    case 'Ù': result = "&Ugrave;"; break;
                    case 'Ú': result = "&Uacute;"; break;
                    case 'Û': result = "&Ucirc;"; break;
                    case 'Ü': result = "&Uuml;"; break;
                    case 'Ý': result = "&Yacute;"; break;
                    case 'Þ': result = "&THORN;"; break;
                    case 'ß': result = "&szlig;"; break;
                    case 'à': result = "&agrave;"; break;
                    case 'á': result = "&aacute;"; break;
                    case 'â': result = "&acirc;"; break;
                    case 'ã': result = "&atilde;"; break;
                    case 'ä': result = "&auml;"; break;
                    case 'å': result = "&aring;"; break;
                    case 'æ': result = "&aelig;"; break;
                    case 'ç': result = "&ccedil;"; break;
                    case 'è': result = "&egrave;"; break;
                    case 'é': result = "&eacute;"; break;
                    case 'ê': result = "&ecirc;"; break;
                    case 'ë': result = "&euml;"; break;
                    case 'ì': result = "&igrave;"; break;
                    case 'í': result = "&iacute;"; break;
                    case 'î': result = "&icirc;"; break;
                    case 'ï': result = "&iuml;"; break;
                    case 'ð': result = "&eth;"; break;
                    case 'ñ': result = "&ntilde;"; break;
                    case 'ò': result = "&ograve;"; break;
                    case 'ó': result = "&oacute;"; break;
                    case 'ô': result = "&ocirc;"; break;
                    case 'õ': result = "&otilde;"; break;
                    case 'ö': result = "&ouml;"; break;
                    case '÷': result = "&divide;"; break;
                    case 'ø': result = "&oslash;"; break;
                    case 'ù': result = "&ugrave;"; break;
                    case 'ú': result = "&uacute;"; break;
                    case 'û': result = "&ucirc;"; break;
                    case 'ü': result = "&uuml;"; break;
                    case 'ý': result = "&yacute;"; break;
                    case 'þ': result = "&thorn;"; break;
                    case 'ÿ': result = "&yuml;"; break;
                    case 'ƒ': result = "&fnof;"; break;
                    case 'Α': result = "&Alpha;"; break;
                    case 'Β': result = "&Beta;"; break;
                    case 'Γ': result = "&Gamma;"; break;
                    case 'Δ': result = "&Delta;"; break;
                    case 'Ε': result = "&Epsilon;"; break;
                    case 'Ζ': result = "&Zeta;"; break;
                    case 'Η': result = "&Eta;"; break;
                    case 'Θ': result = "&Theta;"; break;
                    case 'Ι': result = "&Iota;"; break;
                    case 'Κ': result = "&Kappa;"; break;
                    case 'Λ': result = "&Lambda;"; break;
                    case 'Μ': result = "&Mu;"; break;
                    case 'Ν': result = "&Nu;"; break;
                    case 'Ξ': result = "&Xi;"; break;
                    case 'Ο': result = "&Omicron;"; break;
                    case 'Π': result = "&Pi;"; break;
                    case 'Ρ': result = "&Rho;"; break;
                    case 'Σ': result = "&Sigma;"; break;
                    case 'Τ': result = "&Tau;"; break;
                    case 'Υ': result = "&Upsilon;"; break;
                    case 'Φ': result = "&Phi;"; break;
                    case 'Χ': result = "&Chi;"; break;
                    case 'Ψ': result = "&Psi;"; break;
                    case 'Ω': result = "&Omega;"; break;
                    case 'α': result = "&alpha;"; break;
                    case 'β': result = "&beta;"; break;
                    case 'γ': result = "&gamma;"; break;
                    case 'δ': result = "&delta;"; break;
                    case 'ε': result = "&epsilon;"; break;
                    case 'ζ': result = "&zeta;"; break;
                    case 'η': result = "&eta;"; break;
                    case 'θ': result = "&theta;"; break;
                    case 'ι': result = "&iota;"; break;
                    case 'κ': result = "&kappa;"; break;
                    case 'λ': result = "&lambda;"; break;
                    case 'μ': result = "&mu;"; break;
                    case 'ν': result = "&nu;"; break;
                    case 'ξ': result = "&xi;"; break;
                    case 'ο': result = "&omicron;"; break;
                    case 'π': result = "&pi;"; break;
                    case 'ρ': result = "&rho;"; break;
                    case 'ς': result = "&sigmaf;"; break;
                    case 'σ': result = "&sigma;"; break;
                    case 'τ': result = "&tau;"; break;
                    case 'υ': result = "&upsilon;"; break;
                    case 'φ': result = "&phi;"; break;
                    case 'χ': result = "&chi;"; break;
                    case 'ψ': result = "&psi;"; break;
                    case 'ω': result = "&omega;"; break;
                    case 'ϑ': result = "&thetasym;"; break;
                    case 'ϒ': result = "&upsih;"; break;
                    case 'ϖ': result = "&piv;"; break;
                    case '•': result = "&bull;"; break;
                    case '…': result = "&hellip;"; break;
                    case '′': result = "&prime;"; break;
                    case '″': result = "&Prime;"; break;
                    case '‾': result = "&oline;"; break;
                    case '⁄': result = "&frasl;"; break;
                    case '℘': result = "&weierp;"; break;
                    case 'ℑ': result = "&image;"; break;
                    case 'ℜ': result = "&real;"; break;
                    case '™': result = "&trade;"; break;
                    case 'ℵ': result = "&alefsym;"; break;
                    case '←': result = "&larr;"; break;
                    case '↑': result = "&uarr;"; break;
                    case '→': result = "&rarr;"; break;
                    case '↓': result = "&darr;"; break;
                    case '↔': result = "&harr;"; break;
                    case '↵': result = "&crarr;"; break;
                    case '⇐': result = "&lArr;"; break;
                    case '⇑': result = "&uArr;"; break;
                    case '⇒': result = "&rArr;"; break;
                    case '⇓': result = "&dArr;"; break;
                    case '⇔': result = "&hArr;"; break;
                    case '∀': result = "&forall;"; break;
                    case '∂': result = "&part;"; break;
                    case '∃': result = "&exist;"; break;
                    case '∅': result = "&empty;"; break;
                    case '∇': result = "&nabla;"; break;
                    case '∈': result = "&isin;"; break;
                    case '∉': result = "&notin;"; break;
                    case '∋': result = "&ni;"; break;
                    case '∏': result = "&prod;"; break;
                    case '∑': result = "&sum;"; break;
                    case '−': result = "&minus;"; break;
                    case '∗': result = "&lowast;"; break;
                    case '√': result = "&radic;"; break;
                    case '∝': result = "&prop;"; break;
                    case '∞': result = "&infin;"; break;
                    case '∠': result = "&ang;"; break;
                    case '∧': result = "&and;"; break;
                    case '∨': result = "&or;"; break;
                    case '∩': result = "&cap;"; break;
                    case '∪': result = "&cup;"; break;
                    case '∫': result = "&int;"; break;
                    case '∴': result = "&there4;"; break;
                    case '∼': result = "&sim;"; break;
                    case '≅': result = "&cong;"; break;
                    case '≈': result = "&asymp;"; break;
                    case '≠': result = "&ne;"; break;
                    case '≡': result = "&equiv;"; break;
                    case '≤': result = "&le;"; break;
                    case '≥': result = "&ge;"; break;
                    case '⊂': result = "&sub;"; break;
                    case '⊃': result = "&sup;"; break;
                    case '⊄': result = "&nsub;"; break;
                    case '⊆': result = "&sube;"; break;
                    case '⊇': result = "&supe;"; break;
                    case '⊕': result = "&oplus;"; break;
                    case '⊗': result = "&otimes;"; break;
                    case '⊥': result = "&perp;"; break;
                    case '⋅': result = "&sdot;"; break;
                    case '⌈': result = "&lceil;"; break;
                    case '⌉': result = "&rceil;"; break;
                    case '⌊': result = "&lfloor;"; break;
                    case '⌋': result = "&rfloor;"; break;
                    case '〈': result = "&lang;"; break;
                    case '〉': result = "&rang;"; break;
                    case '◊': result = "&loz;"; break;
                    case '♠': result = "&spades;"; break;
                    case '♣': result = "&clubs;"; break;
                    case '♥': result = "&hearts;"; break;
                    case '♦': result = "&diams;"; break;
                    case '\'': result = "&apos;"; break;
                    case '"': result = "&quot;"; break;
                    case '&': result = "&amp;"; break;
                    case '<': result = "&lt;"; break;
                    case '>': result = "&gt;"; break;
                    case 'Œ': result = "&OElig;"; break;
                    case 'œ': result = "&oelig;"; break;
                    case 'Š': result = "&Scaron;"; break;
                    case 'š': result = "&scaron;"; break;
                    case 'Ÿ': result = "&Yuml;"; break;
                    case 'ˆ': result = "&circ;"; break;
                    case '˜': result = "&tilde;"; break;
                    case ' ': result = "&ensp;"; break;
                    case ' ': result = "&emsp;"; break;
                    case ' ': result = "&thinsp;"; break;
                    case '‌': result = "&zwnj;"; break;
                    case '‍': result = "&zwj;"; break;
                    case 'l': result = "&lrm;"; break;
                    case '‏': result = "&rlm;"; break;
                    case '–': result = "&ndash;"; break;
                    case '—': result = "&mdash;"; break;
                    case '‘': result = "&lsquo;"; break;
                    case '’': result = "&rsquo;"; break;
                    case '‚': result = "&sbquo;"; break;
                    case '“': result = "&ldquo;"; break;
                    case '”': result = "&rdquo;"; break;
                    case '„': result = "&bdquo;"; break;
                    case '†': result = "&dagger;"; break;
                    case '‡': result = "&Dagger;"; break;
                    case '‰': result = "&permil;"; break;
                    case '‹': result = "&lsaquo;"; break;
                    case '›': result = "&rsaquo;"; break;
                    case '€': result = "&euro;"; break;

                    case '	': result = "&shy;"; break;
                    case '⅓': result = "#8531"; break;
                    case '◄' : result = "#9668"; break;
                    case '▲': result = "#9650"; break;
                    case '►': result = "#9658"; break;
                    case '▼': result = "#9660"; break;
                    case '[': result = "#91"; break;
                    case ']' : result = "#93"; break;
                    case '/': result = "#47"; break;
                    case '\\': result = "#92"; break;
                    case '‑': result = "#8209"; break;
                    case '₽': result = "#8381"; break;
                    case '№': result = "#8470"; break;
                    case '́': result = "#769"; break;

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
                    case XMLValueEncoder.EncodeFull:
                    {
                        if (GetNameByCharFull(find[0], out var result))
                            return result;
                        break;
                    }
                    case XMLValueEncoder.EncodeLight:
                    {
                        if (GetNameByCharLight(find[0], out var result))
                            return result;
                        break;
                    }
                }

                return find;
            }
        }

        public static string RemoveUnallowable(string str, bool replaceToUTFCode = false)
        {
            var builder = new StringBuilder(str.Length);
            foreach (var ch in str)
            {
                if (IsUnallowable(ch, out var res))
                {
                    if (replaceToUTFCode)
                        builder.Append(res);
                }
                else
                {
                    builder.Append(ch);
                }
            }

            return builder.ToString();
        }

        public static string RemoveUnallowable(string str, string replaceTo)
        {
            var builder = new StringBuilder(str.Length);
            foreach (var ch in str)
            {
                if (IsUnallowable(ch, out var res))
                {
                    builder.Append(replaceTo);
                }
                else
                {
                    builder.Append(ch);
                }
            }

            return builder.ToString();
        }

        static bool IsUnallowable(char input, out string code)
        {
            code = "";
            switch (input)
            {
                case '\u0001': code = @"\u0001"; return true;
                case '\u0002': code = @"\u0002"; return true;
                case '\u0003': code = @"\u0003"; return true;
                case '\u0004': code = @"\u0004"; return true;
                case '\u0005': code = @"\u0005"; return true;
                case '\u0006': code = @"\u0006"; return true;
                case '\u0007': code = @"\u0007"; return true;
                case '\u0008': code = @"\u0008"; return true;
                case '\u000B': code = @"\u000B"; return true;
                case '\u000C': code = @"\u000C"; return true;
                case '\u000E': code = @"\u000E"; return true;
                case '\u000F': code = @"\u000F"; return true;
                case '\u0010': code = @"\u0010"; return true;
                case '\u0011': code = @"\u0011"; return true;
                case '\u0012': code = @"\u0012"; return true;
                case '\u0013': code = @"\u0013"; return true;
                case '\u0014': code = @"\u0014"; return true;
                case '\u0015': code = @"\u0015"; return true;
                case '\u0016': code = @"\u0016"; return true;
                case '\u0017': code = @"\u0017"; return true;
                case '\u0018': code = @"\u0018"; return true;
                case '\u0019': code = @"\u0019"; return true;
                case '\u001A': code = @"\u001A"; return true;
                case '\u001B': code = @"\u001B"; return true;
                case '\u001C': code = @"\u001C"; return true;
                case '\u001D': code = @"\u001D"; return true;
                case '\u001E': code = @"\u001E"; return true;
                case '\u001F': code = @"\u001F"; return true;
            }

            return false;
        }

        #endregion

        /// <summary>
        /// Красиво отформатировать XML документ
        /// </summary>
        /// <param name="xmlString"></param>
        /// <param name="removeCommentsAfter">Удалить комментарий после определенной длинны</param>
        /// <returns></returns>
        public static string PrintXml(string xmlString, int removeCommentsAfter = -1)
        {
            var xml = new XmlDocument();
            xml.LoadXml(xmlString);
            return xml.PrintXml(removeCommentsAfter);
        }

        /// <summary>
        /// Красиво отформатировать XML документ
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="removeCommentsAfter">Удалить комментарий после определенной длинны</param>
        /// <returns></returns>
        public static string PrintXml(this XmlDocument xml, int removeCommentsAfter = -1)
        {
            var source = new StringBuilder(xml.OuterXml.Length + 100);
            ProcessXmlInnerText(xml.DocumentElement, source, 0, removeCommentsAfter);
            return source.ToString();
        }

        static bool ProcessXmlInnerText(XmlNode node, StringBuilder source, int nested, int removeCommentsAfter)
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
                        var outerComment = node.OuterXml.Trim();
                        if (removeCommentsAfter <= -1 || outerComment.Length <= removeCommentsAfter)
                        {
                            source.Append(Environment.NewLine);
                            source.Append(whiteSpaces);
                            source.Append(outerComment);
                        }
                        break;
                    case "#cdata-section":
                        source.Append(Environment.NewLine);
                        source.Append(whiteSpaces);
                        source.Append(node.InnerText.IsXml(out var xmlCdata) ? $"<![CDATA[{xmlCdata.PrintXml()}]]>" : node.OuterXml.Trim());
                        break;
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

                    nested += 2;
                    var addNewLine = true;
                    foreach (XmlNode node2 in node.ChildNodes)
                    {
                        addNewLine = ProcessXmlInnerText(node2, source, nested, removeCommentsAfter);
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
            var formattedXML = new StringBuilder(xmlDocument.OuterXml.Length + 100);
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
            var indexStartEscapeWhiteSpace = -1;
            var indexEndEscapeWhiteSpace = -1;

            var docIndex = innerText.Length - targetText.TrimStart().Length;
            var findedRange = innerText.Length;

            var j = 0;
            for (var i = 0; i < innerText.Length; i++)
            {
                if (i == docIndex)
                    indexStartEscapeWhiteSpace = j;

                var ch = innerText[i];
                if (char.IsWhiteSpace(ch))
                    continue;

                j++;
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
            for (var i = 0; i < sourceText.Length; i++)
            {
                var ch = sourceText[i];

                if (isOpen > 0)
                {
                    switch (ch)
                    {
                        case ';':
                        {
                            isOpen--;
                            if (XmlEntityNames.GetCharByName(charName.ToString(), out var res))
                            {
                                symbolsIdents += charName.Length + 1 + (char.IsWhiteSpace(res) ? 1 : 0);
                            }
                            charName.Clear();
                            break;
                        }
                        case '&':
                        {
                            charName.Clear();
                            break;
                        }
                        default:
                        {
                            if (charName.Length >= 6 || char.IsWhiteSpace(ch))
                            {
                                isOpen--;
                                charName.Clear();
                            }
                            else
                            {
                                charName.Append(ch);
                            }
                            break;
                        }
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