using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Utils.CollectionHelper;

namespace Utils
{
    public static class HOST
    {
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
