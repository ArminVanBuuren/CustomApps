using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
//using AForge.Video.FFMPEG;
using AForge.Video.VFW;
using Utils.CollectionHelper;
using Timer = System.Timers.Timer;

namespace Utils.WinForm.AForge
{
    public class AForgeCapture
    {
        private readonly System.Timers.Timer _clearMemory;
        private readonly Action<Bitmap, bool> _updatePicFrame;
        private VideoCaptureDevice FinalVideo = null;
        private readonly AVIWriter AVIwriter;
        //private VideoFileWriter FileWriter = new VideoFileWriter();

        // MSVC - с компрессией; wmv3 - с компрессией но нужен кодек
        // MRLE
        // http://sundar1984.blogspot.com/2007_08_01_archive.html


        
        public CaptureMode Mode { get; private set; } = CaptureMode.None;
        public List<VideoDevice> VideoCapabilites { get; } = new List<VideoDevice>();
        public VideoDevice CurrentDevice { get; private set; }

        public AForgeCapture(PictureBox pictureBox = null)
        {
            FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in videoDevices)
            {
                var videoideoDevice = new VideoCaptureDevice(device.MonikerString);
                var videoCapabilities = videoideoDevice.VideoCapabilities;

                foreach (var capabilty in videoCapabilities)
                {
                    VideoCapabilites.Add(new VideoDevice(device.Name, videoideoDevice, capabilty));
                }

                if (CurrentDevice == null && VideoCapabilites.Count > 0)
                {
                    var res = VideoCapabilites.OrderBy(p => p.Height).ThenByDescending(p => p.Width);
                    CurrentDevice = res.FirstOrDefault();
                }
            }

            if(CurrentDevice == null)
                throw new Exception("No devices founded.");

            AVIwriter = new AVIWriter("MSVC");
            if (pictureBox != null)
            {
                _updatePicFrame = (frame, isResizable) => { pictureBox.Image = isResizable ? (Bitmap)frame.Clone() : ((Bitmap)frame.Clone()).ResizeImage(CurrentDevice.Width, CurrentDevice.Height); };
            }

            _clearMemory = new System.Timers.Timer
            {
                Interval = 2000
            };
            _clearMemory.Elapsed += ClearMemory;
            _clearMemory.AutoReset = false;
            _clearMemory.Enabled = true;
        }

        private void ClearMemory(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Mode != CaptureMode.None)
                GC.Collect();
            _clearMemory.Enabled = true;
        }

        public bool StartPreview()
        {
            if (_updatePicFrame == null)
                return false;

            if(Mode == CaptureMode.Previewing || Mode == CaptureMode.Recording)
                return true;

            StartCapturing();
            Mode = CaptureMode.Previewing;
            return true;
        }

        public void StopPreview()
        {
            StopCapturing();
        }

        public async Task StartRecording(string destinationFile, int timeRecSec = 60)
        {
            if (Mode == CaptureMode.Recording)
                return;

            AVIwriter.Open(destinationFile, CurrentDevice.Width, CurrentDevice.Height);

            if (Mode == CaptureMode.None)
            {
                StartCapturing();
            }

            Mode = CaptureMode.Recording;

            await Task.Delay(timeRecSec * 1000);

            StopCapturing();
        }

        public async Task<Bitmap> GetPicture()
        {
            if (Mode != CaptureMode.None)
                return null;

            Bitmap result = null;
            var getImage = CurrentDevice.Device;

            try
            {
                getImage.NewFrame += (sender, args) =>
                {
                    try
                    {
                        result = (Bitmap) args.Frame.Clone();
                        getImage.Stop();
                    }
                    catch (Exception)
                    {
                        //null
                    }
                };
                getImage.Start();

                int timeOut = 0;
                while (result == null || timeOut <= 10)
                {
                    await Task.Delay(1000);
                    timeOut++;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (getImage.IsRunning)
                    getImage.Stop();
                GC.Collect();
            }

            return result;
        }


        void StartCapturing()
        {
            FinalVideo = CurrentDevice.Device;
            FinalVideo.NewFrame += new NewFrameEventHandler(FinalVideo_NewFrame);
            FinalVideo.Start();
        }

        void StopCapturing()
        {
            try
            {
                if (FinalVideo != null && FinalVideo.IsRunning)
                    FinalVideo.Stop();
                //FileWriter.Close();
                AVIwriter.Close();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                GC.Collect();
                Mode = CaptureMode.None;
            }
        }

        void FinalVideo_NewFrame(object sender, NewFrameEventArgs args)
        {
            switch (Mode)
            {
                case CaptureMode.Recording:
                {
                    var video = ((Bitmap)args.Frame.Clone()).ResizeImage(CurrentDevice.Width, CurrentDevice.Height);
                    _updatePicFrame?.Invoke(video, true);

                    AVIwriter.Quality = 0;
                    //FileWriter.WriteVideoFrame(video);
                    AVIwriter.AddFrame(video);
                    break;
                }
                case CaptureMode.Previewing:
                    _updatePicFrame?.Invoke(args.Frame, false);
                    break;
            }
        }
    }
}
