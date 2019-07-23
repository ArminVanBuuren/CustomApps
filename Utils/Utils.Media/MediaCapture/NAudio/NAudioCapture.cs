using System;
using System.Threading;
using NAudio.Wave;

namespace Utils.Media.MediaCapture.NAudio
{
    //NAudio library
    public class NAudioCapture : Media.MediaCapture.MediaCapture, IDisposable
    {
        readonly object syncAudio = new object();
        public WaveInEvent _waveSource = null;
        public WaveFileWriter _waveFileWriter = null;

        public NAudioCapture(string destinationDir, int secondsRecDuration = 60) :base(destinationDir, secondsRecDuration)
        {

        }

        public override void StartRecording(string fileName = null)
        {
            if (Mode == MediaCaptureMode.Recording)
                throw new MediaCaptureRunningException("You must stop the previous process first!");

            _waveSource = new WaveInEvent
            {
                WaveFormat = new WaveFormat(44100, 1)
            };
            _waveSource.DataAvailable += WaveSource_DataAvailable;
            _waveSource.RecordingStopped += WaveSource_RecordingStopped;

            string destinationFilePath = GetNewVideoFilePath(fileName, ".wav");
            lock (syncAudio)
            {
                _waveFileWriter = new WaveFileWriter(destinationFilePath, _waveSource.WaveFormat);
            }

            var asyncRec = new Action<string>(DoRecordingAsync).BeginInvoke(destinationFilePath, null, null);
        }

        void DoRecordingAsync(string destinationFilePath)
        {
            MediaCaptureEventArgs result = null;
            Mode = MediaCaptureMode.Recording;

            try
            {
                _waveSource?.StartRecording();

                DateTime startCapture = DateTime.Now;
                while (DateTime.Now.Subtract(startCapture).TotalSeconds < SecondsRecordDuration)
                {
                    Thread.Sleep(100);
                }

                result = new MediaCaptureEventArgs(new[] { destinationFilePath });
            }
            catch (Exception ex)
            {
                result = new MediaCaptureEventArgs(new[] { destinationFilePath }, ex);
            }

            Stop();

            if (result?.Error != null)
                DeleteRecordedFile(new[] { destinationFilePath });

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
            finally
            {
                Mode = MediaCaptureMode.None;
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
