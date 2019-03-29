using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//using Utils.WinForm.Notepad;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Video.FFMPEG;
using AForge.Video.VFW;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace Tester.WinForm
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection VideoCaptureDevices;

        private VideoCaptureDevice FinalVideo = null;
        private VideoCaptureDeviceForm captureDevice;
        private Bitmap video;
        //private AVIWriter AVIwriter;
        private VideoFileWriter FileWriter = new VideoFileWriter();
        private SaveFileDialog saveAvi;

        // MSVC - с компрессией; wmv3 - с компрессией но нужен кодек
        // MRLE
        // http://sundar1984.blogspot.com/2007_08_01_archive.html

        public Form1()
        {
            InitializeComponent();
            //AVIwriter = new AVIWriter("MSVC");
            this.Load += Form1_Load;
            this.FormClosing += Form1_FormClosing;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            VideoCaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            captureDevice = new VideoCaptureDeviceForm();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (captureDevice.ShowDialog(this) == DialogResult.OK)
            {
                FinalVideo = captureDevice.VideoDevice;
                FinalVideo.NewFrame += new NewFrameEventHandler(FinalVideo_NewFrame);
                FinalVideo.Start();
            }
        }

        void FinalVideo_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (butStop.Text == "Stop Record")
            {
                video = (Bitmap)eventArgs.Frame.Clone();
                video = ResizeImage(video, 320, 240);
                //pictureBox1.Image = (Bitmap)eventArgs.Frame.Clone();
                //AVIwriter.Quality = 0;
                FileWriter.WriteVideoFrame(video);
                //AVIwriter.AddFrame(video);
            }
            else
            {
                video = (Bitmap)eventArgs.Frame.Clone();
                //pictureBox1.Image = (Bitmap)eventArgs.Frame.Clone();
            }
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }


            //using (var mss = new MemoryStream())
            //{
            //    EncoderParameter qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 60L);
            //    ImageCodecInfo imageCodec = ImageCodecInfo.GetImageEncoders().FirstOrDefault(o => o.FormatID == ImageFormat.Jpeg.Guid);
            //    EncoderParameters parameters = new EncoderParameters(1);
            //    parameters.Param[0] = qualityParam;
            //    destImage.Save(mss, imageCodec, parameters);
            //    return (Bitmap)Image.FromStream(mss);
            //}

            return destImage;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            saveAvi = new SaveFileDialog();
            saveAvi.Filter = "Avi Files (*.avi)|*.avi";
            if (saveAvi.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Dictionary<string, FilterInfo> devices = new Dictionary<string, FilterInfo>();
                FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                foreach (FilterInfo dev in videoDevices)
                {
                    devices.Add(dev.Name, dev);
                }

                FilterInfo device = devices.FirstOrDefault().Value;
                VideoCaptureDevice VideoDevice = new VideoCaptureDevice(device.MonikerString);
                VideoCapabilities[] videoCapabilities = VideoDevice.VideoCapabilities;
                Dictionary<string, VideoCapabilities> videoCapabilitiesDictionary = new Dictionary<string, VideoCapabilities>();
                foreach (VideoCapabilities capabilty in videoCapabilities)
                {
                    string item = string.Format("{0} x {1}", capabilty.FrameSize.Width, capabilty.FrameSize.Height);

                    if (!videoCapabilitiesDictionary.ContainsKey(item))
                    {
                        videoCapabilitiesDictionary.Add(item, capabilty);
                    }
                }
                //VideoCapabilities caps = videoCapabilitiesDictionary.FirstOrDefault().Value;
                VideoCapabilities caps = videoCapabilitiesDictionary["320 x 240"];


                int h = caps.FrameSize.Height;
                int w = caps.FrameSize.Width;
                FileWriter.Open(saveAvi.FileName, w, h, 25, VideoCodec.Default, 5000000);

                //AVIwriter.Open(saveAvi.FileName, w, h);
                butStop.Text = "Stop Record";
                FinalVideo = VideoDevice;
                FinalVideo.NewFrame += new NewFrameEventHandler(FinalVideo_NewFrame);
                FinalVideo.Start();
            }
        }

        private void butStop_Click(object sender, EventArgs e)
        {
            if (butStop.Text == "Stop Record")
            {
                butStop.Text = "Stop";
                if (FinalVideo == null)
                { return; }
                if (FinalVideo.IsRunning)
                {
                    this.FinalVideo.Stop();
                    FileWriter.Close();
                    //this.AVIwriter.Close();
                    pictureBox1.Image = null;
                }
            }
            else
            {
                this.FinalVideo.Stop();
                FileWriter.Close();
                //this.AVIwriter.Close();
                pictureBox1.Image = null;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            pictureBox1.Image.Save("IMG" + DateTime.Now.ToString("hhmmss") + ".jpg");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (FinalVideo == null)
            { return; }
            if (FinalVideo.IsRunning)
            {
                this.FinalVideo.Stop();
                FileWriter.Close();
                //this.AVIwriter.Close();
            }
        }
    }
}
