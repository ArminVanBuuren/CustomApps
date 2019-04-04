using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WinForm.MediaCapture
{
    public class MediaCaptureRunningException : Exception
    {
        public MediaCaptureRunningException(string message) : base(message)
        {

        }
    }

    public class DeviceInitializationTimeoutException : Exception
    {
        public DeviceInitializationTimeoutException(string message) : base(message)
        {

        }
    }
}
