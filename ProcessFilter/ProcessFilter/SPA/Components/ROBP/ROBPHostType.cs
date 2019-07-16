using System;
using System.IO;
using System.Linq;

namespace SPAFilter.SPA.Components.ROBP
{
    public class ROBPHostType : HostType
    {
        public ROBPHostType(string path) : base(path)
        {
            var files = Directory.GetFiles(path).ToList();
            files.Sort(StringComparer.CurrentCulture);
            foreach (var robpOperation in files)
            {
                Operations.Add(new ROBPOperation(robpOperation, this));
            }
        }
    }
}
