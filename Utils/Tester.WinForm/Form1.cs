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
            var camDevices = new CamMediaDevices();

            //CamCaptureProcess(aforgeDevices, camDevices);
            AForgeCaptureProcess(aforgeDevices, camDevices);
        }

        private AForgeCapture aforge;
        async void AForgeCaptureProcess(AForgeMediaDevices a, CamMediaDevices c)
        {
            aforge = new AForgeCapture(a, c, @"C:\VideoClips", 20);
            aforge.OnRecordingCompleted += Aforge_OnRecordingCompleted;

            DateTime startCapture = DateTime.Now;
            while (DateTime.Now.Subtract(startCapture).TotalSeconds < 60)
            //while (true)
            {
                var pic = await aforge.GetPicture();
                pic?.Save(Path.Combine(aforge.DestinationDir, STRING.RandomString(15) + ".png"), ImageFormat.Png);
                await Task.Delay(100);
            }

            MessageBox.Show("OK");
        }

        private void Aforge_OnRecordingCompleted(object sender, MediaCaptureEventArgs args)
        {
            if (args.Error != null)
                MessageBox.Show(args.Error.ToString());
        }

        private CamCapture camp;
        void CamCaptureProcess(AForgeMediaDevices a, CamMediaDevices c)
        {
            try
            {
                camp = new CamCapture(a, c, @"C:\VideoClips", 20);
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
