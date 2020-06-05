using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogsReader.Config;
using LogsReader.Properties;
using LogsReader.Reader.Forms;
using Utils;

namespace LogsReader.Reader
{
	public abstract class LogsReaderFiles : LogsReaderControl
	{
		private readonly IEnumerable<string> _servers;
		private readonly IEnumerable<string> _fileTypes;
		private readonly IReadOnlyDictionary<string, bool> _folders;

		/// <summary>
		/// Запрос на ожидание остановки выполнения поиска
		/// </summary>
		public bool IsStopPending { get; private set; } = false;

		protected IReadOnlyCollection<TraceReader> TraceReaders { get; private set; }

		protected List<NetworkConnection> Connections { get; } = new List<NetworkConnection>();

		protected LogsReaderFiles(
			LRSettingsScheme settings, 
			string findMessage,  
			bool useRegex, 
			IEnumerable<string> servers, 
			IEnumerable<string> fileTypes,
			IReadOnlyDictionary<string, bool> folders)
			:base(settings, findMessage, useRegex)
		{
			_servers = servers;
			_fileTypes = fileTypes;
			_folders = folders;
		}

		public async Task GetTargetFilesAsync()
		{
			if (IsStopPending)
				return;

			TraceReaders = new ReadOnlyCollection<TraceReader>(await Task<List<TraceReader>>.Factory.StartNew(GetFileLogs));
			if (TraceReaders == null || TraceReaders.Count <= 0)
				throw new Exception(Resources.Txt_LogsReaderPerformer_NoFilesLogsFound);
		}

		List<TraceReader> GetFileLogs()
		{
			var kvpList = new List<TraceReader>();

			foreach (var serverName in _servers)
			{
				if (IsStopPending)
					return kvpList;

				foreach (var (originalFolder, isAllDirectories) in _folders)
				{
					if (IsStopPending)
						return kvpList;

					var folderMatch = IO.CHECK_PATH.Match(originalFolder);
					if (!folderMatch.Success)
						throw new Exception(string.Format(Resources.Txt_Forms_FolderIsIncorrect, originalFolder));

					var serverRoot = $"\\\\{serverName}\\{folderMatch.Groups["DISC"]}$";
					var serverFolder = $"{serverRoot}\\{folderMatch.Groups["FULL"]}";

					if (!IsExistAndAvailable(serverFolder, serverName, serverRoot, originalFolder))
						continue;

					var files = Directory.GetFiles(serverFolder, "*", isAllDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
					foreach (var fileLog in files.Select(filePath => GetTraceReader((serverName, filePath, originalFolder))))
					{
						if (IsStopPending)
							return kvpList;

						if (!IsAllowedExtension(fileLog.File.Name))
							continue;

						if (_fileTypes.Any(x => fileLog.File.Name.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) != -1))
							kvpList.Add(fileLog);
					}
				}
			}

			//var stop = new Stopwatch();
			//stop.Start();
			//var getCountLines = new MTFuncResult<FileLog, long>((input) => IO.CountLinesReader(input.FilePath), kvpList, kvpList.Count, ThreadPriority.Lowest);
			//getCountLines.Start();
			//var lines = getCountLines.Result.Values.Select(x => x.Result).Sum(x => x);
			//stop.Stop();

			return kvpList;
		}

		bool IsExistAndAvailable(string serverFolder, string server, string serverRoot, string folderPath)
		{
			try
			{
				Directory.GetDirectories(serverFolder);
			}
			catch (DirectoryNotFoundException)
			{
				return false;
			}
			catch (SecurityException ex)
			{
				return IsExistAndAvailable(ex, serverFolder, server, serverRoot, folderPath);
			}
			catch (IOException ex)
			{
				return IsExistAndAvailable(ex, serverFolder, server, serverRoot, folderPath);
			}
			catch (UnauthorizedAccessException ex)
			{
				return IsExistAndAvailable(ex, serverFolder, server, serverRoot, folderPath);
			}

			return Directory.Exists(serverFolder);
		}

		bool IsExistAndAvailable(Exception ex, string serverFolder, string server, string serverRoot, string folderPath)
		{
			if (ex.HResult == -2147024843) // не найден сетевой путь
				throw new Exception($"[{server}] {ex.Message.Trim()}", ex);


			foreach (var credential in LogsReaderMainForm.Credentials
				.OrderByDescending(x => x.Value)
				.Select(x => x.Key)
				.ToList())
			{
				if (IsStopPending)
					return false;

				try
				{
					return IsExist(serverRoot, serverFolder, credential);
				}
				catch (Exception)
				{
					// ignore and continue
				}
			}


			var tryCount = 0;
			AddUserCredentials authorizationForm = null;
			var accessDeniedTxt = string.Format(Resources.Txt_LogsReaderForm_AccessDenied, server, folderPath);
			var additionalTxt = string.Empty;
			while (tryCount < 3)
			{
				if (IsStopPending)
					return false;

				authorizationForm = new AddUserCredentials(
					$"{accessDeniedTxt}\r\n\r\n{additionalTxt}".Trim(),
					authorizationForm?.Credential?.UserName);

				if (authorizationForm.ShowDialog() == DialogResult.OK)
				{
					tryCount++;

					try
					{
						return IsExist(serverRoot, serverFolder, authorizationForm.Credential);
					}
					catch (Exception ex2)
					{
						additionalTxt = ex2.Message;
					}
				}
				else
				{
					break;
				}
			}

			return Directory.Exists(serverFolder);
		}

