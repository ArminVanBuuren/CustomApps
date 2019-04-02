using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Expression.Encoder;
using Microsoft.Expression.Encoder.Devices;
using Microsoft.Expression.Encoder.Live;
using Microsoft.Expression.Encoder.ScreenCapture;

namespace Utils.WinForm.CamCapture
{
    public class CamCapture : IDisposable
    {
        private LiveJob _job;
        private ScreenCaptureJob _jobScreen;
        private LiveDeviceSource _deviceSource;
        private readonly Dictionary<string, EncoderDevice> _videoDevices;
        private readonly Dictionary<string, EncoderDevice> _audioDevices;

        private System.Threading.Thread _asyncThread;
        private CoverRecord _cover;

        public event CamCaptureEventHandler OnRecordingCompleted;
        public List<string> VideoDevices { get; } = new List<string>();
        public List<string> AudioDevices { get; } = new List<string>();

        public Thread MainThread { get; }

        CamCaptureMode _mode = CamCaptureMode.None;
        public CamCaptureMode Mode
        {
            get => _mode;
            private set
            {
                _mode = value;

                if (_mode == CamCaptureMode.None)
                    ProcessingStarted = null;
                else
                    ProcessingStarted = DateTime.Now;
            }
        }

        public bool IsWroking { get; private set; } = false;
        public DateTime? ProcessingStarted { get; private set; }

        /// <summary>
        /// Невозможно развернуть приложение, использующее EE4 SDK, без установки всего приложения на целевой машине. Даже если вы попытаетесь "скопировать локальные" DLL файлы в ваше местоположение приложения, для этого необходимо установить 25-мегабайтное EE4-приложение. Поэтому перед использованием сначала установите Microsoft Expression Encoder 4 (Encoder_en.exe)
        /// </summary>
        public CamCapture()
        {
            MainThread = Thread.CurrentThread;

            _videoDevices = new Dictionary<string, EncoderDevice>();
            _audioDevices = new Dictionary<string, EncoderDevice>();

            foreach (EncoderDevice edv in EncoderDevices.FindDevices(EncoderDeviceType.Video))
            {
                _videoDevices.Add(edv.Name, edv);
                VideoDevices.Add(edv.Name);
            }

            foreach (EncoderDevice eda in EncoderDevices.FindDevices(EncoderDeviceType.Audio))
            {
                _audioDevices.Add(eda.Name, eda);
                AudioDevices.Add(eda.Name);
            }

            TimeoutMonitoring();
        }

        
        void TimeoutMonitoring()
        {
            System.Timers.Timer timeoutInitProcess = new System.Timers.Timer
            {
                Interval = 5000
            };
            timeoutInitProcess.Elapsed += (sender, args) =>
            {
                if (ProcessingStarted != null && _asyncThread != null && _asyncThread.IsAlive && !IsWroking)
                {
                    var timeInit = DateTime.Now.Subtract(ProcessingStarted.Value);
                    // если процесс запущен и инициализация висит больше 60 секунд. Потому что бывает процесс висисит на методе AddDeviceSource или PickBestVideoFormat
                    if (timeInit.TotalSeconds > 20)
                    {
                        try
                        {
                            if (_asyncThread != null && _asyncThread.IsAlive)
                                _asyncThread.Abort();

                            Terminate();

                            if (_asyncThread != null && _asyncThread.IsAlive)
                                _asyncThread.Abort();

                            OnRecordingCompleted?.BeginInvoke(this, new CamCaptureEventArgs(new Exception("Hanging up. Timeout initialization.")), null, null);
                        }
                        catch (Exception)
                        {
                            // null;
                        }
                        finally
                        {
                            Mode = CamCaptureMode.None;
                        }
                    }
                }

                timeoutInitProcess.Enabled = true;
            };
            timeoutInitProcess.AutoReset = false;
            timeoutInitProcess.Enabled = true;
        }

        class CoverRecord
        {
            public CoverRecord(string destinationFile, int timeRec, EncoderDevice videoDevice, EncoderDevice audioDevice)
            {
                DestinationFile = destinationFile;
                TimeRec = timeRec;
                VideoDevice = videoDevice;
                AudioDevice = audioDevice;
            }

