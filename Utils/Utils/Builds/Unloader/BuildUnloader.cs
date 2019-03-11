using System.Collections.Generic;
using System.Reflection;
using Utils.Builds.Updater;

namespace Utils.Builds.Unloader
{
    public class BuildUnloader
    {
        public BuildUnloader(Assembly runningApp)
        {
            Dictionary<string, LocalAssemblyInfo> localVersions = BuildPackCollection.GetLocalVersions(runningApp);
        }
    }
}
