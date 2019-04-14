using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video.VFW;

namespace Utils.WinForm.MediaCapture
{
    public class ScreenCapture : MediaCapture, IDisposable
    {
        Rectangle bounds = Screen.GetBounds(Point.Empty);
        //Rectangle bounds = new Rectangle(0, 0, 720, 480);

        private readonly AVIWriter _aviWriter;

        public ScreenCapture(Thread mainThread, string destinationDir, int secondsRecDuration) : base(mainThread, destinationDir, secondsRecDuration)
        {
            _aviWriter = new AVIWriter("MSVC");
        }

        public override void StartCamRecording(string fileName = null)
        {
            if (Mode == MediaCaptureMode.Recording)
                throw new MediaCaptureRunningException("You must stop the previous process first!");

            
            string destinationFilePath = GetNewVideoFilePath(fileName, ".avi");
            _aviWriter.Open(destinationFilePath, bounds.Width, bounds.Height);
            _aviWriter.Quality = 0;

            var asyncRec = new Action<string>(DoRecordingAsync);
            asyncRec.BeginInvoke(destinationFilePath, null, null);
        }

        void DoRecordingAsync(string destinationFilePath)
        {
            Mode = MediaCaptureMode.Recording;
            MediaCaptureEventArgs result = null;
            int count = 0;
            try
            {
                DateTime startCapture = DateTime.Now;
                Stopwatch watcher = new Stopwatch();
                
                while (DateTime.Now.Subtract(startCapture).TotalSeconds < SecondsRecordDuration)
                {
                    
                    

                    if (!MainThread.IsAlive)
                    {
                        DeleteRecordedFile(destinationFilePath, true);
                        Stop();
                        return;
                    }

                    if (Mode == MediaCaptureMode.None)
                        break;


                    try
                    {
                        watcher.Start();

                        Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);
                        Capture(ref bitmap, bounds);

                        using (bitmap)
                        {
                            //wather.Start();
                            //while (wather.ElapsedMilliseconds < 40)
                            //{
                            //    _aviWriter.Quality = 0;
                            //    _aviWriter.AddFrame(bitmap);
                            //}
                            //wather.Stop();

                            
                            _aviWriter.AddFrame(bitmap);
                            count++;


                            watcher.Stop();

                            if (watcher.ElapsedMilliseconds < 33)
                            {
                                Thread.Sleep(33 - (int) watcher.ElapsedMilliseconds);
                            }
                            else if (watcher.ElapsedMilliseconds > 33)
                            {
                                Stopwatch watcherInternal = new Stopwatch();
                                var brakes = watcher.ElapsedMilliseconds - 33;
                                while (brakes > 16)
                                {
                                    watcherInternal.Start();

                                    var clone = (Bitmap)bitmap.Clone();
                                    _aviWriter.AddFrame(clone);
                                    clone.Dispose();
                                    count++;

                                    watcherInternal.Stop();

                                    brakes = brakes - watcherInternal.ElapsedMilliseconds - 33;

                                    watcherInternal.Reset();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        watcher.Stop();
                    }
                    finally
                    {
                        watcher.Reset();
                    }
                }

                result = new MediaCaptureEventArgs(destinationFilePath);
            }
            catch (Exception ex)
            {
                result = new MediaCaptureEventArgs(ex);
            }

            Stop();

            if (result?.Error != null)
                DeleteRecordedFile(destinationFilePath);

            RecordCompleted(result);
        }

        public override void Stop()
        {
            try
            {
                _aviWriter?.Close();
            }
            catch (Exception)
            {
                // null
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
            Rectangle bounds = Screen.GetBounds(Point.Empty);
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

        public static void Capture(ref Bitmap result, Rectangle bounds)
        {
            using (Graphics g = Graphics.FromImage(result))
            {
                g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
            }
        }

        #endregion

        public void Dispose()
        {

        }
    }
}