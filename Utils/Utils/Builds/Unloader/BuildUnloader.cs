using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utils.BuildUpdater
{
    public class BuildUnloader
    {
        public BuildUnloader(Assembly runningApp)
        {
            Dictionary<string, LocalAssemblyInfo> localVersions = BuildPackCollection.GetLocalVersions(runningApp);
        }
    }
}
