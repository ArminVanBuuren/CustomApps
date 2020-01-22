using System.Text.RegularExpressions;

namespace Utils
{
	public static class NUMBER
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

			var newCoretIndex = 0;
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
            return dbl % 2 == 0 ? true : false;
        }
    }
}
