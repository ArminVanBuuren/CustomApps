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
        internal ApplicationUpdaterProcessingArgs(string exception)
        {
            Error = new Exception(exception);
        }

        internal ApplicationUpdaterProcessingArgs(Exception error = null)
        {
            Error = error;
        }

        internal ApplicationUpdaterProcessingArgs()
        {

        }

        public Exception Error { get; internal set; }
        public List<Exception> InnerException { get; } = new List<Exception>();
    }
}