            public string DestinationFile { get; }
            public int TimeRec { get; }
            public EncoderDevice VideoDevice { get; }
            public EncoderDevice AudioDevice { get; }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="destinationFile"></param>
        /// <param name="timeRecSec"></param>
        /// <param name="videoDevice"></param>
        /// <param name="audioDevice"></param>
        /// <returns></returns>
        public void StartRecording(string destinationFile, int timeRecSec = 60, string videoDevice = null, string audioDevice = null)
        {
            if (Mode != CamCaptureMode.None || (_asyncThread != null && _asyncThread.IsAlive))
                throw new CamCaptureRunningException("You must stop the previous process first!");

            if (OnRecordingCompleted == null)
                throw new Exception("You must initialize callback event first.");

            EncoderDevice videoEnc = GetEncoderDevice(_videoDevices, videoDevice);
            EncoderDevice audioEnc = GetEncoderDevice(_audioDevices, audioDevice);
            
            if (videoEnc == null || audioEnc == null)
                throw new ArgumentException($"Video=[{videoEnc?.ToString()}] or Audio=[{audioEnc?.ToString()}] device is incorrect!");

            int timeRec = timeRecSec;
            if (timeRec > 1800)
                timeRec = 1800;

            if (File.Exists(destinationFile))
                File.Delete(destinationFile);

            var dirDest = Path.GetDirectoryName(destinationFile);
            if (!string.IsNullOrWhiteSpace(dirDest) && !Directory.Exists(dirDest))
            {
                Directory.CreateDirectory(dirDest);
            }

            Mode = CamCaptureMode.Recording;
            _cover = new CoverRecord(destinationFile, timeRec, videoEnc, audioEnc);
            _asyncThread = new Thread(new ThreadStart(DoRecordingThread));
            _asyncThread.IsBackground = true; // обязательно true!! а то при завершении основной программы поток будет продолжать работать 
            _asyncThread.Start();


            //var asyncRec = new Func<CoverRecord, Task<CamCaptureEventArgs>>(DoRecordingAsync);
            //asyncRec.BeginInvoke(new CoverRecord(destinationFile, timeRec, videoEnc, audioEnc), DoRecordingAsyncCompleted, asyncRec);
        }

        void DoRecordingThread()
        {
            CamCaptureEventArgs result = null;
            try
            {
                _job = new LiveJob();
                _deviceSource = _job.AddDeviceSource(_cover.VideoDevice, _cover.AudioDevice);

                // Setup the video resolution and frame rate of the video device
                // NOTE: Of course, the resolution and frame rate you specify must be supported by the device!
                // NOTE2: May be not all video devices support this call, and so it just doesn't work, as if you don't call it (no error is raised)
                // NOTE3: As a workaround, if the .PickBestVideoFormat method doesn't work, you could force the resolution in the 
                //        following instructions (called few lines belows): 'panelVideoPreview.Size=' and '_job.OutputFormat.VideoProfile.Size=' 
                //        to be the one you choosed (640, 480).
                // _deviceSource.PickBestVideoFormat(new Size(640, 480), 25);

                // Get the properties of the device video
                SourceProperties sp = _deviceSource.SourcePropertiesSnapshot();

                // Setup the output video resolution file as the preview
                _job.OutputFormat.VideoProfile.Size = new Size(sp.Size.Width, sp.Size.Height);

                _job.ActivateSource(_deviceSource);

                FileArchivePublishFormat fileOut = new FileArchivePublishFormat
                {
                    OutputFileName = _cover.DestinationFile
                };
                _job.PublishFormats.Add(fileOut);
                _job.StartEncoding();

                IsWroking = true;
                DateTime startCapture = DateTime.Now;
                while (DateTime.Now.Subtract(startCapture).TotalSeconds < _cover.TimeRec)
                {
                    if (!MainThread.IsAlive)
                    {
                        Terminate();
                        return;
                    }

                    Thread.Sleep(1000);
                }

                result = new CamCaptureEventArgs(_cover.DestinationFile);
            }
            catch (ThreadAbortException)
            {
                result = new CamCaptureEventArgs(_cover.DestinationFile);
            }
            catch (Exception ex)
            {
                result = new CamCaptureEventArgs(ex);
            }

            IsWroking = false;
            var stopException = StopJob();
            if (stopException != null)
            {
                result.Error = result.Error != null ? new Exception(result.Error.Message, stopException) : stopException;
            }

            OnRecordingCompleted?.BeginInvoke(this, result, null, null);
        }

