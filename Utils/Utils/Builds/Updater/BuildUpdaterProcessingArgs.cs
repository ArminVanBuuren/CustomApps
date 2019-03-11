using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Builds.Updater
{
    [Serializable]
    public class BuildUpdaterProcessingArgs
    {
        internal BuildUpdaterProcessingArgs(string exception, IUploadProgress faildObj = null)
        {
            FaildObject = faildObj;
            Error = new Exception(exception);
        }

        internal BuildUpdaterProcessingArgs(Exception error = null, IUploadProgress faildObj = null)
        {
            FaildObject = faildObj;
            Error = error;
        }

        internal BuildUpdaterProcessingArgs(IUploadProgress faildObj)
        {
            FaildObject = faildObj;
        }

        public IUploadProgress FaildObject { get; }
        public Exception Error { get; internal set; }
        public List<Exception> InnerException { get; } = new List<Exception>();
    }
}
