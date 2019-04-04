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
        private const int TIMEOUT_INITIALIZE = 60000;
        private const int TIMEOUT_STOP = 5000;
        //private const int TIMEOUT_TERMINATE = 5000;

        private Thread _asyncRecordingThread;
        readonly object syncPT = new object();
        private List<EncoderProcessingThread> ProcessingThreads { get; } = new List<EncoderProcessingThread>();

        public EncoderDevice VideoEncoderDevice { get; private set; }
        public EncoderDevice AudioEncoderDevice { get; private set; }
        

        /// <summary>
        /// Невозможно развернуть приложение, использующее EE4 SDK, без установки всего приложения на целевой машине. Даже если вы попытаетесь "скопировать локальные" DLL файлы в ваше местоположение приложения, для этого необходимо установить 25-мегабайтное EE4-приложение. Поэтому перед использованием сначала установите Microsoft Expression Encoder 4 (Encoder_en.exe)
        /// </summary>
        public EncoderCapture(AForgeMediaDevices aDevices, EncoderMediaDevices cDevices, string destinationDir, int durationRecSec = 60):base(aDevices, cDevices, destinationDir, durationRecSec)
        {
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

        public override async void StartCamRecording(string fileName = null)
        {
            if (Mode != MediaCaptureMode.None || (_asyncRecordingThread != null && _asyncRecordingThread.IsAlive))
                throw new MediaCaptureRunningException("You must stop the previous process first!");

            if (VideoEncoderDevice == null || AudioEncoderDevice == null)
                throw new ArgumentException($"Video=[{VideoEncoderDevice?.ToString()}] or Audio=[{AudioEncoderDevice?.ToString()}] device is incorrect!");


            EncoderProcessingThread procThread = new EncoderProcessingThread(GetNewVideoFilePath(fileName));
            if (await Initialization(procThread, DoCamInitializeAsync))
            {
                Recording(procThread);
            }
        }

        public override async void StartScreenRecording(string fileName = null)
        {
            if (Mode != MediaCaptureMode.None || (_asyncRecordingThread != null && _asyncRecordingThread.IsAlive))
                throw new MediaCaptureRunningException("You must stop the previous process first!");

            if (AudioEncoderDevice == null)
                throw new ArgumentException("Audio device is incorrect!");

            EncoderProcessingThread procThread = new EncoderProcessingThread(GetNewVideoFilePath(fileName));
            if (await Initialization(procThread, DoScreenInitializeAsync))
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
            return await Initialization(procThread, DoBroadcastInitializeAsync);
        }

        async Task<bool> Initialization(EncoderProcessingThread procThread, Func<EncoderProcessingThread, Task<bool>> initializeMethod)
        {
            Mode = MediaCaptureMode.Initialization;

            lock (syncPT)
            {
                ProcessingThreads.Add(procThread);
            }

            if (!await ASYNC.ExecuteWithTimeoutAsync(initializeMethod.Invoke(procThread), TIMEOUT_INITIALIZE))
            {
                procThread.IsCanceled = true;
                await ASYNC.ExecuteWithTimeoutAsync(procThread.Stop(), TIMEOUT_STOP);
                Mode = MediaCaptureMode.None;
                throw new DeviceInitializationTimeoutException("Initialization timeout");
            }

            // если во время инициализации была произведена команда отмены
            if (Mode == MediaCaptureMode.None)
            {
                await ASYNC.ExecuteWithTimeoutAsync(procThread.Stop(), TIMEOUT_STOP);
                return false;
            }

            return true;
        }

        Task<bool> DoCamInitializeAsync(EncoderProcessingThread procThread)
        {
            return Task.Run(() =>
            {
                try
                {
                    procThread.Job = new LiveJob();
                    procThread.Device = procThread.Job.AddDeviceSource(VideoEncoderDevice, AudioEncoderDevice);

                    if (procThread.IsCanceled) return false;

                    // Setup the video resolution and frame rate of the video device
                    // NOTE: Of course, the resolution and frame rate you specify must be supported by the device!
                    // NOTE2: May be not all video devices support this call, and so it just doesn't work, as if you don't call it (no error is raised)
                    // NOTE3: As a workaround, if the .PickBestVideoFormat method doesn't work, you could force the resolution in the 
                    //        following instructions (called few lines belows): 'panelVideoPreview.Size=' and '_job.OutputFormat.VideoProfile.Size=' 
                    //        to be the one you choosed (640, 480).
                    // _deviceSource.PickBestVideoFormat(new Size(640, 480), 25);

                    // Get the properties of the device video
                    // SourceProperties sp = _deviceSource.SourcePropertiesSnapshot();

                    var defaultSize = new Size(640, 480);
                    var aforgeSearch = AForgeDevices.GetVideoDevice(VideoEncoderDevice.Name);
                    var findedDevice = aforgeSearch.FirstOrDefault();

                    procThread.Job.OutputFormat.VideoProfile.Size = findedDevice == null ? new Size(defaultSize.Width, defaultSize.Height) : new Size(findedDevice.Width, findedDevice.Height);
                    procThread.Job.ActivateSource(procThread.Device);

                    if (procThread.IsCanceled) return false;

                    var fileOut = new FileArchivePublishFormat
                    {
                        OutputFileName = procThread.DestinationFilePath
                    };
                    procThread.Job.PublishFormats.Add(fileOut);

                    if (procThread.IsCanceled) return false;

                    procThread.Job.StartEncoding();

                    if (procThread.IsCanceled)
                    {
                        procThread.Stop();
                        return false;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    RecordCompleted(new MediaCaptureEventArgs(ex), true);
                    return false;
                }
            });
        }

        Task<bool> DoScreenInitializeAsync(EncoderProcessingThread procThread)
        {
            return Task.Run(() =>
            {
                try
                {
                    //string pcName = System.Environment.MachineName;
                    procThread.ScreenJob = new ScreenCaptureJob();
                    procThread.ScreenJob.ScreenCaptureVideoProfile.FrameRate = 5;
                    procThread.ScreenJob.AddAudioDeviceSource(AudioEncoderDevice);

                    if (procThread.IsCanceled) return false;

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
                        procThread.Stop();
                        return false;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    RecordCompleted(new MediaCaptureEventArgs(ex), true);
                    return false;
                }
            });
        }

        Task<bool> DoBroadcastInitializeAsync(EncoderProcessingThread procThread)
        {
            return Task.Run(() =>
            {
                try
                {
                    procThread.Job = new LiveJob();
                    procThread.Device = procThread.Job.AddDeviceSource(VideoEncoderDevice, AudioEncoderDevice);

                    if (procThread.IsCanceled) return false;

                    procThread.Job.ActivateSource(procThread.Device);

                    if (procThread.IsCanceled) return false;

                    // Finds and applys a smooth streaming preset        
                    procThread.Job.ApplyPreset(LivePresets.VC1256kDSL16x9);

                    if (procThread.IsCanceled) return false;

                    // Creates the publishing format for the job
                    PullBroadcastPublishFormat format = new PullBroadcastPublishFormat
                    {
                        BroadcastPort = procThread.BroadcastPort,
                        MaximumNumberOfConnections = 2
                    };

                    // Adds the publishing format to the job
                    procThread.Job.PublishFormats.Add(format);

                    if (procThread.IsCanceled) return false;

                    // Starts encoding
                    procThread.Job.StartEncoding();

                    if (procThread.IsCanceled)
                    {
                        procThread.Stop();
                        return false;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    RecordCompleted(new MediaCaptureEventArgs(ex), true);
                    return false;
                }
            });
        }

        void Recording(EncoderProcessingThread procThread)
        {
            Mode = MediaCaptureMode.Recording;

            _asyncRecordingThread = new Thread(DoRecording)
            {
                IsBackground = true // обязательно true!! а то при завершении основной программы поток будет продолжать работать 
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
                if (processingThread == null)
                    throw new Exception($"{nameof(EncoderProcessingThread)} not initialized.");

                processingThread.ThreadProc = Thread.CurrentThread;

                result = new MediaCaptureEventArgs(processingThread.DestinationFilePath);

                var startCapture = DateTime.Now;
                while (DateTime.Now.Subtract(startCapture).TotalSeconds < SecondsRecordDuration)
                {
                    if (!MainThread.IsAlive)
                    {
                        processingThread.DeleteFile(true);
                        await processingThread.Dispose();
                        Mode = MediaCaptureMode.None;
                        return;
                    }

                    if (Mode == MediaCaptureMode.None)
                        break;

                    await Task.Delay(100);
                }
            }
            catch (ThreadAbortException ex)
            {
                switch (result)
                {
                    case null when processingThread != null:
                        result = new MediaCaptureEventArgs(processingThread.DestinationFilePath);
                        break;
                    case null:
                        result = new MediaCaptureEventArgs(ex);
                        break;
                }
            }
            catch (Exception ex)
            {
                result = new MediaCaptureEventArgs(ex);
            }

            if (processingThread != null)
            {
                await ASYNC.ExecuteWithTimeoutAsync(processingThread.Stop(), TIMEOUT_STOP);
                if (_asyncRecordingThread != null && processingThread.ThreadProc != null && _asyncRecordingThread.ManagedThreadId == processingThread.ThreadProc.ManagedThreadId)
                    _asyncRecordingThread = null;
            }

            if(result?.Error != null)
                processingThread?.DeleteFile();

            Mode = MediaCaptureMode.None;
            RecordCompleted(result, true);
        }

        public override async void Stop()
        {
            if (_asyncRecordingThread != null)
            {
                var isStopped = false;
                List<EncoderProcessingThread> currentProcessingThread = null;
                lock (syncPT)
                {
                    currentProcessingThread = ProcessingThreads.Where(p => p.ThreadProc != null && p.ThreadProc.ManagedThreadId == _asyncRecordingThread.ManagedThreadId).ToList();
                }

                foreach (var processingThread in currentProcessingThread)
                {
                    try
                    {
                        await ASYNC.ExecuteWithTimeoutAsync(processingThread.Stop(), TIMEOUT_STOP);
                        if (processingThread.ThreadProc.IsAlive)
                            processingThread.ThreadProc.Abort();

                        isStopped = true;
                    }
                    catch (Exception)
                    {
                        //null;
                    }
                }

                if (isStopped)
                {
                    _asyncRecordingThread = null;
                    Mode = MediaCaptureMode.None;
                    return;
                }
            }

            await StopAllActiveProcesses();
        }

        private async Task StopAllActiveProcesses()
        {
            List<EncoderProcessingThread> allprocessingThreads = null;
            lock (syncPT)
            {
                allprocessingThreads = new List<EncoderProcessingThread>();
                allprocessingThreads.AddRange(ProcessingThreads);
            }

            foreach (var processingThread in allprocessingThreads)
            {
                try
                {
                    if (processingThread.ThreadProc == null || !processingThread.ThreadProc.IsAlive)
                        continue;

                    await ASYNC.ExecuteWithTimeoutAsync(processingThread.Stop(), TIMEOUT_STOP);
                    processingThread.ThreadProc.Abort();
                }
                catch (Exception)
                {
                    // null
                }
            }

            _asyncRecordingThread = null;
            Mode = MediaCaptureMode.None;
        }

        public async void Dispose()
        {
            try
            {
                await StopAllActiveProcesses();
            }
            catch (Exception)
            {
                // null;
            }
        }
    }
}