        //void Test()
        //{
        //    var state = Thread.CurrentThread.GetApartmentState();
        //    if (state == ApartmentState.STA && !Thread.CurrentThread.IsBackground && !Thread.CurrentThread.IsThreadPoolThread && Thread.CurrentThread.IsAlive)
        //    {
        //        MethodInfo correctEntryMethod = Assembly.GetEntryAssembly().EntryPoint;
        //        StackTrace trace = new StackTrace();
        //        StackFrame[] frames = trace.GetFrames();
        //        for (int i = frames.Length - 1; i >= 0; i--)
        //        {
        //            MethodBase method = frames[i].GetMethod();
        //            if (correctEntryMethod == method)
        //            {
        //                return;
        //            }
        //        }
        //    }
        //}

        //async Task<CamCaptureEventArgs> DoRecordingAsync(CoverRecord cover)
        //{
        //    try
        //    {
        //        _job = new LiveJob();
        //        _deviceSource = _job.AddDeviceSource(cover.VideoDevice, cover.AudioDevice);

        //        // Setup the video resolution and frame rate of the video device
        //        // NOTE: Of course, the resolution and frame rate you specify must be supported by the device!
        //        // NOTE2: May be not all video devices support this call, and so it just doesn't work, as if you don't call it (no error is raised)
        //        // NOTE3: As a workaround, if the .PickBestVideoFormat method doesn't work, you could force the resolution in the 
        //        //        following instructions (called few lines belows): 'panelVideoPreview.Size=' and '_job.OutputFormat.VideoProfile.Size=' 
        //        //        to be the one you choosed (640, 480).
        //        _deviceSource.PickBestVideoFormat(new Size(640, 480), 25);

        //        // Get the properties of the device video
        //        SourceProperties sp = _deviceSource.SourcePropertiesSnapshot();

        //        // Setup the output video resolution file as the preview
        //        _job.OutputFormat.VideoProfile.Size = new Size(sp.Size.Width, sp.Size.Height);

        //        _job.ActivateSource(_deviceSource);

        //        FileArchivePublishFormat fileOut = new FileArchivePublishFormat
        //        {
        //            OutputFileName = cover.DestinationFile
        //        };
        //        _job.PublishFormats.Add(fileOut);
        //        _job.StartEncoding();

        //        await Task.Delay(cover.TimeRec * 1000);

        //        return new CamCaptureEventArgs(cover.DestinationFile);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new CamCaptureEventArgs(ex);
        //    }
        //}

        public void StartScreenRecording(string destinationFile, int timeRecSec = 60, string audioDevice = null)
        {
            if (Mode != CamCaptureMode.None || (_asyncThread != null && _asyncThread.IsAlive))
                throw new CamCaptureRunningException("You must stop the previous process first!");

            if (OnRecordingCompleted == null)
                throw new Exception("You must initialize callback event first.");

            EncoderDevice audioEnc = GetEncoderDevice(_audioDevices, audioDevice);
            if (audioEnc == null)
                throw new ArgumentException("Audio device is incorrect!");

            int timeRec = timeRecSec;
            if (timeRec > 1800)
                timeRec = 1800;

            if (File.Exists(destinationFile))
                File.Delete(destinationFile);

            Mode = CamCaptureMode.Recording;
            _cover = new CoverRecord(destinationFile, timeRec, null, audioEnc);
            _asyncThread = new Thread(new ThreadStart(DoScreenRecordingThread));
            _asyncThread.IsBackground = true; // обязательно true!! а то при завершении основной программы поток будет продолжать работать 
            _asyncThread.Start();


            //var asyncRec = new Func<CoverRecord, Task<CamCaptureEventArgs>>(DoScreenRecordingAsync);
            //asyncRec.BeginInvoke(new CoverRecord(destinationFile, timeRec, null, audioEnc), DoRecordingAsyncCompleted, asyncRec);
        }

