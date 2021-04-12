using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
	
	public interface ILogger
	{
		void LogWriteInfo(string message);

		void LogWriteError(Exception ex);

		void LogWriteError(string message);
	}

	[Serializable]
	public class Logger : ILogger
	{
		public string Application { get; }

		public Logger(string appName) => Application = appName;

		public void LogWriteInfo(string message)
		{
			Check();
			LOGGING.LogWriteInfo(Application, message);
		}

		public void LogWriteError(Exception ex)
		{
			if (ex != null)
				LogWriteError(ex.ToString());
		}

		public void LogWriteError(string message)
		{
			Check();
			LOGGING.LogWriteError(Application, message);
		}

		void Check()
		{
			try
			{
				if (!EventLog.SourceExists(Application))
					EventLog.CreateEventSource(Application, LOGGING.LogName);
			}
			catch (Exception)
			{
				// ignored
			}
		}

	}

	public static class LOGGING
	{
		public static readonly string LogName = "Application";

		public static Logger GetLogger(string appName) => new Logger(appName);

		public static void LogWriteInfo(string application, string message) => WriteLog(application, message, EventLogEntryType.Information);

		public static void LogWriteError(string application, Exception ex)
		{
			if (ex != null)
				LogWriteError(application, ex.ToString());
		}

		public static void LogWriteError(string application, string message) => WriteLog(application, message, EventLogEntryType.Error);

		static void WriteLog(string application, string message, EventLogEntryType type)
		{
			try
			{
				using (var eventLog = new EventLog(LogName))
				{
					eventLog.Source = application;
					eventLog.WriteEntry(message, type);
				}
			}
			catch (Exception e)
			{
				// ignored
			}
		}
	}
}
