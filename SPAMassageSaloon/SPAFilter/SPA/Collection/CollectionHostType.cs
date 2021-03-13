using System.Collections.Generic;
using System.Linq;

namespace SPAFilter.SPA.Collection
{
    public class CollectionHostType : CollectionTemplate<IHostType>
    {
        private readonly object sync = new object();

        public CollectionTemplate<IOperation> Operations { get; private set; }

        public IReadOnlyCollection<string> HostTypeNames { get; private set; }

        public IReadOnlyCollection<string> OperationNames { get; private set; }

        public int DriveOperationsCount => Operations.OfType<DriveTemplate>().Count();

        public CollectionHostType()
        {

        }

        public void FetchNames()
        {
            HostTypeNames = this.Where(x => x.Operations.Count > 0).Select(x => x.Name).OrderBy(x => x).ToList();
            OperationNames = this.SelectMany(x => x.Operations).OrderBy(x => x.HostTypeName).ThenBy(x => x.Name).Select(x => x.Name).ToList();
        }

        public void FetchOperations()
	        => Operations = new CollectionTemplate<IOperation>(this.SelectMany(x => x.Operations).OrderBy(x => x.HostTypeName).ThenBy(x => x.Name).ToList());
    }
}