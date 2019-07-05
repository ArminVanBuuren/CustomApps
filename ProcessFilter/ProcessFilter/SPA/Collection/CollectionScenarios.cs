using System.Collections.Generic;
using SPAFilter.SPA.Components;

namespace SPAFilter.SPA.Collection
{
    public class CollectionScenarios : List<Scenario>
    {
        internal CollectionScenarios()
        {
            
        }

        //public CollectionScenarios(string dirPath)
        //{
        //    var files = GetConfigFiles(dirPath);

        //    int i = 0;
        //    foreach (string scenario in files)
        //    {
        //        Add(new Scenario(scenario, ++i));
        //    }
        //}

        public CollectionScenarios Clone()
        {
            CollectionScenarios clone = new CollectionScenarios();
            clone.AddRange(this);
            return clone;
        }
    }
}
