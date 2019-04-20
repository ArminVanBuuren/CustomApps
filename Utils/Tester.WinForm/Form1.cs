using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
//using AForge.Video.FFMPEG;
using AForge.Video.VFW;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using Utils.WinForm.MediaCapture;
using Utils.WinForm.MediaCapture.AForge;
using Utils.WinForm.MediaCapture.Encoder;
using Utils.WinForm.MediaCapture.Screen;

namespace Tester.WinForm
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            //TestAForge test = new TestAForge();
            //test.button2_Click(this, EventArgs.Empty);

            InitializeComponent();
            
            var aforgeDevices = new AForgeMediaDevices();
            var camDevices = new EncoderMediaDevices();


            EncoderCaptureProcess(aforgeDevices, camDevices);
            //AForgeCaptureProcess(aforgeDevices);
            //ScreenCapture();


        }

        //private int count = 0;
        private AForgeCapture aforge;
        async void AForgeCaptureProcess(AForgeMediaDevices a)
        {
            aforge = new AForgeCapture(Thread.CurrentThread, a, @"E:\VideoClips", 30);
            //aforge.ChangeVideoDevice("test");

            aforge.OnRecordingCompleted += Aforge_OnRecordingCompleted;
            aforge.StartCamRecording();


            //DateTime startCapture = DateTime.Now;
            //while (DateTime.Now.Subtract(startCapture).TotalSeconds < 10)
            ////while (true)
            //{
            //    Bitmap pic = await ASYNC.ExecuteWithTimeoutAsync(aforge.GetPictureAsync(), 300); // 300 - самое оптимальное

            //    if (pic == null)
            //    {
            //        count++;
            //        //MessageBox.Show(@"Timeout");
            //    }
            //    else
            //    {
            //        pic.Save(Path.Combine(aforge.DestinationDir, STRING.RandomString(15) + ".png"), ImageFormat.Png);
            //    }
            //}

            //MessageBox.Show($"OK - {count}");
        }
        
        private async void Aforge_OnRecordingCompleted(object sender, MediaCaptureEventArgs args)
        {
            MessageBox.Show((args?.Error == null ? args?.DestinationFile : args?.Error.ToString()));
            Bitmap pic = await ASYNC.ExecuteWithTimeoutAsync(aforge.GetPictureAsync(), 1000);
            pic?.Save(Path.Combine(aforge.DestinationDir, STRING.RandomString(15) + ".png"), ImageFormat.Png);
        }

        private EncoderCapture camp;
        async void EncoderCaptureProcess(AForgeMediaDevices a, EncoderMediaDevices c)
        {
            try
            {
                camp = new EncoderCapture(Thread.CurrentThread, a, c, @"E:\VideoClips", 30);
                //camp.ChangeVideoDevice("test");
                //camp.ChangeAudioDevice("test");

                camp.OnRecordingCompleted += EncoderCaptureOnRecordingCompleted;

                labl1:
                try
                {
                    await camp.StartCamRecordingAsync();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    goto labl1;
                }


                //await Task.Delay(5000);

                //camp.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void EncoderCaptureOnRecordingCompleted(object sender, MediaCaptureEventArgs args)
        {
            MessageBox.Show(args?.Error == null ? args?.DestinationFile : args?.Error.ToString());
        }

        private ScreenCapture screen;
        void ScreenCapture()
        {
            //screen = new ScreenCapture(Thread.CurrentThread, @"E:\VideoClips", 30);
            screen.OnRecordingCompleted += Screen_OnRecordingCompleted;
            screen.StartCamRecording();
        }

        private void Screen_OnRecordingCompleted(object sender, MediaCaptureEventArgs args)
        {
            MessageBox.Show(args?.Error == null ? args?.DestinationFile : args?.Error.ToString());
        }
    }
}
