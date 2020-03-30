using System;
using System.Text;
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

        public static string GetValueByReplacement(this Match match, string format)
        {
            var builder = new StringBuilder();
            int opened = 0;
            foreach (var ch in format)
            {
                if (ch == '$')
                {
                    if (opened == 0)
                    {
                        opened++;
                        continue;
                    }
                    else
                    {
                        builder.Append('$');
                        continue;
                    }
                }

                if (opened == 1)
                {
                    if (int.TryParse(ch.ToString(), out var res))
                    {
                        opened = 0;
                        builder.Append(match.Groups[res.ToString()]);
                        continue;
                    }
                    else
                    {
                        opened = 0;
                        builder.Append('$');
                    }
                }

                builder.Append(ch);
            }

            if (opened > 0)
                builder.Append('$');

            return builder.ToString();
        }

        public static bool Verify(string testPattern, out Exception error)
        {
            error = null;
            if (testPattern.IsNullOrEmptyTrim())
            {
                error = new ArgumentException("Regex pattern cannot be empty.");
                return false; //BAD PATTERN: Pattern is null or blank
            }

            try
            {
                Regex.Match("", testPattern);
                return true;
            }
            catch (ArgumentException ex)
            {
                error = ex;
                return false; // BAD PATTERN: Syntax error
            }
            catch (Exception ex)
            {
                error = ex;
                return false;
            }
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
            if(str.Equals("multiline", StringComparison.InvariantCultureIgnoreCase))
                return RegexOptions.Multiline;
            else if (str.Equals("ignorecase", StringComparison.InvariantCultureIgnoreCase))
                return RegexOptions.IgnoreCase;
            else if (str.Equals("singleline", StringComparison.InvariantCultureIgnoreCase))
                return RegexOptions.Singleline;
            else if (str.Equals("righttoleft", StringComparison.InvariantCultureIgnoreCase))
                return RegexOptions.RightToLeft;
            return RegexOptions.None;
        }
    }
}
