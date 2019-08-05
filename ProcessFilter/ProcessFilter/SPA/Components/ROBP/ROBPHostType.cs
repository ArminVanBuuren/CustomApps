using System;
using System.IO;
using System.Linq;
using SPAFilter.SPA.Collection;
using Utils;

namespace SPAFilter.SPA.Components.ROBP
{
    public class ROBPHostType : DriveTemplate, IHostType
    {
        public CollectionTemplate<IOperation> Operations { get; private set; } = new CollectionTemplate<IOperation>();

        public ROBPHostType(string path) : base(path)
        {
            var files = Directory.GetFiles(path).ToList();
            files.Sort(StringComparer.CurrentCulture);
            foreach (var robpOperation in files)
            {
                var document = XML.LoadXml(robpOperation);
                if (document == null || document.SelectNodes(@"/OperationData")?.Count == 0)
                    continue;

                Operations.Add(new ROBPOperation(robpOperation, this));
            }
        }
    }
}