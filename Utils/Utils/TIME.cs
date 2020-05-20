using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Utils.Properties;

namespace Utils
{
	public static class TIME
	{
		public static string ToReadableAgeString(this TimeSpan span)
		{
			return $"{span.Days / 365.25:0}";
		}

		public static string ToReadableString(this TimeSpan span)
		{
			var formatted = string.Format("{0}{1}{2}{3}",
				span.Duration().Days > 0 ? $"{span.Days:0} {(span.Days == 1 ? Resources.day : (span.Days < 5 ? Resources.days2 : Resources.days5))}, " : string.Empty,
				span.Duration().Hours > 0 ? $"{span.Hours:0} {(span.Hours == 1 ? Resources.hour : (span.Hours < 5 ? Resources.hours2 : Resources.hours5))}, " : string.Empty,
				span.Duration().Minutes > 0
					? $"{span.Minutes:0} {(span.Minutes == 1 ? Resources.minute : (span.Minutes < 5 ? Resources.minutes2 : Resources.minutes5))}, "
					: string.Empty,
				span.Duration().Seconds > 0
					? $"{span.Seconds:0} {(span.Seconds == 1 ? Resources.second : (span.Seconds < 5 ? Resources.seconds2 : Resources.seconds5))}"
					: string.Empty);

			if (formatted.EndsWith(", "))
				formatted = formatted.Substring(0, formatted.Length - 2);

			if (string.IsNullOrEmpty(formatted))
				formatted = $"0 {Resources.seconds5}";

			return formatted;
		}

		public static bool TryParseAnyDate(string dateValue, out DateTime result)
		{
			return TryParseAnyDate(dateValue, DateTimeStyles.None, out result);
		}

		public static bool TryParseAnyDate(string dateValue, DateTimeStyles style, out DateTime result)
		{
			result = DateTime.MinValue;

			foreach (var cultureInfo in GetCurrentCultures())
			{
				if (DateTime.TryParse(dateValue, cultureInfo, style, out result))
					return true;
			}

			foreach (var cultureInfo in DateCultures)
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

			foreach (var cultureInfo in GetCurrentCultures())
			{
				if (DateTime.TryParseExact(dateValue, format, cultureInfo, style, out result))
					return true;
			}

			foreach (var cultureInfo in DateCultures)
			{
				if (DateTime.TryParseExact(dateValue, format, cultureInfo, style, out result))
					return true;
			}

			return false;
		}

		static IEnumerable<CultureInfo> GetCurrentCultures()
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

		private static List<CultureInfo> _dateCultures;

		// https://www.basicdatepicker.com/samples/cultureinfo.aspx
		private static IEnumerable<CultureInfo> DateCultures
		{
			get
			{
				if (_dateCultures != null)
					return _dateCultures;

				_dateCultures = new List<CultureInfo>()
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
				return _dateCultures;
			}
		}
	}
}