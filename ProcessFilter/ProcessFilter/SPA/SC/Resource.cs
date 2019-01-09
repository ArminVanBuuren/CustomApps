using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessFilter.SPA.SC
{
    public class Resource
    {
        public string Name { get; }
        public string Description { get; }
        public HostOperation HostOperation { get; }

        public Resource(HostOperation hostOperation)
        {
            Name = $"RES_{hostOperation.HostType}_{hostOperation.OperationName}";
            Description = $"Общий ресурс для услуг - {string.Join(",", hostOperation.ChildCFS.Keys)}";
            HostOperation = hostOperation;
        }

        public string GetBaseCFSResource()
        {
            return $"<Resource name=\"{Name}\" useType=\"Mandatory\" />";
        }

        public string GetChildCFSResource(string serviceCode)
        {
            string resourceValue = serviceCode;
            if (resourceValue.Substring(0, 2).Equals("CB", StringComparison.CurrentCultureIgnoreCase) || resourceValue.Substring(0, 2).Equals("FR", StringComparison.CurrentCultureIgnoreCase))
            {
                resourceValue = resourceValue.Substring(2, resourceValue.Length - 2);
            }

            return $"<Resource name=\"{Name}\" value=\"{resourceValue}\" />";
        }

        public string ToXml()
        {
            return $"<Resource name=\"{Name}\" description=\"{Description}\"/>";
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
