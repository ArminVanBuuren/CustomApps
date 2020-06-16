using System;

namespace Utils
{
	public static class BOOLEAN
	{
		public static bool IsBool(string input)
		{
			if (string.IsNullOrEmpty(input))
				return false;
			var trimVal = input.Trim();
			return trimVal.Like("true") || trimVal.Like("false");
		}

		/// <summary>
		/// Преобразовывает значение в булево значение.
		/// </summary>
		/// <param name="value">Объект.</param>
		/// <returns>true или false.</returns>
		public static bool ToBoolean(object value)
		{
			return Convert.ToBoolean(value, COMMON.CommonCulture);
		}

		/// <summary>
		/// Выполняет преобразование строки к <see cref="T:System.Boolean"/>.
		/// </summary>
		/// <param name="value">Строка.</param>
		/// <returns>true, если строка равна "true" или "1"; false в противном случае.</returns>
		public static bool ToBoolean(string value)
		{
			return !value.IsEmpty() && (StringComparer.Ordinal.Equals(value, "1") || StringComparer.OrdinalIgnoreCase.Equals(value, "TRUE"));
		}

		/// <summary>
		/// Выполняет преобразование строки к <see cref="T:System.Boolean?"/>.
		/// </summary>
		/// <param name="value">Строка.</param>
		/// <returns>true, если строка равна "true" или "1"; false в противном случае.</returns>
		public static bool? ToBooleanOrNull(string value)
		{
			if (string.IsNullOrEmpty(value)) return null;
			return (StringComparer.Ordinal.Equals(value, "1") || StringComparer.OrdinalIgnoreCase.Equals(value, "TRUE"));
		}
	}
}
