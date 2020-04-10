using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Utils;

namespace SPAFilter.SPA.Components
{
    public sealed class ServiceActivator : DriveTemplate
    {
        internal List<ServiceInstance> Instances { get; } = new List<ServiceInstance>();

        public ServiceActivator(string filePath) : base(filePath)
        {
            var serviceInstances = LoadConfig();

            foreach (var serviceInstance in serviceInstances)
            {
                Instances.Add(new ServiceInstance(this, serviceInstance.Node));
            }
        }

        public void Refresh()
        {
            foreach (var instance in Instances)
            {
                try
                {
                    instance.Refresh();
                }
                catch (Exception ex)
                {
                    instance.IsCorrect = false;
                    Program.ReportMessage(ex.Message, MessageBoxIcon.Error, $"[{instance.HardwareID}]={FilePath}");
                }
            }
        }

        List<XPathResult> LoadConfig()
        {
            if (!File.Exists(FilePath))
                throw new Exception($"File \"{FilePath}\" not found");

            if (!XML.IsFileXml(FilePath, out var activatorConfig))
                throw new Exception($"Xml file \"{FilePath}\" is invalid");

            var activatorConfigNavigator = activatorConfig.CreateNavigator();
            var serviceInstances = XPATH.Select(activatorConfigNavigator, @"/Configuration/serviceInstances/serviceInstance");
            if (serviceInstances == null || serviceInstances.Count == 0)
                throw new Exception($"Activator config \"{FilePath}\" is incorrect");

            return serviceInstances;
        }

        public override string ToString()
        {
            return string.Join(";", Instances);
        }
    }
}