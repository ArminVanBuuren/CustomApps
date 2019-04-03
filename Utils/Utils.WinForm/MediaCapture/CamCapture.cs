using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Expression.Encoder;
using Microsoft.Expression.Encoder.Devices;
using Microsoft.Expression.Encoder.Live;
using Microsoft.Expression.Encoder.ScreenCapture;

namespace Utils.WinForm.MediaCapture
{
    public class CamCapture : MediaCapture, IDisposable
    {
        readonly object sync = new object();
        List<CamCaptureProcessThread> _allProcesses = new List<CamCaptureProcessThread>();
        private System.Threading.Thread _asyncRecordingThread;
        

        public Thread MainThread { get; }
        public bool IsInitiating { get; private set; } = false;


        public EncoderDevice VideoEncoderDevice { get; private set; }
        public EncoderDevice AudioEncoderDevice { get; private set; }
        

        /// <summary>
        /// Невозможно развернуть приложение, использующее EE4 SDK, без установки всего приложения на целевой машине. Даже если вы попытаетесь "скопировать локальные" DLL файлы в ваше местоположение приложения, для этого необходимо установить 25-мегабайтное EE4-приложение. Поэтому перед использованием сначала установите Microsoft Expression Encoder 4 (Encoder_en.exe)
        /// </summary>
        public CamCapture(AForgeMediaDevices aDevices, CamMediaDevices cDevices, string destinationDir, int durationRecSec = 60):base(aDevices, cDevices, destinationDir, durationRecSec)
        {
            MainThread = Thread.CurrentThread;

            VideoEncoderDevice = CamDevices.GetVideoDevice();
            AudioEncoderDevice = CamDevices.GetAudioDevice();

            TimeoutMonitoringTask();
        }

        public override void ChangeVideoDevice(string name)
        {
            if(name.IsNullOrEmptyTrim())
                throw new ArgumentNullException();

            var res = CamDevices.GetVideoDevice(name);

            VideoEncoderDevice = res ?? throw new Exception($"Video device [{name}] not found.");
        }

        public void ChangeAudioDevice(string name)
        {
            if (name.IsNullOrEmptyTrim())
                throw new ArgumentNullException();

            var res = CamDevices.GetAudioDevice(name);

            AudioEncoderDevice = res ?? throw new Exception($"Audio device [{name}] not found.");
        }


        void TimeoutMonitoringTask()
        {
            System.Timers.Timer timeoutInitProcess = new System.Timers.Timer
            {
                Interval = 5000
            };
            timeoutInitProcess.Elapsed += (sender, args) =>
            {
                if (TimeOfStart != null && _asyncRecordingThread != null && _asyncRecordingThread.IsAlive && IsInitiating)
                {
                    var timeInit = DateTime.Now.Subtract(TimeOfStart.Value);

                    // если процесс запущен и инициализация висит больше 60 секунд. Потому что бывает процесс висисит на методе AddDeviceSource или PickBestVideoFormat
                    if (timeInit.TotalSeconds > 20)
                    {
                        try
                        {
                            Terminate();
                            RecordCompleted(new MediaCaptureEventArgs(new Exception("Hanging up. Timeout initialization.")), true);
                        }
                        catch (Exception)
                        {
                            // null;
                        }
                    }
                }

                timeoutInitProcess.Enabled = true;
            };
            timeoutInitProcess.AutoReset = false;
            timeoutInitProcess.Enabled = true;
        }

        public override void StartCamRecording()
        {
            if (Mode != MediaCaptureMode.None || (_asyncRecordingThread != null && _asyncRecordingThread.IsAlive))
                throw new MediaCaptureRunningException("You must stop the previous process first!");

            if (VideoEncoderDevice == null || AudioEncoderDevice == null)
                throw new ArgumentException($"Video=[{VideoEncoderDevice?.ToString()}] or Audio=[{AudioEncoderDevice?.ToString()}] device is incorrect!");

            
            Mode = MediaCaptureMode.Recording;
            _asyncRecordingThread = new Thread(new ThreadStart(DoRecordingThread));
            _asyncRecordingThread.IsBackground = true; // обязательно true!! а то при завершении основной программы поток будет продолжать работать 
            _asyncRecordingThread.Start();


            //var asyncRec = new Func<CoverRecord, Task<CamCaptureEventArgs>>(DoRecordingAsync);
            //asyncRec.BeginInvoke(new CoverRecord(destinationFile, timeRec, videoEnc, audioEnc), DoRecordingAsyncCompleted, asyncRec);
        }

