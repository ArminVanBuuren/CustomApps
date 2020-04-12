using System;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using SPAFilter.SPA.Components;

namespace SPAFilter.SPA.Collection
{
    public class CollectionBusinessProcess : CollectionTemplate<BusinessProcess>
    {
        private readonly object sync = new object(); 
        readonly SortedList<string, bool> _allOperationsName;

        public IDictionary<string, bool> AllOperationsNames
        {
            get
            {
                lock (sync)
                    return _allOperationsName;
            }
        }

        public bool AnyHasCatalogCall => this.Any(x => x.HasCatalogCall);

        public CollectionBusinessProcess() : base()
        {
            _allOperationsName = new SortedList<string, bool>(StringComparer.InvariantCultureIgnoreCase);
        }

        public override void AddRange(IEnumerable<BusinessProcess> collection)
        {
            foreach (var businessProcess in collection)
                Add(businessProcess);
        }

        public override void Add(BusinessProcess businessProcess)
        {
            lock (sync)
            {
                base.Add(businessProcess);

                foreach (var operation in businessProcess.Operations)
                {
                    if (!_allOperationsName.ContainsKey(operation))
                        _allOperationsName.Add(operation, true);
                }
            }
        }
    }
}
