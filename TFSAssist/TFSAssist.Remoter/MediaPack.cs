using System;
using System.Drawing.Imaging;
using System.Threading;
using Utils;
using Utils.Media.MediaCapture;
using Utils.Media.MediaCapture.AForge;
using Utils.Media.MediaCapture.Encoder;
using Utils.Media.MediaCapture.NAudio;
using Utils.Media.MediaCapture.Screen;

namespace TFSAssist.Remoter
{
    delegate void ProcessingCompleteHandler(object sender, string[] fileDestinations);
    delegate void ProcessingErrorHandler(string log);
    class MediaPack : IDisposable
    {
        readonly object _rootLock = new object();

        private readonly Thread _mainThread;
        private readonly string _projectDirPath;

        private AForgeMediaDevices _aforgeDevices;
        private AForgeCapture _aforgeCapture;
        private IFrameWriter _aforgeWriter;

        private EncoderMediaDevices _encDevices;
        private EncoderCapture _encoderCapture;

        private ScreenCapture _screenCapture;
        private IFrameWriter _screenWriter;

        private NAudioCapture _naudioCapture;

        public event ProcessingCompleteHandler OnCompleted;
        public event ProcessingErrorHandler OnProcessingExceptions;

        private int _countOfPlannedAforge = 0;
        private int _countOfPlannedEncoder = 0;
        private int _countOfPlannedScreen = 0;
        private int _countOfPlannedNAudio = 0;

        public bool IsBusy => !((_aforgeCapture == null || _aforgeCapture.Mode == MediaCaptureMode.None)
                              && (_encoderCapture == null || _encoderCapture.Mode == MediaCaptureMode.None)
                              && (_screenCapture == null || _screenCapture.Mode == MediaCaptureMode.None)
                              && (_naudioCapture == null || _naudioCapture.Mode == MediaCaptureMode.None));

        public MediaPack(Thread mainThread, string projectDirPath)
        {
            _mainThread = mainThread;
            _projectDirPath = projectDirPath;
        }

