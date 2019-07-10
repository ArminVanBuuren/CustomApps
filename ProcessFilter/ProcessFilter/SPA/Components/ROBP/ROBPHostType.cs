using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SPAFilter.SPA.Components.ROBP
{
    public class ROBPHostType : HostType
    {
        public ROBPHostType(string path, int id, IEnumerable<Operation> robpOps) : base(path, id)
        {
            Operations.AddRange(robpOps);
        }

        public ROBPHostType(string path, int id) : base(path, id)
        {
            var files = Directory.GetFiles(path).ToList();
            files.Sort(StringComparer.CurrentCulture);

            var i = 0;
            foreach (var robpOperation in files)
            {
                Operations.Add(new ROBPOperation(robpOperation, ++i, this));
            }
        }
    }
}
