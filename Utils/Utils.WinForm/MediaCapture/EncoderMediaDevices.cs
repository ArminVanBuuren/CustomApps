using Microsoft.Expression.Encoder.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WinForm.MediaCapture
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

            var res = GetDefaultEncoderDevice(VideoDevices, name);
            return res ?? VideoDevices.FirstOrDefault().Value;
        }

        public EncoderDevice GetDefaultAudioDevice(string name = null)
        {
            if (AudioDevices.Count == 0)
                return null;

            var res = GetDefaultEncoderDevice(AudioDevices, name);
            return res ?? AudioDevices.FirstOrDefault().Value;
        }

        static EncoderDevice GetDefaultEncoderDevice(Dictionary<string, EncoderDevice> encoders, string encoderName)
        {
            if (string.IsNullOrEmpty(encoderName))
                return encoders.FirstOrDefault().Value;
            else if (encoders.TryGetValue(encoderName, out var encDev))
                return encDev;
            return null;
        }

        public override string ToString()
        {
            if (VideoDevices.Count == 0 && AudioDevices.Count == 0)
                return string.Empty;

            var resultVideo = "EncoderVideo:\r\n" + string.Join("\r\n", VideoDevices.Keys);
            var resultAudio = "EncoderAudio:\r\n" + string.Join("\r\n", AudioDevices.Keys);
            return (resultVideo + "\r\n" + resultAudio).Trim();
        }
    }
}
