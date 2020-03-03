using System;

namespace Utils.AppUpdater.Updater
{
    [Serializable]
    internal class BuildUpdaterProcessingArgs
    {
        public Exception Error { get; internal set; }
    }
}
