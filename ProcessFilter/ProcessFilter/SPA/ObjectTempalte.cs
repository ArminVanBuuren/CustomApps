using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.IOExploitation;

namespace ProcessFilter.SPA
{

    public class ObjectTempalte
    {
        public ObjectTempalte(string path)
        {
            Name = path.GetLastNameInPath(true);
            Path = path;
        }

        public string Name { get; protected set; }
        public string Path { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}
