using System;
using System.Collections.Generic;
using System.Management;
using Protas.Components.PerformanceLog;
using Protas.Components.XPackage;
using Protas.Control.Resource.Base;
using Protas.System.Statistic.Resource;
using Protas.System.Statistic.Handlers;

namespace Protas.System.Statistic
{
    public class UserSettings
    {
        public UserSettings(string userName, string password, string domain)
        {
            UserName = userName;
            Password = password;
            Domain = (!string.IsNullOrEmpty(domain)) ? string.Format(@"kerberos:{0}", domain) : string.Empty;
            //_connection.Authority = "ntlmdomain:DOMAIN";
        }
        public string UserName { get; }
        public string Password { get; }
        public string Domain { get; }
    }
    [Serializable]
    public class Context : ShellLog3Net, IResourceContext
    {
        public bool IsIntialized { get; } = false;
        UserSettings USettings { get; }

        public ResourceEntityCollection EntityCollection { get; } = new ResourceEntityCollection
        {
            new ResourceEntity("cpu",typeof(RIUMachineCPU)) ,
            new ResourceEntity("ram",typeof(RIUMachineRAM), EntityMode.Unusual) ,
            new ResourceEntity("disc",typeof(RIUMachineDisc), EntityMode.Unusual) ,
            new ResourceEntity("msmq",typeof(RIUMachineMsmq), EntityMode.Unusual, 1) ,
            new ResourceEntity("os",typeof(RCOSVersion), EntityMode.Unusual) ,
            new ResourceEntity("user", typeof(RCUser), EntityMode.Unusual) ,
            new ResourceEntity("machine", typeof(RCMachine), EntityMode.Unusual)
        };

        public Context(XPack config, ILog3NetMain log) : base(log)
        {
            if (config == null)
                return;
            try
            {
                XPack connection = config["Connection"]?[0];
                if (connection != null)
                    USettings = new UserSettings(connection.Attributes["username"], connection.Attributes["password"], connection.Attributes["domain"]);
                IsIntialized = true;
            }
            catch (Exception ex)
            {
                AddLogForm(Log3NetSeverity.Error, "Exception:{0}\r\n{1}", ex.Message, ex.Data);
            }
        }
        public IResource GetResource(Type resource, ResourceConstructor constructor)
        {
            object result;
            result = Activator.CreateInstance(resource, constructor, USettings);
            if (result != null)
                return (IResource)result;
            return null;
        }
        public IHandler GetHandler(XPack pack)
        {
            return new MSMQHandler();
        }
        public void Dispose()
        {

        }

    }
}