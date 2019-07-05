using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Utils;

namespace SPAFilter.SPA.Components
{
    public sealed class BusinessProcess : ObjectTemplate
    {
        internal List<string> Operations { get; }

        BusinessProcess(string filePath, int id, List<string> operations) : base(filePath, id)
        {
            if (GetNameWithId(Name, out var newName, out var newId))
            {
                Name = newName;
                ID = newId;
            }

            Operations = operations;
        }

        public static bool IsBusinessProcess(string filePath, int id, out BusinessProcess bpResult)
        {
            bpResult = null;
            var document = XML.LoadXml(filePath, true);
            if (document == null || document.SelectNodes(@"/businessprocessdata")?.Count == 0)
                return false;

            var operations = new List<string>();
            var getOperations = document.SelectNodes(@"//param[@name='operation']/@value");
            if (getOperations != null)
            {
                foreach (XmlNode xm in getOperations)
                {
                    if (!operations.Any(p => p.Equals(xm.InnerText)))
                        operations.Add(xm.InnerText);
                }
            }

            if (operations.Count <= 0)
                return false;

            bpResult = new BusinessProcess(filePath, id, operations);
            return true;
        }
    }
}