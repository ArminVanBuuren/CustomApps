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
	public abstract class LogsReaderPerformerFiles : LogsReaderPerformerBase
	{
		private readonly IReadOnlyDictionary<string, int> _servers;
		private readonly IReadOnlyDictionary<string[], int> _fileTypes;
		private readonly IReadOnlyDictionary<string, bool> _folders;

		public IReadOnlyDictionary<int, TraceReader> TraceReaders { get; protected set; }

		protected List<NetworkConnection> Connections { get; } = new List<NetworkConnection>();

		protected LogsReaderPerformerFiles(LRSettingsScheme settings,
		                                   string findMessage,
		                                   bool useRegex,
		                                   IReadOnlyDictionary<string, int> servers,
		                                   IReadOnlyDictionary<string, int> fileTypes,
		                                   IReadOnlyDictionary<string, bool> folders,
		                                   DataFilter filter)
			: base(settings, findMessage, useRegex, filter)
		{
			_servers = servers;
			var fileTypesNew = new Dictionary<string[], int>();
			foreach (var (fileType, priority) in fileTypes)
				fileTypesNew.Add(fileType.Split('*'), priority);
			_fileTypes = new ReadOnlyDictionary<string[], int>(fileTypesNew);
			_folders = folders;
		}

		public virtual async Task GetTargetFilesAsync()
		{
			if (IsStopPending)
				return;

			Reset();
			TraceReaders = new ReadOnlyDictionary<int, TraceReader>(await Task<Dictionary<int, TraceReader>>.Factory.StartNew(GetFileLogs));
			if (TraceReaders == null || TraceReaders.Count <= 0)
				throw new Exception(Resources.Txt_LogsReaderPerformer_NoFilesLogsFound);
		}

		private Dictionary<int, TraceReader> GetFileLogs()
		{
			var readerIndex = -1;
			var traceReaders = new Dictionary<int, TraceReader>();

			foreach (var (serverName, serverPriority) in _servers)
			{
				if (IsStopPending)
					return traceReaders;

				foreach (var (originalFolder, isAllDirectories) in _folders)
				{
					if (IsStopPending)
						return traceReaders;

					string serverFolder;
					var folderMatch = IO.CHECK_PATH.Match(originalFolder);

					if (folderMatch.Success)
					{
						var serverRoot = $"\\\\{serverName}\\{folderMatch.Groups["DISC"]}$";
						serverFolder = $"{serverRoot}\\{folderMatch.Groups["FULL"]}"; // @"\\LOCALHOST\D$\TEST\MG2\CRMCON_CRMCON_01_data.log.2020-06-06.0"
						
						if (!IsExistAndAvailable(serverFolder, serverName, serverRoot, originalFolder))
							continue;
					}
					else
					{
						var serverRoot = $"\\\\{serverName}";
						serverFolder = $"{serverRoot}\\{originalFolder}"; // @"\\MSK-DEV-FORIS\forislog\mg\CRMCON_CRMCON_MTS_activity.log.2020-08-04.0"
						
						if (!IsExistAndAvailable(serverFolder, serverName, serverRoot, originalFolder))
							continue;
					}

					var files = Directory.GetFiles(serverFolder, "*", isAllDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

					foreach (var traceReader in files.Select(filePath => GetTraceReader((serverName, filePath, originalFolder))))
					{
						if (IsStopPending)
							return traceReaders;

						if (!IsAllowedExtension(traceReader.File.Name))
							continue;

						foreach (var (fileType, filePriority) in _fileTypes)
						{
							var index = -1;
							var fileName = traceReader.File.Name;

							foreach (var fileTypePart in fileType)
							{
								index = fileName.IndexOf(fileTypePart, StringComparison.InvariantCultureIgnoreCase);

								if (index > -1)
								{
									var indexWithWord = index + fileTypePart.Length;
									fileName = fileName.Substring(indexWithWord, fileName.Length - indexWithWord);
								}
								else
								{
									break;
								}
							}

							if (index > -1)
							{
								traceReader.PrivateID = ++readerIndex;
								traceReader.Priority = int.Parse($"1{serverPriority:D2}{filePriority:D2}");
								traceReaders.Add(traceReader.PrivateID, traceReader);
								break;
							}
						}
					}
				}
			}

			return traceReaders;
		}

		private bool IsExistAndAvailable(string serverFolder, string server, string serverRoot, string folderPath)
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

		private bool IsExistAndAvailable(Exception ex, string serverFolder, string server, string serverRoot, string folderPath)
		{
			if (ex.HResult == -2147024843) // не найден сетевой путь
				throw new Exception($"[{server}] {ex.Message.Trim()}", ex);

			foreach (var credential in LogsReaderMainForm.Credentials.OrderByDescending(x => x.Value).Select(x => x.Key).ToList())
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

				authorizationForm = new AddUserCredentials($"{accessDeniedTxt}\r\n\r\n{additionalTxt}".Trim(), authorizationForm?.Credential?.UserName);

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

		private bool IsExist(string serverRoot, string serverFolder, CryptoNetworkCredential credentialList)
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

		private static bool IsExist(string serverFolder, CryptoNetworkCredential credential)
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

		public override void Pause()
		{
			if (TraceReaders != null)
				foreach (var reader in TraceReaders.Values)
					reader.Pause();
		}

		public override void Resume()
		{
			if (TraceReaders != null)
				foreach (var reader in TraceReaders.Values)
					reader.Resume();
		}

		public override void Abort()
		{
			// Processing и OnPause должны сами оcтановиться по свойству IsStopPending
			if (TraceReaders != null)
				foreach (var reader in TraceReaders.Values.Where(x => x.ThreadId.IsNullOrEmpty()))
					reader.Abort();
		}

		/// <summary>
		///     Останавливаем все процессы
		/// </summary>
		public override void Clear()
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
			if (TraceReaders != null)
				foreach (var reader in TraceReaders.Values)
					reader.Clear();
			base.Clear();
		}

		public void Reset()
		{
			if (TraceReaders != null)
				foreach (var reader in TraceReaders.Values)
					reader.Dispose();
			TraceReaders = null;
			base.Clear();
		}

		private static bool IsAllowedExtension(string fileName)
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
				case ".wpd":
					return false;
			}

			return true;
		}
	}
}