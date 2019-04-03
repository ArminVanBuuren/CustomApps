using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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

        public AForgeCapture(AForgeMediaDevices aDevices, CamMediaDevices cDevices, string destinationDir, int durationRecSec = 60) : base(aDevices, cDevices, destinationDir, durationRecSec)
        {
            VideoDevice = aDevices.GetVideoDevice().FirstOrDefault();

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
            if (Mode != MediaCaptureMode.None)
                throw new MediaCaptureRunningException("You must stop the previous process first!");

            var res = AForgeDevices.GetVideoDevice(name).FirstOrDefault();

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

        public override void StartCamRecording()
        {
            if (Mode == MediaCaptureMode.Recording)
                throw new MediaCaptureRunningException("You must stop the previous process first!");

            string destinationFilePath = GetNewVideoFilePath();
            _aviWriter.Open(destinationFilePath, VideoDevice.Width, VideoDevice.Height);

            var asyncRec = new Func<string, Task<MediaCaptureEventArgs>>(DoRecordingAsync);
            asyncRec.BeginInvoke(destinationFilePath, DoRecordingAsyncCompleted, asyncRec);
        }

        async Task<MediaCaptureEventArgs> DoRecordingAsync(string destinationFilePath)
        {
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
                while (DateTime.Now.Subtract(startCapture).TotalSeconds < RecDurationSec)
                {
                    await Task.Delay(RecDurationSec * 1000);
                }

                return new MediaCaptureEventArgs(destinationFilePath);
            }
            catch (Exception ex)
            {
                try
                {
                    File.Delete(destinationFilePath);
                }
                catch (Exception)
                {
                    // null
                }

                return new MediaCaptureEventArgs(ex);
            }
        }

        async void DoRecordingAsyncCompleted(IAsyncResult asyncResult)
        {
            try
            {
                AsyncResult ar = asyncResult as AsyncResult;
                var caller = (Func<string, int, Task<MediaCaptureEventArgs>>)ar.AsyncDelegate;
                Task<MediaCaptureEventArgs> taskResult = caller.EndInvoke(asyncResult);
                await taskResult;

                StopCapturing();

                RecordCompleted(taskResult.Result);
            }
            catch (Exception ex)
            {
                RecordCompleted(new MediaCaptureEventArgs(ex));
            }
        }

        public override async Task<Bitmap> GetPicture()
        {
            Bitmap result = null;
            var getImage = VideoDevice.Device;

            Exception catched1 = null;
            Exception catched2 = null;
            try
            {
                getImage.NewFrame += (sender, args) =>
                {
                    try
                    {
                        result = (Bitmap) args.Frame.Clone();
                        getImage.Stop();
                    }
                    catch (Exception ex)
                    {
                        OnUnexpectedError?.Invoke(this, new MediaCaptureEventArgs(ex));
                    }
                };
                getImage.Start();

                int timeOut = 0;
                while (result == null || timeOut <= 10)
                {
                    await Task.Delay(1000);
                    timeOut++;
                }
            }
            catch (Exception ex1)
            {
                catched1 = ex1;
            }
            finally
            {
                try
                {
                    //if (getImage.IsRunning)
                    getImage.Stop();
                    GC.Collect();
                }
                catch (Exception ex2)
                {
                    catched2 = ex2;
                }
            }

            if(catched1 != null || catched2 != null)
                throw new Exception($"Exception when get image. ImageStart=[{catched1?.Message}] ImageStop=[{catched2?.Message}]");

            return result;
        }

        void StartCapturing()
        {
            if (_finalVideo != null)
                Terminate();

            _finalVideo = VideoDevice.Device;
            _finalVideo.NewFrame += new NewFrameEventHandler(FinalVideo_NewFrame);
            _finalVideo.Start();
        }

        public void StopAnyProcess()
        {
            StopCapturing();
        }

        void StopCapturing()
        {
            Exception ex = Terminate();

            GC.Collect();
            Mode = MediaCaptureMode.None;

            if (ex != null)
                throw ex;
        }

        void FinalVideo_NewFrame(object sender, NewFrameEventArgs args)
        {
            try
            {
                switch (Mode)
                {
                    case MediaCaptureMode.Recording:
                    {
                        var video = ((Bitmap)args.Frame.Clone()).ResizeImage(VideoDevice.Width, VideoDevice.Height);
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
                        StopCapturing();
                        break;
                }
            }
            catch (Exception ex)
            {
                OnUnexpectedError?.Invoke(this, new MediaCaptureEventArgs(ex));
            }
        }

        Exception Terminate()
        {
            Exception catched1 = null;
            Exception catched2 = null;
            try
            {
                //if (FinalVideo != null && FinalVideo.IsRunning)
                _finalVideo?.Stop();
                _finalVideo = null;
            }
            catch (Exception ex)
            {
                catched1 = ex;
            }

            try
            {
                //FileWriter.Close();
                _aviWriter.Close();
            }
            catch (Exception ex)
            {
                catched2 = ex;
            }

            if(catched1 != null || catched2 != null)
                return new Exception($"Terminate exception. Device=[{catched1?.Message}] Writer=[{catched2?.Message}]");
            return null;
        }

        public void Dispose()
        {
            Terminate();
        }
    }
}
