using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace SPAFilter.SPA.Components
{
    public sealed class BusinessProcess : ObjectTemplate
    {
        internal List<string> Operations { get; } = new List<string>();

        public BusinessProcess(string path, int id) : base(path, id)
        {
            if (GetNameWithId(Name, out string newName, out int newId))
            {
                Name = newName;
                ID = newId;
            }
        }

        public void AddBodyOperations(XmlDocument document)
        {
            Operations.Clear();
            foreach (XmlNode xm in document.SelectNodes(@"//param[@name='operation']/@value"))
            {
                if (!Operations.Any(p => p.Equals(xm.InnerText)))
                    Operations.Add(xm.InnerText);
            }
        }
    }
}
