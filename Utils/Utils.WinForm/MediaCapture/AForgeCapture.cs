using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Video.VFW;

//using AForge.Video.FFMPEG;

namespace Utils.WinForm.MediaCapture
{
    public class AForgeCapture : MediaCapture, IDisposable
    {
        private VideoCaptureDevice _finalVideo = null;
        private readonly AVIWriter _aviWriter;
        //private VideoFileWriter FileWriter = new VideoFileWriter();

        // MSVC - с компрессией; wmv3 - с компрессией но нужен кодек
        // MRLE
        // http://sundar1984.blogspot.com/2007_08_01_archive.html

        private Action<Bitmap, bool> _updatePicFrame;

        public event MediaCaptureEventHandler OnUnexpectedError;
        public AForgeDevice VideoDevice { get; private set; }

        public AForgeCapture(Thread thread, AForgeMediaDevices aDevices, EncoderMediaDevices cDevices, string destinationDir, int secondsRecDuration = 60) : base(thread, aDevices, cDevices, destinationDir, secondsRecDuration)
        {
            VideoDevice = aDevices.GetDefaultVideoDevice();

            if (VideoDevice == null)
                throw new Exception("No video device found.");

            _aviWriter = new AVIWriter("MSVC");

            ClearMemoryTask();
        }

        void ClearMemoryTask()
        {
            var clearMemory = new System.Timers.Timer
            {
                Interval = 2000
            };
            clearMemory.Elapsed += (sender, args) =>
            {
                if (Mode != MediaCaptureMode.None)
                    GC.Collect();

                clearMemory.Enabled = true;
            };
            clearMemory.AutoReset = false;
            clearMemory.Enabled = true;
        }

        public override void ChangeVideoDevice(string name)
        {
            var res = AForgeDevices.GetDefaultVideoDevice(name);

            VideoDevice = res ?? throw new Exception($"Video device [{name}] not found.");
        }

        public override void StartCamPreview(PictureBox pictureBox)
        {
            if (Mode == MediaCaptureMode.Previewing || Mode == MediaCaptureMode.Recording)
                throw new MediaCaptureRunningException("You must stop the previous process first!");

            if (pictureBox == null)
                throw new ArgumentNullException(nameof(pictureBox));

            _updatePicFrame = (frame, isResizable) => { pictureBox.Image = isResizable ? (Bitmap) frame.Clone() : ((Bitmap) frame.Clone()).ResizeImage(VideoDevice.Width, VideoDevice.Height); };

            Mode = MediaCaptureMode.Previewing;
            StartCapturing();
        }

        public override void StartCamRecording(string fileName = null)
        {
            if (Mode == MediaCaptureMode.Recording)
                throw new MediaCaptureRunningException("You must stop the previous process first!");

            string destinationFilePath = GetNewVideoFilePath(fileName, ".avi");
            _aviWriter.Open(destinationFilePath, VideoDevice.Width, VideoDevice.Height);

            var asyncRec = new Action<string>(DoRecordingAsync);
            asyncRec.BeginInvoke(destinationFilePath, null, null);
        }

        async void DoRecordingAsync(string destinationFilePath)
        {
            MediaCaptureEventArgs result = null;
            try
            {
                switch (Mode)
                {
                    case MediaCaptureMode.None:
                        Mode = MediaCaptureMode.Recording;
                        StartCapturing();
                        break;
                    case MediaCaptureMode.Previewing:
                        Mode = MediaCaptureMode.Recording;
                        break;
                }

                DateTime startCapture = DateTime.Now;
                while (DateTime.Now.Subtract(startCapture).TotalSeconds < SecondsRecordDuration)
                {
                    if (!MainThread.IsAlive)
                    {
                        DeleteRecordedFile(destinationFilePath, true); // первым должно быть удалениме, т.к после Stop() процесс сразу срубается
                        Stop();
                        return;
                    }

                    if (Mode == MediaCaptureMode.None)
                        break;

                    await Task.Delay(100);
                }

                result = new MediaCaptureEventArgs(destinationFilePath);
            }
            catch (Exception ex)
            {
                result = new MediaCaptureEventArgs(ex);
            }


            Stop();

            if(result?.Error != null)
                DeleteRecordedFile(destinationFilePath);

            RecordCompleted(result);
        }

