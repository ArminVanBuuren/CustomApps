using System;
using System.Globalization;
using Utils.Properties;

namespace Utils
{
	public static class TIME
	{
		
		/// <summary>
		/// Перевести период дней в годы
		/// </summary>
		/// <param name="span"></param>
		/// <returns></returns>
		public static string ToReadableAgeString(this TimeSpan span)
		{
			return $"{span.Days / 365.25:0}";
		}

		/// <summary>
		/// Первевести в человеко-читаемый формат периода
		/// </summary>
		/// <param name="span"></param>
		/// <returns></returns>
		public static string ToReadableString(this TimeSpan span)
		{
			var duration = span.Duration();
			var formatted = string.Format("{0}{1}{2}{3}",
				duration.Days > 0 ? $"{duration.Days} {duration.Days.GetWordInCorrectCase(Resources.day, Resources.days2, Resources.days5)}" : string.Empty,
				duration.Hours > 0 ? $" {duration.Hours} {duration.Hours.GetWordInCorrectCase(Resources.hour, Resources.hours2, Resources.hours5)}" : string.Empty,
				duration.Minutes > 0 ? $" {duration.Minutes} {duration.Minutes.GetWordInCorrectCase(Resources.minute, Resources.minutes2, Resources.minutes5)}" : string.Empty,
				duration.Seconds > 0 ? $" {duration.Seconds} {duration.Seconds.GetWordInCorrectCase(Resources.second, Resources.seconds2, Resources.seconds5)}" : string.Empty
			).Trim();

			if (formatted.EndsWith(", "))
				formatted = formatted.Substring(0, formatted.Length - 2);

			if (string.IsNullOrEmpty(formatted))
				formatted = $"0 {Resources.seconds5}";

			return formatted;
		}

		/// <summary>
		/// Попытка перевести дату учитывая разные культуры
		/// </summary>
		/// <param name="dateValue"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryParseAnyDate(string dateValue, out DateTime result)
		{
			return TryParseAnyDate(dateValue, DateTimeStyles.None, out result);
		}

		/// <summary>
		/// Попытка перевести дату учитывая разные культуры
		/// </summary>
		/// <param name="dateValue"></param>
		/// <param name="style"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryParseAnyDate(string dateValue, DateTimeStyles style, out DateTime result)
		{
			result = DateTime.MinValue;

			foreach (var cultureInfo in COMMON.GetCurrentCultures())
			{
				if (DateTime.TryParse(dateValue, cultureInfo, style, out result))
					return true;
			}

			foreach (var cultureInfo in COMMON.Cultures)
			{
				if (DateTime.TryParse(dateValue, cultureInfo, style, out result))
					return true;
			}

			return false;
		}

		public static bool TryParseAnyDateExtract(string dateValue, string format, out DateTime result)
		{
			return TryParseAnyDateExtract(dateValue, format, DateTimeStyles.None, out result);
		}

		public static bool TryParseAnyDateExtract(string dateValue, string format, DateTimeStyles style, out DateTime result)
		{
			result = DateTime.MinValue;

			foreach (var cultureInfo in COMMON.GetCurrentCultures())
			{
				if (DateTime.TryParseExact(dateValue, format, cultureInfo, style, out result))
					return true;
			}

			foreach (var cultureInfo in COMMON.Cultures)
			{
				if (DateTime.TryParseExact(dateValue, format, cultureInfo, style, out result))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Преобразовывает объект к дате-времени.
		/// </summary>
		/// <param name="value">Объект.</param>
		/// <returns>Значение типа <see cref="DateTime"/>. Если <paramref name="value"/> <c>null</c>, то возвращается эквивалент <see cref="DateTime.MinValue"/>.</returns>
		public static DateTime ToDateTime(object value)
		{
			return Convert.ToDateTime(value, COMMON.CommonCulture);
		}

		/// <summary>
		/// Преобразовывает объект к дате-времени.
		/// </summary>
		/// <param name="value">Объект.</param>
		/// <returns>Значение типа <see cref="DateTime"/>. Если <paramref name="value"/> <c>null</c>, то возвращается эквивалент <see cref="DateTime.MinValue"/>.</returns>
		public static TimeSpan ToTimeSpan(string value)
		{
			return DateTime.ParseExact(value, "HH:mm:ss", COMMON.CommonCulture).TimeOfDay;
		}

		/// <summary>
		/// Преобразовывает строку к дате-времени с использованием указанного формата.
		/// </summary>
		/// <param name="value">Строка, содержащая дату.</param>
		/// <param name="format">Формат даты.</param>
		/// <returns>Объект типа <see cref="DateTime"/>.</returns>
		public static DateTime ToDateTime(string value, string format)
		{
			return DateTime.ParseExact(value, format, COMMON.CommonCulture);
		}

		/// <summary>
		/// Преобразовывает значение в дату-время.
		/// </summary>
		/// <param name="value">Значение.</param>
		/// <returns><c>null</c>, если значение нулевое; в противном случае значение даты-времени.</returns>
		public static DateTime? ToDateTimeOrNull(object value)
		{
			return COMMON.IsNull(value) ? (DateTime?)null : ToDateTime(value);
		}

		/// <summary>
		/// Преобразовывает значение в дату-время используя указанный формат
		/// </summary>
		/// <param name="value">Значение.</param>
		/// <param name="format">Формат даты.</param>
		/// <returns><c>null</c>, если значение нулевое; в противном случае значение даты-времени.</returns>
		public static DateTime? ToDateTimeOrNull(string value, string format)
		{
			return String.IsNullOrWhiteSpace(value) ? (DateTime?)null : ToDateTime(value, format);
		}

		/// <summary>
		/// Преобразовывает значение в дату-время.
		/// </summary>
		/// <param name="value">Значение.</param>
		/// <param name="errorValue">Значение, возвращаемое, если объект пустой.</param>
		/// <returns><c>DateTime.Now</c>, если значение нулевое; в противном случае заданное значение даты-времени.</returns>
		public static DateTime ToDateTime(object value, DateTime errorValue)
		{
			return COMMON.IsNull(value) ? errorValue : ToDateTime(value);
		}

		/// <summary>
		/// Преобразовывает значение в дату-время.
		/// </summary>
		/// <param name="value">Значение.</param>
		/// <returns><c>DateTime.Now</c>, если значение нулевое; в противном случае заданное значение даты-времени.</returns>
		public static DateTime ToDateTimeOrNow(this DateTime? value)
		{
			return value ?? DateTime.Now;
		}

		/// <summary>
		/// Преобразовывает значение в дату-время.
		/// </summary>
		/// <param name="value">Строка.</param>
		/// <param name="errorValue">Значение, возвращаемое при ошибке преобразования.</param>
		/// <returns>Эквивалент строки или <paramref name="errorValue"/>.</returns>
		public static DateTime? ToDateTime(string value, DateTime? errorValue)
		{
			return DateTime.TryParse(value, COMMON.CommonCulture, DateTimeStyles.None, out var parsed) ? parsed : errorValue;
		}
	}
}