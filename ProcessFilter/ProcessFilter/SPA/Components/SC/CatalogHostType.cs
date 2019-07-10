using System;
using System.Collections.Generic;

namespace SPAFilter.SPA.Components.SC
{
    public sealed class CatalogHostType : HostType
    {
        public override double FileSize { get; } = 0;
        public override string FilePath { get; } = null;

        public CatalogHostType(int id, string name, IEnumerable<Operation> allCatalogOps) : base(id)
        {
            Name = name;

            foreach (var catalogOperation in allCatalogOps)
            {
                if (catalogOperation.HostTypeName.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                {
                    Operations.Add(catalogOperation);
                }
            }
        }
    }
}
