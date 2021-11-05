using System;

namespace Utils.Media.MediaCapture
{
    public class MediaCaptureRunningException : Exception
    {
        public MediaCaptureRunningException(string message) : base(message)
        {

        }
    }
}
