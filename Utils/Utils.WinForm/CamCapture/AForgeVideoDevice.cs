using AForge.Video.DirectShow;

namespace Utils.WinForm.CamCapture
{
    public class AForgeVideoDevice
    {
        public AForgeVideoDevice(string name, VideoCaptureDevice device, VideoCapabilities videoCapabilities)
        {
            DeviceName = name;
            Device = device;
            VideoCapabilities = videoCapabilities;
        }

        public string DeviceName { get; }
        public VideoCaptureDevice Device { get; }
        public VideoCapabilities VideoCapabilities { get; }
        public int Width => VideoCapabilities.FrameSize.Width;
        public int Height => VideoCapabilities.FrameSize.Width;

        public override string ToString()
        {
            return $"{DeviceName}=[W={Width};H={Height}]";
        }
    }
}
