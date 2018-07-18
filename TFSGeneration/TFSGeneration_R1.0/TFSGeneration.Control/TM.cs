
namespace TFSAssist.Control
{
	public static class TM
	{
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
    }
}
