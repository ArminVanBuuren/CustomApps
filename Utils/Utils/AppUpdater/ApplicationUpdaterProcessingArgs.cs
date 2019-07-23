using System;
using Utils.AppUpdater.Updater;

namespace Utils.AppUpdater
{

    [Serializable]
    public delegate void AppUpdatingHandler(object sender, ApplicationUpdatingArgs args);

    [Serializable]
    public delegate void AppFetchingHandler(object sender, ApplicationFetchingArgs args);

    [Serializable]
    public delegate void AppUpdaterErrorHandler(object sender, ApplicationUpdatingArgs args);


    [Serializable]
    public enum UpdateBuildResult
    {
        Cancel = 0,
        Fetch = 1
    }

    [Serializable]
    public class ApplicationUpdatingArgs
    {
        internal ApplicationUpdatingArgs(IUpdater control)
        {
            Control = control;
        }

        internal ApplicationUpdatingArgs(IUpdater control, Exception exception, string message = null)
        {
            Control = control;
            Error = message.IsNullOrEmptyTrim() ? exception : new Exception(message, exception);
        }

        public IUpdater Control { get; }
        public Exception Error { get; }
    }

    [Serializable]
    public class ApplicationFetchingArgs : ApplicationUpdatingArgs
    {
        internal ApplicationFetchingArgs(IUpdater control):base(control)
        {
            Result = UpdateBuildResult.Fetch;
        }

        internal ApplicationFetchingArgs(IUpdater control, Exception exception, string message = null) : base(control, exception, message)
        {
            Result = UpdateBuildResult.Cancel;
        }

        public UpdateBuildResult Result { get; set; }
    }
}