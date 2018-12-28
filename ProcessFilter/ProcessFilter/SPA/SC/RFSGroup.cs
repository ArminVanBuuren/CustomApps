using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessFilter.SPA.SC
{
    public class RFSGroup
    {
        public string Name { get; } = "RFS_GROUP";
        public string Type { get; }
        public IEnumerable<string> DependenceRFSList { get; }

        public RFSGroup(string type, IEnumerable<string> listOfRFS)
        {
            Type = type;
            DependenceRFSList = listOfRFS;
            foreach (string rfsName in DependenceRFSList)
            {
                if (rfsName.StartsWith("RFS_", StringComparison.CurrentCultureIgnoreCase))
                    Name += "_" + rfsName.Substring(4, rfsName.Length - 4);
                else
                    Name += "_" + rfsName;
            }
        }

        public string ToXml()
        {
            string header = $"<RFSGroup name=\"{Name}\" type=\"{Type}\" description=\"Группа RFS\">";
            string rfsList = string.Empty;
            foreach (string rfsName in DependenceRFSList)
            {
                rfsList += $"<RFS name=\"{rfsName}\" linkType=\"Add\" />";
            }

            return header + rfsList + "</RFSGroup>";
        }
    }
}