		bool IsExist(string serverRoot, string serverFolder, CryptoNetworkCredential credentialList)
		{
			// сначала проверяем доступ к рутовой папке. Если все ОК, то вероятно до основной папки тоже есть доступ
			NetworkConnection connectionRoot = null;
			foreach (var credential in credentialList.Value)
			{
				try
				{
					connectionRoot = new NetworkConnection(serverRoot, credential);
					Connections.Add(connectionRoot);
					return IsExist(serverFolder, credentialList);
				}
				catch (Exception)
				{
					// ошибочные коннекты логофим
					if (connectionRoot != null)
					{
						Connections.Remove(connectionRoot);
						connectionRoot.Dispose();
					}
				}
			}


			Exception accessDenied = new Win32Exception(1326);
			// если доступа к руту нет, то проверяем доступ к конкретной папке
			foreach (var credential in credentialList.Value)
			{
				NetworkConnection connection = null;
				try
				{
					connection = new NetworkConnection(serverFolder, credential);
					Connections.Add(connection);
					return IsExist(serverFolder, credentialList);
				}
				catch (Exception ex)
				{
					// ошибочные коннекты логофим
					if (connection != null)
					{
						Connections.Remove(connection);
						connection.Dispose();
					}

					accessDenied = ex;
				}
			}

			throw accessDenied;
		}

		static bool IsExist(string serverFolder, CryptoNetworkCredential credential)
		{
			try
			{
				Directory.GetDirectories(serverFolder);
				LogsReaderMainForm.Credentials[credential] = DateTime.Now;
				return Directory.Exists(serverFolder);
			}
			catch (DirectoryNotFoundException)
			{
				LogsReaderMainForm.Credentials[credential] = DateTime.Now;
				return false;
			}
		}

		public abstract Task StartAsync();

		public virtual void Stop()
		{
			IsStopPending = true;
		}

		public virtual void Reset()
		{
			//if (Connections != null)
			//{
			//	foreach (var connection in Connections)
			//	{
			//		try
			//		{
			//			connection.Dispose();
			//		}
			//		catch (Exception ex)
			//		{
			//			// ignored
			//		}
			//	}

			//	Connections.Clear();
			//}

			TraceReaders = null;

			IsStopPending = false;
		}

		public override void Dispose()
		{
			Reset();
			base.Dispose();
		}

		static bool IsAllowedExtension(string fileName)
		{
			switch (Path.GetExtension(fileName)?.ToLower())
			{
				case ".aif":
				case ".cda":
				case ".mid":
				case ".mp3":
				case ".mpa":
				case ".ogg":
				case ".wav":
				case ".wma":
				case ".wpl":
				case ".7z":
				case ".arj":
				case ".deb":
				case ".pkg":
				case ".rar":
				case ".rpm":
				case ".tar.gz":
				case ".z":
				case ".zip":
				case ".bin":
				case ".dmg":
				case ".iso":
				case ".toast":
				case ".vcd":
				case ".mdb":
				case ".dat":
				case ".exe":
				case ".dll":
				case ".ai":
				case ".bmp":
				case ".gif":
				case ".ico":
				case ".jpeg":
				case ".png":
				case ".ps":
				case ".psd":
				case ".svg":
				case ".tif":
				case ".ods":
				case ".xls":
				case ".xlsm":
				case ".xlsx":
				case ".bak":
				case ".cab":
				case ".cfg":
				case ".cpl":
				case ".cur":
				case ".dmp":
				case ".drv":
				case ".icns":
				case ".ini":
				case ".lnk":
				case ".msi":
				case ".sys":
				case ".tmp":
				case ".3g2":
				case ".3gp":
				case ".avi":
				case ".flv":
				case ".h264":
				case ".m4v":
				case ".mkv":
				case ".mov":
				case ".mp4":
				case ".mpg":
				case ".rm":
				case ".swf":
				case ".vob":
				case ".wmv":
				case ".doc":
				case ".odt":
				case ".pdf":
				case ".wpd": return false;
			}

			return true;
		}
	}
}
