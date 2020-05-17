using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogsReader.Config;
using LogsReader.Properties;
using LogsReader.Reader.Forms;
using Utils;

namespace LogsReader.Reader
{
	public abstract class LogsReaderFiles : LogsReaderControl, IDisposable
	{
		private readonly IEnumerable<string> _servers;
		private readonly IEnumerable<string> _fileTypes;
		private readonly IReadOnlyDictionary<string, bool> _folders;

		/// <summary>
		/// Запрос на ожидание остановки выполнения поиска
		/// </summary>
		public bool IsStopPending { get; private set; } = false;

		public IReadOnlyCollection<TraceReader> TraceReaders { get; private set; }

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

			Func<string, string, string, TraceReader> _getTraceReader;
			if (CurrentSettings.TraceParse.StartTraceWith != null && CurrentSettings.TraceParse.EndTraceWith != null)
				_getTraceReader = (server, filePath, originalFolder) => new TraceReaderStartWithEndWith(this, server, filePath, originalFolder);
			else if (CurrentSettings.TraceParse.StartTraceWith != null)
				_getTraceReader = (server, filePath, originalFolder) => new TraceReaderStartWith(this, server, filePath, originalFolder);
			else if (CurrentSettings.TraceParse.EndTraceWith != null)
				_getTraceReader = (server, filePath, originalFolder) => new TraceReaderEndWith(this, server, filePath, originalFolder);
			else
				_getTraceReader = (server, filePath, originalFolder) => new TraceReaderSimple(this, server, filePath, originalFolder);

			foreach (var serverName in _servers)
			{
				if (IsStopPending)
					return kvpList;

				foreach (var originalFolder in _folders)
				{
					if (IsStopPending)
						return kvpList;

					var folderMatch = IO.CHECK_PATH.Match(originalFolder.Key);
					if (!folderMatch.Success)
						throw new Exception(string.Format(Resources.Txt_Forms_FolderIsIncorrect, originalFolder.Key));

					var serverFolder = $"\\\\{serverName}\\{folderMatch.Groups["DISC"]}$\\{folderMatch.Groups["FULL"]}";

					if(!IsExistAndAvailable(serverFolder, serverName, originalFolder.Key))
						continue;

					var files = Directory.GetFiles(serverFolder, "*", originalFolder.Value ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
					foreach (var fileLog in files.Select(filePath => _getTraceReader(serverName, filePath, originalFolder.Key)))
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

		bool IsExistAndAvailable(string serverFolder, string server, string folderPath)
		{
			try
			{
				var access = Directory.GetAccessControl(serverFolder);
			}
			catch (Exception)
			{
				var success = false;
				foreach (var credential in LogsReaderMainForm.UserCredentials
					.OrderByDescending(x => x.Value)
					.Select(x => x.Key)
					.ToList())
				{
					try
					{
						if (IsStopPending)
							return false;

						using (var connection = new NetworkConnection(serverFolder, credential.Value))
						{
							var access = Directory.GetAccessControl(serverFolder);
							Connections.Add(connection);
							LogsReaderMainForm.UserCredentials[credential] = DateTime.Now;
							success = true;
							break;
						}
					}
					catch (Exception ex)
					{
						// ignored
					}
				}

				if (!success)
				{
					var tryCount = 0;
					AddUserCredentials credential = null;
					var accessDeniedTxt = string.Format(Resources.Txt_LogsReaderForm_AccessDenied, server, folderPath);
					var additionalTxt = Resources.Txt_LogsReaderForm_AccessDeniedAuthor;
					while (tryCount < 3)
					{
						if (IsStopPending)
							return false;

						credential = new AddUserCredentials(
							$"{accessDeniedTxt}\r\n{additionalTxt}".Trim(),
							credential?.Credential?.Domain,
							credential?.Credential?.UserName);

						if (credential.ShowDialog() == DialogResult.OK)
						{
							tryCount++;
							try
							{
								using (var connection = new NetworkConnection(serverFolder, credential.Credential.Value))
								{
									var access = Directory.GetAccessControl(serverFolder);
									Connections.Add(connection);
									LogsReaderMainForm.UserCredentials[credential.Credential] = DateTime.Now;
									break;
								}
							}
							catch (Exception ex)
							{
								additionalTxt = ex.Message;
								NetworkConnection.CancelConnection(serverFolder);
							}
						}
						else
						{
							break;
						}
					}
				}
			}

			return Directory.Exists(serverFolder);
		}

		public abstract Task StartAsync();

		public virtual void Stop()
		{
			IsStopPending = true;
		}

		public virtual void Reset()
		{
			if (Connections != null)
			{
				foreach (var connect in Connections)
				{
					try
					{
						connect.Dispose();
					}
					catch (Exception ex)
					{
						// ignored
					}
				}

				Connections.Clear();
			}

			TraceReaders = null;

			IsStopPending = false;
		}

		public void Dispose()
		{
			Reset();
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
