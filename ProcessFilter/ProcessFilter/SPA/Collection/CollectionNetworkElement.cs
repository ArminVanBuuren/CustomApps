using System.Collections.Generic;
using System.Linq;
using SPAFilter.SPA.Components;

namespace SPAFilter.SPA.Collection
{
    public class CollectionNetworkElement : ObjectCollection<NetworkElement>
    {
        public List<string> AllNetworkElements => this.Where(x => x.Operations.Count > 0).Select(p => p.Name).ToList();

        public List<string> AllOperationsName
        {
            get
            {
                var allOps = new List<string>();
                foreach (NetworkElement netEl in this)
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
                foreach (NetworkElement netEl in this)
                {
                    allOps.AddRange(netEl.Operations);
                }
                return allOps;
            }
        }

        public CollectionNetworkElement Clone()
        {
            var currentClone = new CollectionNetworkElement();
            foreach (NetworkElement netElement in this)
            {
                currentClone.Add(netElement.Clone());
            }
            return currentClone;
        }
    }
}
