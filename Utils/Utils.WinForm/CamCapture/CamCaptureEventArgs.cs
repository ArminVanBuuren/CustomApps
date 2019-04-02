using System;

namespace Utils.WinForm.CamCapture
{
    public delegate void CamCaptureEventHandler(object sender, CamCaptureEventArgs args);
    public class CamCaptureEventArgs : EventArgs
    {
        public string DestinationFile { get; }
        public Exception Error { get; internal set; }

        internal CamCaptureEventArgs(Exception ex)
        {
            Error = ex;
        }

        internal CamCaptureEventArgs(string file)
        {
            DestinationFile = file;
        }
    }

    public class CamCaptureRunningException : Exception
    {
        public CamCaptureRunningException(string message) : base(message)
        {

        }
    }
}
