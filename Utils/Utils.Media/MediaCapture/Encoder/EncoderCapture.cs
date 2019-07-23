using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Expression.Encoder;
using Microsoft.Expression.Encoder.Devices;
using Microsoft.Expression.Encoder.Live;
using Microsoft.Expression.Encoder.ScreenCapture;
using Utils.Media.MediaCapture.AForge;

namespace Utils.Media.MediaCapture.Encoder
{
    public class EncoderCapture : MediaCapture, IDisposable
    {
        Thread MainThread { get; }

        private const int TIMEOUT_INITIALIZE = 60000;
        private const int TIMEOUT_STOP = 5000;
        //private const int TIMEOUT_TERMINATE = 5000;

        private Thread _asyncRecordingThread;
        readonly object syncPT = new object();
        private List<EncoderProcessingThread> ProcessingThreads { get; } = new List<EncoderProcessingThread>();

        /// <summary>
        /// Все видео устройства полученные через библиотеку AForge
        /// </summary>
        protected AForgeMediaDevices AForgeDevices { get; }

        /// <summary>
        /// Все видео и аудио устройства полученные через библиотеку Expression.Encoder
        /// </summary>
        protected EncoderMediaDevices EncoderDevices { get; }

        public AForgeDevice AForgeVideoDevice { get; private set; }
        public EncoderDevice VideoEncoderDevice { get; private set; }
        public EncoderDevice AudioEncoderDevice { get; private set; }
        

        /// <summary>
        /// Невозможно развернуть приложение, использующее EE4 SDK, без установки всего приложения на целевой машине. Даже если вы попытаетесь "скопировать локальные" DLL файлы в ваше местоположение приложения, для этого необходимо установить 25-мегабайтное EE4-приложение. Поэтому перед использованием сначала установите Microsoft Expression Encoder 4 (Encoder_en.exe)
        /// </summary>
        public EncoderCapture(Thread mainThread, AForgeMediaDevices aDevices, EncoderMediaDevices cDevices, string destinationDir, int secondsRecDuration = 60):base(destinationDir, secondsRecDuration)
        {
            MainThread = mainThread;
            AForgeDevices = aDevices;
            EncoderDevices = cDevices;

            AForgeVideoDevice = AForgeDevices?.GetDefaultVideoDevice();
            VideoEncoderDevice = EncoderDevices.GetDefaultVideoDevice();
            AudioEncoderDevice = EncoderDevices.GetDefaultAudioDevice("Microph|Микроф");
        }

        public override void ChangeVideoDevice(string name)
        {
            var resEncoderVideo = EncoderDevices.GetDefaultVideoDevice(name);

            VideoEncoderDevice = resEncoderVideo ?? throw new Exception($"Video device [{name}] not found.");

            var resAForgeVideoDevice = AForgeDevices?.GetDefaultVideoDevice(name);
            if (resAForgeVideoDevice != null)
                AForgeVideoDevice = resAForgeVideoDevice;
        }

        public void ChangeAudioDevice(string name)
        {
            var res = EncoderDevices.GetDefaultAudioDevice(name);

            AudioEncoderDevice = res ?? throw new Exception($"Audio device [{name}] not found.");
        }

        public override void StartRecording(string fileName = null)
        {
            if (Mode != MediaCaptureMode.None || (_asyncRecordingThread != null && _asyncRecordingThread.IsAlive))
                throw new MediaCaptureRunningException("You must stop the previous process first!");

            if (VideoEncoderDevice == null || AudioEncoderDevice == null)
                throw new ArgumentException($"Video=[{VideoEncoderDevice}] or Audio=[{AudioEncoderDevice}] device is incorrect!");


            var procThread = new EncoderProcessingThread(GetNewVideoFilePath(fileName));
            Initialization(procThread, DoCamInitialize);

            if (!procThread.IsCanceled)
            {
                StartRecordingThread(procThread);
            }
        }

        //public override async Task StartCamRecordingAsync(string fileName = null)
        //{
        //    if (Mode != MediaCaptureMode.None || (_asyncRecordingThread != null && _asyncRecordingThread.IsAlive))
        //        throw new MediaCaptureRunningException("You must stop the previous process first!");

