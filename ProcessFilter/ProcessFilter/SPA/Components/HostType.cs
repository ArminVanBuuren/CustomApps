using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SPAFilter.SPA.Collection;

namespace SPAFilter.SPA.Components
{
    public class HostType : ObjectTemplate
    {
        public CollectionTemplate<Operation> Operations { get; private set; } = new CollectionTemplate<Operation>();

        public HostType() { }

        public HostType(string path) : base(path)
        {

        }

        public HostType Clone()
        {
            if (!(MemberwiseClone() is HostType cloneElement))
                return null;
            cloneElement.Operations = new CollectionTemplate<Operation>();
            cloneElement.Operations.AddRange(Operations);
            return cloneElement;
        }
    }
}
