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
            InitializeComponent();
            
            var aforgeDevices = new AForgeMediaDevices();
            var camDevices = new EncoderMediaDevices();

            //CamCaptureProcess(aforgeDevices, camDevices);
            AForgeCaptureProcess(aforgeDevices, camDevices);
        }

        private AForgeCapture aforge;
        async void AForgeCaptureProcess(AForgeMediaDevices a, EncoderMediaDevices c)
        {
            aforge = new AForgeCapture(a, c, @"C:\VideoClips", 20);
            aforge.OnRecordingCompleted += Aforge_OnRecordingCompleted;

            DateTime startCapture = DateTime.Now;
            while (DateTime.Now.Subtract(startCapture).TotalSeconds < 60)
            //while (true)
            {
                var pic = await ASYNC.ExecuteWithTimeoutAsync(aforge.GetPicture(), 10000);
                
                if (pic == null)
                {
                    MessageBox.Show(@"Timeout");
                }
                else
                {
                    pic.Save(Path.Combine(aforge.DestinationDir, STRING.RandomString(15) + ".png"), ImageFormat.Png);
                }
            }

            MessageBox.Show(@"OK");
        }

        async Task Test()
        {
            await Task.Delay(1000);
        }

        private void Aforge_OnRecordingCompleted(object sender, MediaCaptureEventArgs args)
        {
            if (args.Error != null)
                MessageBox.Show(args.Error.ToString());
        }

        private EncoderCapture camp;
        void CamCaptureProcess(AForgeMediaDevices a, EncoderMediaDevices c)
        {
            try
            {
                camp = new EncoderCapture(a, c, @"C:\VideoClips", 60);
                camp.OnRecordingCompleted += Camp_OnRecordingCompleted;
                camp.StartCamRecording();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Camp_OnRecordingCompleted(object sender, MediaCaptureEventArgs args)
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
