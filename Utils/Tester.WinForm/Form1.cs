using System;
using System.Windows.Forms;
//using AForge.Video.FFMPEG;
using System.Threading;
using Utils.Media.MediaCapture;
using Utils.Media.MediaCapture.AForge;
using Utils.Media.MediaCapture.Encoder;
using Utils.Media.MediaCapture.NAudio;
using Utils.Media.MediaCapture.Screen;

namespace Tester.WinForm
{
    public partial class Form1 : Form
    {
        private int completed = 0;
        public Form1()
        {
            //TestAForge test = new TestAForge();
            //test.button2_Click(this, EventArgs.Empty);

            InitializeComponent();
            
            //var aforgeDevices = new AForgeMediaDevices();
            //var camDevices = new EncoderMediaDevices();

            //while (true)
            //{
            //    completed = 0;
            //    //AForgeCaptureProcess(aforgeDevices);
            //    EncoderCaptureProcess(aforgeDevices, camDevices);
            //    ScreenCapture();
            //    while (completed < 2)
            //    {
            //        Thread.Sleep(1000);
            //    }
            //    Thread.Sleep(5000);
            //}

            //ScreenCapture();

            //NAudioCapture();

            //AForgeCaptureProcess(aforgeDevices);

            
        }

        //private int count = 0;
        private AForgeCapture aforge;
        async void AForgeCaptureProcess(AForgeMediaDevices a)
        {
            var frameWriter = new AviFrameWriter();
            //var frameWriter = new MpegWriter();
            aforge = new AForgeCapture(Thread.CurrentThread, a, frameWriter, @"E:\VideoClips", 10);
            //aforge.ChangeVideoDevice("test");

            aforge.OnRecordingCompleted += Aforge_OnRecordingCompleted;
            aforge.StartRecording();


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
        
        private void Aforge_OnRecordingCompleted(object sender, MediaCaptureEventArgs args)
        {
            completed++;

            if (args != null)
                MessageBox.Show(args.Error == null ? string.Join("\r\n ", args.FilesDestinations) : args.Error.ToString());

            //MessageBox.Show((args?.Error == null ? args?.DestinationFile : args?.Error.ToString()));
            //Bitmap pic = await ASYNC.ExecuteWithTimeoutAsync(aforge.GetPictureAsync(), 1000);
            //pic?.Save(Path.Combine(aforge.DestinationDir, STRING.RandomString(15) + ".png"), ImageFormat.Png);
        }

        private EncoderCapture camp;
        void EncoderCaptureProcess(AForgeMediaDevices a, EncoderMediaDevices c)
        {
            try
            {
                camp = new EncoderCapture(Thread.CurrentThread, a, c, @"E:\VideoClips", 30);
                //camp.ChangeVideoDevice("test");
                //camp.ChangeAudioDevice("test");

                camp.OnRecordingCompleted += EncoderCaptureOnRecordingCompleted;
                camp.StartRecording();

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
            completed++;

            if (args != null)
                MessageBox.Show(args.Error == null ? string.Join("\r\n ", args.FilesDestinations) : args.Error.ToString());
        }

        private ScreenCapture screen;
        void ScreenCapture()
        {
            //var frameWriter = new AviFrameWriter(7);
            //var frameWriter = new MpegWriter(13);
            var frameWriter = new MpegWriter(7);

            screen = new ScreenCapture(frameWriter, @"E:\VideoClips", 1200);
            screen.OnRecordingCompleted += Screen_OnRecordingCompleted;
            screen.StartRecording();
        }

        private void Screen_OnRecordingCompleted(object sender, MediaCaptureEventArgs args)
        {
            completed++;

            if (args != null)
                MessageBox.Show(args.Error == null ? string.Join("\r\n ", args.FilesDestinations) : args.Error.ToString());
        }

        private NAudioCapture naudio;
        void NAudioCapture()
        {
            naudio = new NAudioCapture(@"E:\VideoClips", 10);
            naudio.OnRecordingCompleted += Naudio_OnRecordingCompleted;
            naudio.StartRecording();
        }

        private void Naudio_OnRecordingCompleted(object sender, MediaCaptureEventArgs args)
        {
            if (args != null)
                MessageBox.Show(args.Error == null ? string.Join("\r\n ", args.FilesDestinations) : args.Error.ToString());
        }
    }
}
