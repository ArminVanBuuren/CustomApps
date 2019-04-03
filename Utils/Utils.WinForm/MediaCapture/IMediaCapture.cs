using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utils.WinForm.MediaCapture
{
    public interface IMediaCapture
    {
        event MediaCaptureEventHandler OnRecordingCompleted;
        AForgeMediaDevices AForgeDevices { get; }
        CamMediaDevices CamDevices { get; }
        string DestinationDir { get; set; }
        int RecDurationSec { get; set; }
        DateTime? TimeOfStart { get; }
        MediaCaptureMode Mode { get; }
        void ChangeVideoDevice(string name);
        void StartCamPreview(PictureBox pictureBox);
        void StartCamRecording();
        void StartScreenRecording();
        void StartBroadcast(int port = 8080);
        void Stop();
        Task<Bitmap> GetPicture();
    }

    public abstract class MediaCapture : IMediaCapture
    {
        private string _destinationDir = string.Empty;
        MediaCaptureMode _mode = MediaCaptureMode.None;

        public event MediaCaptureEventHandler OnRecordingCompleted;

        public AForgeMediaDevices AForgeDevices { get; }

        public CamMediaDevices CamDevices { get; }

        public string DestinationDir
        {
            get => _destinationDir;
            set
            {
                if (Mode == MediaCaptureMode.Recording)
                    return;

                _destinationDir = value;
                if (!Directory.Exists(_destinationDir))
                    Directory.CreateDirectory(_destinationDir);
            }
        }
        public int RecDurationSec { get; set; }

        public MediaCaptureMode Mode
        {
            get => _mode;
            protected set
            {
                _mode = value;

                if (_mode == MediaCaptureMode.None)
                    TimeOfStart = null;
                else
                    TimeOfStart = DateTime.Now;
            }
        }

        public DateTime? TimeOfStart { get; private set; }


        protected MediaCapture(AForgeMediaDevices aDevices, CamMediaDevices cDevices, string destinationDir, int durationRecSec)
        {
            AForgeDevices = aDevices;
            CamDevices = cDevices;

            DestinationDir = destinationDir.IsNullOrEmptyTrim() ? ASSEMBLY.ApplicationDirectory : destinationDir;

            int duration = durationRecSec;
            if (duration > 1800)
                duration = 1800;
            else if (duration < 1)
                duration = 1;
            RecDurationSec = duration;
        }

        public virtual void ChangeVideoDevice(string name)
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual Task<Bitmap> GetPicture()
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual void StartBroadcast(int port = 8080)
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual void StartCamPreview(PictureBox pictureBox)
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual void StartCamRecording()
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual void StartScreenRecording()
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual void Stop()
        {
            throw new NotSupportedException("Not supported.");
        }

        protected string GetNewVideoFilePath()
        {
            return Path.Combine(DestinationDir, STRING.RandomString(15) + ".wmv");
        }

        protected void RecordCompleted(MediaCaptureEventArgs args, bool isAsync = false)
        {
            if(isAsync)
                OnRecordingCompleted?.BeginInvoke(this, args, null,null);
            else
                OnRecordingCompleted?.Invoke(this, args);
        }
    }
}