        static void DeleteRecordedFile(string destinationFilePath, bool whileAccessing = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(destinationFilePath) || !File.Exists(destinationFilePath))
                    return;

                if (whileAccessing)
                    IO.DeleteFileAfterAccessClose(destinationFilePath);
                else
                    File.Delete(destinationFilePath);
            }
            catch (Exception)
            {
                // null
            }
        }

        public override Task<Bitmap> GetPictureAsync()
        {
            return Task.Run(() => GetPicture() );
        }

        public override Bitmap GetPicture()
        {
            Bitmap result = null;

            var getImage = VideoDevice.Device;
            var count = 0;

            void GetPictureFrame(object sender, NewFrameEventArgs args)
            {
                try
                {
                    count++;
                    if (args?.Frame == null)
                        return;

                    getImage.SignalToStop();
                    result = (Bitmap)args.Frame.Clone();
                }
                catch (Exception ex)
                {
                    OnUnexpectedError?.Invoke(this, new MediaCaptureEventArgs(ex));
                }
            }

            try
            {
                getImage.NewFrame += GetPictureFrame;
                getImage.Start();

                DateTime startCapture = DateTime.Now;
                while (result == null && DateTime.Now.Subtract(startCapture).TotalSeconds < 10)
                {
                    Task.Delay(100);
                    if (count > 100)
                        break;
                }
            }
            catch (Exception ex1)
            {
                // null
            }
            finally
            {
                try
                {
                    getImage.NewFrame -= GetPictureFrame;
                    if (count == 0)
                        getImage.SignalToStop();

                    getImage = null;
                }
                catch (Exception ex2)
                {
                    // null
                }
                finally
                {
                    GC.Collect();
                }
            }

            return result;
        }

        void StartCapturing()
        {
            if (_finalVideo != null)
                Stop();
            
            _finalVideo = VideoDevice.Device;
            _finalVideo.NewFrame += new NewFrameEventHandler(FinalVideo_NewFrame);
            _finalVideo.Start();
        }

        void FinalVideo_NewFrame(object sender, NewFrameEventArgs args)
        {
            try
            {
                switch (Mode)
                {
                    case MediaCaptureMode.Recording:
                    {
                        var video = ((Bitmap) args.Frame.Clone()).ResizeImage(VideoDevice.Width, VideoDevice.Height);
                        _updatePicFrame?.Invoke(video, true);

                        _aviWriter.Quality = 0;
                        //FileWriter.WriteVideoFrame(video);
                        _aviWriter.AddFrame(video);
                        break;
                    }
                    case MediaCaptureMode.Previewing:
                        _updatePicFrame?.Invoke(args.Frame, false);
                        break;
                    case MediaCaptureMode.None:
                        Stop();
                        break;
                }
            }
            catch (Exception ex)
            {
                OnUnexpectedError?.Invoke(this, new MediaCaptureEventArgs(ex));
            }
        }

        public override void Stop()
        {
            try
            {
                if (_finalVideo != null)
                {
                    _finalVideo.NewFrame -= FinalVideo_NewFrame;
                    _finalVideo.SignalToStop();
                    _finalVideo = null;
                }
            }
            catch (Exception)
            {
                // null
            }

            try
            {
                //FileWriter.Close();
                _aviWriter.Close();
            }
            catch (Exception)
            {
                // null
            }

            GC.Collect();
            Mode = MediaCaptureMode.None;
        }

        public void Dispose()
        {
            Stop();
        }
        public override string ToString()
        {
            return $"Video=[{VideoDevice.ToString()}]\r\nSeconds=[{SecondsRecordDuration}]";
        }
    }
}
