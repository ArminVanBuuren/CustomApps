using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace SPAFilter.SPA
{
    public class CollectionBusinessProcess : List<BusinessProcess>
    {
        internal CollectionBusinessProcess()
        {
            
        }

        public CollectionBusinessProcess(string prcsPath)
        {
            List<string> files = Directory.GetFiles(prcsPath).ToList();
            files.Sort(StringComparer.CurrentCulture);

            int i = 0;
            foreach (string bpPath in files)
            {
                Add(new BusinessProcess(bpPath, ++i));
            }
        }

        public CollectionBusinessProcess Clone()
        {
            CollectionBusinessProcess currentClone = new CollectionBusinessProcess();
            currentClone.AddRange(this);
            return currentClone;
        }
    }

    public sealed class BusinessProcess : ObjectTempalte
    {
        internal List<string> Operations { get; } = new List<string>();

        public BusinessProcess(string path, int id) : base(path, id)
        {
            if (IsNameWithId(Name, out string newName, out int newId))
            {
                Name = newName;
                ID = newId;
            }
        }

        internal static bool IsNameWithId(string name, out string newName, out int newId)
        {
            Match match = Regex.Match(name, @"(.+)\.\(\s*(\d+)\s*\)");
            if (match.Success && int.TryParse(match.Groups[2].Value, out int res))
            {
                newName = match.Groups[1].Value;
                newId = res;
                return true;
            }

            newName = name;
            newId = -1;
            return false;
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
