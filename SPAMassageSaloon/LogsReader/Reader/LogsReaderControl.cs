using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LogsReader.Config;
using LogsReader.Properties;
using Utils;

namespace LogsReader.Reader
{
	public abstract class LogsReaderControl
	{
		/// <summary>
		/// Текущая схема настроек
		/// </summary>
		public LRSettingsScheme CurrentSettings { get; }

		/// <summary>
		/// Поиск совпадения в одной строке
		/// </summary>
		public Func<string, bool> IsMatchLineFunc { get; }

		protected LogsReaderControl(LRSettingsScheme settings, string findMessage, bool useRegex)
		{
			CurrentSettings = settings;

			if (useRegex)
			{
				if (!REGEX.Verify(findMessage))
					throw new ArgumentException(string.Format(Resources.Txt_LogsReaderPerformer_IncorrectSearchPattern, findMessage));

				var searchPattern = new Regex(findMessage, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, new TimeSpan(0, 0, 1));
				IsMatchLineFunc = input => searchPattern.IsMatch(input);
			}
			else
			{
				IsMatchLineFunc = input => input.IndexOf(findMessage, StringComparison.InvariantCultureIgnoreCase) != -1;
			}
		}

		protected LogsReaderControl(LogsReaderControl control)
		{
			CurrentSettings = control.CurrentSettings;
			IsMatchLineFunc = control.IsMatchLineFunc;
		}
	}
}
