using System;
using System.Collections.Generic;
using System.Linq;
using AForge.Video.DirectShow;

namespace Utils.Media.MediaCapture.AForge
{
    public class AForgeMediaDevices
    {
        public Dictionary<string, AForgeDevice> VideoDevices { get; } = new Dictionary<string, AForgeDevice>();

        public AForgeMediaDevices()
        {
            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in videoDevices)
            {
                var videoDevice = new VideoCaptureDevice(device.MonikerString);
                var videoCapabilities = videoDevice.VideoCapabilities;

                foreach (var capabilty in videoCapabilities)
                {
                    var vidDevice = new AForgeDevice(device.Name, device.MonikerString, videoDevice, capabilty);
                    VideoDevices.Add(vidDevice.ToString(), vidDevice);
                }
            }
        }

        public AForgeDevice GetDefaultVideoDevice(string name = null)
        {
            if (VideoDevices.Values.Count == 0)
                return null;

            if (string.IsNullOrWhiteSpace(name))
            {
                var orderByBestQuolity = VideoDevices.Values.OrderByDescending(p => p.Height).ThenByDescending(p => p.Width);
                return GetOptimalQuality(orderByBestQuolity);
            }

            if (VideoDevices.TryGetValue(name, out var res))
            {
                return res;
            }

            var result1 = VideoDevices.Values.Where(p => p.DeviceName == name || p.MonikerString == name);
            if (result1.Any())
            {
                var orderByBestQuolity = result1.OrderByDescending(p => p.Height).ToList();
                return GetOptimalQuality(orderByBestQuolity);
            }

            var result2 = VideoDevices.Values.Where(p => p.DeviceName.IndexOf(name, StringComparison.CurrentCultureIgnoreCase) != -1);
            if (result2.Any())
            {
                var orderByBestQuolity = result2.OrderByDescending(p => p.Height).ToList();
                return GetOptimalQuality(orderByBestQuolity);
            }

            return VideoDevices.FirstOrDefault().Value;
        }

        static AForgeDevice GetOptimalQuality(IEnumerable<AForgeDevice> all)
        {
            var optimal = all.Where(p => p.Width < 700);

            if (optimal.Any())
                return optimal.FirstOrDefault();

            return all.FirstOrDefault();
        }

        public override string ToString()
        {
            if (VideoDevices.Count == 0)
                return string.Empty;

            var result = ("AForge all video devices:\r\n" + string.Join("\r\n", VideoDevices.Keys)).Trim();
            return result;
        }
    }

    public class AForgeDevice
    {
        internal AForgeDevice(string name, string monikerString, VideoCaptureDevice device, VideoCapabilities videoCapabilities)
        {
            DeviceName = name;
            MonikerString = monikerString;
            Device = device;
            VideoCapabilities = videoCapabilities;
        }

        public string DeviceName { get; }
        public string MonikerString { get; }
        public VideoCaptureDevice Device { get; }
        public VideoCapabilities VideoCapabilities { get; }
        public int Width => VideoCapabilities.FrameSize.Width;
        public int Height => VideoCapabilities.FrameSize.Height;

        public override string ToString()
        {
            return $"{DeviceName}=[W={Width};H={Height}]";
        }
    }
}
