using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SPAFilter.SPA.Components
{
    public class HostType : ObjectTemplate
    {
        public List<Operation> Operations { get; private set; } = new List<Operation>();

        public HostType(string path, int id) : base(path, id)
        {

        }

        public HostType(int id) : base(id)
        {

        }

        public HostType Clone()
        {
            var cloneElement = MemberwiseClone() as HostType;
            cloneElement.Operations = new List<Operation>();
            cloneElement.Operations.AddRange(Operations);
            return cloneElement;
        }
    }
}
