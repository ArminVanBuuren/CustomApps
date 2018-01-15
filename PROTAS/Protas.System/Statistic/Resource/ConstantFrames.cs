using System;
using System.Collections.Generic;
using System.Management;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using Protas.Components.XPackage;
using Protas.System.Statistic.Components;
using Protas.Control.Resource.Base;

namespace Protas.System.Statistic.Resource
{
    internal class SysResourceBase
    {
        public bool IsIntialized { get; } = false;
        public IpAddressModule MachineModule { get; }
        public ManagementObjectSearcher Searcher { get; }
        public ConnectionOptions Connection { get; }
        public SysResourceBase(string machine, UserSettings uSetting, string managescope, string objectquery)
        {
            MachineModule = new IpAddressModule(machine);
            if (MachineModule.Status != IPStatus.Success)
                return;

            ManagementScope scope;
            if (uSetting != null)
            {
                Connection = new ConnectionOptions
                {
                    Username = uSetting.UserName,
                    Password = uSetting.Password,
                    Authority = uSetting.Domain + @"\" + MachineModule.MachineName,
                    EnablePrivileges = true
                };
                scope = new ManagementScope(string.Format(@"\\{0}{1}", MachineModule.MachineName, managescope), Connection);
            }
            else
                scope = new ManagementScope(string.Format(@"\\{0}{1}", MachineModule.MachineName, managescope));

            ObjectQuery query = new ObjectQuery(objectquery);
            Searcher = new ManagementObjectSearcher(scope, query);

            IsIntialized = true;
        }

    }

    internal class RCBase : ResourceConstantFrame
    {
        public XPack Result { get; }
        public RCBase(ResourceConstructor constructor, UserSettings uSetting, string managescope, string objectquery) : base(constructor)
        {
            SysResourceBase sysres = new SysResourceBase(constructor?[0], uSetting, managescope, objectquery);
            if (!sysres.IsIntialized)
            {
                IsIntialized = false;
                return;
            }

            Result = new XPack(string.Empty, string.Empty);
            foreach (ManagementBaseObject obj in sysres.Searcher.Get())
            {
                InitializeOrUpdateObjectFields(Result, obj);
            }
        }
        public override XPack GetResult()
        {
            return Result;
        }
    }

    internal class RCOSVersion : RCBase
    {
        public RCOSVersion(ResourceConstructor constructor, UserSettings uSetting) 
            : base(constructor, uSetting, @"\ROOT\CIMV2", "SELECT * FROM Win32_OperatingSystem")
        {
        }
    }
    internal class RCUser : RCBase
    {
        public RCUser(ResourceConstructor constructor, UserSettings uSetting)
            : base(constructor, uSetting, @"\ROOT\CIMV2", "SELECT * FROM Win32_ComputerSystem")
        {
        }
    }
    internal class RCMachine : RCBase
    {
        public RCMachine(ResourceConstructor constructor, UserSettings uSetting)
            : base(constructor, uSetting,  @"\ROOT\CIMV2", "SELECT * FROM Win32_ComputerSystemProduct")
        {
        }
    }
}