        void DoScreenRecordingThread()
        {
            CamCaptureEventArgs result = null;
            try
            {
                string pcName = System.Environment.MachineName;
                _jobScreen = new ScreenCaptureJob();
                _jobScreen.ScreenCaptureVideoProfile.FrameRate = 5;
                _jobScreen.AddAudioDeviceSource(_cover.AudioDevice);
                _jobScreen.ScreenCaptureAudioProfile.Channels = 1;
                _jobScreen.ScreenCaptureAudioProfile.SamplesPerSecond = 32000;
                _jobScreen.ScreenCaptureAudioProfile.BitsPerSample = 16;
                _jobScreen.ScreenCaptureAudioProfile.Bitrate = new Microsoft.Expression.Encoder.Profiles.ConstantBitrate(20);

                //Rectangle capRect = new Rectangle(388, 222, 1056, 608);
                Rectangle capRect = new Rectangle(10, 10, 640, 480);
                _jobScreen.CaptureRectangle = capRect;

                _jobScreen.OutputScreenCaptureFileName = _cover.DestinationFile;
                _jobScreen.Start();

                IsWroking = true;
                Thread.Sleep(_cover.TimeRec * 1000);

                result = new CamCaptureEventArgs(_cover.DestinationFile);
            }
            catch (ThreadAbortException)
            {
                result = new CamCaptureEventArgs(_cover.DestinationFile);
            }
            catch (Exception ex)
            {
                result = new CamCaptureEventArgs(ex);
            }
            finally
            {
                IsWroking = false;
                var ex = StopJob();
                if (ex != null && result != null)
                {
                    result.Error = result.Error != null ? new Exception(result.Error.Message, ex) : ex;
                }

                OnRecordingCompleted?.BeginInvoke(this, result, null, null);
            }
        }

        //async Task<CamCaptureEventArgs> DoScreenRecordingAsync(CoverRecord cover)
        //{
        //    try
        //    {
        //        string pcName = System.Environment.MachineName;
        //        _jobScreen = new ScreenCaptureJob();
        //        _jobScreen.ScreenCaptureVideoProfile.FrameRate = 5;
        //        _jobScreen.AddAudioDeviceSource(cover.AudioDevice);
        //        _jobScreen.ScreenCaptureAudioProfile.Channels = 1;
        //        _jobScreen.ScreenCaptureAudioProfile.SamplesPerSecond = 32000;
        //        _jobScreen.ScreenCaptureAudioProfile.BitsPerSample = 16;
        //        _jobScreen.ScreenCaptureAudioProfile.Bitrate = new Microsoft.Expression.Encoder.Profiles.ConstantBitrate(20);

        //        //Rectangle capRect = new Rectangle(388, 222, 1056, 608);
        //        Rectangle capRect = new Rectangle(10, 10, 640, 480);
        //        _jobScreen.CaptureRectangle = capRect;

        //        _jobScreen.OutputScreenCaptureFileName = cover.DestinationFile;
        //        _jobScreen.Start();

        //        await Task.Delay(cover.TimeRec * 1000);

        //        return new CamCaptureEventArgs(cover.DestinationFile);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new CamCaptureEventArgs(ex);
        //    }
        //}

        //async void DoRecordingAsyncCompleted(IAsyncResult asyncResult)
        //{
        //    try
        //    {
        //        AsyncResult ar = asyncResult as AsyncResult;
        //        var caller = (Func<CoverRecord, Task<CamCaptureEventArgs>>)ar.AsyncDelegate;
        //        Task<CamCaptureEventArgs> taskResult = caller.EndInvoke(asyncResult);
        //        await taskResult;

        //        StopJob();

