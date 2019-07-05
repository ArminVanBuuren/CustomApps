using System.Collections.Generic;
using SPAFilter.SPA.Components;

namespace SPAFilter.SPA.Collection
{
    public class CollectionBusinessProcess : List<BusinessProcess>
    {
        internal CollectionBusinessProcess()
        {
            
        }

        public CollectionBusinessProcess Clone()
        {
            CollectionBusinessProcess currentClone = new CollectionBusinessProcess();
            currentClone.AddRange(this);
            return currentClone;
        }
    }
}
