using Microsoft.Expression.Encoder;
using Microsoft.Expression.Encoder.Devices;
using Microsoft.Expression.Encoder.Live;
using Microsoft.Expression.Encoder.Profiles;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Timers;
using System.Linq;

namespace TFSAssist.WebCam
{
    public enum RecordingType
    {
        Audio = 0,
        Video = 1
    }

    public class WebcamControl
    {
        public ImageFormat SnapshotFormat => ImageFormat.Jpeg;
        public EncoderDevice VideoDevice { get; private set; }
        public EncoderDevice AudioDevice { get; private set; }

        public Collection<EncoderDevice> VideoDevices { get; set; }
        public Collection<EncoderDevice> AudioDevices { get; set; }
        public string VideoFileFormat { get; set; } = "wmv";
        public bool StatusRecording { get; private set; } = false;
        private Timer _stopWatch;

        public WebcamControl()
        {
            VideoDevices = EncoderDevices.FindDevices(EncoderDeviceType.Video);
            AudioDevices = EncoderDevices.FindDevices(EncoderDeviceType.Audio);

            _stopWatch = new Timer
            {
                Interval = 60
            };
            _stopWatch.Elapsed += _stopWatch_Elapsed;
            _stopWatch.AutoReset = false;
        }

        public void ChangeVideoDevice(string name)
        {
            EncoderDevice result = ChangeDevice(VideoDevices, name);
            if (name != null)
                VideoDevice = result;
        }

        public void ChangeAudioDevice(string name)
        {
            EncoderDevice result = ChangeDevice(AudioDevices, name);
            if (name != null)
                AudioDevice = result;
        }

        public EncoderDevice ChangeDevice(Collection<EncoderDevice> devices, string name)
        {
            var videoDevice = devices.Where(p => p.Name == name);
            if (videoDevice.Count() > 0)
            {
                return videoDevice.First();
            }

            return null;
        }

        public void StartVideoRecording(int seconds = 60)
        {
            _stopWatch.Interval = seconds * 1000;
            if (StatusRecording)
                return;

            _stopWatch.Start();
            StatusRecording = true;
        }

        public void StartAudioRecording(int seconds = 60)
        {
            _stopWatch.Interval = seconds * 1000;
            if (StatusRecording)
                return;

            _stopWatch.Start();
            StatusRecording = true;
        }

        private void _stopWatch_Elapsed(object sender, ElapsedEventArgs e)
        {
            
        }

        public void StopRecording()
        {

        }

        public void TakeSnapshot()
        {

        }

        public void TakeScreenshot()
        {

        }
    }
}
