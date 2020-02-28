using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Utils
{
    public static class STRING
    {
        private static readonly Random random = new Random();
        const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        const string nums = "1234567890";
        const string numsWithLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";

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