        //    if (VideoEncoderDevice == null || AudioEncoderDevice == null)
        //        throw new ArgumentException($"Video=[{VideoEncoderDevice?.ToString()}] or Audio=[{AudioEncoderDevice?.ToString()}] device is incorrect!");


        //    EncoderProcessingThread procThread = new EncoderProcessingThread(GetNewVideoFilePath(fileName));
        //    try
        //    {
        //        await InitializationAsync(procThread, DoCamInitializeAsync);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new TargetInvocationException(ex);
        //    }

        //    if (!procThread.IsCanceled)
        //    {
        //        StartRecordingThread(procThread);
        //    }
        //}

        public void StartScreenRecording(string fileName = null)
        {
            if (Mode != MediaCaptureMode.None || (_asyncRecordingThread != null && _asyncRecordingThread.IsAlive))
                throw new MediaCaptureRunningException("You must stop the previous process first!");

            if (AudioEncoderDevice == null)
                throw new ArgumentException("Audio device is incorrect!");

            var procThread = new EncoderProcessingThread(GetNewVideoFilePath(fileName));
            Initialization(procThread, DoScreenInitialize);

            if (!procThread.IsCanceled)
            {
                StartRecordingThread(procThread);
            }
        }

        //public override async Task StartScreenRecordingAsync(string fileName = null)
        //{
        //    if (Mode != MediaCaptureMode.None || (_asyncRecordingThread != null && _asyncRecordingThread.IsAlive))
        //        throw new MediaCaptureRunningException("You must stop the previous process first!");

        //    if (AudioEncoderDevice == null)
        //        throw new ArgumentException("Audio device is incorrect!");

        //    EncoderProcessingThread procThread = new EncoderProcessingThread(GetNewVideoFilePath(fileName));
        //    try
        //    {
        //        await InitializationAsync(procThread, DoScreenInitializeAsync);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new TargetInvocationException(ex);
        //    }
            

        //    if (!procThread.IsCanceled)
        //    {
        //        StartRecordingThread(procThread);
        //    }
        //}

        public override bool StartBroadcast(int port = 8080)
        {
            // <MediaElement Name = "VideoControl" Source = "http://localhost:8080" />

            if (Mode != MediaCaptureMode.None || (_asyncRecordingThread != null && _asyncRecordingThread.IsAlive))
                throw new MediaCaptureRunningException("You must stop the previous process first!");

            if (VideoEncoderDevice == null || AudioEncoderDevice == null)
                throw new ArgumentException("Video or Audio device is incorrect!");

            var procThread = new EncoderProcessingThread(port);
            Initialization(procThread, DoBroadcastInitialize);

            return !procThread.IsCanceled;
        }

        //public override async Task<bool> StartBroadcastAsync(int port = 8080)
        //{
        //    // <MediaElement Name = "VideoControl" Source = "http://localhost:8080" />

        //    if (Mode != MediaCaptureMode.None || (_asyncRecordingThread != null && _asyncRecordingThread.IsAlive))
        //        throw new MediaCaptureRunningException("You must stop the previous process first!");

        //    if (VideoEncoderDevice == null || AudioEncoderDevice == null)
        //        throw new ArgumentException("Video or Audio device is incorrect!");

        //    EncoderProcessingThread procThread = new EncoderProcessingThread(port);
        //    try
        //    {
        //        await InitializationAsync(procThread, DoBroadcastInitializeAsync);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new TargetInvocationException(ex);
        //    }

        //    return !procThread.IsCanceled;
        //}

        void Initialization(EncoderProcessingThread procThread, Func<EncoderProcessingThread, bool> initializeDeviceMethod)
        {
            Mode = MediaCaptureMode.Initialization;

            lock (syncPT)
            {
                ProcessingThreads.Add(procThread);
            }

            if (!initializeDeviceMethod.BeginInvoke(procThread, null, null).AsyncWaitHandle.WaitOne(TIMEOUT_INITIALIZE))
            {
                procThread.IsCanceled = true;
                var stopProcess = new Action<bool>(procThread.Stop);
                stopProcess.BeginInvoke(false, null, null).AsyncWaitHandle.WaitOne(TIMEOUT_STOP);
                Mode = MediaCaptureMode.None;
                throw new TimeoutException();
            }

            // если во время инициализации была произведена команда отмены
            if (Mode == MediaCaptureMode.None)
            {
                procThread.IsCanceled = true;
                var stopProcess = new Action<bool>(procThread.Stop);
                stopProcess.BeginInvoke(false, null, null).AsyncWaitHandle.WaitOne(TIMEOUT_STOP);
            }
        }