        //        OnRecordingCompleted?.Invoke(this, taskResult.Result);
        //    }
        //    catch (Exception ex)
        //    {
        //        OnRecordingCompleted?.Invoke(this, new CamCaptureEventArgs(ex));
        //    }
        //}

        //private void GrabImage(string destiationFileName)
        //{
        //    // Create a Bitmap of the same dimension of panelVideoPreview (Width x Height)
        //    using (Bitmap bitmap = new Bitmap(640, 480))
        //    {
        //        using (Graphics g = Graphics.FromImage(bitmap))
        //        {
        //            // Get the paramters to call g.CopyFromScreen and get the image
        //            Rectangle rectanglePanelVideoPreview = panelVideoPreview.Bounds;
        //            Point sourcePoints = panelVideoPreview.PointToScreen(new Point(panelVideoPreview.ClientRectangle.X, panelVideoPreview.ClientRectangle.Y));
        //            g.CopyFromScreen(sourcePoints, Point.Empty, rectanglePanelVideoPreview.Size);
        //        }

        //        bitmap.Save(destiationFileName, System.Drawing.Imaging.ImageFormat.Png);
        //    }
        //}

        private void StartBroadcast(int port = 8080, string videoDevice = null, string audioDevice = null)
        {
            // <MediaElement Name = "VideoControl" Source = "http://localhost:8080" />

            if (Mode != CamCaptureMode.None || (_asyncThread != null && _asyncThread.IsAlive))
                throw new CamCaptureRunningException("You must stop the previous process first!");

            EncoderDevice videoEnc = GetEncoderDevice(_videoDevices, videoDevice);
            EncoderDevice audioEnc = GetEncoderDevice(_audioDevices, audioDevice);

            if (videoEnc == null || audioEnc == null)
                throw new ArgumentException("Video or Audio device is incorrect!");

            _job = new LiveJob();

            _deviceSource = _job.AddDeviceSource(videoEnc, audioEnc);
            _job.ActivateSource(_deviceSource);

            // Finds and applys a smooth streaming preset        
            _job.ApplyPreset(LivePresets.VC1256kDSL16x9);

            // Creates the publishing format for the job
            PullBroadcastPublishFormat format = new PullBroadcastPublishFormat
            {
                BroadcastPort = port,
                MaximumNumberOfConnections = 2
            };

            // Adds the publishing format to the job
            _job.PublishFormats.Add(format);

            // Starts encoding
            _job.StartEncoding();

            Mode = CamCaptureMode.Broadcast;

            //toolStripStatusLabel1.Text = "Broadcast started on localhost at port 8080, run WpfShowBroadcast.exe now to see it";
        }

        public void StopAnyProcess()
        {
            if(_asyncThread != null && _asyncThread.IsAlive)
                _asyncThread.Abort();

            var ex = StopJob();

            if (ex != null)
                throw ex;
        }

        Exception StopJob()
        {
            try
            {
                _job?.StopEncoding();
                _jobScreen?.Stop();
                _job?.RemoveDeviceSource(_deviceSource);
                if (_deviceSource != null)
                {
                    _deviceSource.PreviewWindow = null;
                    _deviceSource.Dispose();
                }

                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
            finally
            {
                Mode = CamCaptureMode.None;
            }
        }

        void Terminate()
        {
            try
            {
                _job?.Dispose();
            }
            catch (Exception)
            {

            }

            try
            {
                _jobScreen?.Dispose();
            }
            catch (Exception)
            {

            }

            try
            {
                _deviceSource?.Dispose();
            }
            catch (Exception)
            {

            }
        }

        static EncoderDevice GetEncoderDevice(Dictionary<string, EncoderDevice> encoders, string encoderName)
        {
            if (string.IsNullOrEmpty(encoderName))
                return encoders.FirstOrDefault().Value;
            else if (encoders.TryGetValue(encoderName, out EncoderDevice encDev))
                return encDev;
            return null;
        }

        public void Dispose()
        {
            try
            {
                StopAnyProcess();
                Terminate();
            }
            catch (Exception)
            {
                // null;
            }
        }
    }
}