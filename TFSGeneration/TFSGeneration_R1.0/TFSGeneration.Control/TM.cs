
namespace TFSGeneration.Control
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
	}
}