        void DoRecordingThread()
        {
            string destinationFilePath = GetNewVideoFilePath();
            MediaCaptureEventArgs result = null;
            CamCaptureProcessThread procThread = null;

            try
            {
                try
                {
                    IsInitiating = true;

                    var job = new LiveJob();
                    procThread = new CamCaptureProcessThread(Thread.CurrentThread, job);
                    lock (sync)
                    {
                        _allProcesses.Add(procThread);
                    }

                    var deviceSource = job.AddDeviceSource(VideoEncoderDevice, AudioEncoderDevice);
                    procThread.Device = deviceSource;

                    if (procThread.IfItAbortedThenEndProcess(_asyncRecordingThread)) return;

                    // Setup the video resolution and frame rate of the video device
                    // NOTE: Of course, the resolution and frame rate you specify must be supported by the device!
                    // NOTE2: May be not all video devices support this call, and so it just doesn't work, as if you don't call it (no error is raised)
                    // NOTE3: As a workaround, if the .PickBestVideoFormat method doesn't work, you could force the resolution in the 
                    //        following instructions (called few lines belows): 'panelVideoPreview.Size=' and '_job.OutputFormat.VideoProfile.Size=' 
                    //        to be the one you choosed (640, 480).
                    // _deviceSource.PickBestVideoFormat(new Size(640, 480), 25);

                    // Get the properties of the device video
                    //SourceProperties sp = _deviceSource.SourcePropertiesSnapshot();

                    var defaultSize = new Size(640, 480);
                    var aforgeSearch = AForgeDevices.GetVideoDevice(VideoEncoderDevice.Name);
                    var findedDevice = aforgeSearch.FirstOrDefault();

                    job.OutputFormat.VideoProfile.Size = findedDevice == null ? new Size(defaultSize.Width, defaultSize.Height) : new Size(findedDevice.Width, findedDevice.Height);
                    if (procThread.IfItAbortedThenEndProcess(_asyncRecordingThread)) return;

                    job.ActivateSource(deviceSource);
                    if (procThread.IfItAbortedThenEndProcess(_asyncRecordingThread)) return;

                    var fileOut = new FileArchivePublishFormat
                    {
                        OutputFileName = destinationFilePath
                    };
                    job.PublishFormats.Add(fileOut);
                    if (procThread.IfItAbortedThenEndProcess(_asyncRecordingThread)) return;
                    job.StartEncoding();
                    if (procThread.IfItAbortedThenEndProcess(_asyncRecordingThread)) return;
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (_asyncRecordingThread != null && _asyncRecordingThread == Thread.CurrentThread)
                        IsInitiating = false;
                }

                DateTime startCapture = DateTime.Now;
                while (DateTime.Now.Subtract(startCapture).TotalSeconds < RecDurationSec)
                {
                    if (!MainThread.IsAlive)
                    {
                        procThread.Terminate();
                        return;
                    }

                    Thread.Sleep(1000);
                }

                result = new MediaCaptureEventArgs(destinationFilePath);
            }
            catch (ThreadAbortException)
            {
                result = new MediaCaptureEventArgs(destinationFilePath);
            }
            catch (Exception ex)
            {
                result = new MediaCaptureEventArgs(ex);
            }

            procThread?.Stop();
            Mode = MediaCaptureMode.None;
            RecordCompleted(result, true);
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

        public override void StartScreenRecording()
        {
            if (Mode != MediaCaptureMode.None || (_asyncRecordingThread != null && _asyncRecordingThread.IsAlive))
                throw new MediaCaptureRunningException("You must stop the previous process first!");

            if (AudioEncoderDevice == null)
                throw new ArgumentException("Audio device is incorrect!");


            Mode = MediaCaptureMode.Recording;
            _asyncRecordingThread = new Thread(new ThreadStart(DoScreenRecordingThread));
            _asyncRecordingThread.IsBackground = true; // обязательно true!! а то при завершении основной программы поток будет продолжать работать 
            _asyncRecordingThread.Start();
        }

        void DoScreenRecordingThread()
        {
            string destinationFilePath = GetNewVideoFilePath();
            MediaCaptureEventArgs result = null;
            CamCaptureProcessThread procThread = null;

            try
            {
                try
                {
                    IsInitiating = true;

                    //string pcName = System.Environment.MachineName;
                    var jobScreen = new ScreenCaptureJob();
                    procThread = new CamCaptureProcessThread(Thread.CurrentThread, jobScreen);
                    lock (sync)
                    {
                        _allProcesses.Add(procThread);
                    }


                    jobScreen.ScreenCaptureVideoProfile.FrameRate = 5;
                    jobScreen.AddAudioDeviceSource(AudioEncoderDevice);
                    if (procThread.IfItAbortedThenEndProcess(_asyncRecordingThread)) return;
                    jobScreen.ScreenCaptureAudioProfile.Channels = 1;
                    jobScreen.ScreenCaptureAudioProfile.SamplesPerSecond = 32000;
                    jobScreen.ScreenCaptureAudioProfile.BitsPerSample = 16;
                    jobScreen.ScreenCaptureAudioProfile.Bitrate = new Microsoft.Expression.Encoder.Profiles.ConstantBitrate(20);

                    //Rectangle capRect = new Rectangle(388, 222, 1056, 608);
                    Rectangle capRect = new Rectangle(10, 10, 640, 480);
                    jobScreen.CaptureRectangle = capRect;

                    jobScreen.OutputScreenCaptureFileName = destinationFilePath;
                    jobScreen.Start();
                    if (procThread.IfItAbortedThenEndProcess(_asyncRecordingThread)) return;
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (_asyncRecordingThread != null && _asyncRecordingThread == Thread.CurrentThread)
                        IsInitiating = false;
                }

                DateTime startCapture = DateTime.Now;
                while (DateTime.Now.Subtract(startCapture).TotalSeconds < RecDurationSec)
                {
                    if (!MainThread.IsAlive)
                    {
                        procThread.Terminate();
                        return;
                    }

                    Thread.Sleep(1000);
                }

                result = new MediaCaptureEventArgs(destinationFilePath);
            }
            catch (ThreadAbortException)
            {
                result = new MediaCaptureEventArgs(destinationFilePath);
            }
            catch (Exception ex1)
            {
                result = new MediaCaptureEventArgs(ex1);
            }

            
            procThread?.Stop();
            Mode = MediaCaptureMode.None;
            RecordCompleted(result, true);
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

        public override void StartBroadcast(int port = 8080)
        {
            // <MediaElement Name = "VideoControl" Source = "http://localhost:8080" />

            if (Mode != MediaCaptureMode.None || (_asyncRecordingThread != null && _asyncRecordingThread.IsAlive))
                throw new MediaCaptureRunningException("You must stop the previous process first!");

            if (VideoEncoderDevice == null || AudioEncoderDevice == null)
                throw new ArgumentException("Video or Audio device is incorrect!");

            Mode = MediaCaptureMode.Broadcast;
            _asyncRecordingThread = new Thread(new ParameterizedThreadStart(DoBroadcastThread));
            _asyncRecordingThread.IsBackground = true; // обязательно true!! а то при завершении основной программы поток будет продолжать работать 
            _asyncRecordingThread.Start(port);
        }

        void DoBroadcastThread(object port)
        {
            MediaCaptureEventArgs result = null;

            try
            {
                try
                {
                    IsInitiating = true;
                    var job = new LiveJob();
                    var procThread = new CamCaptureProcessThread(Thread.CurrentThread, job);
                    lock (sync)
                    {
                        _allProcesses.Add(procThread);
                    }

                    var deviceSource = job.AddDeviceSource(VideoEncoderDevice, AudioEncoderDevice);
                    procThread.Device = deviceSource;
                    if (procThread.IfItAbortedThenEndProcess(_asyncRecordingThread)) return;

                    job.ActivateSource(deviceSource);
                    if (procThread.IfItAbortedThenEndProcess(_asyncRecordingThread)) return;

                    // Finds and applys a smooth streaming preset        
                    job.ApplyPreset(LivePresets.VC1256kDSL16x9);
                    if (procThread.IfItAbortedThenEndProcess(_asyncRecordingThread)) return;

                    // Creates the publishing format for the job
                    PullBroadcastPublishFormat format = new PullBroadcastPublishFormat
                    {
                        BroadcastPort = (int) port,
                        MaximumNumberOfConnections = 2
                    };

                    // Adds the publishing format to the job
                    job.PublishFormats.Add(format);
                    if (procThread.IfItAbortedThenEndProcess(_asyncRecordingThread)) return;

                    // Starts encoding
                    job.StartEncoding();
                    if (procThread.IfItAbortedThenEndProcess(_asyncRecordingThread)) return;
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (_asyncRecordingThread != null && _asyncRecordingThread == Thread.CurrentThread)
                        IsInitiating = false;
                }

                result = new MediaCaptureEventArgs($"Broadcast started on {HOST.GetLocalIPAddress()} at port {port}");
            }
            catch (Exception ex1)
            {
                result = new MediaCaptureEventArgs(ex1);
            }

            RecordCompleted(result, true);
        }

        public override void Stop()
        {
            if (_asyncRecordingThread != null)
            {
                var isStopped = false;
                List<CamCaptureProcessThread> currentRecThread = null;
                lock (sync)
                {
                    currentRecThread = _allProcesses.Where(p => p.ThreadProc.ManagedThreadId == _asyncRecordingThread.ManagedThreadId).ToList();
                }


                foreach (var camProc in currentRecThread)
                {
                    try
                    {
                        camProc.Stop();
                        if (camProc.ThreadProc.IsAlive)
                            camProc.ThreadProc.Abort();

                        isStopped = true;
                    }
                    catch (Exception)
                    {
                        //null;
                    }
                }


                if (isStopped)
                    _asyncRecordingThread = null;

                IsInitiating = false;
                Mode = MediaCaptureMode.None;
            }
            else
            {
                Terminate();
            }
        }

        void Terminate()
        {
            lock (sync)
            {
                foreach (var camProc in _allProcesses)
                {
                    try
                    {
                        if (!camProc.ThreadProc.IsAlive)
                            continue;

                        camProc.Terminate();
                        camProc.ThreadProc.Abort();
                    }
                    catch (Exception)
                    {
                        // null
                    }
                }
            }

            IsInitiating = false;
            Mode = MediaCaptureMode.None;
        }

        public void Dispose()
        {
            try
            {
                Terminate();
            }
            catch (Exception)
            {
                // null;
            }
        }

    }
}