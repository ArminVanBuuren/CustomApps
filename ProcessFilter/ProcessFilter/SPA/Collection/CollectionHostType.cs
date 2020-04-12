﻿using System.Collections.Generic;
using System.Linq;
using SPAFilter.SPA.Components;

namespace SPAFilter.SPA.Collection
{
    public class CollectionHostType : CollectionTemplate<IHostType>
    {
        private readonly object sync = new object();

        private readonly List<string> _hostTypeNames;
        private readonly List<string> _operationNames;
        private readonly CollectionTemplate<IOperation> _operations;

        public IReadOnlyCollection<string> HostTypeNames
        {
            get
            {
                lock (sync)
                    return _hostTypeNames;
            }
        }

        public IReadOnlyCollection<string> OperationNames
        {
            get
            {
                lock (sync)
                    return _operationNames;
            }
        }

        public CollectionTemplate<IOperation> Operations
        {
            get
            {
                lock (sync)
                    return _operations;
            }
        }


        public int OperationsCount => this.Sum(x => x.Operations.Count);

        public int DriveOperationsCount => this.Sum(x => x.Operations.OfType<DriveTemplate>().Count());

        public CollectionHostType()
        {
            _hostTypeNames = new List<string>();
            _operationNames = new List<string>();
            _operations = new CollectionTemplate<IOperation>();
        }

        public override void AddRange(IEnumerable<IHostType> collection)
        {
            foreach (var hostType in collection)
                Add(hostType);
        }

        public override void Add(IHostType hostType)
        {
            lock (sync)
            {
                base.Add(hostType);

                if (hostType.Operations.Count > 0)
                    _hostTypeNames.Add(hostType.Name);

                foreach (var operation in hostType.Operations)
                {
                    _operationNames.Add(operation.Name);
                    _operations.Add(operation);
                }
            }
        }
    }
}