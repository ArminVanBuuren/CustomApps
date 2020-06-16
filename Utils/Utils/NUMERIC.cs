using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Utils
{
	public static class NUMERIC
	{
		static readonly Regex getNotNumber = new Regex(@"[^0-9]", RegexOptions.Compiled);

		/// <summary>
		/// Обработчик который корректно провыеряет поле, чтобы ввод был строго числовой. Также вставляется позиция корретки
		/// </summary>
		/// <param name="oldValue"></param>
		/// <param name="caretIndex"></param>
		/// <param name="maxLength"></param>
		public static void GetOnlyNumberWithCaret(ref string oldValue, ref int caretIndex, int maxLength)
		{
			var correctValue = getNotNumber.Replace(oldValue, "");
			if (correctValue.Length > maxLength)
				correctValue = correctValue.Substring(0, maxLength);

			int newCoretIndex;
			if (oldValue.Length > correctValue.Length)
				newCoretIndex = caretIndex - (oldValue.Length - correctValue.Length);
			else
				newCoretIndex = caretIndex;

			caretIndex = newCoretIndex < 0 ? 0 : newCoretIndex > correctValue.Length ? correctValue.Length : newCoretIndex;
			oldValue = correctValue;
		}

        public static bool IsNumber(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            var charIn = input.ToCharArray(0, input.Length);
            int i = 0, j = 0;
            foreach (var ch in charIn)
            {
                if (char.IsWhiteSpace(ch))
                    continue;

                if (!char.IsNumber(ch))
                {
                    if ((ch == '.' || ch == ',') && i == 0)
                        i++;
                    else switch (ch)
                    {
                        case '-' when j == 0:
                            break;
                        default:
                            return false;
                    }
                }

                j++;
            }
            return true;
        }

        /// <summary>
        /// Проверка на четность числа
        /// </summary>
        public static bool IsParity(this int dbl)
        {
            return dbl % 2 == 0;
        }


		/// <summary>
		/// Возвращает значение, показывающее, входит ли значение в отрезок между <paramref name="minValue"/> и <paramref name="maxValue"/>.
		/// </summary>
		/// <remarks>
		/// Не производится проверки того, что <paramref name="minValue"/> меньше <paramref name="maxValue"/>. Данное условие необходимо
		/// обеспечить вызывающему коду.
		/// </remarks>
		/// <param name="value">Значение.</param>
		/// <param name="minValue">Нижняя граница отрезка.</param>
		/// <param name="maxValue">Верхняя граница отрезка.</param>
		/// <returns>true, если входит; false в противном случае.</returns>
		public static bool IsInSegment(int value, int minValue, int maxValue)
		{
			return minValue <= value && value <= maxValue;
		}

		/// <summary>
		/// Возвращает значение, показывающее, входит ли значение в отрезок между <paramref name="minValue"/> и <paramref name="maxValue"/>.
		/// </summary>
		/// <remarks>
		/// Не производится проверки того, что <paramref name="minValue"/> меньше <paramref name="maxValue"/>. Данное условие необходимо
		/// обеспечить вызывающему коду.
		/// </remarks>
		/// <param name="value">Значение.</param>
		/// <param name="minValue">Нижняя граница отрезка.</param>
		/// <param name="maxValue">Верхняя граница отрезка.</param>
		/// <returns>true, если входит; false в противном случае.</returns>
		public static bool IsInSegment(decimal value, decimal minValue, decimal maxValue)
		{
			return minValue <= value && value <= maxValue;
		}

		/// <summary>
		/// Возвращает значение, показывающее, входит ли значение в отрезок между <paramref name="minValue"/> и <paramref name="maxValue"/>.
		/// </summary>
		/// <remarks>
		/// Не производится проверки того, что <paramref name="minValue"/> меньше <paramref name="maxValue"/>. Данное условие необходимо
		/// обеспечить вызывающему коду.
		/// </remarks>
		/// <param name="value">Значение.</param>
		/// <param name="minValue">Нижняя граница отрезка.</param>
		/// <param name="maxValue">Верхняя граница отрезка.</param>
		/// <returns>true, если входит; false в противном случае.</returns>
		public static bool IsInSegment(string value, int minValue, int maxValue)
		{
			return value != null && IsInSegment(value.Length, minValue, maxValue);
		}

		/// <summary>
		/// Возвращает значение, показывающее, есть ли в массиве <paramref name="array"/> элемент с индексом
		/// <paramref name="index"/>.
		/// </summary>
		/// <param name="index">Индекс элемента.</param>
		/// <param name="array">Массив. Может быть null.</param>
		/// <returns>true, если в указанном массиве есть элемент с указанным индексом; false в противном случае.</returns>
		public static bool IsInSegment<T>(int index, T[] array)
		{
			return !array.IsEmpty() && IsInSegment(index, 0, array.Length - 1);
		}

		/// <summary>
		/// Проверяет, входит ли индекс в указанный список.
		/// </summary>
		/// <typeparam name="T">Тип списка.</typeparam>
		/// <param name="index">Индекс.</param>
		/// <param name="list">Список. Может быть null.</param>
		/// <returns>true, если в указанном массиве есть элемент с указанным индексом; false в противном случае.</returns>
		public static bool IsInSegment<T>(int index, IList<T> list)
		{
			return !list.IsEmpty() && IsInSegment(index, 0, list.Count - 1);
		}

		/// <summary>
		/// Возвращает значение, показывающее, есть ли в таблице <paramref name="table"/> строка с индексом
		/// <paramref name="rowIndex"/>.
		/// </summary>
		/// <param name="rowIndex">Индекс строки.</param>
		/// <param name="table">Таблица. Может быть null.</param>
		/// <returns>true, если в таблице есть строка с указанным индексом; false в противном случае.</returns>
		public static bool IsInSegment(int rowIndex, DataTable table)
		{
			return !table.IsEmpty() && IsInSegment(rowIndex, 0, table.Rows.Count - 1);
		}


		/// <summary>
		/// Преобразовывает значение к целочисленному.
		/// </summary>
		/// <param name="value">Значение.</param>
		/// <returns>Целочисленное значение; 0, если объект нулевой.</returns>
		public static int ToInt32(object value)
		{
			return Convert.ToInt32(value, COMMON.CommonCulture);
		}

		/// <summary>
		/// Преобразовывает значение к целочисленному.
		/// </summary>
		/// <param name="value">Объект.</param>
		/// <param name="errorValue">Значение, возвращаемое, если объект пустой.</param>
		/// <returns>Эквивалент объекта. <paramref name="errorValue"/>, если объект пустой.</returns>
		public static int ToInt32(object value, int errorValue)
		{
			return COMMON.IsNull(value) ? errorValue : Convert.ToInt32(value, COMMON.CommonCulture);
		}

		/// <summary>
		/// Преобразовывает значение к целочисленному.
		/// </summary>
		/// <param name="value">Объект.</param>
		/// <returns>Целочисленное значение объекта или null.</returns>
		public static int? ToInt32OrNull(object value)
		{
			return COMMON.IsNull(value) ? (int?)null : Convert.ToInt32(value, COMMON.CommonCulture);
		}

		/// <summary>
		/// Преобразовывает значение к целочисленному.
		/// </summary>
		/// <param name="value">Строка.</param>
		/// <returns>Целочисленное значение строки или null.</returns>
		public static int? ToInt32OrNull(string value)
		{
			return int.TryParse(value, NumberStyles.Integer, COMMON.CommonCulture, out var val) ? val : (int?)null;
		}

		/// <summary>
		/// Преобразовывает значение к целочисленному.
		/// </summary>
		/// <param name="value">Строка.</param>
		/// <returns>Целое число.</returns>
		public static int ToInt32(string value)
		{
			return Convert.ToInt32(value, COMMON.CommonCulture);
		}

		/// <summary>
		/// Преобразовывает значение к целочисленному.
		/// </summary>
		/// <param name="value">Строка.</param>
		/// <param name="errorValue">Значение, которое будет возвращено при ошибке преобразования.</param>
		/// <returns>Эквивалент строки или <paramref name="errorValue"/> в случае ошибки.</returns>
		public static int ToInt32(string value, int errorValue)
		{
			return int.TryParse(value, NumberStyles.Integer, COMMON.CommonCulture, out var parsed) ? parsed : errorValue;
		}

		/// <summary>
		/// Выполняет преобразование булевого числа в целое.
		/// </summary>
		/// <param name="value">Булево значение.</param>
		/// <returns>1, если значение true; 0 в противном случае.</returns>
		public static int ToInt32(bool value)
		{
			return value ? 1 : 0;
		}

		/// <summary>
		/// Преобразовывает значение к целочисленному.
		/// </summary>
		/// <param name="value">Значение.</param>
		/// <returns>Целочисленное значение. 0, если объект null.</returns>
		public static long ToInt64(object value)
		{
			return Convert.ToInt64(value, COMMON.CommonCulture);
		}

		/// <summary>
		/// Преобразовывает значение к целочисленному.
		/// </summary>
		/// <param name="value">Объект.</param>
		/// <returns>Целочисленное значение строки или null.</returns>
		public static long? ToInt64OrNull(object value)
		{
			return COMMON.IsNull(value) ? (long?)null : ToInt64(value);
		}

		/// <summary>
		/// Преобразовывает значение к целочисленному.
		/// </summary>
		/// <param name="value">Объект.</param>
		/// <param name="errorValue">Значение, которое будет возвращено при ошибке преобразования.</param>
		/// <returns>Эквивалент строки или <paramref name="errorValue"/> в случае ошибки.</returns>
		public static long? ToInt64OrNull(string value, long? errorValue)
		{
			return long.TryParse(value, NumberStyles.Integer, COMMON.CommonCulture, out var parsed)
				? parsed
				: errorValue;
		}

		/// <summary>
		/// Преобразовывает значение к целочисленному.
		/// </summary>
		/// <param name="value">Строка.</param>
		/// <returns>Эквивалент строки.</returns>
		public static long ToInt64(string value)
		{
			return long.Parse(value, NumberStyles.Integer, COMMON.CommonCulture);
		}

		/// <summary>
		/// Преобразовывает значение к целочисленному.
		/// </summary>
		/// <param name="value">Строка.</param>
		/// <param name="errorValue">Значение, которое будет возвращено при ошибке преобразования.</param>
		/// <returns>Эквивалент строки или <paramref name="errorValue"/> в случае ошибки.</returns>
		public static long ToInt64(string value, long errorValue)
		{
			return long.TryParse(value, NumberStyles.Integer, COMMON.CommonCulture, out var parsed) ? parsed : errorValue;
		}

		/// <summary>
		/// Преобразовывает значение к целочисленному.
		/// </summary>
		/// <param name="value">Объект.</param>
		/// <param name="errorValue">Значение, возвращаемое, если объект пустой.</param>
		/// <returns>Эквивалент объекта. <paramref name="errorValue"/>, если объект пустой.</returns>
		public static long ToInt64(object value, long errorValue)
		{
			return COMMON.IsNull(value) ? errorValue : Convert.ToInt64(value, COMMON.CommonCulture);
		}

		/// <summary>
		/// Преобразовывает значение к десятичному.
		/// </summary>
		/// <param name="value">Объект.</param>
		/// <returns>Эквивалент объекта. 0, если объект null.</returns>
		public static decimal ToDecimal(object value)
		{
			return Convert.ToDecimal(value, COMMON.CommonCulture);
		}

		/// <summary>
		/// Преобразовывает значение к десятичному.
		/// </summary>
		/// <param name="value">Объект.</param>
		/// <returns>Эквивалент объекта. 0, если объект null.</returns>
		public static decimal? ToDecimalOrNull(object value)
		{
			return !COMMON.IsNull(value) ? ToDecimal(value) : (decimal?)null;
		}

		/// <summary>
		/// Преобразовывает значение к десятичному.
		/// </summary>
		/// <param name="value">Строка.</param>
		/// <returns>Экземпляр <see cref="T:System.Decimal"/>.</returns>
		public static decimal ToDecimal(string value)
		{
			return decimal.Parse(value, COMMON.CommonCulture);
		}

		/// <summary>
		/// Преобразовывает значение к десятичному.
		/// </summary>
		/// <param name="value">Строка.</param>
		/// <param name="culture">CultureInfo.</param>
		/// <returns>Экземпляр <see cref="T:System.Decimal"/>.</returns>
		public static decimal ToDecimal(string value, CultureInfo culture)
		{
			return decimal.Parse(value, culture);
		}

		/// <summary>
		/// Преобразовывает значение к десятичному.
		/// </summary>
		/// <param name="value">Строка.</param>
		/// <param name="errorValue">Значение, возвращаемое при ошибке преобразования.</param>
		/// <returns>Эквивалент строки или <paramref name="errorValue"/>.</returns>
		public static decimal ToDecimal(string value, decimal errorValue)
		{
			return decimal.TryParse(value, NumberStyles.Number, COMMON.CommonCulture, out var parsed) ? parsed : errorValue;
		}

		/// <summary>
		/// Преобразовывает значение к десятичному значению в денежных единицах.
		/// </summary>
		/// <param name="value">Строка.</param>
		/// <param name="errorValue">Значение, возвращаемое при ошибке преобразования.</param>
		/// <returns>Эквивалент строки или <paramref name="errorValue"/>.</returns>
		public static decimal ToCurrency(string value, decimal errorValue)
		{
			return decimal.TryParse(value, NumberStyles.Currency, COMMON.CommonCulture, out var parsed) ? parsed : errorValue;
		}

		/// <summary>
		/// Преобразовывает значение к десятичному.
		/// </summary>
		/// <param name="value">Объект.</param>
		/// <param name="errorValue">Значение, возвращаемое при ошибке преобразования.</param>
		/// <returns>Эквивалент объекта или <paramref name="errorValue"/>.</returns>
		public static decimal ToDecimal(object value, decimal errorValue)
		{
			if (value == null || value == DBNull.Value)
			{
				return errorValue;
			}
			return ToDecimal(value);
		}

		/// <summary>
		/// Преобразовывает значение к десятичному.
		/// </summary>
		/// <param name="value">Строка.</param>
		/// <param name="errorValue">Значение, возвращаемое при ошибке преобразования.</param>
		/// <returns>Эквивалент строки или <paramref name="errorValue"/>.</returns>
		public static decimal? ToDecimal(string value, decimal? errorValue)
		{
			return decimal.TryParse(value, NumberStyles.Number, COMMON.CommonCulture, out var parsed) ? parsed : errorValue;
		}

		/// <summary>
		/// Проверяет, есть ли в <paramref name="value"/> бит.
		/// </summary>
		/// <param name="value">Значение для проверки.</param>
		/// <param name="bit">Значение, которое должно содержаться в value.</param>
		/// <returns>true, если <paramref name="bit"/> содержится в <paramref name="value"/>; false в противном случае.</returns>
		public static bool HasBit(int value, int bit)
		{
			return (value & bit) == bit;
		}

		/// <summary>
		/// Проверяет, есть ли в <paramref name="value"/> бит.
		/// </summary>
		/// <param name="value">Значение для проверки.</param>
		/// <param name="flag">Значение, которое должно содержаться в value.</param>
		/// <returns>true, если <paramref name="flag"/> содержится в <paramref name="value"/>; false в противном случае.</returns>
		public static bool HasBit<T>(this T value, T flag) where T : struct, IComparable, IConvertible, IFormattable
		{
			return HasBit(ToInt32(value), ToInt32(flag));
		}

		/// <summary>
		/// Проверяет, есть ли в <paramref name="value"/> бит.
		/// </summary>
		/// <param name="value">Значение для проверки.</param>
		/// <param name="bit">Значение, которое должно содержаться в value.</param>
		/// <returns>true, если <paramref name="bit"/> содержится в <paramref name="value"/>; false в противном случае.</returns>
		public static bool HasAnyBit(int value, int bit)
		{
			return (value & bit) != 0;
		}

		/// <summary>
		/// Проверяет, есть ли в <paramref name="value"/> бит.
		/// </summary>
		/// <param name="value">Значение для проверки.</param>
		/// <param name="flag">Значение, которое должно содержаться в value.</param>
		/// <returns>true, если <paramref name="flag"/> содержится в <paramref name="value"/>; false в противном случае.</returns>
		public static bool HasAnyBit<T>(this T value, T flag) where T : struct, IComparable, IConvertible, IFormattable
		{
			return HasAnyBit(ToInt32(value), ToInt32(flag));
		}

		/// <summary>
		/// Проверяет, есть ли в <paramref name="value"/> бит.
		/// </summary>
		/// <param name="value">Значение для проверки.</param>
		/// <param name="flag">Значение, которое должно содержаться в value.</param>
		/// <returns>true, если <paramref name="flag"/> содержится в <paramref name="value"/>; false в противном случае.</returns>
		public static bool HasBit<T>(this T? value, T? flag) where T : struct, IComparable, IConvertible, IFormattable
		{
			return HasBit(value.GetValueOrDefault(), flag.GetValueOrDefault());
		}

		/// <summary>
		/// Преобразовывает значение к double.
		/// </summary>
		/// <param name="value">Значение.</param>
		/// <returns>Преобразованное значение; 0, если объект нулевой.</returns>
		public static double ToDouble(object value)
		{
			return Convert.ToDouble(value, COMMON.CommonCulture);
		}

		/// <summary>
		/// Преобразовывает значение к десятичному.
		/// </summary>
		/// <param name="value">Объект.</param>
		/// <returns>Эквивалент объекта.</returns>
		public static double? ToDoubleOrNull(object value)
		{
			return !COMMON.IsNull(value) ? ToDouble(value) : (double?)null;
		}
	}
}
