using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using Utils.CollectionHelper;

namespace Utils
{
    public static class WIN32
    {
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
                if(manageValue == null)
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
