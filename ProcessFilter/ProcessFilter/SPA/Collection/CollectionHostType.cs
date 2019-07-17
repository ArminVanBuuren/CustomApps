using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using SPAFilter.SPA.Components;

namespace SPAFilter.SPA.Collection
{
    public class CollectionHostType : CollectionTemplate<HostType>
    {
        public List<string> HostTypeNames => this.Where(x => x.Operations.Count > 0).OrderBy(p => p.Name).Select(p => p.Name).ToList();

        public int OperationsCount => this.Sum(x => x.Operations.Count);

        public List<string> OperationNames => this.SelectMany(x => x.Operations.OrderBy(p => p.HostTypeName).ThenBy(p => p.Name).Select(p => p.Name)).ToList();

        public CollectionTemplate<Operation> Operations => CollectionTemplate<Operation>.ToCollection(this.SelectMany(x => x.Operations).OrderBy(p => p.HostTypeName).ThenBy(p => p.Name));
    }
}
