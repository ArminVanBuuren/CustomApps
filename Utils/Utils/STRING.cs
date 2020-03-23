using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Utils
{
    public static class STRING
    {
        static readonly Random random = new Random();
        private const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private const string nums = "1234567890";
        private const string numsWithLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";
        private const string symbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890!@#$%^&*(){}:<>./,\"'=-?\\ ";

        /// <summary>
        /// Возвращает рандомные буквы
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomString(int length)
        {
            return ReturnRandomObject(letters, length);
        }

        /// <summary>
        /// Возвращает рандомные цифры
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomNumbers(int length)
        {
            return ReturnRandomObject(nums, length);
        }

        /// <summary>
        /// Возвращает рандомные цифры и буквы
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomStringNumbers(int length)
        {
            return ReturnRandomObject(numsWithLetters, length);
        }

        /// <summary>
        /// Возвращает рандомные цифры и буквы
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomStringSimbols(int length)
        {
            return ReturnRandomObject(symbols, length);
        }

        static string ReturnRandomObject(string chars, int length)
        {
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNullOrEmptyTrim(this string value)
        {
            return string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value) || string.IsNullOrEmpty(value.TrimWhiteSpaces());
        }

        public static bool IsNumber(this string value)
        {
            return int.TryParse(value, out _);
        }

        public static string TrimEnd(this string input, string suffixToRemove, StringComparison comparisonType = StringComparison.CurrentCultureIgnoreCase)
        {
            if (input != null && suffixToRemove != null && input.EndsWith(suffixToRemove, comparisonType))
                return input.Substring(0, input.Length - suffixToRemove.Length);
            else
                return input;
        }

        /// <summary>
        /// Если объекты примерно равны. Либо значение первого есть во втором, либо значение второго есть в первом. Регистр игнорируется.
        /// </summary>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <returns></returns>
        public static bool IsObjectsSimilar(string param1, string param2)
        {
            return param1.IndexOf(param2, StringComparison.CurrentCultureIgnoreCase) != -1 || param2.IndexOf(param1, StringComparison.CurrentCultureIgnoreCase) != -1;
        }

        public static string TrimWhiteSpaces(this string input)
        {
            return input.Trim('\r', '\n', '\t', ' ');
        }

        public static bool StringEquals(this string input, string value, bool ignoreCase = true)
        {
            if (input.IsNullOrEmpty() && value.IsNullOrEmpty())
                return true;
            else if (input.IsNullOrEmpty())
                return false;
            else if (value.IsNullOrEmpty())
                return false;

            if (ignoreCase)
                return input.Like(value);
            else
                return input.Equals(value);
        }

        public static bool Like(this string input, string value)
        {
            if (input.IsNullOrEmpty() && value.IsNullOrEmpty())
                return true;
            else if (input.IsNullOrEmpty())
                return false;
            else if (value.IsNullOrEmpty())
                return false;

            return input.Equals(value, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool StringContains(this string input, string value, bool ignoreCase = true)
        {
            if (input.IsNullOrEmpty() && value.IsNullOrEmpty())
                return true;
            else if (input.IsNullOrEmpty())
                return false;
            else if (value.IsNullOrEmpty())
                return false;

            if (ignoreCase)
                return input.IndexOf(value, StringComparison.CurrentCultureIgnoreCase) != -1;
            else
                return input.IndexOf(value, StringComparison.Ordinal) != -1;
        }

        public static bool StringContains2(this string input, string value, StringComparison? comp = null)
        {
            int[] c = new int[value.Length];
            if (comp != null)
            {
                for (int y = 0; y < value.Length; y++)
                    c[y] += ((input.Length - input.Replace(value, string.Empty, comp.Value).Length) / value.Length > 0 ? 1 : 0);
            }
            else
            {
                for (int y = 0; y < value.Length; y++)
                    c[y] += ((input.Length - input.Replace(value, string.Empty).Length) / value.Length > 0 ? 1 : 0);
            }

            var total = c.Sum();

            return total != 0;
        }

        /// <summary>
        /// Returns a new string in which all occurrences of a specified string in the current instance are replaced with another 
        /// specified string according the type of search to use for the specified string.
        /// </summary>
        /// <param name="str">The string performing the replace method.</param>
        /// <param name="oldValue">The string to be replaced.</param>
        /// <param name="newValue">The string replace all occurrences of <paramref name="oldValue"/>. 
        /// If value is equal to <c>null</c>, than all occurrences of <paramref name="oldValue"/> will be removed from the <paramref name="str"/>.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
        /// <returns>A string that is equivalent to the current string except that all instances of <paramref name="oldValue"/> are replaced with <paramref name="newValue"/>. 
        /// If <paramref name="oldValue"/> is not found in the current instance, the method returns the current instance unchanged.</returns>
        [DebuggerStepThrough]
        public static string Replace(this string str, string oldValue, string @newValue, StringComparison comparisonType)
        {

            // Check inputs.
            if (str == null)
            {
                // Same as original .NET C# string.Replace behavior.
                throw new ArgumentNullException(nameof(str));
            }
            if (str.Length == 0)
            {
                // Same as original .NET C# string.Replace behavior.
                return str;
            }
            if (oldValue == null)
            {
                // Same as original .NET C# string.Replace behavior.
                throw new ArgumentNullException(nameof(oldValue));
            }
            if (oldValue.Length == 0)
            {
                // Same as original .NET C# string.Replace behavior.
                throw new ArgumentException("String cannot be of zero length.");
            }


            //if (oldValue.Equals(newValue, comparisonType))
            //{
            //This condition has no sense
            //It will prevent method from replacesing: "Example", "ExAmPlE", "EXAMPLE" to "example"
            //return str;
            //}



            // Prepare string builder for storing the processed string.
            // Note: StringBuilder has a better performance than String by 30-40%.
            StringBuilder resultStringBuilder = new StringBuilder(str.Length);



            // Analyze the replacement: replace or remove.
            bool isReplacementNullOrEmpty = string.IsNullOrEmpty(@newValue);



            // Replace all values.
            const int valueNotFound = -1;
            int foundAt;
            int startSearchFromIndex = 0;
            while ((foundAt = str.IndexOf(oldValue, startSearchFromIndex, comparisonType)) != valueNotFound)
            {

                // Append all characters until the found replacement.
                int @charsUntilReplacment = foundAt - startSearchFromIndex;
                bool isNothingToAppend = @charsUntilReplacment == 0;
                if (!isNothingToAppend)
                {
                    resultStringBuilder.Append(str, startSearchFromIndex, @charsUntilReplacment);
                }



                // Process the replacement.
                if (!isReplacementNullOrEmpty)
                {
                    resultStringBuilder.Append(@newValue);
                }


                // Prepare start index for the next search.
                // This needed to prevent infinite loop, otherwise method always start search 
                // from the start of the string. For example: if an oldValue == "EXAMPLE", newValue == "example"
                // and comparisonType == "any ignore case" will conquer to replacing:
                // "EXAMPLE" to "example" to "example" to "example" … infinite loop.
                startSearchFromIndex = foundAt + oldValue.Length;
                if (startSearchFromIndex == str.Length)
                {
                    // It is end of the input string: no more space for the next search.
                    // The input string ends with a value that has already been replaced. 
                    // Therefore, the string builder with the result is complete and no further action is required.
                    return resultStringBuilder.ToString();
                }
            }


            // Append the last part to the result.
            int @charsUntilStringEnd = str.Length - startSearchFromIndex;
            resultStringBuilder.Append(str, startSearchFromIndex, @charsUntilStringEnd);


            return resultStringBuilder.ToString();

        }

        public class EncodingParcer
        {
            internal EncodingParcer(Encoding from, Encoding to)
            {
                From = from;
                To = to;
            }
            public Encoding From { get; }
            public Encoding To { get; }
            public override string ToString()
            {
                return $"From[{From.HeaderName}] To[{To.HeaderName}]";
            }
        }

        /// <summary>
        /// Определить правильную кодировку по неизвестному тексту используя регулярные выражения
        /// </summary>
        /// <param name="source"></param>
        /// <param name="regexPatternMatcher">Паттерн регулярного выражения с чем текст должен совпадать. Например [А-я]</param>
        /// <returns></returns>
        public static Dictionary<EncodingParcer, string> GetEncoding(this string source, string regexPatternMatcher)
        {
            //const string source = @"Ïðîöåññ íå ìîæåò ïîëó÷èòü äîñòóï ê ôàéëó ""C:\@MyRepos\TestingPrj\CalculateCRC\CalculateCRC\bin\Debug\2\A7563639.bin"", òàê êàê ýòîò ôàéë èñïîëüçóåòñÿ äðóãèì ïðîöåññîì.";
            //const string destination = @"Процесс не может получить доступ к файлу ""C:\@MyRepos\TestingPrj\CalculateCRC\CalculateCRC\bin\Debug\2\A7563639.bin"", так как этот файл используется другим процессом.";
            var collectionMatches = new Dictionary<EncodingParcer, string>();
            var regex = new Regex(regexPatternMatcher, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            foreach (var sourceEncInfo in Encoding.GetEncodings())
            {
                var encodingSource = sourceEncInfo.GetEncoding();
                var bytes = encodingSource.GetBytes(source);
                foreach (var targetEncInfo in Encoding.GetEncodings())
                {
                    var targetEncoding = targetEncInfo.GetEncoding();
                    var encoding = new EncodingParcer(encodingSource, targetEncoding);
                    var res = targetEncoding.GetString(bytes);

                    if (regex.IsMatch(res))
                    {
                        collectionMatches.Add(encoding, res);
                    }
                }
            }

            return collectionMatches;
        }

        public static string StringConvert(this string source, Encoding from, Encoding to)
        {
            return to.GetString(from.GetBytes(source));
        }

        public static string ReplaceUTFCodeToSymbol(this string str)
        {
            var builder = new StringBuilder(str.Length);
            var builderChar = new StringBuilder(6);
            int start = 0;
            int startUnicode = 0;
            foreach (var ch in str)
            {
                if (ch == '\\')
                {
                    start++;
                    builderChar.Append(ch);
                    continue;
                }
                else if ((ch == 'u' || ch == 'U') && start == 1)
                {
                    startUnicode++;
                    builderChar.Append(ch);
                    continue;
                }

                if (startUnicode > 0)
                {
                    if (startUnicode <= 4)
                    {
                        startUnicode++;
                        builderChar.Append(ch);
                        continue;
                    }
                    else if (startUnicode == 5)
                    {
                        if (GetCharByUTFCode(builderChar.ToString(2, builderChar.Length - 2), out var ch2))
                        {
                            builder.Append(ch2);
                            builderChar.Clear();
                        }
                    }
                }

                start = 0;
                startUnicode = 0;
                if (builderChar.Length > 0)
                {
                    builder.Append(builderChar);
                    builderChar.Clear();
                }

                builder.Append(ch);
            }

            builder.Append(builderChar);
            var res = builder.ToString();
            return res;
        }

        static bool GetCharByUTFCode(string input, out char result)
        {
            result = char.MinValue;
            switch (input)
            {
                case "0000": result = '�'; return true;
                case "0001": result = ''; return true;
                case "0002": result = ''; return true;
                case "0003": result = ''; return true;
                case "0004": result = ''; return true;
                case "0005": result = ''; return true;
                case "0006": result = ''; return true;
                case "0007": result = ''; return true;
                case "0008": result = ''; return true;
                case "0009": result = '	'; return true;
                case "000B": result = ''; return true;
                case "000C": result = ''; return true;
                case "000E": result = ''; return true;
                case "000F": result = ''; return true;
                case "0010": result = ''; return true;
                case "0011": result = ''; return true;
                case "0012": result = ''; return true;
                case "0013": result = ''; return true;
                case "0014": result = ''; return true;
                case "0015": result = ''; return true;
                case "0016": result = ''; return true;
                case "0017": result = ''; return true;
                case "0018": result = ''; return true;
                case "0019": result = ''; return true;
                case "001A": result = ''; return true;
                case "001B": result = ''; return true;
                case "001C": result = ''; return true;
                case "001D": result = ''; return true;
                case "001E": result = ''; return true;
                case "001F": result = ''; return true;
                case "0020": result = ' '; return true;
                case "0021": result = '!'; return true;
                case "0022": result = '"'; return true;
                case "0023": result = '#'; return true;
                case "0024": result = '$'; return true;
                case "0025": result = '%'; return true;
                case "0026": result = '&'; return true;
                case "0027": result = '\''; return true;
                case "0028": result = '('; return true;
                case "0029": result = ')'; return true;
                case "002A": result = '*'; return true;
                case "002B": result = '+'; return true;
                case "002C": result = ','; return true;
                case "002D": result = '-'; return true;
                case "002E": result = '.'; return true;
                case "002F": result = '/'; return true;
                case "0030": result = '0'; return true;
                case "0031": result = '1'; return true;
                case "0032": result = '2'; return true;
                case "0033": result = '3'; return true;
                case "0034": result = '4'; return true;
                case "0035": result = '5'; return true;
                case "0036": result = '6'; return true;
                case "0037": result = '7'; return true;
                case "0038": result = '8'; return true;
                case "0039": result = '9'; return true;
                case "003A": result = ':'; return true;
                case "003B": result = ';'; return true;
                case "003C": result = '<'; return true;
                case "003D": result = '='; return true;
                case "003E": result = '>'; return true;
                case "003F": result = '?'; return true;
                case "0040": result = '@'; return true;
                case "0041": result = 'A'; return true;
                case "0042": result = 'B'; return true;
                case "0043": result = 'C'; return true;
                case "0044": result = 'D'; return true;
                case "0045": result = 'E'; return true;
                case "0046": result = 'F'; return true;
                case "0047": result = 'G'; return true;
                case "0048": result = 'H'; return true;
                case "0049": result = 'I'; return true;
                case "004A": result = 'J'; return true;
                case "004B": result = 'K'; return true;
                case "004C": result = 'L'; return true;
                case "004D": result = 'M'; return true;
                case "004E": result = 'N'; return true;
                case "004F": result = 'O'; return true;
                case "0050": result = 'P'; return true;
                case "0051": result = 'Q'; return true;
                case "0052": result = 'R'; return true;
                case "0053": result = 'S'; return true;
                case "0054": result = 'T'; return true;
                case "0055": result = 'U'; return true;
                case "0056": result = 'V'; return true;
                case "0057": result = 'W'; return true;
                case "0058": result = 'X'; return true;
                case "0059": result = 'Y'; return true;
                case "005A": result = 'Z'; return true;
                case "005B": result = '['; return true;
                case "005C": result = '\\'; return true;
                case "005D": result = ']'; return true;
                case "005E": result = '^'; return true;
                case "005F": result = '_'; return true;
                case "0060": result = '`'; return true;
                case "0061": result = 'a'; return true;
                case "0062": result = 'b'; return true;
                case "0063": result = 'c'; return true;
                case "0064": result = 'd'; return true;
                case "0065": result = 'e'; return true;
                case "0066": result = 'f'; return true;
                case "0067": result = 'g'; return true;
                case "0068": result = 'h'; return true;
                case "0069": result = 'i'; return true;
                case "006A": result = 'j'; return true;
                case "006B": result = 'k'; return true;
                case "006C": result = 'l'; return true;
                case "006D": result = 'm'; return true;
                case "006E": result = 'n'; return true;
                case "006F": result = 'o'; return true;
                case "0070": result = 'p'; return true;
                case "0071": result = 'q'; return true;
                case "0072": result = 'r'; return true;
                case "0073": result = 's'; return true;
                case "0074": result = 't'; return true;
                case "0075": result = 'u'; return true;
                case "0076": result = 'v'; return true;
                case "0077": result = 'w'; return true;
                case "0078": result = 'x'; return true;
                case "0079": result = 'y'; return true;
                case "007A": result = 'z'; return true;
                case "007B": result = '{'; return true;
                case "007C": result = '|'; return true;
                case "007D": result = '}'; return true;
                case "007E": result = '~'; return true;
                case "007F": result = ''; return true;
                case "0080": result = '€'; return true;
                case "0081": result = ''; return true;
                case "0082": result = '‚'; return true;
                case "0083": result = 'ƒ'; return true;
                case "0084": result = '„'; return true;
                case "0085": result = '…'; return true;
                case "0086": result = '†'; return true;
                case "0087": result = '‡'; return true;
                case "0088": result = 'ˆ'; return true;
                case "0089": result = '‰'; return true;
                case "008A": result = 'Š'; return true;
                case "008B": result = '‹'; return true;
                case "008C": result = 'Œ'; return true;
                case "008D": result = ''; return true;
                case "008E": result = 'Ž'; return true;
                case "008F": result = ''; return true;
                case "0090": result = ''; return true;
                case "0091": result = '‘'; return true;
                case "0092": result = '’'; return true;
                case "0093": result = '“'; return true;
                case "0094": result = '”'; return true;
                case "0095": result = '•'; return true;
                case "0096": result = '–'; return true;
                case "0097": result = '—'; return true;
                case "0098": result = '˜'; return true;
                case "0099": result = '™'; return true;
                case "009A": result = 'š'; return true;
                case "009B": result = '›'; return true;
                case "009C": result = 'œ'; return true;
                case "009D": result = ''; return true;
                case "009E": result = 'ž'; return true;
                case "009F": result = 'Ÿ'; return true;
                case "00A0": result = ' '; return true;
                case "00A1": result = '¡'; return true;
                case "00A2": result = '¢'; return true;
                case "00A3": result = '£'; return true;
                case "00A4": result = '¤'; return true;
                case "00A5": result = '¥'; return true;
                case "00A6": result = '¦'; return true;
                case "00A7": result = '§'; return true;
                case "00A8": result = '¨'; return true;
                case "00A9": result = '©'; return true;
                case "00AA": result = 'ª'; return true;
                case "00AB": result = '«'; return true;
                case "00AC": result = '¬'; return true;
                case "00AD": result = '­'; return true;
                case "00AE": result = '®'; return true;
                case "00AF": result = '¯'; return true;
                case "00B0": result = '°'; return true;
                case "00B1": result = '±'; return true;
                case "00B2": result = '²'; return true;
                case "00B3": result = '³'; return true;
                case "00B4": result = '´'; return true;
                case "00B5": result = 'µ'; return true;
                case "00B6": result = '¶'; return true;
                case "00B7": result = '·'; return true;
                case "00B8": result = '¸'; return true;
                case "00B9": result = '¹'; return true;
                case "00BA": result = 'º'; return true;
                case "00BB": result = '»'; return true;
                case "00BC": result = '¼'; return true;
                case "00BD": result = '½'; return true;
                case "00BE": result = '¾'; return true;
                case "00BF": result = '¿'; return true;
                case "00C0": result = 'À'; return true;
                case "00C1": result = 'Á'; return true;
                case "00C2": result = 'Â'; return true;
                case "00C3": result = 'Ã'; return true;
                case "00C4": result = 'Ä'; return true;
                case "00C5": result = 'Å'; return true;
                case "00C6": result = 'Æ'; return true;
                case "00C7": result = 'Ç'; return true;
                case "00C8": result = 'È'; return true;
                case "00C9": result = 'É'; return true;
                case "00CA": result = 'Ê'; return true;
                case "00CB": result = 'Ë'; return true;
                case "00CC": result = 'Ì'; return true;
                case "00CD": result = 'Í'; return true;
                case "00CE": result = 'Î'; return true;
                case "00CF": result = 'Ï'; return true;
                case "00D0": result = 'Ð'; return true;
                case "00D1": result = 'Ñ'; return true;
                case "00D2": result = 'Ò'; return true;
                case "00D3": result = 'Ó'; return true;
                case "00D4": result = 'Ô'; return true;
                case "00D5": result = 'Õ'; return true;
                case "00D6": result = 'Ö'; return true;
                case "00D7": result = '×'; return true;
                case "00D8": result = 'Ø'; return true;
                case "00D9": result = 'Ù'; return true;
                case "00DA": result = 'Ú'; return true;
                case "00DB": result = 'Û'; return true;
                case "00DC": result = 'Ü'; return true;
                case "00DD": result = 'Ý'; return true;
                case "00DE": result = 'Þ'; return true;
                case "00DF": result = 'ß'; return true;
                case "00E0": result = 'à'; return true;
                case "00E1": result = 'á'; return true;
                case "00E2": result = 'â'; return true;
                case "00E3": result = 'ã'; return true;
                case "00E4": result = 'ä'; return true;
                case "00E5": result = 'å'; return true;
                case "00E6": result = 'æ'; return true;
                case "00E7": result = 'ç'; return true;
                case "00E8": result = 'è'; return true;
                case "00E9": result = 'é'; return true;
                case "00EA": result = 'ê'; return true;
                case "00EB": result = 'ë'; return true;
                case "00EC": result = 'ì'; return true;
                case "00ED": result = 'í'; return true;
                case "00EE": result = 'î'; return true;
                case "00EF": result = 'ï'; return true;
                case "00F0": result = 'ð'; return true;
                case "00F1": result = 'ñ'; return true;
                case "00F2": result = 'ò'; return true;
                case "00F3": result = 'ó'; return true;
                case "00F4": result = 'ô'; return true;
                case "00F5": result = 'õ'; return true;
                case "00F6": result = 'ö'; return true;
                case "00F7": result = '÷'; return true;
                case "00F8": result = 'ø'; return true;
                case "00F9": result = 'ù'; return true;
                case "00FA": result = 'ú'; return true;
                case "00FB": result = 'û'; return true;
                case "00FC": result = 'ü'; return true;
                case "00FD": result = 'ý'; return true;
                case "00FE": result = 'þ'; return true;
                case "00FF": result = 'ÿ'; return true;
                case "0100": result = 'Ā'; return true;
                case "0101": result = 'ā'; return true;
                case "0102": result = 'Ă'; return true;
                case "0103": result = 'ă'; return true;
                case "0104": result = 'Ą'; return true;
                case "0105": result = 'ą'; return true;
                case "0106": result = 'Ć'; return true;
                case "0107": result = 'ć'; return true;
                case "0108": result = 'Ĉ'; return true;
                case "0109": result = 'ĉ'; return true;
                case "010A": result = 'Ċ'; return true;
                case "010B": result = 'ċ'; return true;
                case "010C": result = 'Č'; return true;
                case "010D": result = 'č'; return true;
                case "010E": result = 'Ď'; return true;
                case "010F": result = 'ď'; return true;
                case "0110": result = 'Đ'; return true;
                case "0111": result = 'đ'; return true;
                case "0112": result = 'Ē'; return true;
                case "0113": result = 'ē'; return true;
                case "0114": result = 'Ĕ'; return true;
                case "0115": result = 'ĕ'; return true;
                case "0116": result = 'Ė'; return true;
                case "0117": result = 'ė'; return true;
                case "0118": result = 'Ę'; return true;
                case "0119": result = 'ę'; return true;
                case "011A": result = 'Ě'; return true;
                case "011B": result = 'ě'; return true;
                case "011C": result = 'Ĝ'; return true;
                case "011D": result = 'ĝ'; return true;
                case "011E": result = 'Ğ'; return true;
                case "011F": result = 'ğ'; return true;
                case "0120": result = 'Ġ'; return true;
                case "0121": result = 'ġ'; return true;
                case "0122": result = 'Ģ'; return true;
                case "0123": result = 'ģ'; return true;
                case "0124": result = 'Ĥ'; return true;
                case "0125": result = 'ĥ'; return true;
                case "0126": result = 'Ħ'; return true;
                case "0127": result = 'ħ'; return true;
                case "0128": result = 'Ĩ'; return true;
                case "0129": result = 'ĩ'; return true;
                case "012A": result = 'Ī'; return true;
                case "012B": result = 'ī'; return true;
                case "012C": result = 'Ĭ'; return true;
                case "012D": result = 'ĭ'; return true;
                case "012E": result = 'Į'; return true;
                case "012F": result = 'į'; return true;
                case "0130": result = 'İ'; return true;
                case "0131": result = 'ı'; return true;
                case "0132": result = 'Ĳ'; return true;
                case "0133": result = 'ĳ'; return true;
                case "0134": result = 'Ĵ'; return true;
                case "0135": result = 'ĵ'; return true;
                case "0136": result = 'Ķ'; return true;
                case "0137": result = 'ķ'; return true;
                case "0138": result = 'ĸ'; return true;
                case "0139": result = 'Ĺ'; return true;
                case "013A": result = 'ĺ'; return true;
                case "013B": result = 'Ļ'; return true;
                case "013C": result = 'ļ'; return true;
                case "013D": result = 'Ľ'; return true;
                case "013E": result = 'ľ'; return true;
                case "013F": result = 'Ŀ'; return true;
                case "0140": result = 'ŀ'; return true;
                case "0141": result = 'Ł'; return true;
                case "0142": result = 'ł'; return true;
                case "0143": result = 'Ń'; return true;
                case "0144": result = 'ń'; return true;
                case "0145": result = 'Ņ'; return true;
                case "0146": result = 'ņ'; return true;
                case "0147": result = 'Ň'; return true;
                case "0148": result = 'ň'; return true;
                case "0149": result = 'ŉ'; return true;
                case "014A": result = 'Ŋ'; return true;
                case "014B": result = 'ŋ'; return true;
                case "014C": result = 'Ō'; return true;
                case "014D": result = 'ō'; return true;
                case "014E": result = 'Ŏ'; return true;
                case "014F": result = 'ŏ'; return true;
                case "0150": result = 'Ő'; return true;
                case "0151": result = 'ő'; return true;
                case "0152": result = 'Œ'; return true;
                case "0153": result = 'œ'; return true;
                case "0154": result = 'Ŕ'; return true;
                case "0155": result = 'ŕ'; return true;
                case "0156": result = 'Ŗ'; return true;
                case "0157": result = 'ŗ'; return true;
                case "0158": result = 'Ř'; return true;
                case "0159": result = 'ř'; return true;
                case "015A": result = 'Ś'; return true;
                case "015B": result = 'ś'; return true;
                case "015C": result = 'Ŝ'; return true;
                case "015D": result = 'ŝ'; return true;
                case "015E": result = 'Ş'; return true;
                case "015F": result = 'ş'; return true;
                case "0160": result = 'Š'; return true;
                case "0161": result = 'š'; return true;
                case "0162": result = 'Ţ'; return true;
                case "0163": result = 'ţ'; return true;
                case "0164": result = 'Ť'; return true;
                case "0165": result = 'ť'; return true;
                case "0166": result = 'Ŧ'; return true;
                case "0167": result = 'ŧ'; return true;
                case "0168": result = 'Ũ'; return true;
                case "0169": result = 'ũ'; return true;
                case "016A": result = 'Ū'; return true;
                case "016B": result = 'ū'; return true;
                case "016C": result = 'Ŭ'; return true;
                case "016D": result = 'ŭ'; return true;
                case "016E": result = 'Ů'; return true;
                case "016F": result = 'ů'; return true;
                case "0170": result = 'Ű'; return true;
                case "0171": result = 'ű'; return true;
                case "0172": result = 'Ų'; return true;
                case "0173": result = 'ų'; return true;
                case "0174": result = 'Ŵ'; return true;
                case "0175": result = 'ŵ'; return true;
                case "0176": result = 'Ŷ'; return true;
                case "0177": result = 'ŷ'; return true;
                case "0178": result = 'Ÿ'; return true;
                case "0179": result = 'Ź'; return true;
                case "017A": result = 'ź'; return true;
                case "017B": result = 'Ż'; return true;
                case "017C": result = 'ż'; return true;
                case "017D": result = 'Ž'; return true;
                case "017E": result = 'ž'; return true;
                case "017F": result = 'ſ'; return true;
                case "0180": result = 'ƀ'; return true;
                case "0181": result = 'Ɓ'; return true;
                case "0182": result = 'Ƃ'; return true;
                case "0183": result = 'ƃ'; return true;
                case "0184": result = 'Ƅ'; return true;
                case "0185": result = 'ƅ'; return true;
                case "0186": result = 'Ɔ'; return true;
                case "0187": result = 'Ƈ'; return true;
                case "0188": result = 'ƈ'; return true;
                case "0189": result = 'Ɖ'; return true;
                case "018A": result = 'Ɗ'; return true;
                case "018B": result = 'Ƌ'; return true;
                case "018C": result = 'ƌ'; return true;
                case "018D": result = 'ƍ'; return true;
                case "018E": result = 'Ǝ'; return true;
                case "018F": result = 'Ə'; return true;
                case "0190": result = 'Ɛ'; return true;
                case "0191": result = 'Ƒ'; return true;
                case "0192": result = 'ƒ'; return true;
                case "0193": result = 'Ɠ'; return true;
                case "0194": result = 'Ɣ'; return true;
                case "0195": result = 'ƕ'; return true;
                case "0196": result = 'Ɩ'; return true;
                case "0197": result = 'Ɨ'; return true;
                case "0198": result = 'Ƙ'; return true;
                case "0199": result = 'ƙ'; return true;
                case "019A": result = 'ƚ'; return true;
                case "019B": result = 'ƛ'; return true;
                case "019C": result = 'Ɯ'; return true;
                case "019D": result = 'Ɲ'; return true;
                case "019E": result = 'ƞ'; return true;
                case "019F": result = 'Ɵ'; return true;
                case "01A0": result = 'Ơ'; return true;
                case "01A1": result = 'ơ'; return true;
                case "01A2": result = 'Ƣ'; return true;
                case "01A3": result = 'ƣ'; return true;
                case "01A4": result = 'Ƥ'; return true;
                case "01A5": result = 'ƥ'; return true;
                case "01A6": result = 'Ʀ'; return true;
                case "01A7": result = 'Ƨ'; return true;
                case "01A8": result = 'ƨ'; return true;
                case "01A9": result = 'Ʃ'; return true;
                case "01AA": result = 'ƪ'; return true;
                case "01AB": result = 'ƫ'; return true;
                case "01AC": result = 'Ƭ'; return true;
                case "01AD": result = 'ƭ'; return true;
                case "01AE": result = 'Ʈ'; return true;
                case "01AF": result = 'Ư'; return true;
                case "01B0": result = 'ư'; return true;
                case "01B1": result = 'Ʊ'; return true;
                case "01B2": result = 'Ʋ'; return true;
                case "01B3": result = 'Ƴ'; return true;
                case "01B4": result = 'ƴ'; return true;
                case "01B5": result = 'Ƶ'; return true;
                case "01B6": result = 'ƶ'; return true;
                case "01B7": result = 'Ʒ'; return true;
                case "01B8": result = 'Ƹ'; return true;
                case "01B9": result = 'ƹ'; return true;
                case "01BA": result = 'ƺ'; return true;
                case "01BB": result = 'ƻ'; return true;
                case "01BC": result = 'Ƽ'; return true;
                case "01BD": result = 'ƽ'; return true;
                case "01BE": result = 'ƾ'; return true;
                case "01BF": result = 'ƿ'; return true;
                case "01C0": result = 'ǀ'; return true;
                case "01C1": result = 'ǁ'; return true;
                case "01C2": result = 'ǂ'; return true;
                case "01C3": result = 'ǃ'; return true;
                case "01C4": result = 'Ǆ'; return true;
                case "01C5": result = 'ǅ'; return true;
                case "01C6": result = 'ǆ'; return true;
                case "01C7": result = 'Ǉ'; return true;
                case "01C8": result = 'ǈ'; return true;
                case "01C9": result = 'ǉ'; return true;
                case "01CA": result = 'Ǌ'; return true;
                case "01CB": result = 'ǋ'; return true;
                case "01CC": result = 'ǌ'; return true;
                case "01CD": result = 'Ǎ'; return true;
                case "01CE": result = 'ǎ'; return true;
                case "01CF": result = 'Ǐ'; return true;
                case "01D0": result = 'ǐ'; return true;
                case "01D1": result = 'Ǒ'; return true;
                case "01D2": result = 'ǒ'; return true;
                case "01D3": result = 'Ǔ'; return true;
                case "01D4": result = 'ǔ'; return true;
                case "01D5": result = 'Ǖ'; return true;
                case "01D6": result = 'ǖ'; return true;
                case "01D7": result = 'Ǘ'; return true;
                case "01D8": result = 'ǘ'; return true;
                case "01D9": result = 'Ǚ'; return true;
                case "01DA": result = 'ǚ'; return true;
                case "01DB": result = 'Ǜ'; return true;
                case "01DC": result = 'ǜ'; return true;
                case "01DD": result = 'ǝ'; return true;
                case "01DE": result = 'Ǟ'; return true;
                case "01DF": result = 'ǟ'; return true;
                case "01E0": result = 'Ǡ'; return true;
                case "01E1": result = 'ǡ'; return true;
                case "01E2": result = 'Ǣ'; return true;
                case "01E3": result = 'ǣ'; return true;
                case "01E4": result = 'Ǥ'; return true;
                case "01E5": result = 'ǥ'; return true;
                case "01E6": result = 'Ǧ'; return true;
                case "01E7": result = 'ǧ'; return true;
                case "01E8": result = 'Ǩ'; return true;
                case "01E9": result = 'ǩ'; return true;
                case "01EA": result = 'Ǫ'; return true;
                case "01EB": result = 'ǫ'; return true;
                case "01EC": result = 'Ǭ'; return true;
                case "01ED": result = 'ǭ'; return true;
                case "01EE": result = 'Ǯ'; return true;
                case "01EF": result = 'ǯ'; return true;
                case "01F0": result = 'ǰ'; return true;
                case "01F1": result = 'Ǳ'; return true;
                case "01F2": result = 'ǲ'; return true;
                case "01F3": result = 'ǳ'; return true;
                case "01F4": result = 'Ǵ'; return true;
                case "01F5": result = 'ǵ'; return true;
                case "01F6": result = 'Ƕ'; return true;
                case "01F7": result = 'Ƿ'; return true;
                case "01F8": result = 'Ǹ'; return true;
                case "01F9": result = 'ǹ'; return true;
                case "01FA": result = 'Ǻ'; return true;
                case "01FB": result = 'ǻ'; return true;
                case "01FC": result = 'Ǽ'; return true;
                case "01FD": result = 'ǽ'; return true;
                case "01FE": result = 'Ǿ'; return true;
                case "01FF": result = 'ǿ'; return true;
            }

            return false;
        }
    }
}
