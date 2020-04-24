using System;
using Utils.AppUpdater.Updater;

namespace Utils.AppUpdater
{

    [Serializable]
    public delegate void AppUpdatingHandler(object sender, ApplicationUpdatingArgs args);

    [Serializable]
    public delegate void AppUpdaterErrorHandler(object sender, ApplicationUpdatingArgs args);

    [Serializable]
    public delegate void AppFetchingHandler(object sender, ApplicationFetchingArgs args);

    [Serializable]
    public class ApplicationUpdatingArgs
    {
        internal ApplicationUpdatingArgs(BuildPackUpdater control)
        {
            Control = control;
        }

        internal ApplicationUpdatingArgs(BuildPackUpdater control, Exception exception, string message = null)
        {
            Control = control;
            Error = message.IsNullOrEmptyTrim() ? exception : new Exception(message, exception);
        }

        public BuildPackUpdater Control { get; }
        public Exception Error { get; }
    }

    [Serializable]
    public enum UpdateBuildResult
    {
        Cancel = 0,
        Fetch = 1
    }

    [Serializable]
    public class ApplicationFetchingArgs : ApplicationUpdatingArgs
    {
        internal ApplicationFetchingArgs(BuildPackUpdater control):base(control)
        {
            Result = UpdateBuildResult.Fetch;
        }

        internal ApplicationFetchingArgs(BuildPackUpdater control, Exception exception, string message = null) : base(control, exception, message)
        {
            Result = UpdateBuildResult.Cancel;
        }

        public UpdateBuildResult Result { get; set; }
    }
}