        //async Task InitializationAsync(EncoderProcessingThread procThread, Func<EncoderProcessingThread, Task<bool>> initializeDeviceMethod)
        //{
        //    Mode = MediaCaptureMode.Initialization;

        //    lock (syncPT)
        //    {
        //        ProcessingThreads.Add(procThread);
        //    }

        //    if (!await ASYNC.ExecuteWithTimeoutAsync(initializeDeviceMethod.Invoke(procThread), TIMEOUT_INITIALIZE))
        //    {
        //        procThread.IsCanceled = true;
        //        await ASYNC.ExecuteWithTimeoutAsync(procThread.StopAsync(), TIMEOUT_STOP);
        //        Mode = MediaCaptureMode.None;
        //        throw new TimeoutException();
        //    }

        //    // если во время инициализации была произведена команда отмены
        //    if (Mode == MediaCaptureMode.None)
        //    {
        //        procThread.IsCanceled = true;
        //        await ASYNC.ExecuteWithTimeoutAsync(procThread.StopAsync(), TIMEOUT_STOP);
        //        return;
        //    }
        //}

        void InitializationAsyncToSync(EncoderProcessingThread procThread, Func<EncoderProcessingThread, Task<bool>> initializeDeviceMethod)
        {
            Mode = MediaCaptureMode.Initialization;

            lock (syncPT)
            {
                ProcessingThreads.Add(procThread);
            }

            Task task = initializeDeviceMethod.Invoke(procThread);
            if (!task.Wait(TIMEOUT_INITIALIZE))
            {
                procThread.IsCanceled = true;
                var taskStop = procThread.StopAsync();
                taskStop.Wait(TIMEOUT_STOP);
                Mode = MediaCaptureMode.None;
                throw new TimeoutException();
            }

            // если во время инициализации была произведена команда отмены
            if (Mode == MediaCaptureMode.None)
            {
                procThread.IsCanceled = true;
                var taskStop = procThread.StopAsync();
                taskStop.Wait(TIMEOUT_STOP);
            }
        }

        Task<bool> DoCamInitializeAsync(EncoderProcessingThread procThread)
        {
            return Task.Run(() => DoCamInitialize(procThread));
        }

        bool DoCamInitialize(EncoderProcessingThread procThread)
        {
            try
            {
                //Thread.Sleep(15000);

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
                var aforgeDevice = AForgeVideoDevice;

                procThread.Job.OutputFormat.VideoProfile.Size =
                    aforgeDevice == null ? new Size(defaultSize.Width, defaultSize.Height) : new Size(aforgeDevice.Width, aforgeDevice.Height);
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
                RecordCompleted(new MediaCaptureEventArgs(new[] {procThread.DestinationFilePath}, ex), true);
                return false;
            }
        }

        Task<bool> DoScreenInitializeAsync(EncoderProcessingThread procThread)
        {
            return Task.Run(() => DoScreenInitialize(procThread));
        }

