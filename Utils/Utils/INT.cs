using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Win32;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace Utils
{
	public static class INT
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

		static Regex getNotNumber = new Regex(@"[^0-9]", RegexOptions.Compiled);
		/// <summary>
		/// Обработчик который корректно провыеряет поле TextBox из окна приложения чтобы ввод был строго чисел, также вставляется позиция корретки
		/// </summary>
		/// <param name="oldValue"></param>
		/// <param name="caretIndex"></param>
		/// <param name="maxLength"></param>
		public static void GetOnlyNumberWithCaret(ref string oldValue, ref int caretIndex, int maxLength)
		{
			string correctValue = getNotNumber.Replace(oldValue, "");
			if (correctValue.Length > maxLength)
				correctValue = correctValue.Substring(0, maxLength);

			int newCoretIndex = 0;
			if (oldValue.Length > correctValue.Length)
				newCoretIndex = caretIndex - (oldValue.Length - correctValue.Length);
			else
				newCoretIndex = caretIndex;

			caretIndex = newCoretIndex < 0 ? 0 : newCoretIndex > correctValue.Length ? correctValue.Length : newCoretIndex;
			oldValue = correctValue;
		}
    }
}
