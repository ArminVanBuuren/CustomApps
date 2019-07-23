using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;

namespace Utils.Media.MediaCapture.Screen
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

            var commonDestination = Path.Combine(DestinationDir, $"{DateTime.Now:ddHHmmss}_{STRING.RandomString(15)}");

            // video
            _frameWriter.Refresh();
            var destinationVideoPath = commonDestination + _frameWriter.VideoExtension;
            _frameWriter.Open(destinationVideoPath, screenBounds.Width, screenBounds.Height);

            // audio
            var destinationAudioPath = commonDestination + ".wav";
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

            new Action<string, string>(DoRecordingAsync).BeginInvoke(destinationVideoPath, destinationAudioPath, null, null);
        }

        void DoRecordingAsync(string destinationVideo, string destinationAudio)
        {
            MediaCaptureEventArgs result = null;
            Mode = MediaCaptureMode.Recording;

            try
            {
                try
                {
                    _waveSource?.StartRecording();
                }
                catch (Exception)
                {
                    // ignored
                }

                using (var framesInfo = new FramesInfo(_frameWriter.FrameRate))
                {
                    var startCapture = DateTime.Now;
                    var isContinue = true;

                    while (isContinue)
                    {
                        framesInfo.InhibitStart();

                        if (Mode == MediaCaptureMode.None)
                            break;

                        using (var bitmap = new Bitmap(screenBounds.Width, screenBounds.Height))
                        {
                            using (var g = Graphics.FromImage(bitmap))
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

            if (result.Error != null)
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
                _waveSource?.StopRecording();

                lock (syncAudio)
                {
                    if (_waveFileWriter == null)
                        return;

                    _waveFileWriter.Close();
                    _waveFileWriter.Dispose();
                    _waveFileWriter = null;
                }
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
                if (_waveSource == null)
                    return;

                _waveSource.DataAvailable -= WaveSource_DataAvailable;
                _waveSource.RecordingStopped -= WaveSource_RecordingStopped;
                _waveSource.Dispose();
                _waveSource = null;
            }
            catch (Exception)
            {
                // ignored
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
            var bounds = System.Windows.Forms.Screen.GetBounds(Point.Empty);
            using (var bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                }

                bitmap.Save(destinationPath, format);
            }
        }

        public static void Capture(Form form, string destinationPath, ImageFormat format)
        {
            var bounds = form.Bounds;
            using (var bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (var g = Graphics.FromImage(bitmap))
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
            return $"{base.ToString()}\r\n{_frameWriter}".Trim();
        }
    }
}