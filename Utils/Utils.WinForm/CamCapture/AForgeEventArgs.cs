using System;

namespace Utils.WinForm.CamCapture
{
    public delegate void AForgeEventHandler(object sender, AForgeEventArgs args);
    public class AForgeEventArgs : EventArgs
    {
        public string DestinationFile { get; }
        public Exception Error { get; }

        internal AForgeEventArgs(Exception ex)
        {
            Error = ex;
        }

        internal AForgeEventArgs(string file)
        {
            DestinationFile = file;
        }
    }

    public class AForgeRunningException : Exception
    {
        public AForgeRunningException(string message) : base(message)
        {

        }
    }

}
