using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using Microsoft.Expression.Encoder;
using Microsoft.Expression.Encoder.Devices;
using Microsoft.Expression.Encoder.Live;

namespace Utils.UIControls.Tools.CamCapture
{
    public class CamCapture
    {
        private LiveJob _job;
        private LiveDeviceSource _deviceSource;
        private readonly Dictionary<string, EncoderDevice> _videoEncoders;
        private readonly Dictionary<string, EncoderDevice> _audioEncoders;

        public event CamCaptureEventHandler OnRecordingCompleted;
        public List<string> VideoEncoders { get; } = new List<string>();
        public List<string> AudioEncoders { get; } = new List<string>();
        public CamCaptureMode Mode { get; private set; } = CamCaptureMode.None;

        public CamCapture()
        {
            _videoEncoders = new Dictionary<string, EncoderDevice>();
            _audioEncoders = new Dictionary<string, EncoderDevice>();

            foreach (EncoderDevice edv in EncoderDevices.FindDevices(EncoderDeviceType.Video))
            {
                _videoEncoders.Add(edv.Name, edv);
                VideoEncoders.Add(edv.Name);
            }

            foreach (EncoderDevice eda in EncoderDevices.FindDevices(EncoderDeviceType.Audio))
            {
                _audioEncoders.Add(eda.Name, eda);
                AudioEncoders.Add(eda.Name);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="destinationFile"></param>
        /// <param name="timeRecSec"></param>
        /// <param name="videoEncoder"></param>
        /// <param name="audioEncoder"></param>
        /// <returns></returns>
        public bool StartRecording(string destinationFile, int timeRecSec = 60, string videoEncoder = null, string audioEncoder = null)
        {
            if (Mode != CamCaptureMode.None)
                return false;

            EncoderDevice videoEnc = GetEncoder(_videoEncoders, videoEncoder);
            EncoderDevice audioEnc = GetEncoder(_audioEncoders, audioEncoder);

            if (videoEnc == null || audioEnc == null || timeRecSec > 1800)
                return false;

            if (File.Exists(destinationFile) || !Directory.Exists(Path.GetDirectoryName(destinationFile)))
                return false;

            _job = new LiveJob();
            _deviceSource = _job.AddDeviceSource(videoEnc, audioEnc);

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


            Mode = CamCaptureMode.Recording;
            var asyncRec = new Func<string, int, Task<CamCaptureEventArgs>>(DoRecordingAsync);
            asyncRec.BeginInvoke(destinationFile, timeRecSec, DoRecordingAsyncCompleted, asyncRec);
            
            return true;
        }

        async Task<CamCaptureEventArgs> DoRecordingAsync(string destinationFile, int timeRecSec)
        {
            try
            {
                FileArchivePublishFormat fileOut = new FileArchivePublishFormat
                {
                    OutputFileName = destinationFile
                };
                _job.PublishFormats.Add(fileOut);
                _job.StartEncoding();

                await Task.Delay(timeRecSec * 1000);

                return new CamCaptureEventArgs(destinationFile);
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
                var caller = (Func<string, int, Task<CamCaptureEventArgs>>)ar.AsyncDelegate;
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

        private bool StartBroadcast(int port = 8080, string videoEncoder = null, string audioEncoder = null)
        {
            // <MediaElement Name = "VideoControl" Source = "http://localhost:8080" />

            if (Mode != CamCaptureMode.None)
                return false;

            EncoderDevice videoEnc = GetEncoder(_videoEncoders, videoEncoder);
            EncoderDevice audioEnc = GetEncoder(_audioEncoders, audioEncoder);

            if (videoEnc == null || audioEnc == null)
                return false;

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
            return true;
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

        static EncoderDevice GetEncoder(Dictionary<string, EncoderDevice> encoders, string encoderName)
        {
            if (string.IsNullOrEmpty(encoderName))
                return encoders.FirstOrDefault().Value;
            else if (encoders.TryGetValue(encoderName, out EncoderDevice encDev))
                return encDev;
            return null;
        }
    }
}