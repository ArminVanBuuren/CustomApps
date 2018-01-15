using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XPathEvaluator.XmlRtfStyle
{
    [Flags]
    public enum RtfFontStyles
    {
        Bold = 1,
        Italic = 2,
        Regular = 0,
        Strikeout = 8,
        Underline = 4,
        GdiCharSet = 204
    }
}
