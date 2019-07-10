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
                foreach (HostType netEl in this)
                {
                    allOps.AddRange(netEl.Operations.Select(p => p.Name));
                }
                return allOps;
            }
        }

        public List<Operation> AllOperations
        {
            get
            {
                var allOps = new List<Operation>();
                foreach (HostType netEl in this)
                {
                    allOps.AddRange(netEl.Operations);
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
            foreach (HostType netElement in this)
            {
                currentClone.Add(netElement.Clone());
            }
            return currentClone;
        }
    }
}
