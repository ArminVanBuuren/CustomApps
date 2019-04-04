using System;
using System.Collections.Generic;
using System.Linq;
using AForge.Video.DirectShow;
using Microsoft.Expression.Encoder.Devices;

namespace Utils.WinForm.MediaCapture
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

        public List<AForgeDevice> GetVideoDevice(string name = null)
        {
            var result = new List<AForgeDevice>();

            if (VideoDevices.Count == 0)
                return result;

            if (string.IsNullOrWhiteSpace(name))
            {
                var resMin = VideoDevices.Values.OrderBy(p => p.Height).ThenByDescending(p => p.Width);
                result.Add(resMin.FirstOrDefault());
                return result;
            }

            if (VideoDevices.TryGetValue(name, out var res))
            {
                result.Add(res);
                return result;
            }

            var find = VideoDevices.Values.Where(p => p.DeviceName == name || p.MonikerString == name);
            if (find.Any())
            {
                result.AddRange(find);
            }

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
