using System;
using System.Drawing;
using AForge.Video.FFMPEG;
using Utils.Media.MediaCapture;

namespace Tester.WinForm
{
    public class MpegWriter : IFrameWriter, IDisposable
    {
        private VideoFileWriter _fileWriter;

        public int FrameRate { get; set; }
        public string VideoExtension { get; } = ".avi";

        public MpegWriter(int frameRate = 25)
        {
            _fileWriter = new VideoFileWriter();
            FrameRate = frameRate;
        }

        public void Open(string fileName, int width, int height)
        {
            Close();
            _fileWriter.Open(fileName, width, height, FrameRate, VideoCodec.MPEG4);
        }

        public void AddFrame(Bitmap frameImage)
        {
            if (_fileWriter.IsOpen)
                _fileWriter.WriteVideoFrame(frameImage);
        }

        public void Refresh()
        {
            Close();
            _fileWriter = new VideoFileWriter();
        }

        public void Close()
        {
            _fileWriter.Close();
        }

        public void Dispose()
        {
            Close();
            _fileWriter.Dispose();
        }
    }
}
