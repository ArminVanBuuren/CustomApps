using System.Collections.Generic;
using SPAFilter.SPA.Components;

namespace SPAFilter.SPA.Collection
{
    public class CollectionCommands : List<Command>
    {
        internal CollectionCommands()
        {

        }

        public CollectionCommands Clone()
        {
            var clone = new CollectionCommands();
            clone.AddRange(this);
            return clone;
        }
    }
}
