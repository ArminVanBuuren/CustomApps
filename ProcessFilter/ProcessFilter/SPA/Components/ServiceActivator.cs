﻿using System;
using System.Collections.Generic;
using System.IO;
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
            List<XPathResult> serviceInstances;
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

                MessageBox.Show(ex.Message, FilePath, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            Instances.Clear();
            foreach (var serviceInstance in serviceInstances)
            {
                Instances.Add(new ServiceInstance(FilePath, serviceInstance.Node));
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
    }
}