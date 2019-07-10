using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SPAFilter.SPA.Components;
using Utils;

namespace SPAFilter.SPA.Collection
{
    public class CollectionServiceActivator
    {
        readonly List<ServiceActivator> _activators = new List<ServiceActivator>();

        public List<ServiceInstance> ServiceInstances { get; private set; }
        public List<Scenario> Scenarios { get; private set; }
        public List<Command> Commands { get; private set; }

        public void Add(string filePath)
        {
            if (_activators.Any(x => x.FilePath.Equals(filePath, StringComparison.CurrentCultureIgnoreCase)))
            {
                MessageBox.Show($"Activator \"{filePath}\" already exist.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _activators.Add(new ServiceActivator(filePath));
            ReInitCollection();
        }

        public void Remove(List<string> removeActivatorList)
        {
            foreach (var filePath in removeActivatorList)
            {
                for (var index = 0; index < _activators.Count; index++)
                {
                    var activator = _activators[index];
                    if (!activator.FilePath.Equals(filePath, StringComparison.CurrentCultureIgnoreCase))
                        continue;

                    _activators.Remove(activator);
                    break;
                }
            }

            ReInitCollection();
        }

        public void Refresh()
        {
            foreach (var activator in _activators)
            {
                activator.Refresh();
            }
            ReInitCollection();
        }

        void ReInitCollection()
        {
            ServiceInstances = GetServiceInstances();
            Scenarios = GetScenarios(GetServiceInstances(true));
            Commands = null;
        }

        List<ServiceInstance> GetServiceInstances(bool getValid = false)
        {
            var intsances = new List<ServiceInstance>();
            foreach (var activator in _activators)
            {
                foreach (var instance in activator.Instances)
                {
                    if (getValid && !instance.IsCorrect)
                        continue;
                    intsances.Add(instance);
                }
            }

            return intsances;
        }

        static List<Scenario> GetScenarios(List<ServiceInstance> serviceInstances)
        {
            var allScenarios = new List<Scenario>();
            foreach (var instance in serviceInstances)
            {
                allScenarios.AddRange(instance.Scenarios);
            }

            return allScenarios.DistinctBy(p => p.FilePath.ToLower()).ToList();
        }
    }
}
