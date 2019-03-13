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
        internal ApplicationUpdaterProcessingArgs(string exception, IUploadProgress faildObj = null)
        {
            FaildObject = faildObj;
            Error = new Exception(exception);
        }

        internal ApplicationUpdaterProcessingArgs(Exception error = null, IUploadProgress faildObj = null)
        {
            FaildObject = faildObj;
            Error = error;
        }

        internal ApplicationUpdaterProcessingArgs(IUploadProgress faildObj)
        {
            FaildObject = faildObj;
        }

        public IUploadProgress FaildObject { get; }
        public Exception Error { get; internal set; }
        public List<Exception> InnerException { get; } = new List<Exception>();
    }
}
