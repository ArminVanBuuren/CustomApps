using System.Collections.Generic;
using SPAFilter.SPA.Collection;

namespace SPAFilter.SPA.Components.SRI
{
    public sealed class CatalogHostType : ObjectTemplate, IHostType
    {
        public CollectionTemplate<IOperation> Operations { get; private set; } = new CollectionTemplate<IOperation>();

        public CatalogHostType(string name, IEnumerable<IOperation> allCatalogOps)
        {
            Name = name;
            Operations.AddRange(allCatalogOps);
        }
    }
}
