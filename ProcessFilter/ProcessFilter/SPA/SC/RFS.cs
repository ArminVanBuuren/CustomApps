using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessFilter.SPA.SC
{
    public class RFS
    {
        public string Name { get; }
        public string Description { get; }

        public string HostType { get; }

        public IEnumerable<string> Depends { get; }

        public RFS(string srvName, string description, string netElement, IEnumerable<string> depends)
        {
            Description = description;
            Depends = depends;
            if (srvName.Length > 2 && (srvName.Substring(0, 2).Equals("CB", StringComparison.CurrentCultureIgnoreCase)
                || srvName.Substring(0, 2).Equals("FR", StringComparison.CurrentCultureIgnoreCase)))
            {
                Name = "RES_" + netElement + "_" + srvName.Substring(2, srvName.Length - 2);
            }
            else
            {
                Name = "RES_" + netElement + "_" + srvName;
            }

            HostType = netElement;
        }

        public string ToXmlCFSChild()
        {
            if (this.Depends.Any())
            {
                return $"      <RFS name=\"{Name}\" linkType=\"Add\" dependsOn=\"{string.Join(",", Depends)}\" />";
            }

            return $"      <RFS name=\"{Name}\" linkType=\"Add\" />";
        }

        public string ToXml()
        {
            return $"    <RFS name=\"{Name}\" hostType=\"{HostType}\" description=\"{Description}\" />";
        }
    }
}
