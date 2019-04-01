using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.UIControls.Tools.CamCapture
{
    public delegate void CamCaptureEventHandler(object sender, CamCaptureEventArgs args);
    public class CamCaptureEventArgs : EventArgs
    {
        public bool Result { get; }
        public Exception Error { get; }

        internal CamCaptureEventArgs(Exception ex, bool result = false)
        {
            Error = ex;
            Result = result;
        }
    }
}
