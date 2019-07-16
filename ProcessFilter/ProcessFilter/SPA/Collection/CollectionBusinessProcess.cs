using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPAFilter.SPA.Components;

namespace SPAFilter.SPA.Collection
{
    public class CollectionBusinessProcess : CollectionTemplate<BusinessProcess>
    {
        public Dictionary<string, string> AllOperationsNames => this.SelectMany(x => x.Operations.Select(p => p)).Distinct(StringComparer.CurrentCultureIgnoreCase).OrderBy(p => p).ToDictionary(x => x, r => r, StringComparer.CurrentCultureIgnoreCase);

        //public Dictionary<string, string> AllOperationsNames
        //{
        //    get
        //    {
        //        var collectionOperation = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        //        foreach (var bp in this)
        //        {
        //            foreach (var op in bp.Operations)
        //            {
        //                if (!collectionOperation.ContainsKey(op))
        //                    collectionOperation.Add(op, op);
        //            }
        //        }

        //        return collectionOperation;
        //    }
        //}

        public bool AnyHasCatalogCall => this.Any(x => x.HasCatalogCall);
    }
}
