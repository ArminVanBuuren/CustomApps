using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.AppUpdater.Updater;

namespace Utils.AppUpdater
{
    public enum UpdateBuildResult
    {
        Cancel = 0,
        Update = 1,
        SelfUpdate = 2
    }

    public class ApplicationUpdaterArgs
    {
        internal ApplicationUpdaterArgs(IUpdater control)
        {
            Control = control;
            Result = UpdateBuildResult.Cancel;
        }
        public IUpdater Control { get; }
        public UpdateBuildResult Result { get; set; }
    }
}
