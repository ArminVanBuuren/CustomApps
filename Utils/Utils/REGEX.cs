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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="match"></param>
        /// <param name="groupsFormat">Формат группировок. Например = "$1.$2"</param>
        /// <param name="itemFormatExist">Если существует формат даты или валюты. Например = "$1:{dd.MM.yyyy HH:mm:ss.fff}", то срабатывает триггер и вызов функции.</param>
        /// <returns></returns>
        public static string GetValueByReplacement(this Match match, string groupsFormat, Func<string, string, string> itemFormatExist)
        {
	        var builder = new StringBuilder();
	        var builderItemFormat = new StringBuilder();
	        var builderItemTrash = new StringBuilder();
	        var opened = 0;
	        var waitFormat = 0;
	        var currentGroup = 0;
	        foreach (var ch in groupsFormat)
	        {
                label:
		        if (ch == '$' && waitFormat == 0)
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

		        if (waitFormat > 0)
		        {
			        if (ch == ' ' && (waitFormat == 1 || waitFormat == 2))
			        {
				        builderItemTrash.Append(ch);
				        continue;
			        }
			        else if ((ch == ':' && waitFormat == 1) || (ch == '{' && waitFormat == 2))
			        {
				        builderItemTrash.Append(ch);
				        waitFormat++;
				        continue;
			        }
			        else if (ch == '}' && waitFormat == 3)
			        {
				        builder.Append(itemFormatExist(match.Groups[currentGroup.ToString()].Value, builderItemFormat.ToString()));
				        builderItemTrash.Clear();
				        builderItemFormat.Clear();
				        waitFormat = 0;
				        continue;
			        }
			        else if (waitFormat == 3 && ch != '{' && ch != '$')
			        {
				        builderItemFormat.Append(ch);
				        continue;
			        }
			        else if (builderItemTrash.Length > 0 || builderItemFormat.Length > 0)
			        {
				        builder.Append(builderItemTrash);
				        builderItemTrash.Clear();
				        builder.Append(builderItemFormat);
				        builderItemFormat.Clear();
				        waitFormat = 0;
				        goto label;
			        }
		        }

		        if (opened == 1)
		        {
			        if (int.TryParse(ch.ToString(), out var res))
			        {
				        opened = 0;
				        builderItemTrash.Append(match.Groups[res.ToString()].Value);
                        currentGroup = res;
                        waitFormat++;
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

	        if (builderItemTrash.Length > 0)
		        builder.Append(builderItemTrash);

            if (builderItemFormat.Length > 0)
		        builder.Append(builderItemFormat);

            if (opened > 0)
		        builder.Append('$');

	        return builder.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="match"></param>
        /// <param name="groupsFormat">Формат группировок. Например = "$1.$2"</param>
        /// <returns></returns>
        public static string GetValueByReplacement(this Match match, string groupsFormat)
        {
	        var builder = new StringBuilder();
            var opened = 0;
            foreach (var ch in groupsFormat)
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
                        builder.Append(match.Groups[res.ToString()].Value);
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

        public static bool Verify(string testPattern)
        {
            if (testPattern.IsNullOrEmptyTrim())
                return false; //BAD PATTERN: Pattern is null or blank

            try
            {
                Regex.Match("", testPattern);
                return true;
            }
            catch (ArgumentException ex)
            {
                return false; // BAD PATTERN: Syntax error
            }
            catch (Exception ex)
            {
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
