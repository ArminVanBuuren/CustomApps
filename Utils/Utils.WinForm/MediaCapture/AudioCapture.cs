using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WinForm.MediaCapture
{
    //NAudio library
    public class AudioCapture
    {
        //WaveIn sourceStream;
        //WaveFileWriter waveWriter;
        //readonly string FilePath;
        //readonly string FileName;
        //readonly int InputDeviceIndex;

        //public AudioCapture(int inputDeviceIndex, string filePath, string fileName)
        //{
        //    this.InputDeviceIndex = inputDeviceIndex;
        //    this.FileName = fileName;
        //    this.FilePath = filePath;
        //}

        //public void StartRecording(object sender, EventArgs e)
        //{
        //    sourceStream = new WaveIn
        //    {
        //        DeviceNumber = this.InputDeviceIndex,
        //        WaveFormat =
        //            new WaveFormat(44100, WaveIn.GetCapabilities(this.InputDeviceIndex).Channels)
        //    };

        //    sourceStream.DataAvailable += this.SourceStreamDataAvailable;

        //    if (!Directory.Exists(FilePath))
        //    {
        //        Directory.CreateDirectory(FilePath);
        //    }

        //    waveWriter = new WaveFileWriter(FilePath + FileName, sourceStream.WaveFormat);
        //    sourceStream.StartRecording();
        //}

        //void SourceStreamDataAvailable(object sender, WaveInEventArgs e)
        //{
        //    if (waveWriter == null) return;
        //    waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
        //    waveWriter.Flush();
        //}

        //private void RecordEnd(object sender, EventArgs e)
        //{
        //    if (sourceStream != null)
        //    {
        //        sourceStream.StopRecording();
        //        sourceStream.Dispose();
        //        sourceStream = null;
        //    }
        //    if (this.waveWriter == null)
        //    {
        //        return;
        //    }
        //    this.waveWriter.Dispose();
        //    this.waveWriter = null;
        //    recordEndButton.Enabled = false;
        //}
    }
}
