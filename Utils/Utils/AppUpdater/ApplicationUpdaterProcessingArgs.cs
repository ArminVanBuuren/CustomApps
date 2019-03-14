using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.AppUpdater.Updater;

namespace Utils.AppUpdater
{
    [Serializable]
    public class ApplicationUpdaterProcessingArgs
    {
        internal ApplicationUpdaterProcessingArgs(string exception, IUpdater control = null)
        {
            Control = control;
            Error = new Exception(exception);
        }

        internal ApplicationUpdaterProcessingArgs(Exception error = null, IUpdater control = null)
        {
            Control = control;
            Error = error;
        }

        internal ApplicationUpdaterProcessingArgs(IUpdater control)
        {
            Control = control;
        }

        public IUpdater Control { get; }
        public Exception Error { get; internal set; }
        public List<Exception> InnerException { get; } = new List<Exception>();
    }
}
