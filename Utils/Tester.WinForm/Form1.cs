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
using System.Threading.Tasks;
using Utils;
using Utils.WinForm.MediaCapture;

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

            //EncoderCaptureProcess(aforgeDevices, camDevices);
            AForgeCaptureProcess(aforgeDevices, camDevices);

           
        }

        private AForgeCapture aforge;
        async void AForgeCaptureProcess(AForgeMediaDevices a, EncoderMediaDevices c)
        {
            aforge = new AForgeCapture(a, c, @"C:\VideoClips", 20);
            aforge.OnRecordingCompleted += Aforge_OnRecordingCompleted;
            //aforge.StartCamRecording();


            DateTime startCapture = DateTime.Now;
            while (DateTime.Now.Subtract(startCapture).TotalSeconds < 10)
            //while (true)
            {
                Bitmap pic = await ASYNC.ExecuteWithTimeoutAsync(aforge.GetPictureAsync(), 300); // 300 - самое оптимальное
                
                if (pic == null)
                {
                    count++;
                    //MessageBox.Show(@"Timeout");
                }
                else
                {
                    pic.Save(Path.Combine(aforge.DestinationDir, STRING.RandomString(15) + ".png"), ImageFormat.Png);
                }
            }

            MessageBox.Show($"OK - {count}");
        }

        async Task Test()
        {
            await Task.Delay(1000);
        }

        private int count = 0;
        private void Aforge_OnRecordingCompleted(object sender, MediaCaptureEventArgs args)
        {
            if (args.Error != null)
            {
                MessageBox.Show(args.Error.ToString());
            }
        }

        private EncoderCapture camp;
        async void EncoderCaptureProcess(AForgeMediaDevices a, EncoderMediaDevices c)
        {
            try
            {
                camp = new EncoderCapture(a, c, @"C:\VideoClips", 30);
                camp.OnRecordingCompleted += EncoderCaptureOnRecordingCompleted;
                camp.StartCamRecording();

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
            if (args.Error != null)
            {
                MessageBox.Show(args.Error.Message);
                camp?.StartCamRecording();
            }
            else
                MessageBox.Show($"Competed - {args?.DestinationFile}");
        }

    }
}
