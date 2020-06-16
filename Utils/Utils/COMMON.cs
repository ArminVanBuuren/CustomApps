using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Utils
{
    public static class COMMON
    {
	    private static List<CultureInfo> _cultures;

	    /// <summary>
	    /// Возвращает экземпляр класса <see cref="CultureInfo"/>, используемый для преобразования данных.
	    /// </summary>
	    public static CultureInfo CommonCulture => CultureInfo.InvariantCulture;

	    internal static IEnumerable<CultureInfo> GetCurrentCultures()
	    {
		    return new HashSet<CultureInfo>()
		    {
			    Thread.CurrentThread.CurrentCulture,
			    CultureInfo.InstalledUICulture,
			    CultureInfo.CurrentUICulture,
			    CultureInfo.CurrentCulture,
			    CultureInfo.InvariantCulture,
			    CultureInfo.InstalledUICulture
		    };
	    }

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

        public static AssemblyInfo GetAssemblyInfo(this object input)
        {
            return new AssemblyInfo(input);
        }

        /// <summary>
        /// Преобразовывает параметр к указанному типу
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <exception cref="InvalidOperationException">При попытке преобразовать null к структуре.</exception>
        /// <returns></returns>
        public static T To<T>(object value)
        {
	        var requiredType = typeof(T);

	        if (value == null)
	        {
		        if (!requiredType.IsValueType || Nullable.GetUnderlyingType(requiredType) != null)
			        return default;

		        throw new InvalidOperationException($"Cannot convert null to value type {requiredType.Name}.");
	        }

	        return (T)Convert.ChangeType(value, Nullable.GetUnderlyingType(requiredType) ?? requiredType, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Создаёт исключение.
        /// </summary>
        /// <param name="format">Формат сообщения.</param>
        /// <param name="parameters">Параметры форматирования сообщения.</param>
        /// <returns>Экземпляр <see cref="Exception"/>.</returns>
        public static Exception CreateException(string format, params object[] parameters)
        {
	        return new Exception(STRING.Format(format, parameters));
        }

        /// <summary>
        /// Создаёт исключение.
        /// </summary>
        /// <param name="inner">Исключение, повлекшее данное.</param>
        /// <param name="format">Формат сообщения.</param>
        /// <param name="parameters">Параметры форматирования сообщения.</param>
        /// <returns>Экземпляр <see cref="Exception"/>.</returns>
        public static Exception CreateException(Exception inner, string format, params object[] parameters)
        {
	        return new Exception(STRING.Format(format, parameters), inner);
        }

        /// <summary>
        /// Проверяет, является ли значение нулевым (т.е. оно <c>null</c> или <see cref="DBNull.Value"/>).
        /// </summary>
        /// <param name="value">Значение.</param>
        /// <returns>true, если значение нулевое; false в противном случае.</returns>
        public static bool IsNull(object value)
        {
	        return value == null || Convert.IsDBNull(value);
        }

		/// <summary>
		/// Проверяет, является ли строка пустой или <c>null</c>.
		/// </summary>
		/// <remarks>
		/// <para>
		///		Данный метод должен использоваться вместо <see cref="System.String.IsNullOrEmpty"/> из-за
		///		возможных оптимизаций JIT-компилятора (см. google), который в некоторых случаях убирает
		///		проверку на null.
		/// </para>
		/// </remarks>
		/// <param name="value">Строка для проверки.</param>
		/// <returns>true, если строка пуста (или <c>null</c>); false в противном случае.</returns>
		public static bool IsEmpty(this string value)
		{
			return value == null || value.Length <= 0;
		}

		/// <summary>
		/// Проверяет, есть ли в таблице хотя бы одна строка.
		/// </summary>
		/// <param name="value">Таблица.</param>
		/// <returns>true, в таблице есть хотья бы одна строка; false в противном случае.</returns>
		public static bool IsEmpty(this DataTable value)
		{
			return value == null || value.Rows.Count <= 0;
		}

		/// <summary>
		/// Проверяет, есть ли в наборе хотя бы одна таблица и, в зависимости от флага <paramref name="checkOnlyZeroTable"/>,
		/// проверяется, есть ли хотя бы одна строка в нулевой таблице или в каждой из таблиц.
		/// </summary>
		/// <param name="value">Набор.</param>
		/// <param name="checkOnlyZeroTable">Флаг, показывающий, нужно ли проверять только нулевую таблицу или все.</param>
		/// <returns>true, если набор не пуст; false в противном случае.</returns>
		public static bool IsEmpty(this DataSet value, bool checkOnlyZeroTable)
		{
			if (checkOnlyZeroTable)
				return IsEmpty(value);

			if (value == null || value.Tables.Count <= 0)
				return true;

			for (var i = 0; i < value.Tables.Count; i++)
			{
				if (value.Tables[i].Rows.Count <= 0)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Проверяет, есть ли в наборе таблица с индексом <paramref name="tableIndex"/> и есть ли в этой таблице хотя бы одна строка.
		/// </summary>
		/// <param name="value">Набор.</param>
		/// <param name="tableIndex">Индекс таблицы.</param>
		/// <returns>true, если в наборе есть непустая таблица с индексом <paramref name="tableIndex"/>; false в противном случае.</returns>
		public static bool IsEmpty(this DataSet value, int tableIndex = 0)
		{
			return value == null || value.Tables.Count <= tableIndex || value.Tables[tableIndex].Rows.Count <= 0;
		}

		/// <summary>
		/// Проверяет, есть ли в наборе таблица с названием <paramref name="tableName"/> и есть ли в этой таблице хотя бы одна строка.
		/// </summary>
		/// <param name="value">Набор.</param>
		/// <param name="tableName">Название таблицы.</param>
		/// <returns>true, если в наборе есть непустая таблица с названием <paramref name="tableName"/>; false в противном случае.</returns>
		public static bool IsEmpty(this DataSet value, string tableName)
		{
			return value == null || IsEmpty(value.Tables[tableName]);
		}

		/// <summary>
		/// Проверяет, является ли коллекция пустой.
		/// </summary>
		/// <param name="value">Коллекция.</param>
		/// <returns>true, если коллекция пустая; false в противном случае.</returns>
		public static bool IsEmpty<T>(this ICollection<T> value)
		{
			return value == null || value.Count == 0;
		}

		/// <summary>
		/// Проверяет, является ли массив пустым.
		/// </summary>
		/// <param name="value">Массив.</param>
		/// <returns>true, если массив пустой; false в противном случае.</returns>
		public static bool IsEmpty<T>(this T[] value)
		{
			return value == null || value.Length == 0;
		}

		/// <summary>
		/// Проверяет, является ли перечисляемый тип пустым.
		/// </summary>
		/// <param name="value">Перечисляемый тип.</param>
		/// <returns>true, если перечисляемый тип пустой; false в противном случае.</returns>
		public static bool IsEmpty<T>(this IEnumerable<T> value)
		{
			return value == null || !value.Any();
		}

		/// <summary>
		/// Проверяет, является ли словарь пустым.
		/// </summary>
		/// <typeparam name="TKey">Тип ключа.</typeparam>
		/// <typeparam name="TValue">Тип значения.</typeparam>
		/// <param name="dictionary">Словарь.</param>
		/// <returns>true, если словарь null или не имеет записей; false в противном случае.</returns>
		public static bool IsEmpty<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
		{
			return dictionary == null || dictionary.Count == 0;
		}

		///<summary>
		/// Проверяет, являтся ли значение пустым.
		///</summary>
		public static bool IsEmpty<T>(this T? value) where T : struct
		{
			return !value.HasValue;
		}

		// https://www.basicdatepicker.com/samples/cultureinfo.aspx
		internal static IEnumerable<CultureInfo> Cultures
		{
			get
			{
				if (_cultures != null)
					return _cultures;

				_cultures = new List<CultureInfo>()
				{
					CultureInfo.GetCultureInfo("af-ZA"),
					CultureInfo.GetCultureInfo("sq-AL"),
					CultureInfo.GetCultureInfo("gsw-FR"),
					CultureInfo.GetCultureInfo("am-ET"),
					CultureInfo.GetCultureInfo("ar-DZ"),
					CultureInfo.GetCultureInfo("ar-BH"),
					CultureInfo.GetCultureInfo("ar-EG"),
					CultureInfo.GetCultureInfo("ar-IQ"),
					CultureInfo.GetCultureInfo("ar-JO"),
					CultureInfo.GetCultureInfo("ar-KW"),
					CultureInfo.GetCultureInfo("ar-LB"),
					CultureInfo.GetCultureInfo("ar-LY"),
					CultureInfo.GetCultureInfo("ar-MA"),
					CultureInfo.GetCultureInfo("ar-OM"),
					CultureInfo.GetCultureInfo("ar-QA"),
					CultureInfo.GetCultureInfo("ar-SA"),
					CultureInfo.GetCultureInfo("ar-SY"),
					CultureInfo.GetCultureInfo("ar-TN"),
					CultureInfo.GetCultureInfo("ar-AE"),
					CultureInfo.GetCultureInfo("ar-YE"),
					CultureInfo.GetCultureInfo("hy-AM"),
					CultureInfo.GetCultureInfo("as-IN"),
					CultureInfo.GetCultureInfo("az-Cyrl-AZ"),
					CultureInfo.GetCultureInfo("az-Latn-AZ"),
					CultureInfo.GetCultureInfo("bn-BD"),
					CultureInfo.GetCultureInfo("bn-IN"),
					CultureInfo.GetCultureInfo("ba-RU"),
					CultureInfo.GetCultureInfo("eu-ES"),
					CultureInfo.GetCultureInfo("be-BY"),
					CultureInfo.GetCultureInfo("bs-Cyrl-BA"),
					CultureInfo.GetCultureInfo("bs-Latn-BA"),
					CultureInfo.GetCultureInfo("br-FR"),
					CultureInfo.GetCultureInfo("bg-BG"),
					CultureInfo.GetCultureInfo("my-MM"),
					CultureInfo.GetCultureInfo("ca-ES"),
					CultureInfo.GetCultureInfo("tzm-Latn-DZ"),
					CultureInfo.GetCultureInfo("tzm-Tfng-MA"),
					CultureInfo.GetCultureInfo("ku-Arab-IQ"),
					CultureInfo.GetCultureInfo("chr-Cher-US"),
					CultureInfo.GetCultureInfo("zh-CN"),
					CultureInfo.GetCultureInfo("zh-SG"),
					CultureInfo.GetCultureInfo("zh-HK"),
					CultureInfo.GetCultureInfo("zh-MO"),
					CultureInfo.GetCultureInfo("zh-TW"),
					CultureInfo.GetCultureInfo("co-FR"),
					CultureInfo.GetCultureInfo("hr-HR"),
					CultureInfo.GetCultureInfo("hr-BA"),
					CultureInfo.GetCultureInfo("cs-CZ"),
					CultureInfo.GetCultureInfo("da-DK"),
					CultureInfo.GetCultureInfo("prs-AF"),
					CultureInfo.GetCultureInfo("dv-MV"),
					CultureInfo.GetCultureInfo("nl-BE"),
					CultureInfo.GetCultureInfo("nl-NL"),
					CultureInfo.GetCultureInfo("en-AU"),
					CultureInfo.GetCultureInfo("en-BZ"),
					CultureInfo.GetCultureInfo("en-CA"),
					CultureInfo.GetCultureInfo("en-029"),
					CultureInfo.GetCultureInfo("en-HK"),
					CultureInfo.GetCultureInfo("en-IN"),
					CultureInfo.GetCultureInfo("en-IE"),
					CultureInfo.GetCultureInfo("en-JM"),
					CultureInfo.GetCultureInfo("en-MY"),
					CultureInfo.GetCultureInfo("en-NZ"),
					CultureInfo.GetCultureInfo("en-PH"),
					CultureInfo.GetCultureInfo("en-SG"),
					CultureInfo.GetCultureInfo("en-ZA"),
					CultureInfo.GetCultureInfo("en-TT"),
					CultureInfo.GetCultureInfo("en-GB"),
					CultureInfo.GetCultureInfo("en-US"),
					CultureInfo.GetCultureInfo("en-ZW"),
					CultureInfo.GetCultureInfo("et-EE"),
					CultureInfo.GetCultureInfo("fo-FO"),
					CultureInfo.GetCultureInfo("fil-PH"),
					CultureInfo.GetCultureInfo("fi-FI"),
					CultureInfo.GetCultureInfo("fr-BE"),
					CultureInfo.GetCultureInfo("fr-CM"),
					CultureInfo.GetCultureInfo("fr-CA"),
					CultureInfo.GetCultureInfo("fr-CD"),
					CultureInfo.GetCultureInfo("fr-FR"),
					CultureInfo.GetCultureInfo("fr-HT"),
					CultureInfo.GetCultureInfo("fr-CI"),
					CultureInfo.GetCultureInfo("fr-LU"),
					CultureInfo.GetCultureInfo("fr-ML"),
					CultureInfo.GetCultureInfo("fr-MC"),
					CultureInfo.GetCultureInfo("fr-MA"),
					CultureInfo.GetCultureInfo("fr-RE"),
					CultureInfo.GetCultureInfo("fr-SN"),
					CultureInfo.GetCultureInfo("fr-CH"),
					CultureInfo.GetCultureInfo("fy-NL"),
					CultureInfo.GetCultureInfo("ff-Latn-SN"),
					CultureInfo.GetCultureInfo("gl-ES"),
					CultureInfo.GetCultureInfo("ka-GE"),
					CultureInfo.GetCultureInfo("de-AT"),
					CultureInfo.GetCultureInfo("de-DE"),
					CultureInfo.GetCultureInfo("de-LI"),
					CultureInfo.GetCultureInfo("de-LU"),
					CultureInfo.GetCultureInfo("de-CH"),
					CultureInfo.GetCultureInfo("el-GR"),
					CultureInfo.GetCultureInfo("kl-GL"),
					CultureInfo.GetCultureInfo("gn-PY"),
					CultureInfo.GetCultureInfo("gu-IN"),
					CultureInfo.GetCultureInfo("ha-Latn-NG"),
					CultureInfo.GetCultureInfo("haw-US"),
					CultureInfo.GetCultureInfo("he-IL"),
					CultureInfo.GetCultureInfo("hi-IN"),
					CultureInfo.GetCultureInfo("hu-HU"),
					CultureInfo.GetCultureInfo("is-IS"),
					CultureInfo.GetCultureInfo("ig-NG"),
					CultureInfo.GetCultureInfo("id-ID"),
					CultureInfo.GetCultureInfo("iu-Latn-CA"),
					CultureInfo.GetCultureInfo("iu-Cans-CA"),
					CultureInfo.GetCultureInfo("ga-IE"),
					CultureInfo.GetCultureInfo("xh-ZA"),
					CultureInfo.GetCultureInfo("zu-ZA"),
					CultureInfo.GetCultureInfo("it-IT"),
					CultureInfo.GetCultureInfo("it-CH"),
					CultureInfo.GetCultureInfo("ja-JP"),
					CultureInfo.GetCultureInfo("jv-Latn-ID"),
					CultureInfo.GetCultureInfo("kn-IN"),
					CultureInfo.GetCultureInfo("kk-KZ"),
					CultureInfo.GetCultureInfo("km-KH"),
					CultureInfo.GetCultureInfo("qut-GT"),
					CultureInfo.GetCultureInfo("rw-RW"),
					CultureInfo.GetCultureInfo("sw-KE"),
					CultureInfo.GetCultureInfo("kok-IN"),
					CultureInfo.GetCultureInfo("ko-KR"),
					CultureInfo.GetCultureInfo("ky-KG"),
					CultureInfo.GetCultureInfo("lo-LA"),
					CultureInfo.GetCultureInfo("lv-LV"),
					CultureInfo.GetCultureInfo("lt-LT"),
					CultureInfo.GetCultureInfo("dsb-DE"),
					CultureInfo.GetCultureInfo("lb-LU"),
					CultureInfo.GetCultureInfo("mk-MK"),
					CultureInfo.GetCultureInfo("mg-MG"),
					CultureInfo.GetCultureInfo("ms-BN"),
					CultureInfo.GetCultureInfo("ms-MY"),
					CultureInfo.GetCultureInfo("ml-IN"),
					CultureInfo.GetCultureInfo("mt-MT"),
					CultureInfo.GetCultureInfo("mi-NZ"),
					CultureInfo.GetCultureInfo("arn-CL"),
					CultureInfo.GetCultureInfo("mr-IN"),
					CultureInfo.GetCultureInfo("moh-CA"),
					CultureInfo.GetCultureInfo("mn-MN"),
					CultureInfo.GetCultureInfo("mn-Mong-CN"),
					CultureInfo.GetCultureInfo("mn-Mong-MN"),
					CultureInfo.GetCultureInfo("ne-IN"),
					CultureInfo.GetCultureInfo("ne-NP"),
					CultureInfo.GetCultureInfo("nqo-GN"),
					CultureInfo.GetCultureInfo("nb-NO"),
					CultureInfo.GetCultureInfo("nn-NO"),
					CultureInfo.GetCultureInfo("oc-FR"),
					CultureInfo.GetCultureInfo("or-IN"),
					CultureInfo.GetCultureInfo("om-ET"),
					CultureInfo.GetCultureInfo("ps-AF"),
					CultureInfo.GetCultureInfo("fa-IR"),
					CultureInfo.GetCultureInfo("pl-PL"),
					CultureInfo.GetCultureInfo("pt-AO"),
					CultureInfo.GetCultureInfo("pt-BR"),
					CultureInfo.GetCultureInfo("pt-PT"),
					CultureInfo.GetCultureInfo("pa-IN"),
					CultureInfo.GetCultureInfo("pa-Arab-PK"),
					CultureInfo.GetCultureInfo("quz-BO"),
					CultureInfo.GetCultureInfo("quz-PE"),
					CultureInfo.GetCultureInfo("quz-EC"),
					CultureInfo.GetCultureInfo("ro-MD"),
					CultureInfo.GetCultureInfo("ro-RO"),
					CultureInfo.GetCultureInfo("rm-CH"),
					CultureInfo.GetCultureInfo("ru-RU"),
					CultureInfo.GetCultureInfo("sah-RU"),
					CultureInfo.GetCultureInfo("smn-FI"),
					CultureInfo.GetCultureInfo("smj-NO"),
					CultureInfo.GetCultureInfo("smj-SE"),
					CultureInfo.GetCultureInfo("se-FI"),
					CultureInfo.GetCultureInfo("se-NO"),
					CultureInfo.GetCultureInfo("se-SE"),
					CultureInfo.GetCultureInfo("sms-FI"),
					CultureInfo.GetCultureInfo("sma-NO"),
					CultureInfo.GetCultureInfo("sma-SE"),
					CultureInfo.GetCultureInfo("sa-IN"),
					CultureInfo.GetCultureInfo("gd-GB"),
					CultureInfo.GetCultureInfo("sr-Cyrl-BA"),
					CultureInfo.GetCultureInfo("sr-Cyrl-ME"),
					CultureInfo.GetCultureInfo("sr-Cyrl-CS"),
					CultureInfo.GetCultureInfo("sr-Cyrl-RS"),
					CultureInfo.GetCultureInfo("sr-Latn-BA"),
					CultureInfo.GetCultureInfo("sr-Latn-ME"),
					CultureInfo.GetCultureInfo("sr-Latn-CS"),
					CultureInfo.GetCultureInfo("sr-Latn-RS"),
					CultureInfo.GetCultureInfo("nso-ZA"),
					CultureInfo.GetCultureInfo("tn-BW"),
					CultureInfo.GetCultureInfo("tn-ZA"),
					CultureInfo.GetCultureInfo("sn-Latn-ZW"),
					CultureInfo.GetCultureInfo("sd-Arab-PK"),
					CultureInfo.GetCultureInfo("si-LK"),
					CultureInfo.GetCultureInfo("sk-SK"),
					CultureInfo.GetCultureInfo("sl-SI"),
					CultureInfo.GetCultureInfo("so-SO"),
					CultureInfo.GetCultureInfo("st-ZA"),
					CultureInfo.GetCultureInfo("es-AR"),
					CultureInfo.GetCultureInfo("es-VE"),
					CultureInfo.GetCultureInfo("es-BO"),
					CultureInfo.GetCultureInfo("es-CL"),
					CultureInfo.GetCultureInfo("es-CO"),
					CultureInfo.GetCultureInfo("es-CR"),
					CultureInfo.GetCultureInfo("es-DO"),
					CultureInfo.GetCultureInfo("es-EC"),
					CultureInfo.GetCultureInfo("es-SV"),
					CultureInfo.GetCultureInfo("es-GT"),
					CultureInfo.GetCultureInfo("es-HN"),
					CultureInfo.GetCultureInfo("es-419"),
					CultureInfo.GetCultureInfo("es-MX"),
					CultureInfo.GetCultureInfo("es-NI"),
					CultureInfo.GetCultureInfo("es-PA"),
					CultureInfo.GetCultureInfo("es-PY"),
					CultureInfo.GetCultureInfo("es-PE"),
					CultureInfo.GetCultureInfo("es-PR"),
					CultureInfo.GetCultureInfo("es-ES"),
					CultureInfo.GetCultureInfo("es-US"),
					CultureInfo.GetCultureInfo("es-UY"),
					CultureInfo.GetCultureInfo("zgh-Tfng-MA"),
					CultureInfo.GetCultureInfo("sv-FI"),
					CultureInfo.GetCultureInfo("sv-SE"),
					CultureInfo.GetCultureInfo("syr-SY"),
					CultureInfo.GetCultureInfo("tg-Cyrl-TJ"),
					CultureInfo.GetCultureInfo("ta-IN"),
					CultureInfo.GetCultureInfo("ta-LK"),
					CultureInfo.GetCultureInfo("tt-RU"),
					CultureInfo.GetCultureInfo("te-IN"),
					CultureInfo.GetCultureInfo("th-TH"),
					CultureInfo.GetCultureInfo("bo-CN"),
					CultureInfo.GetCultureInfo("ti-ER"),
					CultureInfo.GetCultureInfo("ti-ET"),
					CultureInfo.GetCultureInfo("ts-ZA"),
					CultureInfo.GetCultureInfo("tr-TR"),
					CultureInfo.GetCultureInfo("tk-TM"),
					CultureInfo.GetCultureInfo("uk-UA"),
					CultureInfo.GetCultureInfo("hsb-DE"),
					CultureInfo.GetCultureInfo("ur-IN"),
					CultureInfo.GetCultureInfo("ur-PK"),
					CultureInfo.GetCultureInfo("ug-CN"),
					CultureInfo.GetCultureInfo("uz-Cyrl-UZ"),
					CultureInfo.GetCultureInfo("uz-Latn-UZ"),
					CultureInfo.GetCultureInfo("ca-ES-valencia"),
					CultureInfo.GetCultureInfo("vi-VN"),
					CultureInfo.GetCultureInfo("cy-GB"),
					CultureInfo.GetCultureInfo("wo-SN"),
					CultureInfo.GetCultureInfo("ii-CN"),
					CultureInfo.GetCultureInfo("yo-NG")
				};
				return _cultures;
			}
		}
	}
}
