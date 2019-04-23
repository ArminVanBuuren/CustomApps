using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video.VFW;

namespace Utils.WinForm.MediaCapture.Screen
{
    public class ScreenCapture : MediaCapture, IDisposable
    {
        readonly Rectangle screenBounds = System.Windows.Forms.Screen.GetBounds(Point.Empty);
        //readonly Rectangle outputBounds = new Rectangle(0, 0, 384, 216);

        private readonly IFrameWriter _frameWriter;

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

            string destinationFilePath = GetNewVideoFilePath(fileName, _frameWriter.VideoExtension);
            _frameWriter.Open(destinationFilePath, screenBounds.Width, screenBounds.Height);

            var asyncRec = new Action<string>(DoRecordingAsync).BeginInvoke(destinationFilePath,null, null);
        }

        void DoRecordingAsync(string destinationFilePath)
        {
            Mode = MediaCaptureMode.Recording;
            MediaCaptureEventArgs result = null;
            FramesInfo framesInfo = new FramesInfo(_frameWriter.FrameRate);
            //Queue queue = new Queue();

            try
            {
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

                    result = new MediaCaptureEventArgs(destinationFilePath);
                }
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
            return $"{base.ToString()}\r\nFrame=[{_frameWriter?.FrameRate}]";
        }
    }
}