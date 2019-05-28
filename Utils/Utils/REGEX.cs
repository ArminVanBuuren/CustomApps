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
                RegexOptions rpt = GetRegOptionsEnum(opt.Trim());
                if (rpt != RegexOptions.None)
                    ropt |= rpt;
                i = 0;
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
