using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ProcessFilter
{
    public class BusinessProcesses : List<BusinessProcess>
    {
        internal BusinessProcesses()
        {
            
        }

        public BusinessProcesses(string prcsPath)
        {
            string[] files = Directory.GetFiles(prcsPath);
            foreach (string bpPath in files)
            {
                Add(new BusinessProcess(bpPath));
            }
        }

        public BusinessProcesses Clone()
        {
            BusinessProcesses currentClone = new BusinessProcesses();
            currentClone.AddRange(this);
            return currentClone;
        }
    }

    public class BusinessProcess : ObjectTempalte
    {
        public List<string> Operations { get; } = new List<string>();

        public BusinessProcess(string path):base(path)
        {

        }

        public void AddBodyOperations(XmlDocument document)
        {
            Operations.Clear();
            foreach (XmlNode xm in document.SelectNodes(@"//param[@name='operation']/@value"))
            {
                Operations.Add(xm.InnerText);
            }
        }
    }

    public class ObjectTempalte
    {
        public ObjectTempalte(string path)
        {
            Name = path.GetLastNameInPath();
            Path = path;
        }
        public string Name { get; }
        public string Path { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}
