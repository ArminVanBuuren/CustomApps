using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utils.Media.MediaCapture
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
                var duration = value;
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

        public virtual void StartPreview(PictureBox pictureBox)
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual void StartRecording(string fileName = null)
        {
            throw new NotSupportedException("Not supported.");
        }

        public virtual Task StartRecordingAsync(string fileName = null)
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

        protected static void DeleteRecordedFile(string[] filesDestinations, bool whileAccessing = false)
        {
            try
            {
                if (filesDestinations == null || filesDestinations.Length == 0)
                    return;

                if (whileAccessing)
                {
                    foreach (var file in filesDestinations)
                    {
                        if (!string.IsNullOrWhiteSpace(file) && File.Exists(file))
                            CMD.DeleteFile(file);
                    }
                }
                else
                {
                    foreach (var file in filesDestinations)
                    {
                        if (!string.IsNullOrWhiteSpace(file) && File.Exists(file))
                            File.Delete(file);
                    }
                }
            }
            catch (Exception)
            {
                // null
            }
        }

        protected string GetNewVideoFilePath(string fileName, string extension = ".wmv")
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return Path.Combine(DestinationDir, $"{DateTime.Now:ddHHmmss}_{STRING.RandomString(15)}{extension}");
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
