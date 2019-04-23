using System;
using System.Threading;
using NAudio.Wave;

namespace Utils.WinForm.MediaCapture.NAudio
{
    //NAudio library
    public class NAudioCapture : MediaCapture, IDisposable
    {
        readonly object sync = new object();
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
            lock (sync)
            {
                _waveFileWriter = new WaveFileWriter(destinationFilePath, _waveSource.WaveFormat);
            }

            _waveSource.StartRecording();

            var asyncRec = new Action<string>(DoRecordingAsync).BeginInvoke(destinationFilePath, null, null);
        }

        void DoRecordingAsync(string destinationFilePath)
        {
            Mode = MediaCaptureMode.Recording;

            DateTime startCapture = DateTime.Now;
            while (DateTime.Now.Subtract(startCapture).TotalSeconds < SecondsRecordDuration)
            {
                Thread.Sleep(100);
            }

            Stop();
            RecordCompleted(new MediaCaptureEventArgs(new [] {destinationFilePath}));
        }

        public override void Stop()
        {
            try
            {
                if (_waveSource != null)
                    _waveSource.StopRecording();
                else
                {
                    StopWriter();
                    Mode = MediaCaptureMode.None;
                }
            }
            catch (Exception)
            {
                // null
            }
        }

        void WaveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            lock (sync)
            {
                if (_waveFileWriter == null)
                    return;

                _waveFileWriter.Write(e.Buffer, 0, e.BytesRecorded);
                _waveFileWriter.Flush();
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

                StopWriter();
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

        void StopWriter()
        {
            lock (sync)
            {
                if (_waveFileWriter != null)
                {
                    _waveFileWriter.Close();
                    _waveFileWriter.Dispose();
                    _waveFileWriter = null;
                }
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
