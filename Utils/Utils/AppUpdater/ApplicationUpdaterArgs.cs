using System;

namespace Utils.AppUpdater
{
    [Serializable]
    public enum UpdateBuildResult
    {
        Cancel = 0,
        Update = 1
    }

    [Serializable]
    public class ApplicationUpdaterArgs
    {
        internal ApplicationUpdaterArgs()
        {
            Result = UpdateBuildResult.Update;
        }

        public UpdateBuildResult Result { get; set; }
    }
}
