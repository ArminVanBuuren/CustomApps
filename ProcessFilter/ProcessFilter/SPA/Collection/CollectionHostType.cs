using System.Collections.Generic;
using System.Linq;
using SPAFilter.SPA.Components;

namespace SPAFilter.SPA.Collection
{
    public class CollectionHostType : List<HostType>
    {
        public List<string> AllHostTypes => this.Where(x => x.Operations.Count > 0).Select(p => p.Name).ToList();

        public List<string> AllOperationsName
        {
            get
            {
                var allOps = new List<string>();
                foreach (var hostType in this)
                {
                    allOps.AddRange(hostType.Operations.Select(p => p.Name));
                }
                return allOps;
            }
        }

        public List<Operation> AllOperations
        {
            get
            {
                var allOps = new List<Operation>();
                foreach (var hostType in this)
                {
                    allOps.AddRange(hostType.Operations);
                }
                return allOps;
            }
        }

        //public void RefreshOperationsIDs()
        //{
        //    var i = 0;
        //    foreach (var ht in this)
        //    {
        //        foreach (var op in ht.Operations)
        //        {
        //            i++;
        //            op.ID = i;
        //        }
        //    }
        //}

        public CollectionHostType Clone()
        {
            var currentClone = new CollectionHostType();
            foreach (var netElement in this)
            {
                currentClone.Add(netElement.Clone());
            }
            return currentClone;
        }
    }
}
