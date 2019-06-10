using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (value == null)
                return false;
            return input.Equals(value, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
