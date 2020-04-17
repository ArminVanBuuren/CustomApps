using System;
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
            string formatted = string.Format("{0}{1}{2}{3}",
                span.Duration().Days > 0 ? $"{span.Days:0} {(span.Days == 1 ? Resources.day : (span.Days < 5 ? Resources.days2 : Resources.days5))}, " : string.Empty,
                span.Duration().Hours > 0 ? $"{span.Hours:0} {(span.Hours == 1 ? Resources.hour : (span.Hours < 5 ? Resources.hours2 : Resources.hours5))}, " : string.Empty,
                span.Duration().Minutes > 0 ? $"{span.Minutes:0} {(span.Minutes == 1 ? Resources.minute : (span.Minutes < 5 ? Resources.minutes2 : Resources.minutes5))}, " : string.Empty,
                span.Duration().Seconds > 0 ? $"{span.Seconds:0} {(span.Seconds == 1 ? Resources.second : (span.Seconds < 5 ? Resources.seconds2 : Resources.seconds5))}" : string.Empty);

            if (formatted.EndsWith(", "))
                formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted))
                formatted = $"0 {Resources.seconds5}";

            return formatted;
        }
    }
}
