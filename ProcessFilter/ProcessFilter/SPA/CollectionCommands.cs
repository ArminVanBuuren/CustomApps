using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessFilter.SPA
{
    public class CollectionCommands : List<Command>
    {
        internal CollectionCommands()
        {

        }
        public CollectionCommands(string path)
        {
            string[] files = Directory.GetFiles(path, "*.xml", SearchOption.TopDirectoryOnly);
            foreach (string bpPath in files)
            {
                Add(new Command(bpPath));
            }
        }
        public CollectionCommands Clone()
        {
            CollectionCommands clone = new CollectionCommands();
            clone.AddRange(this);
            return clone;
        }
    }

    public class Command : ObjectTempalte
    {
        public Command(string path):base(path)
        {
            
        }
    }
}
