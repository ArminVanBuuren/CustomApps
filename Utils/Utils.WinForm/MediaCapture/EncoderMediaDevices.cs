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

        public EncoderDevice GetVideoDevice(string name = null)
        {
            return GetEncoderDevice(VideoDevices, name);
        }

        public EncoderDevice GetAudioDevice(string name = null)
        {
            return GetEncoderDevice(AudioDevices, name);
        }

        static EncoderDevice GetEncoderDevice(Dictionary<string, EncoderDevice> encoders, string encoderName)
        {
            if (string.IsNullOrEmpty(encoderName))
                return encoders.FirstOrDefault().Value;
            else if (encoders.TryGetValue(encoderName, out var encDev))
                return encDev;
            return null;
        }
    }
}
