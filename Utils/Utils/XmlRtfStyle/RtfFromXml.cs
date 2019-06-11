﻿using System;
using System.Text;
using System.Xml;
using System.Xml.Resolvers;

namespace Utils.XmlRtfStyle
{
    public class RtfFromXml
    {
        public uint AttrColor { get; set; } = 0xff0000;

        public RtfFontStyles AttrStyle { get; set; } = RtfFontStyles.Regular;

        public uint CdataColor { get; set; } = 0x808080;

        public RtfFontStyles CdataStyle { get; set; } = RtfFontStyles.Regular;

        public uint CommentColor { get; set; } = 0x8000;

        public RtfFontStyles CommentStyle { get; set; } = RtfFontStyles.Regular;

        public string FontName { get; set; } = "Verdana";

        public int FontSize { get; set; } = 20;

        public uint QuoteColor { get; set; } = 0xff;

        public RtfFontStyles QuoteStyle { get; set; } = RtfFontStyles.Regular;

        public uint SpecSymbolColor { get; set; } = 0xc0c0;

        public RtfFontStyles SpecSymbolStyle { get; set; } = RtfFontStyles.Regular;

        public uint SymbolColor { get; set; } = 0xff;

        public RtfFontStyles SymbolStyle { get; set; } = RtfFontStyles.Regular;

        public uint TagColor { get; set; } = 0x800000;

        public RtfFontStyles TagStyle { get; set; } = RtfFontStyles.Regular;

        public uint TextColor { get; set; } = 0;

        public RtfFontStyles TextStyle { get; set; } = RtfFontStyles.Regular;

        public uint ValueColor { get; set; } = 0;

        public RtfFontStyles ValueStyle { get; set; } = RtfFontStyles.Regular;

        public static XmlDocument GetXmlDocument(string input)
        {
            XmlDocument xml = new XmlDocument();
            try
            {
                xml.LoadXml(input);
            }
            catch (Exception)
            {
                return null;
            }

            return xml;
        }

        public static string Convert(string source)
        {
            RtfFromXml settings = new RtfFromXml();
            return Convert(source, settings);
        }

        public static string Convert(XmlDocument xml)
        {
            return Convert(xml, new RtfFromXml());
        }

        public static string Convert(string source, RtfFromXml settings)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(source);
            return Convert(xml, settings);
        }


