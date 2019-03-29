using Microsoft.Expression.Encoder.Devices;
using Microsoft.Expression.Encoder.Live;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Utils.UIControls.Tools
{
    public class CamCapture
    {
        public List<string> VideoEncoders { get; } = new List<string>();
        public List<string> AudioEncoders { get; } = new List<string>();

        private Dictionary<string, EncoderDevice> _videoEncoders;
        private Dictionary<string, EncoderDevice> _audioEncoders;

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
        /// <param name="destiationFileName"></param>
        /// <param name="timeRecSec"></param>
        /// <param name="videoEncoder"></param>
        /// <param name="audioEncoder"></param>
        /// <returns></returns>
        public async Task<bool> StartRec(string destiationFileName, int timeRecSec = 60, string videoEncoder = null, string audioEncoder = null)
        {
            EncoderDevice videoEnc = GetEncoder(_videoEncoders, videoEncoder);
            EncoderDevice audioEnc = GetEncoder(_audioEncoders, audioEncoder);

            if (videoEnc == null || audioEnc == null || timeRecSec > 1800)
                return false;

            if (File.Exists(destiationFileName) || !Directory.Exists(Path.GetDirectoryName(destiationFileName)))
                return false;

            LiveJob job = new LiveJob();
            LiveDeviceSource deviceSource = job.AddDeviceSource(videoEnc, audioEnc);

            // Setup the video resolution and frame rate of the video device
            // NOTE: Of course, the resolution and frame rate you specify must be supported by the device!
            // NOTE2: May be not all video devices support this call, and so it just doesn't work, as if you don't call it (no error is raised)
            // NOTE3: As a workaround, if the .PickBestVideoFormat method doesn't work, you could force the resolution in the 
            //        following instructions (called few lines belows): 'panelVideoPreview.Size=' and '_job.OutputFormat.VideoProfile.Size=' 
            //        to be the one you choosed (640, 480).
            deviceSource.PickBestVideoFormat(new Size(640, 480), 25);

            // Get the properties of the device video
            SourceProperties sp = deviceSource.SourcePropertiesSnapshot();

            // Setup the output video resolution file as the preview
            job.OutputFormat.VideoProfile.Size = new Size(sp.Size.Width, sp.Size.Height);

            job.ActivateSource(deviceSource);

            FileArchivePublishFormat fileOut = new FileArchivePublishFormat();
            fileOut.OutputFileName = destiationFileName;
            job.PublishFormats.Add(fileOut);
            job.StartEncoding();


            await Task.Delay(timeRecSec * 1000);


            job.StopEncoding();
            job.RemoveDeviceSource(deviceSource);
            deviceSource.PreviewWindow = null;

            return true;
        }

        //private void GrabImage(string destiationFileName)
        //{
        //    // Create a Bitmap of the same dimension of panelVideoPreview (Width x Height)
        //    using (Bitmap bitmap = new Bitmap(panelVideoPreview.Width, panelVideoPreview.Height))
        //    {
        //        using (Graphics g = Graphics.FromImage(bitmap))
        //        {
        //            // Get the paramters to call g.CopyFromScreen and get the image
        //            Rectangle rectanglePanelVideoPreview = panelVideoPreview.Bounds;
        //            Point sourcePoints = panelVideoPreview.PointToScreen(new Point(panelVideoPreview.ClientRectangle.X, panelVideoPreview.ClientRectangle.Y));
        //            g.CopyFromScreen(sourcePoints, Point.Empty, rectanglePanelVideoPreview.Size);
        //        }

        //        bitmap.Save(destiationFileName, System.Drawing.Imaging.ImageFormat.Jpeg);
        //    }
        //}

        //private void Broadcast_Click(object sender, EventArgs e)
        //{
        //     <MediaElement Name="VideoControl" Source="http://localhost:8080" />
        //    EncoderDevice video = null;
        //    EncoderDevice audio = null;

        //    GetSelectedVideoAndAudioDevices(out video, out audio);
        //    StopJob();

        //    if (video == null)
        //    {
        //        return;
        //    }

        //    _job = new LiveJob();

        //    _deviceSource = _job.AddDeviceSource(video, audio);
        //    _job.ActivateSource(_deviceSource);

        //    // Finds and applys a smooth streaming preset        
        //    _job.ApplyPreset(LivePresets.VC1256kDSL16x9);

        //    // Creates the publishing format for the job
        //    PullBroadcastPublishFormat format = new PullBroadcastPublishFormat();
        //    format.BroadcastPort = 8080;
        //    format.MaximumNumberOfConnections = 2;

        //    // Adds the publishing format to the job
        //    _job.PublishFormats.Add(format);

        //    // Starts encoding
        //    _job.StartEncoding();

        //    toolStripStatusLabel1.Text = "Broadcast started on localhost at port 8080, run WpfShowBroadcast.exe now to see it";
        //}

        EncoderDevice GetEncoder(Dictionary<string, EncoderDevice> encoders, string encoderName)
        {
            if (string.IsNullOrEmpty(encoderName))
                return encoders.FirstOrDefault().Value;
            else if (encoders.TryGetValue(encoderName, out EncoderDevice encDev))
                return encDev;
            return null;
        }
    }
}