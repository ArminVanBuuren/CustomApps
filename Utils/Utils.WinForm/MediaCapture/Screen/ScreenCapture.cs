using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video.VFW;
using NAudio.Wave;

namespace Utils.WinForm.MediaCapture.Screen
{
    public class ScreenCapture : MediaCapture, IDisposable
    {
        private readonly object syncAudio = new object();
        readonly Rectangle screenBounds = System.Windows.Forms.Screen.GetBounds(Point.Empty);
        //readonly Rectangle outputBounds = new Rectangle(0, 0, 384, 216);

        private readonly IFrameWriter _frameWriter;

        public WaveInEvent _waveSource = null;
        public WaveFileWriter _waveFileWriter = null;

        public ScreenCapture(string destinationDir, int secondsRecDuration = 60) : this(new AviFrameWriter(), destinationDir, secondsRecDuration)
        {
            
        }

        public ScreenCapture(IFrameWriter frameWriter, string destinationDir, int secondsRecDuration = 60) : base(destinationDir, secondsRecDuration)
        {
            _frameWriter = frameWriter;
        }

        public override void StartRecording(string fileName = null)
        {
            if (Mode == MediaCaptureMode.Recording)
                throw new MediaCaptureRunningException("You must stop the previous process first!");

            string commonDestination = Path.Combine(DestinationDir, $"{DateTime.Now:ddHHmmss}_{STRING.RandomString(15)}");

            // video
            string destinationVideoPath = commonDestination + _frameWriter.VideoExtension;
            _frameWriter.Open(destinationVideoPath, screenBounds.Width, screenBounds.Height);

            // audio
            string destinationAudioPath = commonDestination + ".wav";
            try
            {
                _waveSource = new WaveInEvent
                {
                    WaveFormat = new WaveFormat(44100, 1)
                };
                _waveSource.DataAvailable += WaveSource_DataAvailable;
                _waveSource.RecordingStopped += WaveSource_RecordingStopped;


                lock (syncAudio)
                {
                    _waveFileWriter = new WaveFileWriter(destinationAudioPath, _waveSource.WaveFormat);
                }
            }
            catch (Exception)
            {
                // ignored
            }

            var asyncRec = new Action<string, string>(DoRecordingAsync).BeginInvoke(destinationVideoPath, destinationAudioPath, null, null);
        }

        void DoRecordingAsync(string destinationVideo, string destinationAudio)
        {
            Mode = MediaCaptureMode.Recording;
            MediaCaptureEventArgs result = null;
            FramesInfo framesInfo = new FramesInfo(_frameWriter.FrameRate);
            //Queue queue = new Queue();

            try
            {
                _waveSource?.StartRecording();

                using (framesInfo)
                {
                    DateTime startCapture = DateTime.Now;
                    bool isContinue = true;

                    while (isContinue)
                    {
                        framesInfo.InhibitStart();

                        if (Mode == MediaCaptureMode.None)
                            break;

                        using (Bitmap bitmap = new Bitmap(screenBounds.Width, screenBounds.Height))
                        {
                            using (Graphics g = Graphics.FromImage(bitmap))
                            {
                                g.CopyFromScreen(Point.Empty, Point.Empty, screenBounds.Size);
                            }

                            // очень долго - нет смысла делатьй ресайз
                            //using (Bitmap resizedBitmap = bitmap.ResizeImage(outputBounds.Width, outputBounds.Height))
                            //{
                            //    _frameWriter.AddFrame(resizedBitmap);
                            //}

                            _frameWriter.AddFrame(bitmap);
                            framesInfo.PlusFrame();
                        }

                        isContinue = DateTime.Now.Subtract(startCapture).TotalSeconds < SecondsRecordDuration;

                        framesInfo.InhibitStop();
                    }

                    result = new MediaCaptureEventArgs(new []{ destinationVideo, destinationAudio });
                }
            }
            catch (Exception ex)
            {
                result = new MediaCaptureEventArgs(new[] { destinationVideo, destinationAudio }, ex);
            }

            Stop();

            if (result?.Error != null)
                DeleteRecordedFile(new[] { destinationVideo, destinationAudio });

            RecordCompleted(result);
        }

        void WaveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            lock (syncAudio)
            {
                if (_waveFileWriter == null)
                    return;

                _waveFileWriter.Write(e.Buffer, 0, e.BytesRecorded);
                _waveFileWriter.Flush();
            }
        }

        public override void Stop()
        {
            try
            {
                if (_waveSource != null)
                    _waveSource.StopRecording();
                else
                    StopAudioWriter();
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                _frameWriter?.Close();
            }
            catch (Exception)
            {
                // null
            }
            finally
            {
                Mode = MediaCaptureMode.None;
            }
        }

        void WaveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            try
            {
                if (_waveSource != null)
                {
                    _waveSource.DataAvailable -= WaveSource_DataAvailable;
                    _waveSource.RecordingStopped -= WaveSource_RecordingStopped;
                    _waveSource.Dispose();
                    _waveSource = null;
                }

                StopAudioWriter();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        void StopAudioWriter()
        {
            lock (syncAudio)
            {
                if (_waveFileWriter == null)
                    return;

                _waveFileWriter.Close();
                _waveFileWriter.Dispose();
                _waveFileWriter = null;
            }
        }

        #region Static Methods

        public static Task CaptureAsync(string destinationPath, ImageFormat format)
        {
            return Task.Run(() =>
            {
                try
                {
                    Capture(destinationPath, format);
                }
                catch (Exception)
                {
                    // null
                }
            });
        }

        public static void Capture(string destinationPath, ImageFormat format)
        {
            Rectangle bounds = System.Windows.Forms.Screen.GetBounds(Point.Empty);
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                }

                bitmap.Save(destinationPath, format);
            }
        }

        public static void Capture(Form form, string destinationPath, ImageFormat format)
        {
            Rectangle bounds = form.Bounds;
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
                }

                bitmap.Save(destinationPath, format);
            }
        }

        #endregion

        public void Dispose()
        {
            Stop();
        }

        public override string ToString()
        {
            return $"{base.ToString()}\r\n{_frameWriter?.ToString()}".Trim();
        }
    }
}