        public static string Convert(XmlDocument xml, RtfFromXml settings)
        {
            if (xml == null)
            {
                throw new ArgumentNullException(nameof(xml));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            StringBuilder source = new StringBuilder();
            ProcessXmlInnerText(xml.DocumentElement, source, 0);
            return GetRtfString(source.ToString(), settings);
        }

        public static string GetXmlString(string xmlString)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlString);
            StringBuilder source = new StringBuilder();
            ProcessXmlInnerText(xml.DocumentElement, source, 0);
            return source.ToString();
        }

        public static string GetXmlString(XmlDocument xml)
        {
            StringBuilder source = new StringBuilder();
            ProcessXmlInnerText(xml.DocumentElement, source, 0);
            return source.ToString();
        }

        static void ProcessXmlInnerText(XmlNode node, StringBuilder source, int nested)
        {
            var whiteSpaces = new string(' ', nested);

            if (node.Attributes == null)
            {
                source.Append(Environment.NewLine);
                switch (node.Name)
                {
                    case "#text": source.Append(AddSpacesByLine(node.OuterXml, whiteSpaces)); break;
                    case "#comment":
                    case "#cdata-section":
                        source.Append(whiteSpaces);
                        source.Append(node.OuterXml.Trim()); break;
                    default:
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
                    xmlAttributes.Append(XML.NormalizeXmlValueFast(attribute.InnerXml, XMLValueEncoder.EncodeAttribute));
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
                    foreach (XmlNode node2 in node.ChildNodes)
                    {
                        ProcessXmlInnerText(node2, source, nested);
                    }

                    if (((node.FirstChild != null) && (node.ChildNodes.Count == 1)) && string.Equals(node.FirstChild.Name, "#text"))
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
        }

        static string AddSpacesByLine(string source, string spaces)
        {
            int lines = 0;
            StringBuilder builder = new StringBuilder();
            foreach (var line in source.Split('\r', '\n'))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    if (lines > 0)
                        builder.Append(Environment.NewLine);
                    builder.Append(spaces);
                    builder.Append(line.Trim());
                    lines++;
                }
            }

            return builder.ToString();
        }

        static string GetRtfString(string source, RtfFromXml settings)
        {
            RtfBuilder rtf = new RtfBuilder();
            StringBuilder temp = new StringBuilder();
            rtf.SetFont(settings.FontName, settings.FontSize);
            int state = 0;
            foreach (char ch in source)
            {
                switch (state)
                {
                    case 0:
                    {
                        state = WaitTag(settings, rtf, temp, 1, 12, state, ch);
                        continue;
                    }
                    case 1:
                        if (((ch != ' ') && (ch != '\r')) && ((ch != '\n') && (ch != '\t')))
                        {
                            break;
                        }

                        rtf.SetForeColor(settings.TagColor);
                        rtf.AddText(temp.ToString(), settings.TagStyle);
                        temp.Remove(0, temp.Length);
                        state = 2;
                        goto Label_01BC;

                    case 2:
                    {
                        state = WaitAttrOrEndOfTag(settings, rtf, temp, 0, 3, state, ch);
                        continue;
                    }
                    case 3:
                        if (((ch != ' ') && (ch != '\r')) && ((ch != '\n') && (ch != '\t')))
                        {
                            goto Label_02AE;
                        }

                        rtf.SetForeColor(settings.AttrColor);
                        rtf.AddText(temp.ToString(), settings.AttrStyle);
                        temp.Remove(0, temp.Length);
                        state = 4;
                        goto Label_0306;

                    case 4:
                    {
                        state = WaitEqSymbol(settings, rtf, temp, 5, state, ch);
                        continue;
                    }
                    case 5:
                    {
                        state = WaitValue(settings, rtf, temp, 6, 7, state, ch);
                        continue;
                    }
                    case 6:
                    {
                        state = ReadValueSQ(settings, rtf, temp, 2, 10, state, ch);
                        continue;
                    }
                    case 7:
                    {
                        state = ReadValueDQ(settings, rtf, temp, 2, 11, state, ch);
                        continue;
                    }
                    case 8:
                    {
                        state = ReadComments(settings, rtf, temp, 0, state, ch);
                        continue;
                    }
                    case 9:
                    {
                        state = ReadCdata(settings, rtf, temp, 0, state, ch);
                        continue;
                    }
                    case 10:
                    {
                        temp.Append(ch);
                        if (ch == ';')
                        {
                            rtf.SetForeColor(settings.SpecSymbolColor);
                            rtf.AddText(temp.ToString(), settings.SpecSymbolStyle);
                            temp.Remove(0, temp.Length);
                            state = 6;
                        }

                        continue;
                    }
                    case 11:
                    {
                        state = ReadSpecInValueDQ(settings, rtf, temp, 7, state, ch);
                        continue;
                    }
                    case 12:
                    {
                        state = ReadSpecInText(settings, rtf, temp, 0, state, ch);
                        continue;
                    }
                    default:
                    {
                        continue;
                    }
                }

                switch (ch)
                {
                    case '/':
                    case '?':
                    {
                        rtf.SetForeColor(settings.TagColor);
                        rtf.AddText(temp.ToString(), settings.TagStyle);
                        temp.Remove(0, temp.Length);
                        rtf.SetForeColor(settings.SymbolColor);
                        rtf.AddText(ch.ToString(), settings.SymbolStyle);
                        continue;
                    }
                    case '>':
                    {
                        rtf.SetForeColor(settings.TagColor);
                        rtf.AddText(temp.ToString(), settings.TagStyle);
                        temp.Remove(0, temp.Length);
                        rtf.SetForeColor(settings.SymbolColor);
                        rtf.AddText(ch.ToString(), settings.SymbolStyle);
                        state = 0;
                        continue;
                    }
                }

                Label_01BC:
                temp.Append(ch);
                if (temp.ToString() == "!--")
                {
                    rtf.SetForeColor(settings.SymbolColor);
                    rtf.AddText(temp.ToString(), settings.SymbolStyle);
                    temp.Remove(0, temp.Length);
                    state = 8;
                }
                else if (temp.ToString() == "![CDATA[")
                {
                    rtf.SetForeColor(settings.SymbolColor);
                    rtf.AddText(temp.ToString(), settings.SymbolStyle);
                    temp.Remove(0, temp.Length);
                    state = 9;
                }

                continue;
                Label_02AE:
                if (ch == '=')
                {
                    rtf.SetForeColor(settings.AttrColor);
                    rtf.AddText(temp.ToString(), settings.AttrStyle);
                    temp.Remove(0, temp.Length);
                    rtf.SetForeColor(settings.SymbolColor);
                    rtf.AddText(ch.ToString(), settings.SymbolStyle);
                    state = 5;
                    continue;
                }

                Label_0306:
                temp.Append(ch);
            }

            return rtf.ToString();
        }

        //static void GetPosition(XmlNode findNode, ref int position, int rankNewLine, bool isPrimary)
        //{
        //    if (findNode.ParentNode == null)
        //    {
        //        position = position + findNode.Name.Length * 2 + 5;
        //        return;
        //    }

        //    foreach (XmlNode nodeBrothers in findNode.ParentNode.ChildNodes)
        //    {
        //        if (nodeBrothers.Equals(findNode))
        //        {
        //            rankNewLine = rankNewLine - 1;
        //            GetPosition(findNode.ParentNode, ref position, rankNewLine, false);
        //            break;
        //        }

        //        if (isPrimary && nodeBrothers.Attributes != null)
        //        {
        //            if (GetPositionInNode(findNode, nodeBrothers, ref position, rankNewLine))
        //            {
        //                rankNewLine = rankNewLine - 1;
        //                GetPosition(findNode.ParentNode, ref position, rankNewLine, false);
        //                break;
        //            }

        //            AppendPossAndSpaces(nodeBrothers, ref position, rankNewLine);
        //        }
        //        else
        //            AppendPossAndSpaces(nodeBrothers, ref position, rankNewLine);
        //    }
        //}

        //static void AppendPossAndSpaces(XmlNode nodeBrothers, ref int position, int rankNewLine)
        //{
        //    position = position + nodeBrothers.OuterXml.Length + rankNewLine * 3;
        //    AppendSpaces(nodeBrothers, ref position, rankNewLine);
        //}

        //static void AppendSpaces(XmlNode nodeBrothers, ref int position, int rankNewLine)
        //{
        //    foreach (XmlNode childNode in nodeBrothers.ChildNodes)
        //    {
        //        position = position + rankNewLine * 3 + 1;
        //        AppendSpaces(childNode, ref position, rankNewLine + 1);
        //    }
        //}
        //static bool GetPositionInNode(XmlNode findNode, XmlNode nodeBrothers, ref int position, int rankNewLine)
        //{
        //    int getPosAttr = 0;
        //    if (nodeBrothers.Attributes == null)
        //        return false;

        //    foreach (XmlAttribute attribute in nodeBrothers.Attributes)
        //    {
        //        getPosAttr = getPosAttr + $" {attribute.Name}=\"{attribute.InnerXml}\"".Length;
        //        if (attribute.Equals(findNode))
        //        {
        //            position = position + getPosAttr + nodeBrothers.Name.Length + 1 + rankNewLine * 3;
        //            return true;
        //        }
        //    }

        //    return false;
        //}

        static int ReadCdata(RtfFromXml settings, RtfBuilder rtf, StringBuilder temp, int waitTag, int state, char c)
        {
            if ((c == '>') && temp.ToString().EndsWith("]]"))
            {
                temp.Remove(temp.Length - 2, 2);
                rtf.SetForeColor(settings.CdataColor);
                rtf.AddText(temp.ToString(), settings.CdataStyle);
                temp.Remove(0, temp.Length);
                rtf.SetForeColor(settings.SymbolColor);
                rtf.AddText("]]>", settings.SymbolStyle);
                state = waitTag;
                return state;
            }

            temp.Append(c);
            return state;
        }

        static int ReadComments(RtfFromXml settings, RtfBuilder rtf, StringBuilder temp, int waitTag, int state, char c)
        {
            if ((c == '>') && temp.ToString().EndsWith("--"))
            {
                temp.Remove(temp.Length - 2, 2);
                rtf.SetForeColor(settings.CommentColor);
                rtf.AddText(temp.ToString(), settings.CommentStyle);
                temp.Remove(0, temp.Length);
                rtf.SetForeColor(settings.SymbolColor);
                rtf.AddText("-->", settings.SymbolStyle);
                state = waitTag;
                return state;
            }

            temp.Append(c);
            return state;
        }

        static int ReadSpecInText(RtfFromXml settings, RtfBuilder rtf, StringBuilder temp, int waitTag, int state, char c)
        {
            temp.Append(c);
            if ((c == ';') || (c == ' '))
            {
                rtf.SetForeColor(settings.SpecSymbolColor);
                rtf.AddText(temp.ToString(), settings.SpecSymbolStyle);
                temp.Remove(0, temp.Length);
                state = waitTag;
            }

            return state;
        }

        static int ReadSpecInValueDQ(RtfFromXml settings, RtfBuilder rtf, StringBuilder temp, int readValueDQ, int state, char c)
        {
            temp.Append(c);
            if (c == ';')
            {
                if (string.Equals(temp.ToString(), "&#xA;"))
                {
                    rtf.AddText("\n", settings.SpecSymbolStyle);
                }
                else
                {
                    rtf.SetForeColor(settings.SpecSymbolColor);
                    rtf.AddText(temp.ToString(), settings.SpecSymbolStyle);
                }

                temp.Remove(0, temp.Length);
                state = readValueDQ;
            }

            return state;
        }

        static int ReadValueDQ(RtfFromXml settings, RtfBuilder rtf, StringBuilder temp, int waitAttrOrEndOfTag, int readSpecInValueDQ, int state, char c)
        {
            if (c == '"')
            {
                rtf.SetForeColor(settings.ValueColor);
                rtf.AddText(temp.ToString(), settings.ValueStyle);
                temp.Remove(0, temp.Length);
                rtf.SetForeColor(settings.QuoteColor);
                rtf.AddText(c.ToString(), settings.QuoteStyle);
                state = waitAttrOrEndOfTag;
                return state;
            }

            if (c == '&')
            {
                rtf.SetForeColor(settings.ValueColor);
                rtf.AddText(temp.ToString(), settings.ValueStyle);
                temp.Remove(0, temp.Length);
                temp.Append(c);
                state = readSpecInValueDQ;
                return state;
            }

            temp.Append(c);
            return state;
        }

        static int ReadValueSQ(RtfFromXml settings, RtfBuilder rtf, StringBuilder temp, int waitAttrOrEndOfTag, int readSpecInValueSQ, int state, char c)
        {
            if (c == '\'')
            {
                rtf.SetForeColor(settings.ValueColor);
                rtf.AddText(temp.ToString(), settings.ValueStyle);
                temp.Remove(0, temp.Length);
                rtf.SetForeColor(settings.QuoteColor);
                rtf.AddText(c.ToString(), settings.QuoteStyle);
                state = waitAttrOrEndOfTag;
                return state;
            }

            if (c == '&')
            {
                rtf.SetForeColor(settings.ValueColor);
                rtf.AddText(temp.ToString(), settings.ValueStyle);
                temp.Remove(0, temp.Length);
                temp.Append(c);
                state = readSpecInValueSQ;
                return state;
            }

            temp.Append(c);
            return state;
        }

        static int WaitAttrOrEndOfTag(RtfFromXml settings, RtfBuilder rtf, StringBuilder temp, int waitTag, int readAttr, int state, char c)
        {
            if (((c == ' ') || (c == '\r')) || ((c == '\n') || (c == '\t')))
            {
                temp.Append(c);
                return state;
            }

            rtf.SetForeColor(0, 0, 0);
            rtf.AddText(temp.ToString());
            temp.Remove(0, temp.Length);
            if (c == '>')
            {
                rtf.SetForeColor(settings.SymbolColor);
                rtf.AddText(c.ToString(), settings.SymbolStyle);
                state = waitTag;
                return state;
            }

            if ((c == '/') || (c == '?'))
            {
                rtf.SetForeColor(settings.SymbolColor);
                rtf.AddText(c.ToString(), settings.SymbolStyle);
                return state;
            }

            temp.Append(c);
            state = readAttr;
            return state;
        }

        static int WaitEqSymbol(RtfFromXml settings, RtfBuilder rtf, StringBuilder temp, int waitValue, int state, char c)
        {
            if (c == '=')
            {
                rtf.SetForeColor(0, 0, 0);
                rtf.AddText(temp.ToString());
                temp.Remove(0, temp.Length);
                rtf.SetForeColor(settings.SymbolColor);
                rtf.AddText(c.ToString(), settings.SymbolStyle);
                state = waitValue;
                return state;
            }

            temp.Append(c);
            return state;
        }

        static int WaitTag(RtfFromXml settings, RtfBuilder rtf, StringBuilder temp, int readTag, int readSpecInText, int state, char c)
        {
            if (c == '<')
            {
                rtf.SetForeColor(settings.TextColor);
                rtf.AddText(temp.ToString(), settings.TextStyle);
                temp.Remove(0, temp.Length);
                rtf.SetForeColor(settings.SymbolColor);
                rtf.AddText(c.ToString(), settings.SymbolStyle);
                state = readTag;
                return state;
            }

            if (c == '&')
            {
                rtf.SetForeColor(settings.TextColor);
                rtf.AddText(temp.ToString(), settings.TextStyle);
                temp.Remove(0, temp.Length);
                temp.Append(c);
                state = readSpecInText;
                return state;
            }

            temp.Append(c);
            return state;
        }

        static int WaitValue(RtfFromXml settings, RtfBuilder rtf, StringBuilder temp, int readValueSQ, int readValueDQ, int state, char c)
        {
            if (c == '\'')
            {
                rtf.SetForeColor(0, 0, 0);
                rtf.AddText(temp.ToString());
                temp.Remove(0, temp.Length);
                rtf.SetForeColor(settings.QuoteColor);
                rtf.AddText(c.ToString(), settings.QuoteStyle);
                state = readValueSQ;
                return state;
            }

            if (c == '"')
            {
                rtf.SetForeColor(0, 0, 0);
                rtf.AddText(temp.ToString());
                temp.Remove(0, temp.Length);
                rtf.SetForeColor(settings.QuoteColor);
                rtf.AddText(c.ToString(), settings.QuoteStyle);
                state = readValueDQ;
                return state;
            }

            temp.Append(c);
            return state;
        }
    }
}