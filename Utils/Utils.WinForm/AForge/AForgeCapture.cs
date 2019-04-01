using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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
        private VideoCaptureDevice _finalVideo = null;
        private readonly AVIWriter _aviWriter;
        //private VideoFileWriter FileWriter = new VideoFileWriter();

        // MSVC - с компрессией; wmv3 - с компрессией но нужен кодек
        // MRLE
        // http://sundar1984.blogspot.com/2007_08_01_archive.html


        public event EventHandler RecordingCompleted;
        public event AForgeEventHandler ProcessingError;
        public AForgeCaptureMode Mode { get; private set; } = AForgeCaptureMode.None;
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

            _aviWriter = new AVIWriter("MSVC");
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
            if (Mode != AForgeCaptureMode.None)
                GC.Collect();
            _clearMemory.Enabled = true;
        }

        public bool StartPreview()
        {
            if (_updatePicFrame == null)
                return false;

            if(Mode == AForgeCaptureMode.Previewing || Mode == AForgeCaptureMode.Recording)
                return true;

            StartCapturing();
            Mode = AForgeCaptureMode.Previewing;
            return true;
        }

        public void StopPreview()
        {
            StopCapturing();
        }

        public bool StartRecording(string destinationFile, int timeRecSec = 60)
        {
            if (Mode == AForgeCaptureMode.Recording || RecordingCompleted == null)
                return false;

            _aviWriter.Open(destinationFile, CurrentDevice.Width, CurrentDevice.Height);

            var asyncRec = new Func<string, int, Task>(DoRecordingAsync);
            asyncRec.BeginInvoke(destinationFile, timeRecSec, DoRecordingAsyncCompleted, asyncRec);
            return true;
        }

        async Task DoRecordingAsync(string destinationFile, int timeRecSec)
        {
            try
            {
                if (Mode == AForgeCaptureMode.None)
                {
                    StartCapturing();
                }

                Mode = AForgeCaptureMode.Recording;

                await Task.Delay(timeRecSec * 1000);
            }
            catch (Exception ex)
            {
                ProcessingError?.Invoke(this, new AForgeEventArgs(ex));
            }
            finally
            {
                StopCapturing();
            }
        }

        public void StopAnyProcess()
        {
            StopCapturing();
        }

        void DoRecordingAsyncCompleted(IAsyncResult asyncResult)
        {
            //AsyncResult ar = asyncResult as AsyncResult;
            //var caller = (Func<string, int, Task>)ar.AsyncDelegate;
            //Task result = caller.EndInvoke(asyncResult);
            RecordingCompleted?.Invoke(this, EventArgs.Empty);
        }

        public async Task<Bitmap> GetPicture()
        {
            if (Mode != AForgeCaptureMode.None)
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
                    catch (Exception ex)
                    {
                        ProcessingError?.Invoke(this, new AForgeEventArgs(ex));
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
            catch (Exception ex1)
            {
                ProcessingError?.Invoke(this, new AForgeEventArgs(ex1));
            }
            finally
            {
                try
                {
                    //if (getImage.IsRunning)
                    getImage.Stop();
                    GC.Collect();
                }
                catch (Exception ex2)
                {
                    ProcessingError?.Invoke(this, new AForgeEventArgs(ex2));
                }
            }

            return result;
        }

        void StartCapturing()
        {
            if (_finalVideo != null)
                StopCapturing();

            _finalVideo = CurrentDevice.Device;
            _finalVideo.NewFrame += new NewFrameEventHandler(FinalVideo_NewFrame);
            _finalVideo.Start();
        }

        void StopCapturing()
        {
            try
            {
                //if (FinalVideo != null && FinalVideo.IsRunning)
                _finalVideo.Stop();
                _finalVideo = null;
            }
            catch (Exception ex)
            {
                ProcessingError?.Invoke(this, new AForgeEventArgs(ex));
            }

            try
            {
                //FileWriter.Close();
                _aviWriter.Close();
            }
            catch (Exception ex)
            {
                ProcessingError?.Invoke(this, new AForgeEventArgs(ex));
            }

            GC.Collect();
            Mode = AForgeCaptureMode.None;
        }

        void FinalVideo_NewFrame(object sender, NewFrameEventArgs args)
        {
            try
            {
                switch (Mode)
                {
                    case AForgeCaptureMode.Recording:
                    {
                        var video = ((Bitmap)args.Frame.Clone()).ResizeImage(CurrentDevice.Width, CurrentDevice.Height);
                        _updatePicFrame?.Invoke(video, true);

                        _aviWriter.Quality = 0;
                        //FileWriter.WriteVideoFrame(video);
                        _aviWriter.AddFrame(video);
                        break;
                    }
                    case AForgeCaptureMode.Previewing:
                        _updatePicFrame?.Invoke(args.Frame, false);
                        break;
                    case AForgeCaptureMode.None:
                        StopCapturing();
                        break;
                }
            }
            catch (Exception ex)
            {
                ProcessingError?.Invoke(this, new AForgeEventArgs(ex));
            }
        }
    }
}
