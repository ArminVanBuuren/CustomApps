using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAFilter.SPA
{
    public abstract class ObjectCollection<T> : List<T> where T : ObjectTemplate
    {
        public static List<string> GetConfigFiles(string path)
        {
            var files = Directory.GetFiles(path, "*.xml", SearchOption.TopDirectoryOnly).ToList();
            files.Sort(StringComparer.CurrentCulture);
            return files;
        }
    }
}
