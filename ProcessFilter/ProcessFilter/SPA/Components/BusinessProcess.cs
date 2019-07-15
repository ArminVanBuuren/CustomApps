using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Utils;

namespace SPAFilter.SPA.Components
{
    public sealed class BusinessProcess : ObjectTemplate
    {
        internal List<string> Operations { get; }
        public bool HasCatalogCall { get; }

        BusinessProcess(string filePath, int id, List<string> operations, bool hasCatalogCall) : base(filePath, id)
        {
            if (GetNameWithId(Name, out var newName, out var newId))
            {
                Name = newName;
                ID = newId;
            }

            Operations = operations;
            HasCatalogCall = hasCatalogCall;
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

            var navigator = document.CreateNavigator();
            bool hasCatalogCall = false;
            var isExistscObject = XPATH.Execute(navigator, @"/BusinessProcessData/businessprocess/scenario/objectlist/object[@class='FORIS.ServiceProvisioning.BPM.SCProcessingUnit']/@name".ToLower());
            if (isExistscObject != null)
            {
                foreach (var obj in isExistscObject)
                {
                    var res = XPATH.Execute(navigator, $"/BusinessProcessData/businessprocess/scenario/automat/node[@object='{obj.Value}']".ToLower());
                    if (res == null || res.Count <= 0)
                        continue;

                    hasCatalogCall = true;
                    break;
                }
            }

            bpResult = new BusinessProcess(filePath, id, operations, hasCatalogCall);
            return true;
        }
    }
}