using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
            RegexOptions ropt = RegexOptions.None;
            int i = 0;
            foreach (string opt in inputStringOptions.Split('|'))
            {
                RegexOptions rpt = GetRegOptionsEnum(opt);
                if (rpt == RegexOptions.None && i == 0)
                    ropt = rpt;
                else if (rpt != RegexOptions.None)
                    ropt |= rpt;
                i = 0;
            }
            return ropt;
        }

        static RegexOptions GetRegOptionsEnum(string str)
        {
            switch (str.ToLower().Trim())
            {
                case "multiline": return RegexOptions.Multiline;
                case "ignorecase": return RegexOptions.IgnoreCase;
                case "singleline": return RegexOptions.Singleline;
                case "righttoleft": return RegexOptions.RightToLeft;
                default: return RegexOptions.None;
            }
        }
    }
}
