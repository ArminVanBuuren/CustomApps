using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SPAFilter.SPA.Components
{
    public class NetworkElement : ObjectTemplate
    {
        public List<Operation> Operations { get; private set; } = new List<Operation>();

        public NetworkElement(string path, int id, List<Operation> ops)
            : base(path, id)
        {
            Operations.AddRange(ops);
        }

        public NetworkElement(string path, int id) : base(path, id)
        {
            List<string> files = Directory.GetFiles(path).ToList();
            files.Sort(StringComparer.CurrentCulture);

            int i = 0;
            foreach (string operation in files)
            {
                Operations.Add(new Operation(operation, ++i, this));
            }
        }

        public NetworkElement Clone()
        {
            var cloneElement = MemberwiseClone() as NetworkElement;
            cloneElement.Operations = new List<Operation>();
            cloneElement.Operations.AddRange(Operations);
            return cloneElement;
        }
    }
}
