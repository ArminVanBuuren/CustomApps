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
    public class EncoderCapture : MediaCapture, IDisposable
    {
        readonly object syncProcess = new object();
        private System.Threading.Thread _asyncRecordingThread;

        private List<EncoderProcessingThread> AllProcesses { get; } = new List<EncoderProcessingThread>();

        public Thread MainThread { get; }

        public EncoderDevice VideoEncoderDevice { get; private set; }
        public EncoderDevice AudioEncoderDevice { get; private set; }
        

        /// <summary>
        /// Невозможно развернуть приложение, использующее EE4 SDK, без установки всего приложения на целевой машине. Даже если вы попытаетесь "скопировать локальные" DLL файлы в ваше местоположение приложения, для этого необходимо установить 25-мегабайтное EE4-приложение. Поэтому перед использованием сначала установите Microsoft Expression Encoder 4 (Encoder_en.exe)
        /// </summary>
        public EncoderCapture(AForgeMediaDevices aDevices, EncoderMediaDevices cDevices, string destinationDir, int durationRecSec = 60):base(aDevices, cDevices, destinationDir, durationRecSec)
        {
            MainThread = Thread.CurrentThread;

            VideoEncoderDevice = CamDevices.GetVideoDevice();
            AudioEncoderDevice = CamDevices.GetAudioDevice();
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


        //public bool IsInitiating { get; private set; } = false;
        //void TimeoutInitiatingTask()
        //{
        //    TimeoutInitiatingTask();
        //
        //    var timeoutInitProcess = new System.Timers.Timer
        //    {
        //        Interval = 5000
        //    };
        //    timeoutInitProcess.Elapsed += (sender, args) =>
        //    {
        //        if (TimeOfStart != null && _asyncRecordingThread != null && _asyncRecordingThread.IsAlive && IsInitiating)
        //        {
        //            var timeInit = DateTime.Now.Subtract(TimeOfStart.Value);

        //            // если процесс запущен и инициализация висит больше 120 секунд. Потому что бывает процесс висисит на методе AddDeviceSource или PickBestVideoFormat
        //            if (timeInit.TotalSeconds > 120)
        //            {
        //                try
        //                {
        //                    Terminate();
        //                    RecordCompleted(new MediaCaptureEventArgs(new Exception("Hanging up. Timeout initialization.")), true);
        //                }
        //                catch (Exception)
        //                {
        //                    // null;
        //                }
        //            }
        //        }

        //        timeoutInitProcess.Enabled = true;
        //    };
        //    timeoutInitProcess.AutoReset = false;
        //    timeoutInitProcess.Enabled = true;
        //}

        public override async void StartCamRecording(string fileName)
        {
            if (Mode != MediaCaptureMode.None || (_asyncRecordingThread != null && _asyncRecordingThread.IsAlive))
                throw new MediaCaptureRunningException("You must stop the previous process first!");

            if (VideoEncoderDevice == null || AudioEncoderDevice == null)
                throw new ArgumentException($"Video=[{VideoEncoderDevice?.ToString()}] or Audio=[{AudioEncoderDevice?.ToString()}] device is incorrect!");


            EncoderProcessingThread procThread = new EncoderProcessingThread(GetNewVideoFilePath(fileName));
            if (await Initialization(procThread, DoCamInitialize) != null)
            {
                Recording(procThread);
            }
        }

        public override async void StartScreenRecording(string fileName)
        {
            if (Mode != MediaCaptureMode.None || (_asyncRecordingThread != null && _asyncRecordingThread.IsAlive))
                throw new MediaCaptureRunningException("You must stop the previous process first!");

            if (AudioEncoderDevice == null)
                throw new ArgumentException("Audio device is incorrect!");

            EncoderProcessingThread procThread = new EncoderProcessingThread(GetNewVideoFilePath(fileName));
            if (await Initialization(procThread, DoScreenInitialize) != null)
            {
                Recording(procThread);
            }
        }

        public override async Task<bool> StartBroadcast(int port = 8080)
        {
            // <MediaElement Name = "VideoControl" Source = "http://localhost:8080" />

            if (Mode != MediaCaptureMode.None || (_asyncRecordingThread != null && _asyncRecordingThread.IsAlive))
                throw new MediaCaptureRunningException("You must stop the previous process first!");

            if (VideoEncoderDevice == null || AudioEncoderDevice == null)
                throw new ArgumentException("Video or Audio device is incorrect!");

            EncoderProcessingThread procThread = new EncoderProcessingThread(port);
            return await Initialization(procThread, DoBroadcastInitialize) != null;
        }

        async Task<EncoderProcessingThread> Initialization(EncoderProcessingThread procThread, Func<EncoderProcessingThread, Task<EncoderProcessingThread>> initializeMethod)
        {
            Mode = MediaCaptureMode.Initialization;

            lock (syncProcess)
            {
                AllProcesses.Add(procThread);
            }

            if (await ASYNC.ExecuteWithTimeoutAsync(initializeMethod.Invoke(procThread), 60000) == null)
            {
                await ASYNC.ExecuteWithTimeoutAsync(procThread.Terminate(), 5000);
                Mode = MediaCaptureMode.None;
                throw new DeviceInitializationTimeoutException("Initialization timeout");
            }

            // если во время инициализации была команда отмены
            if (Mode == MediaCaptureMode.None)
            {
                await ASYNC.ExecuteWithTimeoutAsync(procThread.Stop(), 5000);
                await ASYNC.ExecuteWithTimeoutAsync(procThread.Terminate(), 5000);
                return null;
            }

            return procThread;
        }

        Task<EncoderProcessingThread> DoCamInitialize(EncoderProcessingThread procThread)
        {
            return Task.Run(() =>
            {
                try
                {
                    procThread.ThreadProc = Thread.CurrentThread;
                    procThread.Job = new LiveJob();
                    var deviceSource = procThread.Job.AddDeviceSource(VideoEncoderDevice, AudioEncoderDevice);
                    procThread.Device = deviceSource;

                    if (procThread.IsCanceled) return null;

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

                    procThread.Job.OutputFormat.VideoProfile.Size = findedDevice == null ? new Size(defaultSize.Width, defaultSize.Height) : new Size(findedDevice.Width, findedDevice.Height);
                    procThread.Job.ActivateSource(deviceSource);

                    if (procThread.IsCanceled) return null;

                    var fileOut = new FileArchivePublishFormat
                    {
                        OutputFileName = procThread.DestinationFilePath
                    };
                    procThread.Job.PublishFormats.Add(fileOut);

                    if (procThread.IsCanceled) return null;

                    procThread.Job.StartEncoding();

                    if (procThread.IsCanceled)
                    {
                        procThread.Terminate();
                        return null;
                    }

                    return procThread;
                }
                catch (Exception ex)
                {
                    RecordCompleted(new MediaCaptureEventArgs(ex), true);
                    return null;
                }
            });
        }

        Task<EncoderProcessingThread> DoScreenInitialize(EncoderProcessingThread procThread)
        {
            return Task.Run(() =>
            {
                try
                {
                    //string pcName = System.Environment.MachineName;
                    procThread.ScreenJob = new ScreenCaptureJob();
                    procThread.ScreenJob.ScreenCaptureVideoProfile.FrameRate = 5;
                    procThread.ScreenJob.AddAudioDeviceSource(AudioEncoderDevice);

                    if (procThread.IsCanceled) return null;

                    procThread.ScreenJob.ScreenCaptureAudioProfile.Channels = 1;
                    procThread.ScreenJob.ScreenCaptureAudioProfile.SamplesPerSecond = 32000;
                    procThread.ScreenJob.ScreenCaptureAudioProfile.BitsPerSample = 16;
                    procThread.ScreenJob.ScreenCaptureAudioProfile.Bitrate = new Microsoft.Expression.Encoder.Profiles.ConstantBitrate(20);

                    //Rectangle capRect = new Rectangle(388, 222, 1056, 608);
                    Rectangle capRect = new Rectangle(10, 10, 640, 480);
                    procThread.ScreenJob.CaptureRectangle = capRect;

                    procThread.ScreenJob.OutputScreenCaptureFileName = procThread.DestinationFilePath;
                    procThread.ScreenJob.Start();

                    if (procThread.IsCanceled)
                    {
                        procThread.Terminate();
                        return null;
                    }

                    return procThread;
                }
                catch (Exception ex)
                {
                    RecordCompleted(new MediaCaptureEventArgs(ex), true);
                    return null;
                }
            });
        }

        Task<EncoderProcessingThread> DoBroadcastInitialize(EncoderProcessingThread procThread)
        {
            return Task.Run(() =>
            {
                try
                {
                    procThread.Job = new LiveJob();
                    var deviceSource = procThread.Job.AddDeviceSource(VideoEncoderDevice, AudioEncoderDevice);
                    procThread.Device = deviceSource;

                    if (procThread.IsCanceled) return null;

                    procThread.Job.ActivateSource(deviceSource);

                    if (procThread.IsCanceled) return null;

                    // Finds and applys a smooth streaming preset        
                    procThread.Job.ApplyPreset(LivePresets.VC1256kDSL16x9);

                    if (procThread.IsCanceled) return null;

                    // Creates the publishing format for the job
                    PullBroadcastPublishFormat format = new PullBroadcastPublishFormat
                    {
                        BroadcastPort = procThread.BroadcastPort,
                        MaximumNumberOfConnections = 2
                    };

                    // Adds the publishing format to the job
                    procThread.Job.PublishFormats.Add(format);

                    if (procThread.IsCanceled) return null;

                    // Starts encoding
                    procThread.Job.StartEncoding();

                    if (procThread.IsCanceled)
                    {
                        procThread.Terminate();
                        return null;
                    }

                    return procThread;
                }
                catch (Exception ex)
                {
                    RecordCompleted(new MediaCaptureEventArgs(ex), true);
                    return null;
                }
            });
        }

        void Recording(EncoderProcessingThread procThread)
        {
            Mode = MediaCaptureMode.Recording;

            _asyncRecordingThread = new Thread(DoRecording)
            {
                IsBackground = true  // обязательно true!! а то при завершении основной программы поток будет продолжать работать 
            };
            _asyncRecordingThread.Start(procThread);
        }

        async void DoRecording(object procThread)
        {
            EncoderProcessingThread processingThread = null;
            MediaCaptureEventArgs result = null;

            try
            {
                processingThread = ((EncoderProcessingThread) procThread);
                if(processingThread == null)
                    throw new Exception($"{nameof(EncoderProcessingThread)} not initialized.");

                var startCapture = DateTime.Now;
                while (DateTime.Now.Subtract(startCapture).TotalSeconds < RecDurationSec)
                {
                    if (!MainThread.IsAlive)
                    {
                        await processingThread.Terminate();
                        Mode = MediaCaptureMode.None;
                        return;
                    }

                    if (Mode == MediaCaptureMode.None)
                    {
                        await processingThread.Stop();
                        await processingThread.Terminate();
                        return;
                    }

                    await Task.Delay(1000);
                }

                result = new MediaCaptureEventArgs(processingThread.DestinationFilePath);
            }
            catch (ThreadAbortException)
            {
                result = new MediaCaptureEventArgs(processingThread?.DestinationFilePath);
            }
            catch (Exception ex)
            {
                result = new MediaCaptureEventArgs(ex);
            }

            if (processingThread != null)
                await processingThread.Stop();
            Mode = MediaCaptureMode.None;
            RecordCompleted(result, true);
        }

        public override void Stop()
        {
            if (_asyncRecordingThread != null)
            {
                var isStopped = false;
                List<EncoderProcessingThread> currentRecThread = null;
                lock (syncProcess)
                {
                    currentRecThread = AllProcesses.Where(p => p.ThreadProc.ManagedThreadId == _asyncRecordingThread.ManagedThreadId).ToList();
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


                if (!isStopped)
                    return;

                _asyncRecordingThread = null;
                Mode = MediaCaptureMode.None;
            }
            else
            {
                Terminate();
            }
        }

        void Terminate()
        {
            lock (syncProcess)
            {
                foreach (var camProc in AllProcesses)
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

            _asyncRecordingThread = null;
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