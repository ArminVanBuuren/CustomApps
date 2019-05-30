using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA
{
    public class CollectionCommands : List<Command>
    {
        internal CollectionCommands()
        {

        }
        public CollectionCommands(string path)
        {
            List<string> files = Directory.GetFiles(path, "*.xml", SearchOption.TopDirectoryOnly).ToList();
            files.Sort(StringComparer.CurrentCulture);

            int i = 0;
            foreach (string bpPath in files)
            {
                Add(new Command(bpPath, ++i));
            }
        }
        public CollectionCommands Clone()
        {
            CollectionCommands clone = new CollectionCommands();
            clone.AddRange(this);
            return clone;
        }
    }

    public class Command : ObjectTemplate
    {
        public Command(string path, int id):base(path, id)
        {
            
        }

        [DGVEnhancer.DGVColumnAttribute(DGVEnhancer.ColumnPosition.Before, "Command")]
        public override string Name { get; protected set; }
    }
}
