using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Builds.Updater
{
    public enum UpdateBuildResult
    {
        Cancel = 0,
        Update = 1,
        SelfUpdate = 2
    }

    public class BuildUpdaterArgs
    {
        internal BuildUpdaterArgs(BuildPackCollection buildPacks)
        {
            BuildPacks = buildPacks;
            Result = UpdateBuildResult.Cancel;
        }
        public BuildPackCollection BuildPacks { get; }
        public UpdateBuildResult Result { get; set; }
    }
}