        bool DoScreenInitialize(EncoderProcessingThread procThread)
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
                var capRect = new Rectangle(10, 10, 640, 480);
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
                RecordCompleted(new MediaCaptureEventArgs(new [] {procThread.DestinationFilePath}, ex), true);
                return false;
            }
        }

        Task<bool> DoBroadcastInitializeAsync(EncoderProcessingThread procThread)
        {
            return Task.Run(() => DoBroadcastInitialize(procThread));
        }

        bool DoBroadcastInitialize(EncoderProcessingThread procThread)
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
                var format = new PullBroadcastPublishFormat
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

                //procThread.ThreadProc = Thread.CurrentThread;  данный поток является базовым, если его срубить то убъется весь процесс который его зпускал, т.к. он данный метод запускается не в отдельном потоке а через BeginInvoke
                Mode = MediaCaptureMode.Broadcast;
                return true;
            }
            catch (Exception ex)
            {
                RecordCompleted(new MediaCaptureEventArgs(new [] {procThread.DestinationFilePath}, ex), true);
                return false;
            }
        }

        void StartRecordingThread(EncoderProcessingThread procThread)
        {
            Mode = MediaCaptureMode.Recording;

            _asyncRecordingThread = new Thread(DoRecordingAsync)
            {
                IsBackground = true // обязательно true!! а то при завершении основной программы поток будет продолжать работать 
            };
            _asyncRecordingThread.Start(procThread);

        }

        async void DoRecordingAsync(object procThread)
        {
            MediaCaptureEventArgs result = null;
            EncoderProcessingThread processingThread = null;

            if (procThread is EncoderProcessingThread inputProcThread)
            {
                try
                {
                    processingThread = inputProcThread;
                    processingThread.ThreadProc = Thread.CurrentThread;

                    result = new MediaCaptureEventArgs(new[] {processingThread.DestinationFilePath});

                    var startCapture = DateTime.Now;
                    while (DateTime.Now.Subtract(startCapture).TotalSeconds < SecondsRecordDuration)
                    {
                        if (!MainThread.IsAlive)
                        {
                            processingThread.DeleteFile(true);
                            await processingThread.DisposeAsync();
                            Mode = MediaCaptureMode.None;
                            return;
                        }

                        if (Mode == MediaCaptureMode.None)
                            break;

                        Thread.Sleep(100);
                    }
                }
                catch (ThreadAbortException ex)
                {
                    result = new MediaCaptureEventArgs(new[] {inputProcThread.DestinationFilePath}, ex);
                }
                catch (Exception ex)
                {
                    result = new MediaCaptureEventArgs(new[] {inputProcThread.DestinationFilePath}, ex);
                }
            }
            else
            {
                result = new MediaCaptureEventArgs(null, new Exception($"{nameof(EncoderProcessingThread)} not initialized."));
            }


            if (processingThread != null)
            {
                Stop();
                if (_asyncRecordingThread != null && processingThread.ThreadProc != null && _asyncRecordingThread.ManagedThreadId == processingThread.ThreadProc.ManagedThreadId)
                    _asyncRecordingThread = null;
            }

            if(result.Error != null)
                processingThread?.DeleteFile();

            Mode = MediaCaptureMode.None;
            RecordCompleted(result, true);
        }

        public override void Stop()
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
                        processingThread.IsCanceled = true;
                        new Action<bool>(processingThread.Stop).BeginInvoke(false, null, null).AsyncWaitHandle.WaitOne(TIMEOUT_STOP);

                        //if (processingThread.ThreadProc.IsAlive)
                        //    processingThread.ThreadProc.Abort();

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

            StopAllActiveProcesses();
        }

        private void StopAllActiveProcesses()
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
                    processingThread.IsCanceled = true;
                    new Action<bool>(processingThread.Stop).BeginInvoke(false, null, null).AsyncWaitHandle.WaitOne(TIMEOUT_STOP);

                    //if (processingThread.ThreadProc == null || !processingThread.ThreadProc.IsAlive)
                    //    continue;
                    //processingThread.ThreadProc.Abort();
                }
                catch (Exception)
                {
                    // null
                }
            }

            _asyncRecordingThread = null;
            Mode = MediaCaptureMode.None;
        }

        public override async Task StopAsync()
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
                        processingThread.IsCanceled = true;
                        await ASYNC.ExecuteWithTimeoutAsync(processingThread.StopAsync(), TIMEOUT_STOP);

                        //if (processingThread.ThreadProc.IsAlive)
                        //    processingThread.ThreadProc.Abort();

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

            await StopAllActiveProcessesAsync();
        }

        private async Task StopAllActiveProcessesAsync()
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
                    processingThread.IsCanceled = true;
                    await ASYNC.ExecuteWithTimeoutAsync(processingThread.StopAsync(), TIMEOUT_STOP);

                    //if (processingThread.ThreadProc == null || !processingThread.ThreadProc.IsAlive)
                    //    continue;
                    //processingThread.ThreadProc.Abort();
                }
                catch (Exception)
                {
                    // null
                }
            }

            _asyncRecordingThread = null;
            Mode = MediaCaptureMode.None;
        }

        public void Dispose()
        {
            try
            {
                StopAllActiveProcesses();
            }
            catch (Exception)
            {
                // null;
            }
        }

        public override string ToString()
        {
            return $"{base.ToString()}\r\nVideo=[{VideoEncoderDevice.Name}]\r\nAudio=[{AudioEncoderDevice.Name}]";
        }
    }
}