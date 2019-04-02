using System;
using System.Collections.Generic;
using System.Data.Design;
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
        private Action<Bitmap, bool> _updatePicFrame;

        private VideoCaptureDevice _finalVideo = null;
        private readonly AVIWriter _aviWriter;
        //private VideoFileWriter FileWriter = new VideoFileWriter();

        // MSVC - с компрессией; wmv3 - с компрессией но нужен кодек
        // MRLE
        // http://sundar1984.blogspot.com/2007_08_01_archive.html


        public event AForgeEventHandler OnRecordingCompleted;
        public event AForgeEventHandler OnUnexpectedError;
        public AForgeCaptureMode Mode { get; private set; } = AForgeCaptureMode.None;
        public List<VideoDevice> VideoDevices { get; } = new List<VideoDevice>();
        public VideoDevice CurrentDevice { get; private set; }

        public AForgeCapture()
        {
            FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in videoDevices)
            {
                var videoideoDevice = new VideoCaptureDevice(device.MonikerString);
                var videoCapabilities = videoideoDevice.VideoCapabilities;

                foreach (var capabilty in videoCapabilities)
                {
                    VideoDevices.Add(new VideoDevice(device.Name, videoideoDevice, capabilty));
                }

                if (CurrentDevice == null && VideoDevices.Count > 0)
                {
                    var res = VideoDevices.OrderBy(p => p.Height).ThenByDescending(p => p.Width);
                    CurrentDevice = res.FirstOrDefault();
                }
            }

            if (CurrentDevice == null)
                throw new Exception("No device found.");

            _aviWriter = new AVIWriter("MSVC");

            System.Timers.Timer clearMemory = new System.Timers.Timer
            {
                Interval = 2000
            };
            clearMemory.Elapsed += (sender, args) =>
            {
                if (Mode != AForgeCaptureMode.None)
                    GC.Collect();

                clearMemory.Enabled = true;
            };
            clearMemory.AutoReset = false;
            clearMemory.Enabled = true;
        }

        public void ChangeDevice(VideoDevice videoDevice)
        {
            if (Mode != AForgeCaptureMode.None)
                throw new AForgeRunningException("You must stop the previous process first!");

            if (VideoDevices.FirstOrDefault(p => p == videoDevice) == null)
                throw new ArgumentException(nameof(videoDevice));

            CurrentDevice = videoDevice;
        }

        public void StartPreview(PictureBox pictureBox)
        {
            if (Mode == AForgeCaptureMode.Previewing || Mode == AForgeCaptureMode.Recording)
                throw new AForgeRunningException("You must stop the previous process first!");

            if (pictureBox == null)
                throw new ArgumentNullException(nameof(pictureBox));

            _updatePicFrame = (frame, isResizable) => { pictureBox.Image = isResizable ? (Bitmap) frame.Clone() : ((Bitmap) frame.Clone()).ResizeImage(CurrentDevice.Width, CurrentDevice.Height); };

            Mode = AForgeCaptureMode.Previewing;
            StartCapturing();
        }

        public bool StartRecording(string destinationFile, int timeRecSec = 60)
        {
            if (Mode == AForgeCaptureMode.Recording)
                throw new AForgeRunningException("You must stop the previous process first!");

            if(OnRecordingCompleted == null)
                throw new Exception("You must initialize callback event first.");

            _aviWriter.Open(destinationFile, CurrentDevice.Width, CurrentDevice.Height);

            var asyncRec = new Func<string, int, Task<AForgeEventArgs>>(DoRecordingAsync);
            asyncRec.BeginInvoke(destinationFile, timeRecSec, DoRecordingAsyncCompleted, asyncRec);
            return true;
        }

        async Task<AForgeEventArgs> DoRecordingAsync(string destinationFile, int timeRecSec)
        {
            try
            {
                switch (Mode)
                {
                    case AForgeCaptureMode.None:
                        Mode = AForgeCaptureMode.Recording;
                        StartCapturing();
                        break;
                    case AForgeCaptureMode.Previewing:
                        Mode = AForgeCaptureMode.Recording;
                        break;
                }

                await Task.Delay(timeRecSec * 1000);

                return new AForgeEventArgs(destinationFile);
            }
            catch (Exception ex)
            {
                return new AForgeEventArgs(ex);
            }
        }

        void DoRecordingAsyncCompleted(IAsyncResult asyncResult)
        {
            try
            {
                StopCapturing();
                AsyncResult ar = asyncResult as AsyncResult;
                var caller = (Func<string, int, Task<AForgeEventArgs>>)ar.AsyncDelegate;
                Task<AForgeEventArgs> taskResult = caller.EndInvoke(asyncResult);
                OnRecordingCompleted?.Invoke(this, taskResult.Result);
            }
            catch (Exception ex)
            {
                OnRecordingCompleted?.Invoke(this, new AForgeEventArgs(ex));
            }
        }

        public async Task<Bitmap> GetPicture()
        {
            Bitmap result = null;
            var getImage = CurrentDevice.Device;

            Exception catched1 = null;
            Exception catched2 = null;
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
                        OnUnexpectedError?.Invoke(this, new AForgeEventArgs(ex));
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
                catched1 = ex1;
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
                    catched2 = ex2;
                }
            }

            if(catched1 != null || catched2 != null)
                throw new Exception($"Exception when get image. ImageStart=[{catched1?.Message}] ImageStop=[{catched2?.Message}]");

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

        public void StopAnyProcess()
        {
            StopCapturing();
        }

        void StopCapturing()
        {
            Exception catched1 = null;
            Exception catched2 = null;
            try
            {
                //if (FinalVideo != null && FinalVideo.IsRunning)
                _finalVideo?.Stop();
                _finalVideo = null;
            }
            catch (Exception ex)
            {
                catched1 = ex;
            }

            try
            {
                //FileWriter.Close();
                _aviWriter.Close();
            }
            catch (Exception ex)
            {
                catched2 = ex;
            }

            GC.Collect();
            Mode = AForgeCaptureMode.None;

            if (catched1 != null || catched2 != null)
            {
                throw new Exception($"Error when stopping process. CaptureStop=[{catched1?.Message}] WriterClose=[{catched2?.Message}]");
            }
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
                OnUnexpectedError?.Invoke(this, new AForgeEventArgs(ex));
            }
        }
    }
}
