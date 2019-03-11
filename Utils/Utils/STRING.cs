﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class STRING
    {
        private static Random random = new Random();
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
            return string.IsNullOrEmpty(value) || string.IsNullOrEmpty(value.Trim());
        }

        public static string ToStringIsNullOrEmptyTrim(this string value)
        {
            return string.IsNullOrEmpty(value) || string.IsNullOrEmpty(value.Trim()) ? "Null" : value;
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

        public static bool Like(this string input, string value)
        {
            if (value == null)
                return false;
            return input.Equals(value, StringComparison.CurrentCultureIgnoreCase);
        }

        
    }
}
