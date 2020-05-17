using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LogsReader.Config;
using LogsReader.Properties;
using Utils;

namespace LogsReader.Reader
{
	public abstract class LogsReaderControl : IDisposable
	{
		/// <summary>
		/// Запрос на ожидание остановки выполнения поиска
		/// </summary>
		public bool IsStopPending { get; private set; } = false;

		/// <summary>
		/// Текущая схема настроек
		/// </summary>
		public LRSettingsScheme CurrentSettings { get; }

		public Func<string, bool> IsMatchSearchPatternFunc { get; }

		protected LogsReaderControl(LRSettingsScheme settings, string findMessage, bool useRegex)
		{
			CurrentSettings = settings;

			if (useRegex)
			{
				if (!REGEX.Verify(findMessage))
					throw new ArgumentException(string.Format(Resources.Txt_LogsReaderPerformer_IncorrectSearchPattern, findMessage));

				var searchPattern = new Regex(findMessage, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, new TimeSpan(0, 0, 1));
				IsMatchSearchPatternFunc = input => searchPattern.IsMatch(input);
			}
			else
			{
				IsMatchSearchPatternFunc = input => input.IndexOf(findMessage, StringComparison.InvariantCultureIgnoreCase) != -1;
			}
		}

		public abstract Task GetTargetFilesAsync();

		public abstract Task StartAsync();

		public virtual void Stop()
		{
			IsStopPending = true;
		}

		public virtual void Reset()
		{
			IsStopPending = false;
		}

		public void Dispose()
		{
			Reset();
		}
	}
}
