using System;
using System.IO;
using System.Linq;
using SPAFilter.SPA.Collection;
using Utils;
using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA.Components.ROBP
{
    public sealed class ROBPHostType : DriveTemplate, IHostType
    {
        public override string UniqueName
        {
            get => FilePath;
            protected set { }
        }

        public CollectionTemplate<IOperation> Operations { get; private set; } = new CollectionTemplate<IOperation>();

        public ROBPHostType(string path) : base(path)
        {
            var files = Directory.GetFiles(path).ToList();
            files.Sort(StringComparer.CurrentCulture);
            foreach (var robpOperation in files)
            {
                var isFailed = false;
                var document = XML.LoadXml(robpOperation);
                if (document == null || document.SelectNodes(@"/OperationData")?.Count == 0)
                    isFailed = true;

                Operations.Add(new ROBPOperation(robpOperation, this, isFailed));
            }
        }
    }
}