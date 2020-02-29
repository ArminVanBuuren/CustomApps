using System;
using System.Text.RegularExpressions;

namespace Utils
{
    public static class REGEX
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern">Регулярное выражение</param>
        /// <param name="input">Текст где парсятся группы (?&lt;GROUP1&gt;.+)</param>
        /// <param name="outFormat">Пост-результирующий формат - "Результат: %GROUP1%"</param>
        /// <returns></returns>
        public static string Replacer(this Regex pattern, string input, string outFormat)
        {
            var result = outFormat;
            var groups = pattern.Match(input).Groups;
            foreach (var groupName in pattern.GetGroupNames())
            {
                if (int.TryParse(groupName, out _))
                    continue;

                result = new Regex($@"%\s*{groupName}\s*%", RegexOptions.IgnoreCase).Replace(result, groups[groupName].Value.Trim());
            }

            return result;
        }


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
