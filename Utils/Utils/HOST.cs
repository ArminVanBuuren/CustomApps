using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utils.CollectionHelper;

namespace Utils
{
    public static class HOST
    {
        static readonly Regex ParceIpAddress = new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}", RegexOptions.Compiled);

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            //throw new Exception("No network adapters with an IPv4 address in the system!");
            return null;
        }

        public class HostInfo
        {
            public HostInfo(IPAddressInformation ip, NetworkInterface netInterface)
            {
                IPAddress = ip;
                Interface = netInterface;
            }
            public IPAddressInformation IPAddress { get; }
            public NetworkInterface Interface { get; }
        }

        /// <summary> 
        /// This utility function displays all the IP (v4, not v6) addresses of the local computer. 
        /// </summary> 
        public static List<HostInfo> GetIPAddresses()
        {
            List<HostInfo> ipAddresses = new List<HostInfo>();

            // Get a list of all network interfaces (usually one per network card, dialup, and VPN connection) 
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface network in networkInterfaces)
            {
                // Read the IP configuration for each network 
                IPInterfaceProperties properties = network.GetIPProperties();

                // Each network interface may have multiple IP addresses 
                foreach (var add in properties.UnicastAddresses)
                {
                    if(!(add is IPAddressInformation address))
                        continue;

                    // We're only interested in IPv4 addresses for now 
                    if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    // Ignore loopback addresses (e.g., 127.0.0.1) 
                    if (IPAddress.IsLoopback(address.Address))
                        continue;

                    ipAddresses.Add(new HostInfo(address, network));
                    //ipAddresses.Add($"IP=[{address.Address.ToString()}] Name=[{network.Name}]");
                }
            }

            return ipAddresses;
        }

        /// <summary>
        /// Для определения используются два сайта http://checkip.dyndns.org и https://www.whatismyip.com/
        /// Есть еще https://ipdata.co/ (но он не используется) https://www.iplocation.net/find-ip-address
        /// https://www.iplocation.net/ - получить локацию по PublicIP и https://www.latlong.net/Show-Latitude-Longitude.html  (но все равно не то определяет)
        /// </summary>
        /// <returns></returns>
        public static string GetExternalIPAddress()
        {
            string checkip = GetExternalIPAddress("http://checkip.dyndns.org/");
            return !string.IsNullOrWhiteSpace(checkip) ? checkip : GetExternalIPAddress("https://www.whatismyip.com/");
        }

        static string GetExternalIPAddress(string externalWebAddress)
        {
            string responceBody = WEB.WebHttpStringData(externalWebAddress , out var httpRespCode);
            if (httpRespCode != HttpStatusCode.OK)
                return string.Empty;

            var result = ParceIpAddress.Matches(responceBody);

            if (result.Count <= 0)
                return string.Empty;

            StringBuilder stringResult = new StringBuilder();
            foreach (Match match in result)
            {
                if(match.Value.Like("75.123.253.255")) // пример ip на сайте https://www.whatismyip.com/
                    continue;

                stringResult.Append(match.Value);
                stringResult.Append("; ");
            }

            return stringResult.ToString().Trim();
        }

        public static DuplicateDictionary<string, string> GetDetailedHostInfo()
        {
            DuplicateDictionary<string, string> allDetailedData = new DuplicateDictionary<string, string>();
            string machineName = System.Environment.MachineName;
            allDetailedData.Add("Host", machineName);
            allDetailedData.Add("FullName", System.Net.Dns.GetHostEntry(machineName).HostName);

            using (var mc = new ManagementClass("Win32_NetworkAdapterConfiguration"))
            using (var instances = mc.GetInstances())
            {
                foreach (ManagementObject instance in instances)
                {
                    if (!(bool)instance["ipEnabled"])
                    {
                        continue;
                    }

                    AddValue(allDetailedData, instance, "Caption");
                    AddValue(allDetailedData, instance, "ServiceName");
                    AddValue(allDetailedData, instance, "MACAddress");
                    AddValue(allDetailedData, instance, "IPAddress");
                    AddValue(allDetailedData, instance, "IPSubnet");
                    AddValue(allDetailedData, instance, "DefaultIPGateway");
                    AddValue(allDetailedData, instance, "DNSDomain");
                    AddValue(allDetailedData, instance, "Description");
                    AddValue(allDetailedData, instance, "DHCPEnabled");
                    AddValue(allDetailedData, instance, "DNSServerSearchOrder");
                }
            }

            return allDetailedData;
        }

        static void AddValue(DuplicateDictionary<string, string> collection, ManagementObject manageObj, string key)
        {
            var manageValue = manageObj[key];
            try
            {
                if (manageValue == null)
                    return;
                if (manageValue is string[])
                {
                    string[] result = (string[])manageValue;
                    foreach (string strVal in result)
                    {
                        collection.Add(key, strVal);
                    }
                }
                else
                    collection.Add(key, manageValue?.ToString());
            }
            catch (Exception)
            {
                // null
            }
        }

    }
}
