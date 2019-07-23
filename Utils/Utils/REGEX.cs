using System;
using System.Text.RegularExpressions;

namespace Utils
{
    public static class REGEX
    {
        /// <summary>
        /// Получить RegexOptions из строки
        /// </summary>
        /// <param name="inputStringOptions">должны быть разделены символом '|'</param>
        /// <returns></returns>
        public static RegexOptions GetRegOptions(string inputStringOptions)
        {
            var ropt = RegexOptions.None;
            foreach (var opt in inputStringOptions.Split('|'))
            {
                var rpt = GetRegOptionsEnum(opt.Trim());
                if (rpt != RegexOptions.None)
                    ropt |= rpt;
            }
            return ropt;
        }

        static RegexOptions GetRegOptionsEnum(string str)
        {
            if(str.Equals("multiline", StringComparison.CurrentCultureIgnoreCase))
                return RegexOptions.Multiline;
            else if (str.Equals("ignorecase", StringComparison.CurrentCultureIgnoreCase))
                return RegexOptions.IgnoreCase;
            else if (str.Equals("singleline", StringComparison.CurrentCultureIgnoreCase))
                return RegexOptions.Singleline;
            else if (str.Equals("righttoleft", StringComparison.CurrentCultureIgnoreCase))
                return RegexOptions.RightToLeft;
            return RegexOptions.None;
        }
    }
}
