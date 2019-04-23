using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Video.VFW;
using TeleSharp.TL;

namespace Utils.WinForm.MediaCapture.AForge
{
    public class AForgeCapture : MediaCapture, IDisposable
    {
        private readonly object sync = new object();

        Thread MainThread { get; }

        private VideoCaptureDevice _finalVideo = null;
        private readonly IFrameWriter _frameWriter;

        private Action<Bitmap, bool> _updatePreviewingPic;

        public event MediaCaptureEventHandler OnUnexpectedError;
        public AForgeDevice VideoDevice { get; private set; }

        /// <summary>
        /// Все видео устройства полученные через библиотеку AForge
        /// </summary>
        protected AForgeMediaDevices AForgeDevices { get; }


        public AForgeCapture(Thread mainThread, AForgeMediaDevices aDevices, string destinationDir, int secondsRecDuration = 60) : this(mainThread, aDevices, new AviFrameWriter(), destinationDir, secondsRecDuration)
        {

        }

        public AForgeCapture(Thread mainThread, AForgeMediaDevices aDevices, IFrameWriter frameWriter, string destinationDir, int secondsRecDuration = 60) : base(destinationDir, secondsRecDuration)
        {
            MainThread = mainThread;
            AForgeDevices = aDevices;
            _frameWriter = frameWriter;

            ChangeVideoDevice(null);

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

        public sealed override void ChangeVideoDevice(string name)
        {
            var res = AForgeDevices.GetDefaultVideoDevice(name);

            VideoDevice = res ?? throw new Exception($"Video device [{name}] not found.");

            _frameWriter.FrameRate = VideoDevice.VideoCapabilities.AverageFrameRate;
        }

        public override void StartPreview(PictureBox pictureBox)
        {
            if (Mode == MediaCaptureMode.Previewing || Mode == MediaCaptureMode.Recording)
                throw new MediaCaptureRunningException("You must stop the previous process first!");

            if (pictureBox == null)
                throw new ArgumentNullException(nameof(pictureBox));

            _updatePreviewingPic = (frame, isResizable) => { pictureBox.Image = isResizable ? (Bitmap) frame.Clone() : ((Bitmap) frame.Clone()).ResizeImage(VideoDevice.Width, VideoDevice.Height); };

            Mode = MediaCaptureMode.Previewing;
            StartCapturing();
        }

        public override void StartRecording(string fileName = null)
        {
            if (Mode == MediaCaptureMode.Recording)
                throw new MediaCaptureRunningException("You must stop the previous process first!");

            fake_frames_Count = 0;
            string destinationFilePath = GetNewVideoFilePath(fileName, _frameWriter.VideoExtension);
            _frameWriter.Open(destinationFilePath, VideoDevice.Width, VideoDevice.Height);

            var asyncRec = new Action<string>(DoRecordingAsync);
            asyncRec.BeginInvoke(destinationFilePath, null, null);
        }

        void DoRecordingAsync(string destinationFilePath)
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

                    Thread.Sleep(100);
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
                    args.Frame.Dispose();
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
            lock (sync)
            {
                if (_finalVideo != null)
                    Stop();

                _finalVideo = VideoDevice.Device;
                _finalVideo.NewFrame += new NewFrameEventHandler(FinalVideo_NewFrame);
                _finalVideo.Start();
            }
        }

        public int fake_frames_Count = 0;
        void FinalVideo_NewFrame(object sender, NewFrameEventArgs args)
        {
            if (args?.Frame == null)
                return;

            try
            {
                lock (sync)
                {
                    switch (Mode)
                    {
                        case MediaCaptureMode.Recording:
                        {
                            fake_frames_Count++;
                            var video = ((Bitmap) args.Frame.Clone()).ResizeImage(VideoDevice.Width, VideoDevice.Height);

                            _frameWriter.AddFrame(video);

                            if (_updatePreviewingPic != null)
                                _updatePreviewingPic.Invoke(video, true);
                            else
                                video.Dispose();

                            break;
                        }
                        case MediaCaptureMode.Previewing:
                            _updatePreviewingPic?.Invoke((Bitmap) args.Frame.Clone(), false);
                            break;
                        case MediaCaptureMode.None:
                            Stop();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                OnUnexpectedError?.Invoke(this, new MediaCaptureEventArgs(ex));
            }
            finally
            {
                args.Frame.Dispose();
            }
        }

        public override void Stop()
        {
            lock (sync)
            {
                new Action(Terminate).BeginInvoke(null, null).AsyncWaitHandle.WaitOne(20000);

                GC.Collect();
                Mode = MediaCaptureMode.None;
            }
        }

        void Terminate()
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
                _frameWriter?.Close();
            }
            catch (Exception)
            {
                // null
            }
        }

        public void Dispose()
        {
            Stop();
        }

        public override string ToString()
        {
            return $"{base.ToString()}\r\nVideo=[{VideoDevice.ToString()}]\r\nFrame=[{_frameWriter?.FrameRate}]";
        }
    }
}
