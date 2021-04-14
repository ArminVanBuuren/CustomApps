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
	public delegate CryptoNetworkCredential GetUserCredential(string information, string userName);

	public abstract class LogsReaderPerformerFiles : LogsReaderPerformerBase
	{
		private readonly IReadOnlyDictionary<string, int> _servers;
		private readonly IReadOnlyDictionary<string[], int> _fileTypes;
		private readonly IReadOnlyDictionary<string, bool> _folders;

		public IReadOnlyDictionary<int, TraceReader> TraceReaders { get; protected set; }

		public int CountMatches => TraceReaders?.Values.Sum(x => x.CountMatches) ?? 0;

		public int CountErrorMatches => TraceReaders?.Values.Sum(x => x.CountErrors) ?? 0;

		private readonly GetUserCredential GetCredential;

		protected LogsReaderPerformerFiles(LRSettingsScheme settings,
		                                   string findMessage,
		                                   bool useRegex,
		                                   IReadOnlyDictionary<string, int> servers,
		                                   IReadOnlyDictionary<string, int> fileTypes,
		                                   IReadOnlyDictionary<string, bool> folders,
		                                   DataFilter filter,
		                                   GetUserCredential getUserCredential)
			: base(settings, findMessage, useRegex, filter)
		{
			_servers = servers;
			var fileTypesNew = new Dictionary<string[], int>();
			foreach (var (fileType, priority) in fileTypes)
				fileTypesNew.Add(fileType.Split('*'), priority);
			_fileTypes = new ReadOnlyDictionary<string[], int>(fileTypesNew);
			_folders = folders;
			GetCredential = getUserCredential;
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

			if (LogsReaderMainForm.TryGetCredential(serverRoot, out var existingCredential) && existingCredential != null)
			{
				try
				{
					return IsExist(serverRoot, serverFolder, existingCredential);
				}
				catch (Exception)
				{
					var result = TryToCheckWithLastCredential(serverRoot, serverFolder, existingCredential);
					if (result.HasValue)
						return result.Value;
				}
			}
			else
			{
				var result = TryToCheckWithLastCredential(serverRoot, serverFolder, null);
				if (result.HasValue)
					return result.Value;
			}

			if (GetCredential != null)
			{
				var tryCount = 0;
				CryptoNetworkCredential credential = null;
				var accessDeniedTxt = string.Format(Resources.Txt_LogsReaderForm_AccessDenied, server, folderPath);
				var additionalInfo = string.Empty;

				while (tryCount < 3)
				{
					if (IsStopPending)
						return false;

					credential = GetCredential($"{accessDeniedTxt}\r\n\r\n{additionalInfo}".Trim(), credential?.GetUserName());
					if (credential != null)
					{
						try
						{
							tryCount++;
							return IsExist(serverRoot, serverFolder, credential);
						}
						catch (Exception ex2)
						{
							additionalInfo = ex2.Message;
						}
					}
					else
					{
						break;
					}
				}
			}

			return Directory.Exists(serverFolder);
		}

		/// <summary>
		/// Пытаемся подключиться по последним учетным данным
		/// </summary>
		/// <param name="serverFolder"></param>
		/// <param name="existingCredential"></param>
		/// <param name="serverRoot"></param>
		bool? TryToCheckWithLastCredential(string serverRoot, string serverFolder, CryptoNetworkCredential existingCredential)
		{
			// пытаемся подключиться по последним учетным данным
			var lastCredential = LogsReaderMainForm.GetLastCredentialItem();

			if (lastCredential != null && existingCredential != lastCredential)
			{
				try
				{
					return IsExist(serverRoot, serverFolder, lastCredential);
				}
				catch (Exception)
				{
					// ignore and continue
				}
			}

			return null;
		}

		private bool IsExist(string serverRoot, string serverFolder, CryptoNetworkCredential credential)
		{
			Exception accessDenied = null;

			// сначала проверяем доступ к рутовой папке. Если все ОК, то вероятно до основной папки тоже есть доступ
			var result = CheckingAccessToServerOrFolder(serverRoot, serverFolder, credential, false, ref accessDenied);
			if (result.HasValue)
				return result.Value;
			
			// если доступа к руту нет, то проверяем доступ к конкретной папке
			result = CheckingAccessToServerOrFolder(serverRoot, serverFolder, credential, true, ref accessDenied);
			if (result.HasValue)
				return result.Value;

			throw accessDenied ?? new Win32Exception(1326);
		}

		private bool? CheckingAccessToServerOrFolder(string serverRoot,
		                                            string serverFolder,
		                                            CryptoNetworkCredential credentialList,
		                                            bool checkFolder,
		                                            ref Exception accessDenied)
		{
			foreach (var credential in credentialList.Value)
			{
				NetworkConnection connection = null;
				try
				{
					connection = new NetworkConnection(checkFolder ? serverFolder : serverRoot, credential);
					return IsExistAndRefreshCredentials(serverRoot, serverFolder, credentialList);
				}
				catch (Exception ex)
				{
					// ошибочные коннекты логофим
					connection?.Dispose();
					accessDenied = ex;
				}
			}

			return null;
		}

		private bool IsExistAndRefreshCredentials(string serverRoot, string serverFolder, CryptoNetworkCredential credential)
		{
			try
			{
				Directory.GetDirectories(serverFolder);
				LogsReaderMainForm.AddOrReplaceToNewCredential(serverRoot, credential);
				return Directory.Exists(serverFolder);
			}
			catch (DirectoryNotFoundException)
			{
				LogsReaderMainForm.AddOrReplaceToNewCredential(serverRoot, credential);
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