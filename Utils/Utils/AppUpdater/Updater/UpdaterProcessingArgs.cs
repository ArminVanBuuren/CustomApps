using System;

namespace Utils.AppUpdater.Updater
{
    [Serializable]
    public class UpdaterProcessingArgs
    {
        public Exception Error { get; internal set; }
    }
}
