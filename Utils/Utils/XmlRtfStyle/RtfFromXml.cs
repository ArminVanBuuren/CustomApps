using System;
using System.Text;
using System.Xml;

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
            var xml = new XmlDocument();
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
            var settings = new RtfFromXml();
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

            var xml = new XmlDocument();
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

            return GetRtfString(xml.OuterXml, settings);
        }

        static string GetRtfString(string source, RtfFromXml settings)
        {
            var rtf = new RtfBuilder();
            var temp = new StringBuilder(50);
            rtf.SetFont(settings.FontName, settings.FontSize);

            var state = 0;
            foreach (var ch in source)
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