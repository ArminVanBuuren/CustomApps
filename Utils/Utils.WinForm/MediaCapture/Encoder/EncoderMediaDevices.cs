using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Expression.Encoder.Devices;

namespace Utils.WinForm.MediaCapture.Encoder
{
    public class EncoderMediaDevices
    {
        public Dictionary<string, EncoderDevice> VideoDevices { get; } = new Dictionary<string, EncoderDevice>();
        public Dictionary<string, EncoderDevice> AudioDevices { get; } = new Dictionary<string, EncoderDevice>();

        public EncoderMediaDevices()
        {
            try
            {
                foreach (var edv in EncoderDevices.FindDevices(EncoderDeviceType.Video))
                {
                    VideoDevices.Add(edv.Name, edv);
                }

                foreach (var eda in EncoderDevices.FindDevices(EncoderDeviceType.Audio))
                {
                    AudioDevices.Add(eda.Name, eda);
                }
            }
            catch (Exception)
            {
                // null
            }
        }

        public EncoderDevice GetDefaultVideoDevice(string name = null)
        {
            if (VideoDevices.Count == 0)
                return null;

            return GetDefaultEncoderDevice(VideoDevices, name);
        }

        public EncoderDevice GetDefaultAudioDevice(string name = null)
        {
            if (AudioDevices.Count == 0)
                return null;

            return GetDefaultEncoderDevice(AudioDevices, name);
        }

        static EncoderDevice GetDefaultEncoderDevice(Dictionary<string, EncoderDevice> encoders, string encoderName)
        {
            if (string.IsNullOrEmpty(encoderName))
                return encoders.FirstOrDefault().Value;

            if (encoders.TryGetValue(encoderName, out var encDev))
                return encDev;

            var result1 = encoders.Where(p => encoderName.StartsWith(p.Key));
            if (result1.Any())
                return result1.FirstOrDefault().Value;

            var severalNames = encoderName.Split('|');
            foreach (var encName in severalNames)
            {
                var result2 = encoders.Where(p => p.Key.IndexOf(encName, StringComparison.CurrentCultureIgnoreCase) != -1);
                if (result2.Any())
                    return result2.FirstOrDefault().Value;
            }

            return encoders.FirstOrDefault().Value;
        }

        public override string ToString()
        {
            if (VideoDevices.Count == 0 && AudioDevices.Count == 0)
                return string.Empty;

            var resultVideo = "Encoder all video devices:\r\n" + string.Join("\r\n", VideoDevices.Keys);
            var resultAudio = "Encoder all audio devices:\r\n" + string.Join("\r\n", AudioDevices.Keys);
            return (resultVideo + "\r\n" + resultAudio).Trim();
        }
    }
}
