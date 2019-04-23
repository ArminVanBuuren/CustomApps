using System;
using System.Collections.Generic;

namespace Utils.WinForm.MediaCapture
{
    public delegate void MediaCaptureEventHandler(object sender, MediaCaptureEventArgs args);
    public class MediaCaptureEventArgs : EventArgs
    {
        public string[] FilesDestinations { get; }
        public Exception Error { get; internal set; }

        internal MediaCaptureEventArgs(string[] files, Exception ex)
        {
            FilesDestinations = files;
            Error = ex;
        }

        internal MediaCaptureEventArgs(string[] files)
        {
            FilesDestinations = files;
        }
    }
}
