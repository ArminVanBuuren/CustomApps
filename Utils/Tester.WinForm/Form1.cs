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
using Utils.WinForm.MediaCapture;

namespace Tester.WinForm
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            Process();
        }

        private CamCapture camp;
        void Process()
        {
            try
            {
                var aforgeDevices = new AForgeMediaDevices();
                var camDevices = new CamMediaDevices();

                camp = new CamCapture(aforgeDevices, camDevices, @"C:\VideoClips", 20);
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
