using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Utils;

namespace SPAFilter.SPA.Components
{
    public sealed class ServiceActivator : DriveTemplate
    {
        public List<ServiceInstance> Instances { get; } = new List<ServiceInstance>();

        public ServiceActivator(string filePath) : base(filePath)
        {
            var serviceInstances = LoadConfig();

            foreach (var serviceInstance in serviceInstances)
            {
                Instances.Add(new ServiceInstance(FilePath, serviceInstance.Node));
            }
        }

        public void Refresh()
        {
            XPathResultCollection serviceInstances;
            try
            {
                serviceInstances = LoadConfig();
            }
            catch (Exception ex)
            {
                foreach (var instance in Instances)
                {
                    instance.IsCorrect = false;
                }

                ShowError(ex.Message);
                return;
            }


            Instances.Clear();
            foreach (var serviceInstance in serviceInstances)
            {
                Instances.Add(new ServiceInstance(FilePath, serviceInstance.Node));
            }
        }

        XPathResultCollection LoadConfig()
        {
            if (!File.Exists(FilePath))
                throw new Exception($"File \"{FilePath}\" not found");

            if (!XML.IsFileXml(FilePath, out var activatorConfig))
                throw new Exception($"Incorrect xml file \"{FilePath}\"");

            var activatorConfigNavigator = activatorConfig.CreateNavigator();
            var serviceInstances = XPATH.Execute(activatorConfigNavigator, @"/Configuration/serviceInstances/serviceInstance");
            if (serviceInstances == null || serviceInstances.Count == 0)
                throw new Exception($"Incorrect activator config \"{FilePath}\"");

            return serviceInstances;
        }

        internal static void ShowError(string message)
        {
            MessageBox.Show(message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}