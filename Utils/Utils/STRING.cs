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

        /// <summary>
        /// Взвращает дефолтное значение если параметр равен null
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetDefaultValue(this string value, string defaultValue)
        {
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNullOrEmptyTrim(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static string ToStringIsNullOrEmptyTrim(this string value)
        {
            if (value == null)
                return "Null";
            return string.IsNullOrWhiteSpace(value) ? "Empty" : value;
        }

        public static bool IsNumber(this string value)
        {
            return int.TryParse(value, out _);
        }

        public static string TrimEnd(this string input, string suffixToRemove, StringComparison comparisonType = StringComparison.CurrentCultureIgnoreCase)
        {
            if (input != null && suffixToRemove != null && input.EndsWith(suffixToRemove, comparisonType))
            {
                return input.Substring(0, input.Length - suffixToRemove.Length);
            }
            else return input;
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

        public static bool StringEquals(this string param1, string param2, bool ignoreCase)
        {
            if (ignoreCase)
                return param1.Like(param2);
            else
                return param1.Equals(param2);
        }

        public static bool Like(this string input, string value)
        {
            return value != null && input.Equals(value, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) != -1;
        }

        public static bool Contains2(this string source, string toCheck, StringComparison? comp = null)
        {
            int[] c = new int[toCheck.Length];
            if (comp != null)
            {
                for (int y = 0; y < toCheck.Length; y++)
                    c[y] += ((source.Length - source.Replace(toCheck, string.Empty, comp.Value).Length) / toCheck.Length > 0 ? 1 : 0);
            }
            else
            {
                for (int y = 0; y < toCheck.Length; y++)
                    c[y] += ((source.Length - source.Replace(toCheck, string.Empty).Length) / toCheck.Length > 0 ? 1 : 0);
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
        public static string Replace(this string str,
            string oldValue, string @newValue,
            StringComparison comparisonType)
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
    }
}
