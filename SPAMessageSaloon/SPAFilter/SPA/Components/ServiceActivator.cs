using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using SPAFilter.Properties;
using SPAMessageSaloon.Common;
using Utils;

namespace SPAFilter.SPA.Components
{
    public sealed class ServiceActivator : DriveTemplate
    {
        internal List<ServiceInstance> Instances { get; } = new List<ServiceInstance>();

        public ServiceActivator(string filePath) : base(filePath)
        {
            var serviceInstances = LoadConfig();

            foreach (var instanceXml in serviceInstances)
                Add(instanceXml);
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
                    ReportMessage.Show(ex.ToString(), MessageBoxIcon.Error, $"{FilePath} \\ [{instance.HardwareID}]", false);
                }
            }
        }

        public void Reload()
        {
            var serviceInstances = LoadConfig();

            Instances.Clear();

            foreach (var instanceXml in serviceInstances)
            {
                try
                {
                    Add(instanceXml);
                }
                catch (Exception ex)
                {
                    ReportMessage.Show(ex.ToString(), MessageBoxIcon.Error, $"{FilePath} \\ {instanceXml}", false);
                }
            }
        }

        void Add(XPathResult instanceXml)
        {
            var instance = new ServiceInstance(this, instanceXml.Node);
            Instances.Add(instance);
        }

        IEnumerable<XPathResult> LoadConfig()
        {
            if (!File.Exists(FilePath))
                throw new Exception(string.Format(Resources.FileNotFound, FilePath));

            if (!XML.IsFileXml(FilePath, out var activatorConfig))
                throw new Exception(string.Format(Resources.InvalidXml, FilePath));

            var activatorConfigNavigator = activatorConfig.CreateNavigator();
            var serviceInstances = XPATH.Select(activatorConfigNavigator, @"/Configuration/serviceInstances/serviceInstance");
            if (serviceInstances == null || serviceInstances.Count == 0)
                throw new Exception(string.Format(Resources.ServiceActivator_ConfigInvalid, FilePath));

            return serviceInstances;
        }

        public override string ToString()
        {
            return string.Join(";", Instances);
        }
    }
}