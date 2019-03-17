using System;
using System.Collections.Generic;
using Utils.AppUpdater.Updater;

namespace Utils.AppUpdater
{
    [Serializable]
    public enum UpdateBuildResult
    {
        Cancel = 0,
        Update = 1
    }

    [Serializable]
    public class ApplicationUpdaterProcessingArgs
    {
        internal ApplicationUpdaterProcessingArgs(IUpdater control)
        {
            Result = UpdateBuildResult.Update;
            Control = control;
        }

        internal ApplicationUpdaterProcessingArgs(IUpdater control, Exception exception, string message = null)
        {
            Result = UpdateBuildResult.Cancel;
            Control = control;
            Error = message.IsNullOrEmptyTrim() ? exception : new Exception(message, exception);
        }

        public IUpdater Control { get; }
        public UpdateBuildResult Result { get; set; }
        public Exception Error { get; }
    }
}