        public void Initialize()
        {
            try
            {
                try
                {
                    _aforgeWriter = new FFMPEGWriter();
                }
                catch (Exception ex)
                {
                    WriteExLog(ex);
                    _aforgeWriter = new AviFrameWriter();
                }

                _aforgeDevices = new AForgeMediaDevices();
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


            try
            {
                try
                {
                    _screenWriter = new FFMPEGWriter(13);
                }
                catch (Exception ex)
                {
                    WriteExLog(ex);
                    _screenWriter = new AviFrameWriter(7);
                }

                _screenCapture = new ScreenCapture(_screenWriter, _projectDirPath, 60);
                _screenCapture.OnRecordingCompleted += OnRecordingCompleted;
            }
            catch (Exception ex)
            {
                WriteExLog(ex);
            }


            try
            {
                _naudioCapture = new NAudioCapture(_projectDirPath, 60);
                _naudioCapture.OnRecordingCompleted += OnRecordingCompleted;
            }
            catch (Exception ex)
            {
                WriteExLog(ex);
            }
        }

        public void StartAForge()
        {
            lock (_rootLock)
            {
                if (!IsBusy)
                {
                    _aforgeCapture.StartRecording();
                }
                else
                {
                    _countOfPlannedAforge++;
                }
            }
        }

        public void StartEncoder()
        {
            lock (_rootLock)
            {
                if (!IsBusy)
                {
                    _encoderCapture.StartRecording();
                }
                else
                {
                    _countOfPlannedEncoder++;
                }
            }
        }

        public void StartBroadcast(int port = 8080)
        {
            lock (_rootLock)
            {
                if (!IsBusy)
                {
                    _encoderCapture.StartBroadcast(port);
                }
            }
        }

        public void StartScreen()
        {
            lock (_rootLock)
            {
                if (!IsBusy)
                {
                    _screenCapture.StartRecording();
                }
                else
                {
                    _countOfPlannedScreen++;
                }
            }
        }

        public void StartNAudio()
        {
            lock (_rootLock)
            {
                if (!IsBusy)
                {
                    _naudioCapture.StartRecording();
                }
                else
                {
                    _countOfPlannedNAudio++;
                }
            }
        }

        private void OnRecordingCompleted(object sender, MediaCaptureEventArgs args)
        {
            try
            {
                if (args == null)
                    return;

                if (args.Error != null)
                    WriteExLog(args.Error);

                if (args.FilesDestinations == null || args.FilesDestinations.Length == 0)
                    return;


                OnCompleted?.Invoke(this, args.FilesDestinations);

                if (_countOfPlannedAforge > 0)
                {
                    _countOfPlannedAforge--;
                    StartAForge();
                }
                else if (_countOfPlannedEncoder > 0)
                {
                    _countOfPlannedEncoder--;
                    StartEncoder();
                }
                else if(_countOfPlannedScreen > 0)
                {
                    _countOfPlannedScreen--;
                    StartScreen();
                }
                else if (_countOfPlannedNAudio > 0)
                {
                    _countOfPlannedNAudio--;
                    StartNAudio();
                }
            }
            catch (Exception e)
            {
                // ignored
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

        public bool GetScreen(string imagePath)
        {
            ScreenCapture.Capture(imagePath, ImageFormat.Png);
            return true;
        }

        public bool GetPhoto(string imagePath)
        {
            if (_aforgeCapture == null || _aforgeCapture.Mode != MediaCaptureMode.None)
                return false;

            var task = _aforgeCapture.GetPictureAsync();
            if (task.Wait(20000))
            {
                var photo = task.Result;
                photo?.Save(imagePath, ImageFormat.Png);
            }

            return true;
        }

        void WriteExLog(string log)
        {
            OnProcessingExceptions?.Invoke(log);
        }

        void WriteExLog(Exception log)
        {
            OnProcessingExceptions?.Invoke(log.ToString());
        }

        public override string ToString()
        {
            var currentDevices = string.Empty;
            var resultCamInfo = string.Empty;

            if (_aforgeDevices != null)
            {
                if (_aforgeCapture != null)
                    currentDevices = $"AForge:\r\n{_aforgeCapture}";
                resultCamInfo = _aforgeDevices.ToString();
            }

            if (_encDevices != null)
            {
                if (_encoderCapture != null)
                    currentDevices = currentDevices + $"\r\n\r\nEncoder:\r\n{_encoderCapture}";
                resultCamInfo = resultCamInfo + "\r\n\r\n" + _encDevices;
            }

            if (resultCamInfo.IsNullOrEmptyTrim())
                resultCamInfo = "No cam device found.";
            else
                resultCamInfo = currentDevices.Trim() + "\r\n===================\r\n" + resultCamInfo;

            resultCamInfo = resultCamInfo + "\r\n===================\r\n";

            resultCamInfo = resultCamInfo + "Screen:\r\n" + _screenCapture;

            resultCamInfo = resultCamInfo + "\r\n===================\r\n";

            resultCamInfo = resultCamInfo + "NAudio:\r\n" + _naudioCapture;

            resultCamInfo = resultCamInfo.Trim();

            return resultCamInfo;
        }

        public void Stop()
        {
            _aforgeCapture?.Stop();
            _encoderCapture?.Stop();
            _screenCapture?.Stop();
            _naudioCapture?.Stop();
        }

        public void Dispose()
        {
            _countOfPlannedAforge = 0;
            _countOfPlannedEncoder = 0;
            _countOfPlannedScreen = 0;
            _countOfPlannedNAudio = 0;
            Stop();

            _aforgeCapture?.Dispose();
            _encoderCapture?.Dispose();
            _screenCapture?.Dispose();
            _naudioCapture?.Dispose();
        }
    }
}