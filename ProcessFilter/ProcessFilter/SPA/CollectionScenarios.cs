using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ProcessFilter.SPA
{
    public class CollectionScenarios : List<Scenario>
    {
        internal CollectionScenarios()
        {
            
        }

        public CollectionScenarios(string path)
        {
            string[] files = Directory.GetFiles(path, "*.xml", SearchOption.AllDirectories);
            foreach (string bpPath in files)
            {
                Add(new Scenario(bpPath));
            }
        }

        public CollectionScenarios Clone()
        {
            CollectionScenarios clone = new CollectionScenarios();
            clone.AddRange(this);
            return clone;
        }
    }

    public class Scenario : ObjectTempalte
    {
        public List<string> Commands { get; } = new List<string>();
        public Scenario(string path) : base(path)
        {

        }

        public void AddBodyCommands(XmlDocument document)
        {
            Commands.Clear();
            foreach (XmlNode xm in document.SelectNodes(@"//parameterslist/param[@name='command']/@value"))
            {
                Commands.Add(xm.InnerText);
            }
        }
    }
}
