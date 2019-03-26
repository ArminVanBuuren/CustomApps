using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TeleSharp.TL;
using TFSAssist.Control;
using Utils;
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

    public class TFSA_TLControl
    {
        public string TempDirectory { get; }
        private string ClientID = string.Empty;
        private TTLControl _control;

        private Func<bool> _checkUpdates;
        private Func<string> _getLogs;
        private Action<WarnSeverity, string> _writeLog;

        public TFSA_TLControl(string clientId, Func<bool> checkUpdates, Func<string> getLogs, Action<WarnSeverity, string> log)
        {
            _checkUpdates = checkUpdates;
            _getLogs = getLogs;
            _writeLog = log;
            ClientID = clientId;
            TempDirectory = Path.Combine(ASSEMBLY.ApplicationDirectory, "Temp");
        }

        public async Task<bool> Initialize()
        {
            await GetTempSession();
            await CreateTempDirectory();

            try
            {
                _control = new TTLControl(770122, "8bf0b952100c9b22fd92499fc329c27e");
                await _control.ConnectAsync();

                await Task.Delay(5000);

                await SendMessageToCurrentUser($"Connected. {GetCurrentServerInfo()}");

                return true;
            }
            catch (Exception ex)
            {
                WriteLog(ex, false);
                return false;
            }
        }

        public async Task EndTransaction()
        {
            await SendMessageToCurrentUser($"Disconnected. {GetCurrentServerInfo()}");
        }

        public async Task Run()
        {
            DateTime lastDate = DateTime.Now;
            while (true)
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
                    WriteLog(ex);
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
                        string message = tlMessage.Message.Substring(isCommand.Length, tlMessage.Message.Length - isCommand.Length).ToLower();
                        switch (message)
                        {
                            case "screen":
                                string imagePath = Path.Combine(TempDirectory, $"{STRING.RandomString(15)}.png");
                                await ScreenCapture.CaptureAsync(imagePath, ImageFormat.Png);
                                break;

                            case "info":
                                await SendMessageToCurrentUser(GetCurrentServerInfo(true));
                                break;

                            case "drive":
                                var drives = System.IO.DriveInfo.GetDrives();
                                if (drives == null)
                                    break;
                                string result = string.Empty;
                                foreach (var drive in drives)
                                {
                                    long freeSpaces = IO.GetTotalFreeSpace(drive.Name);
                                    string freeSize = IO.FormatBytes(freeSpaces, out var newBytes);
                                    result += $"Drive=[{drive.Name}] FreeSize={freeSize}\r\n";
                                }
                                await SendMessageToCurrentUser(result.Trim());
                                break;

                            case "log":
                                string logs = _getLogs?.Invoke();
                                if (logs.IsNullOrEmptyTrim())
                                    break;

                                using (var stream = new FileStream(Path.Combine(TempDirectory, $"{STRING.RandomString(15)}.log"), FileMode.OpenOrCreate))
                                {
                                    byte[] logsBytes = new UTF8Encoding(true).GetBytes(logs);
                                    await stream.WriteAsync(logsBytes, 0, logsBytes.Length);
                                }

                                break;

                            case "update":
                                isUpdate = true;
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
                WriteLog(ex);
            }
            finally
            {
                var tempFiles = Directory.EnumerateFiles(TempDirectory);
                if (tempFiles.Count() > 0)
                {
                    try
                    {
                        string destinationZip = DoZipFile(TempDirectory);
                        if (destinationZip != null && File.Exists(destinationZip))
                        {
                            await _control.SendBigFileAsync(_control.CurrentUser.Destination, destinationZip);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
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
                        WriteLog(ex);
                    }
                }

                if (isUpdate)
                {
                    _checkUpdates?.BeginInvoke(null, null);
                }
            }
        }

        async Task SendMessageToCurrentUser(string message)
        {
            await _control.SendMessageAsync(_control.CurrentUser.Destination, $"CID=[{ClientID}]\r\n{message}", 0);
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
                {
                    var serverName = System.Environment.MachineName;
                    var full = System.Net.Dns.GetHostEntry(serverName).HostName;
                    return $"Host=[{hostName}] FullName=[{full}] Address=[\"{string.Join("\",\"", Dns.GetHostAddresses(hostName).ToList())}\"]";
                }

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
                WriteLog(ex);
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
                    WriteLog(ex, false);
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
                WriteLog(ex, false);
                tryes++;
                if (tryes < 5)
                    goto lableTrys;
            }
        }

        void WriteLog(Exception ex, bool isDebug = true)
        {
            _writeLog?.Invoke(isDebug ? WarnSeverity.Debug : WarnSeverity.Error, $"{nameof(TFSA_TLControl)} - {ex.Message}");
        }
    }
}