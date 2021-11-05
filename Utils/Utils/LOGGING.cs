using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

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
		public string ApplicationName { get; }

		public Logger(string appName)
		{
			ApplicationName = appName;

			Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
			Debug.AutoFlush = true;

			// в обоих случаем приложение должно быть запущено от имени админимтратора. Либо у текущего пользователя должны быть админские права.
			LOGGING.CreateEventSource(ApplicationName ,true);
			//CreateEventSourceInReg(ApplicationName);
		}

		public void LogWriteInfo(string message)
		{
			LOGGING.CreateEventSource(ApplicationName);
			LOGGING.LogWriteInfo(ApplicationName, message);
		}

		public void LogWriteError(Exception ex)
		{
			if (ex != null)
				LogWriteError(ex.ToString());
		}

		public void LogWriteError(string message)
		{
			LOGGING.CreateEventSource(ApplicationName);
			LOGGING.LogWriteError(ApplicationName, message);
		}

		void CreateEventSourceInReg(string appName)
		{
			try
			{
				using (var reg = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\EventLog\Application", true))
				{
					var temp = reg.OpenSubKey(ApplicationName, true) ?? reg.CreateSubKey(ApplicationName);
				}
			}
			catch (Exception ex)
			{
				LOGGING.AddDebug(ex);
			}
		}
	}

	public static class LOGGING
	{
		public static readonly string LogName = "Application";
		public static readonly string Separator = new string('=', 25);

		public static Logger GetLogger(string appName) => new Logger(appName);

		public static void CreateEventSource(string appName, bool addDebug = false)
		{
			try
			{
				if (!EventLog.SourceExists(appName))
					EventLog.CreateEventSource(appName, LogName);
			}
			catch (Exception ex)
			{
				if (addDebug)
					AddDebug(ex);
			}
		}

		public static void LogWriteInfo(string applicationName, string message) => WriteLog(applicationName, message, EventLogEntryType.Information);

		public static void LogWriteError(string application, Exception ex)
		{
			if (ex != null)
				LogWriteError(application, ex.ToString());
		}

		public static void LogWriteError(string applicationName, string message) => WriteLog(applicationName, message, EventLogEntryType.Error);

		internal static void WriteLog(string application, string message, EventLogEntryType type)
		{
			try
			{
				AddDebug($"[{application}]: {message}");

				using (var eventLog = new EventLog(LogName))
				{
					eventLog.Source = application;
					eventLog.WriteEntry(message, type);
				}
			}
			catch (Exception ex)
			{
				// ignore
			}
		}

		internal static void AddDebug(Exception ex) => AddDebug(ex.ToString());

		internal static void AddDebug(string message)
		{
			Debug.Indent();
			Debug.WriteLine(message + "\n");
			Debug.WriteLine(Separator + "\n");
			Debug.Unindent();
		}
	}
}
