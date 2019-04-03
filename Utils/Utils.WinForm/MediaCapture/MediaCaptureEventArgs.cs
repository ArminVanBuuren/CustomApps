using System;

namespace Utils.WinForm.MediaCapture
{
    public delegate void MediaCaptureEventHandler(object sender, MediaCaptureEventArgs args);
    public class MediaCaptureEventArgs : EventArgs
    {
        public string DestinationFile { get; }
        public Exception Error { get; internal set; }

        internal MediaCaptureEventArgs(Exception ex)
        {
            Error = ex;
        }

        internal MediaCaptureEventArgs(string file)
        {
            DestinationFile = file;
        }
    }
}
