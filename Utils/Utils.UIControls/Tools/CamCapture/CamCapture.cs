using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using Microsoft.Expression.Encoder;
using Microsoft.Expression.Encoder.Devices;
using Microsoft.Expression.Encoder.Live;
using Microsoft.Expression.Encoder.ScreenCapture;

namespace Utils.UIControls.Tools.CamCapture
{
    public class CamCapture
    {
        private LiveJob _job;
        private ScreenCaptureJob _jobScreen;
        private LiveDeviceSource _deviceSource;
        private readonly Dictionary<string, EncoderDevice> _videoDevices;
        private readonly Dictionary<string, EncoderDevice> _audioDevices;

        public event CamCaptureEventHandler OnRecordingCompleted;
        public List<string> VideoDevices { get; } = new List<string>();
        public List<string> AudioDevices { get; } = new List<string>();
        public CamCaptureMode Mode { get; private set; } = CamCaptureMode.None;

        /// <summary>
        /// Невозможно развернуть приложение, использующее EE4 SDK, без установки всего приложения на целевой машине. Даже если вы попытаетесь "скопировать локальные" DLL файлы в ваше местоположение приложения, для этого необходимо установить 25-мегабайтное EE4-приложение. Поэтому перед использованием сначала установите Microsoft Expression Encoder 4 (Encoder_en.exe)
        /// </summary>
        public CamCapture()
        {
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
            if (Mode != CamCaptureMode.None)
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
            var asyncRec = new Func<CoverRecord, Task<CamCaptureEventArgs>>(DoRecordingAsync);
            asyncRec.BeginInvoke(new CoverRecord(destinationFile, timeRec, videoEnc, audioEnc), DoRecordingAsyncCompleted, asyncRec);
        }

        async Task<CamCaptureEventArgs> DoRecordingAsync(CoverRecord cover)
        {
            try
            {
                _job = new LiveJob();
                _deviceSource = _job.AddDeviceSource(cover.VideoDevice, cover.AudioDevice);

                // Setup the video resolution and frame rate of the video device
                // NOTE: Of course, the resolution and frame rate you specify must be supported by the device!
                // NOTE2: May be not all video devices support this call, and so it just doesn't work, as if you don't call it (no error is raised)
                // NOTE3: As a workaround, if the .PickBestVideoFormat method doesn't work, you could force the resolution in the 
                //        following instructions (called few lines belows): 'panelVideoPreview.Size=' and '_job.OutputFormat.VideoProfile.Size=' 
                //        to be the one you choosed (640, 480).
                _deviceSource.PickBestVideoFormat(new Size(640, 480), 25);

                // Get the properties of the device video
                SourceProperties sp = _deviceSource.SourcePropertiesSnapshot();

                // Setup the output video resolution file as the preview
                _job.OutputFormat.VideoProfile.Size = new Size(sp.Size.Width, sp.Size.Height);

                _job.ActivateSource(_deviceSource);

                FileArchivePublishFormat fileOut = new FileArchivePublishFormat
                {
                    OutputFileName = cover.DestinationFile
                };
                _job.PublishFormats.Add(fileOut);
                _job.StartEncoding();

                await Task.Delay(cover.TimeRec * 1000);

                return new CamCaptureEventArgs(cover.DestinationFile);
            }
            catch (Exception ex)
            {
                return new CamCaptureEventArgs(ex);
            }
        }

        public void StartScreenRecording(string destinationFile, int timeRecSec = 60, string audioDevice = null)
        {
            if (Mode != CamCaptureMode.None)
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
            var asyncRec = new Func<CoverRecord, Task<CamCaptureEventArgs>>(DoScreenRecordingAsync);
            asyncRec.BeginInvoke(new CoverRecord(destinationFile, timeRec, null, audioEnc), DoRecordingAsyncCompleted, asyncRec);
        }

        async Task<CamCaptureEventArgs> DoScreenRecordingAsync(CoverRecord cover)
        {
            try
            {
                string pcName = System.Environment.MachineName;
                _jobScreen = new ScreenCaptureJob();
                _jobScreen.ScreenCaptureVideoProfile.FrameRate = 5;
                _jobScreen.AddAudioDeviceSource(cover.AudioDevice);
                _jobScreen.ScreenCaptureAudioProfile.Channels = 1;
                _jobScreen.ScreenCaptureAudioProfile.SamplesPerSecond = 32000;
                _jobScreen.ScreenCaptureAudioProfile.BitsPerSample = 16;
                _jobScreen.ScreenCaptureAudioProfile.Bitrate = new Microsoft.Expression.Encoder.Profiles.ConstantBitrate(20);

                //Rectangle capRect = new Rectangle(388, 222, 1056, 608);
                Rectangle capRect = new Rectangle(10, 10, 640, 480);
                _jobScreen.CaptureRectangle = capRect;

                _jobScreen.OutputScreenCaptureFileName = cover.DestinationFile;
                _jobScreen.Start();

                await Task.Delay(cover.TimeRec * 1000);

                return new CamCaptureEventArgs(cover.DestinationFile);
            }
            catch (Exception ex)
            {
                return new CamCaptureEventArgs(ex);
            }
        }

        void DoRecordingAsyncCompleted(IAsyncResult asyncResult)
        {
            try
            {
                StopJob();
                AsyncResult ar = asyncResult as AsyncResult;
                var caller = (Func<CoverRecord, Task<CamCaptureEventArgs>>)ar.AsyncDelegate;
                Task<CamCaptureEventArgs> taskResult = caller.EndInvoke(asyncResult);
                OnRecordingCompleted?.Invoke(this, taskResult.Result);
            }
            catch (Exception ex)
            {
                OnRecordingCompleted?.Invoke(this, new CamCaptureEventArgs(ex));
            }
        }

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

            if (Mode != CamCaptureMode.None)
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
            StopJob();
        }

        void StopJob()
        {
            try
            {
                _job?.StopEncoding();
                _jobScreen?.Stop();
                _job?.RemoveDeviceSource(_deviceSource);
                if (_deviceSource != null)
                    _deviceSource.PreviewWindow = null;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Mode = CamCaptureMode.None;
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
    }
}