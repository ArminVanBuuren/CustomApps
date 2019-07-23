using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using NAudio.Wave;

namespace Utils.Media.MediaCapture.AForge
{
    public class AForgeCapture : MediaCapture, IDisposable
    {
        private readonly object syncVideo = new object();
        private readonly object syncAudio = new object();

        Thread MainThread { get; }

        private VideoCaptureDevice _finalVideo = null;
        private readonly IFrameWriter _frameWriter;

        public WaveInEvent _waveSource = null;
        public WaveFileWriter _waveFileWriter = null;

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
            if (Mode != MediaCaptureMode.None)
                throw new MediaCaptureRunningException("You must stop the previous process first!");

            var res = AForgeDevices.GetDefaultVideoDevice(name);

            VideoDevice = res ?? throw new Exception($"Video device [{name}] not found.");

            _frameWriter.FrameRate = VideoDevice.VideoCapabilities.AverageFrameRate;
        }

        public override void StartPreview(PictureBox pictureBox)
        {
            if (Mode != MediaCaptureMode.None)
                throw new MediaCaptureRunningException("You must stop the previous process first!");

            if (pictureBox == null)
                throw new ArgumentNullException(nameof(pictureBox));

            _updatePreviewingPic = (frame, isResizable) => { pictureBox.Image = isResizable ? (Bitmap) frame.Clone() : ((Bitmap) frame.Clone()).ResizeImage(VideoDevice.Width, VideoDevice.Height); };

            Mode = MediaCaptureMode.Previewing;
            StartVideoGrab();
        }

        public override void StartRecording(string fileName = null)
        {
            if (Mode == MediaCaptureMode.Recording)
                throw new MediaCaptureRunningException("You must stop the previous process first!");

            fake_frames_Count = 0;
            var commonDestination = Path.Combine(DestinationDir, $"{DateTime.Now:ddHHmmss}_{STRING.RandomString(15)}");


            // aforge
            _frameWriter.Refresh();
            var destinationVideoPath = commonDestination + _frameWriter.VideoExtension;
            _frameWriter.Open(destinationVideoPath, VideoDevice.Width, VideoDevice.Height);


            // audio
            var destinationAudioPath = commonDestination + ".wav";
            try
            {
                _waveSource = new WaveInEvent
                {
                    WaveFormat = new WaveFormat(44100, 1)
                };
                _waveSource.DataAvailable += WaveSource_DataAvailable;
                _waveSource.RecordingStopped += WaveSource_RecordingStopped;

                
                lock (syncAudio)
                {
                    _waveFileWriter = new WaveFileWriter(destinationAudioPath, _waveSource.WaveFormat);
                }
            }
            catch (Exception)
            {
                // ignored
            }

            // timer
            var asyncRec = new Action<string, string>(DoRecordingAsync);
            asyncRec.BeginInvoke(destinationVideoPath, destinationAudioPath, null, null);
        }

        void DoRecordingAsync(string destinationVideo, string destinationAudio)
        {
            MediaCaptureEventArgs result = null;
            try
            {
                switch (Mode)
                {
                    case MediaCaptureMode.None:
                        Mode = MediaCaptureMode.Recording;
                        StartVideoGrab();
                        break;
                    case MediaCaptureMode.Previewing:
                        Mode = MediaCaptureMode.Recording;
                        break;
                }

                try
                {
                    _waveSource?.StartRecording();
                }
                catch (Exception)
                {
                    // ignored
                }

                var startCapture = DateTime.Now;
                while (DateTime.Now.Subtract(startCapture).TotalSeconds < SecondsRecordDuration)
                {
                    if (!MainThread.IsAlive)
                    {
                        // первым должно быть удалениме, т.к после Stop() процесс сразу срубается
                        DeleteRecordedFile(new [] { destinationVideo , destinationAudio }, true);
                        Stop();
                        return;
                    }

                    if (Mode == MediaCaptureMode.None)
                        break;

                    Thread.Sleep(100);
                }

                result = new MediaCaptureEventArgs(new[] { destinationVideo, destinationAudio });
            }
            catch (Exception ex)
            {
                result = new MediaCaptureEventArgs(new[] { destinationVideo, destinationAudio }, ex);
            }

            Stop();

            if(result.Error != null)
                DeleteRecordedFile(new[] { destinationVideo, destinationAudio });

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
                    OnUnexpectedError?.Invoke(this, new MediaCaptureEventArgs(null, ex));
                }
            }

            try
            {
                getImage.NewFrame += GetPictureFrame;
                getImage.Start();

                var startCapture = DateTime.Now;
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

        void StartVideoGrab()
        {
            lock (syncVideo)
            {
                _finalVideo = VideoDevice.Device;
                _finalVideo.NewFrame += FinalVideo_NewFrame;
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
                lock (syncVideo)
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
                    }
                }

                if (Mode == MediaCaptureMode.None)
                    Stop();

            }
            catch (Exception ex)
            {
                OnUnexpectedError?.Invoke(this, new MediaCaptureEventArgs(null, ex));
            }
            finally
            {
                args.Frame.Dispose();
            }
        }

        void WaveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            lock (syncAudio)
            {
                if (_waveFileWriter == null)
                    return;

                _waveFileWriter.Write(e.Buffer, 0, e.BytesRecorded);
                _waveFileWriter.Flush();
            }
        }

        public override void Stop()
        {
            lock (syncVideo)
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
                _waveSource?.StopRecording();

                lock (syncAudio)
                {
                    if (_waveFileWriter == null)
                        return;

                    _waveFileWriter.Close();
                    _waveFileWriter.Dispose();
                    _waveFileWriter = null;
                }
            }
            catch (Exception)
            {
                // ignored
            }

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

        void WaveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            try
            {
                if (_waveSource != null)
                {
                    _waveSource.DataAvailable -= WaveSource_DataAvailable;
                    _waveSource.RecordingStopped -= WaveSource_RecordingStopped;
                    _waveSource.Dispose();
                    _waveSource = null;
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void Dispose()
        {
            Stop();
        }

        public override string ToString()
        {
            return $"{base.ToString()}\r\nVideo=[{VideoDevice}]\r\n{_frameWriter}".Trim();
        }
    }
}
