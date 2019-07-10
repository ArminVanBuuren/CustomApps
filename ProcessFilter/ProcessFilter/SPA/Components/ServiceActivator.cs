using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils;

namespace SPAFilter.SPA.Components
{
    public class ServiceActivator : ObjectTemplate
    {
        public List<ServiceInstance> Instances { get; } = new List<ServiceInstance>();

        public ServiceActivator(string filePath) : base(filePath)
        {
            Refresh();
        }

        public void Refresh()
        {
            Instances.Clear();

            if (!XML.IsFileXml(FilePath, out var activatorConfig))
                throw new Exception($"Incorrect xml file \"{FilePath}\".");

            var activatorConfigNavigator = activatorConfig.CreateNavigator();
            var serviceInstances = XPATH.Execute(activatorConfigNavigator, @"/Configuration/serviceInstances/serviceInstance");
            if (serviceInstances == null || serviceInstances.Count == 0)
                throw new Exception($"Incorrect activator config \"{FilePath}\".");

            foreach (var serviceInstance in serviceInstances)
            {
                Instances.Add(new ServiceInstance(FilePath, serviceInstance.Node));
            }
        }
    }
}