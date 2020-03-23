using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class UNIVERSAL
    {
        /// <summary>
        /// Взвращает дефолтное значение если параметр равен null
        /// </summary>
        public static T IsNullSetDefault<T>(this T input, T @default) where T: class
        {
            return input ?? @default;
        }

        /// <summary>
        /// Взвращает дефолтное значение если параметр равен null
        /// </summary>
        public static T IsNullSetDefault<T>(this T input, Func<T> @default) where T : class
        {
            return input ?? @default.Invoke();
        }

        /// <summary>
        /// Взвращает дефолтное значение если параметр равен null
        /// </summary>
        /// <param name="value"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public static string IsNullOrEmptySetDefault(this string value, string @default)
        {
            return string.IsNullOrEmpty(value) ? @default : value;
        }
    }
}
