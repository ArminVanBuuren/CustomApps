using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XPathEvaluator.XmlRtfStyle
{
    public class RtfBuilder
    {
        private StringBuilder _body = new StringBuilder();
        private List<string> _fontTable = new List<string>();
        private bool _lastKeyword;
        private List<string> _palette = new List<string>();

        public RtfBuilder()
        {
            SetFont("Arial Cyr");
        }

        private int AddColor(byte r, byte g, byte b)
        {
            if (((r == 0) && (g == 0)) && (b == 0))
            {
                return 0;
            }
            string item = string.Format(@"\red{0}\green{1}\blue{2};", r, g, b);
            if (!_palette.Contains(item))
            {
                _palette.Add(item);
            }
            return (_palette.IndexOf(item) + 1);
        }

        private int AddFont(string fontName)
        {
            if (!_fontTable.Contains(fontName))
            {
                _fontTable.Add(fontName);
            }
            return _fontTable.IndexOf(fontName);
        }

        public void AddKeywords(string str)
        {
            _body.Append(str);
            _lastKeyword = true;
        }

        public void AddLine()
        {
            _body.Append(@"\line");
            _lastKeyword = true;
        }

        public void AddLine(string str)
        {
            AddText(str);
            _body.Append(@"\line");
            _lastKeyword = true;
        }

        public void AddParagraph()
        {
            _body.Append(@"\par");
            _lastKeyword = true;
        }

        public void AddParagraph(string str)
        {
            AddText(str);
            AddParagraph();
        }

        public void AddText(string str)
        {
            if (str != null)
            {
                str = ReplaceSpecialSymbols(str);
                if (_lastKeyword)
                {
                    _body.Append(" ");
                }
                _body.Append(str);
                _lastKeyword = false;
            }
        }

        public void AddText(string str, RtfFontStyles style)
        {

            if (str != string.Empty)
            {
                string str2 = string.Empty;
                string str3 = string.Empty;
                if (str == "ActionNameAlias")
                {
                    str2 = string.Empty;
                    str3 = string.Empty;
                }

                if ((style & RtfFontStyles.Bold) == RtfFontStyles.Bold)
                {
                    str2 = str2 + @"\b";
                    str3 = str3 + @"\b0";
                }
                if ((style & RtfFontStyles.Italic) == RtfFontStyles.Italic)
                {
                    str2 = str2 + @"\i";
                    str3 = str3 + @"\i0";
                }
                if ((style & RtfFontStyles.Underline) == RtfFontStyles.Underline)
                {
                    str2 = str2 + @"\ul";
                    str3 = str3 + @"\ul0";
                }
                if ((style & RtfFontStyles.Strikeout) == RtfFontStyles.Strikeout)
                {
                    str2 = str2 + @"\strike";
                    str3 = str3 + @"\strike0";
                }
                _lastKeyword = true;
                _body.Append(str2);
                AddText(str);
                _body.Append(str3);
                _lastKeyword = true;
            }
        }

        public void Clear()
        {
            _body.Remove(0, _body.Length);
            _palette.Clear();
            _fontTable.Clear();
            _lastKeyword = false;
        }

        private string ReplaceSpecialSymbols(string str)
        {
            bool flag = false;
            if (str.EndsWith("\n"))
            {
                flag = true;
            }
            str = str.Replace(@"\", @"\\");
            str = str.Replace("{", @"\{");
            str = str.Replace("}", @"\}");
            str = str.Replace("\r\n", @"\line ");
            str = str.Replace("\n", @"\line ");
            if (flag)
            {
                _lastKeyword = true;
            }
            return str;
        }

        public void SetFont(int fontHeight)
        {
            _body.AppendFormat(@"\fs{0}", fontHeight);
            _lastKeyword = true;
        }

        public void SetFont(string fontName)
        {
            _body.AppendFormat(@"\f{0}", AddFont(fontName));
            _lastKeyword = true;
        }

        public void SetFont(string fontName, int fontHeight)
        {
            SetFont(fontName);
            SetFont(fontHeight);
        }

        public void SetForeColor(uint color)
        {
            byte r = (byte)((color & 0xff0000) >> 0x10);
            byte g = (byte)((color & 0xff00) >> 8);
            byte b = (byte)(color & 0xff);
            SetForeColor(r, g, b);
        }

        public void SetForeColor(byte r, byte g, byte b)
        {
            _body.AppendFormat(@"\cf{0}", AddColor(r, g, b));
            _lastKeyword = true;
        }

        public void SetParagraphFormat(RtfAlignment align, int indent, int distance)
        {
            if (align == RtfAlignment.Left)
            {
                _body.Append(@"\ql");
            }
            else if (align == RtfAlignment.Right)
            {
                _body.Append(@"\qr");
            }
            else if (align == RtfAlignment.Center)
            {
                _body.Append(@"\qc");
            }
            else if (align == RtfAlignment.Justify)
            {
                _body.Append(@"\qj");
            }
            _body.AppendFormat(@"\fi{0}", indent);
            _body.AppendFormat(@"\sl{0}", distance);
            _lastKeyword = true;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(@"{\rtf1\unicode\ansicpg1252\deff0\deflang1033");
            builder.Append(@"{\fonttbl");
            for (int i = 0; i < _fontTable.Count; i++)
            {
                builder.Append(string.Format(@"{{\f{0}\fswiss\fcharset0 {1};}}", i, _fontTable[i]));
            }
            builder.Append("}");
            if (_palette.Count > 0)
            {
                builder.Append(@"{\colortbl ;");
                foreach (string str in _palette)
                {
                    builder.Append(str);
                }
                builder.Append("}");
            }
            builder.Append(_body);
            builder.Append("}");
            return builder.ToString();
        }
    }
}
