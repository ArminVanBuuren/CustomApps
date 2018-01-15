using System;
using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using Protas.Components.Functions;

namespace Protas.System.Statistic.Components
{
    class IpAddressModule
    {
        PingReply reply;
        IPHostEntry ipHost;
        string mAddress = string.Empty;
        public EventHandler LocalNetworkChange;
        public string MachineAddress
        {
            get
            {
                return mAddress;
            }
            set
            {
                if (value.IndexOf("localhost", StringComparison.CurrentCultureIgnoreCase) != -1 || string.IsNullOrEmpty(value) || !IsMachineAddress(value))
                    mAddress = FStatic.MachineName;
                else
                    mAddress = value;
                Refresh();
            }
        }
        
        public IpAddressModule(string ipormachinename)
        {
            MachineAddress = ipormachinename;
            NetworkChange.NetworkAddressChanged += (AddressChangedCallback);
        }
        public bool Refresh()
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    reply = ping.Send(MachineAddress);
                    if (reply == null)
                        return false;
                    ipHost = Dns.GetHostEntry(reply.Address.ToString());
                }
                return true;
            }
            catch (Exception ex)
            {
                reply = null;
                ipHost = null;
                return false;
            }
        }
        void AddressChangedCallback(object sender, EventArgs e)
        {
            LocalNetworkChange?.Invoke(this, EventArgs.Empty);
        }

        public string MachineName => ipHost != null ? ipHost.HostName : "Unknown";
        public string[] Aliases => ipHost?.Aliases;
        public IPAddress[] AddressList => ipHost?.AddressList;
        public IPAddress IPAddress => reply != null ? reply.Address : IPAddress.None;
        public IPStatus Status => reply?.Status ?? IPStatus.Unknown;
        public long RoundtripTime => reply?.RoundtripTime ?? 0;
        public NetworkInterface[] Adapters => NetworkInterface.GetAllNetworkInterfaces();

        public static bool IsMachineAddress(string input)
        {
            if (Regex.IsMatch(input, @"^\d{1,4}(\.\d{1,4}){3}$") || string.Equals(input, Environment.MachineName, StringComparison.CurrentCultureIgnoreCase))
                return true;
            return false;
        }
    }
}
