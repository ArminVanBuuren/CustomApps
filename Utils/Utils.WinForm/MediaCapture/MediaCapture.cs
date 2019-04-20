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
    public abstract class MediaCapture : IMediaCapture
    {
        private int _secondsDuration = 60;
        private string _destinationDir = string.Empty;
        MediaCaptureMode _mode = MediaCaptureMode.None;

        public event MediaCaptureEventHandler OnRecordingCompleted;

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

        public int SecondsRecordDuration
        {
            get => _secondsDuration;
            set
            {
                int duration = value;
                if (duration > 1800)
                    duration = 1800;
                else if (duration < 1)
                    duration = 1;
                _secondsDuration = duration;
            }
        }

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



        protected MediaCapture(string destinationDir, int secondsRecDuration)
        {
            DestinationDir = destinationDir.IsNullOrEmptyTrim() ? ASSEMBLY.ApplicationDirectory : destinationDir;

            SecondsRecordDuration = secondsRecDuration;
        }

        public virtual void ChangeVideoDevice(string name)
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual void StartCamPreview(PictureBox pictureBox)
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual void StartCamRecording(string fileName = null)
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual Task StartCamRecordingAsync(string fileName = null)
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual void StartScreenRecording(string fileName = null)
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual Task StartScreenRecordingAsync(string fileName = null)
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual bool StartBroadcast(int port = 8080)
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual Task<bool> StartBroadcastAsync(int port = 8080)
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual void Stop()
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual Task StopAsync()
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual Bitmap GetPicture()
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual Task<Bitmap> GetPictureAsync()
        {
            throw new NotSupportedException("Not supported.");
        }

        protected static void DeleteRecordedFile(string destinationFilePath, bool whileAccessing = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(destinationFilePath) || !File.Exists(destinationFilePath))
                    return;

                if (whileAccessing)
                    CMD.DeleteFile(destinationFilePath);
                else
                    File.Delete(destinationFilePath);
            }
            catch (Exception)
            {
                // null
            }
        }

        protected string GetNewVideoFilePath(string fileName, string extension = ".wmv")
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return Path.Combine(DestinationDir, STRING.RandomString(15) + extension);
            return Path.Combine(DestinationDir, fileName);
        }

        protected void RecordCompleted(MediaCaptureEventArgs args, bool isAsync = false)
        {
            try
            {
                if (isAsync)
                    OnRecordingCompleted?.BeginInvoke(this, args, null, null);
                else
                    OnRecordingCompleted?.Invoke(this, args);
            }
            catch (Exception)
            {
                // null
            }
        }

        public override string ToString()
        {
            return $"Seconds=[{SecondsRecordDuration}]\r\nMode=[{Mode:G}]";
        }
    }
}
