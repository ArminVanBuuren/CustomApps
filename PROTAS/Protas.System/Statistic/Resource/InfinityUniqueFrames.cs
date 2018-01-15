using System.Collections.Generic;
using System.Management;
using System.Diagnostics;
using Protas.System.Statistic.Components;
using System.Net.NetworkInformation;
using System;
using Protas.Components.XPackage;
using Protas.Control.Resource.Base;

namespace Protas.System.Statistic.Resource
{
    internal class RIUMachineCPU : ResourceInfinityUniqueTimerFrame
    {
        XPack _result;
        PerformanceCounter CObjectCPU { get; }
        public RIUMachineCPU(ResourceConstructor constructor) : base(constructor)
        {
            IpAddressModule machineModule = new IpAddressModule(constructor?[0]);
            if (machineModule.Status != IPStatus.Success)
                return;

            Interval = 4000;
            if (!IsIntialized)
                return;
            CObjectCPU = new PerformanceCounter("Processor", "% Processor Time", "_Total", machineModule.MachineName);
            _result = new XPack(string.Empty, CObjectCPU.NextValue().ToString());
        }
        public override XPack GetResult()
        {
            if (!IsIntialized)
                return null;
            _result.Value = CObjectCPU.NextValue().ToString();
            return _result;
        }
    }

    class RIUBaseManagment : ResourceInfinityUniqueTimerFrame
    {
        public XPack Result { get; }
        public SysResourceBase MainResource { get; }
        public RIUBaseManagment(ResourceConstructor constructor, UserSettings uSettings, string managescope, string objectquery) : base(constructor)
        {
            MainResource = new SysResourceBase(constructor?[0], uSettings, managescope, objectquery);
            if (!MainResource.IsIntialized)
            {
                IsIntialized = false;
                return;
            }

            Result = new XPack(string.Empty, string.Empty);
            foreach (ManagementBaseObject obj in MainResource.Searcher.Get())
            {
                InitializeOrUpdateObjectFields(Result, obj);
            }
        }

        public override XPack GetResult()
        {
            foreach (ManagementBaseObject oneVolume in MainResource.Searcher.Get())
            {
                InitializeOrUpdateObjectFields(Result, oneVolume);
            }
            return Result;
        }

    }
    internal class RIUMachineRAM : RIUBaseManagment
    {
        public RIUMachineRAM(ResourceConstructor constructor, UserSettings uSettings) 
            : base(constructor, uSettings,  @"\ROOT\CIMV2", "SELECT * FROM CIM_OperatingSystem")
        {
            Interval = 4000;
        }
    }
    internal class RIUMachineDisc : RIUBaseManagment
    {
        public RIUMachineDisc(ResourceConstructor constructor, UserSettings uSettings)
            : base(constructor, uSettings,  @"\ROOT\Microsoft\Windows\Storage", "SELECT * FROM MSFT_Volume")
        {
            Interval = 5000;
        }
    }
    internal class RIUMachineMsmq : RIUBaseManagment
    {
        public RIUMachineMsmq(ResourceConstructor constructor, UserSettings uSettings)
            : base(constructor, uSettings,  @"\ROOT\CIMV2", "select * from Win32_PerfRawdata_MSMQ_MSMQQueue")
        {
            Interval = 2000;
        }
    }
}
