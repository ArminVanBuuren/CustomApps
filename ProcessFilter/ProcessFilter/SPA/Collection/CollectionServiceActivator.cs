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
            GetScenarios(GetServiceInstances(true), out var distinctScenarios, out var distinctCommands);
            Scenarios = distinctScenarios;
            //Commands = distinctCommands; - на канает будут повторные имена комманд даже если в разных паапках
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

        static void GetScenarios(IEnumerable<ServiceInstance> serviceInstances, out List<Scenario> distinctScenarios, out List<Command> distinctCommands)
        {
            var allScenarios = new List<Scenario>();
            var allCommands = new List<Command>();
            foreach (var instance in serviceInstances)
            {
                allScenarios.AddRange(instance.Scenarios);
                allCommands.AddRange(instance.Commands);
            }

            distinctScenarios = allScenarios.DistinctBy(p => p.FilePath.ToLower()).ToList();
            distinctCommands = allCommands.DistinctBy(p => p.FilePath.ToLower()).ToList();
        }
    }
}
