using System;
using System.Drawing;
using AForge.Video.VFW;

namespace Utils.Media.MediaCapture
{
    public class AviFrameWriter : IFrameWriter, IDisposable
    {
        private readonly string _codec;
        private AVIWriter _aviWriter;

        public int FrameRate
        {
            get => _aviWriter.FrameRate;
            set => _aviWriter.FrameRate = value;
        }

        public string VideoExtension { get; } = ".avi";

        // MSVC - с компрессией; wmv3 - с компрессией но нужен кодек
        // MRLE
        // http://sundar1984.blogspot.com/2007_08_01_archive.html

        /// <summary>
        /// Для того чтобы установить без компрессии, то должно быть значение [codec = "NON_COMPRESSION"]
        /// </summary>
        /// <param name="codec"></param>
        /// <param name="frameRate"></param>
        public AviFrameWriter(int frameRate = 25, string codec = "MSVC")
        {
            _codec = codec;
            Init(frameRate);
        }

        void Init(int frameRate)
        {
            _aviWriter = _codec == "NON_COMPRESSION" ? new AVIWriter() : new AVIWriter(_codec);
            _aviWriter.FrameRate = frameRate;
        }

        public void Open(string fileName, int width, int height)
        {
            _aviWriter.Open(fileName, width, height);
            _aviWriter.Quality = 0;
        }

        public void AddFrame(Bitmap frameImage)
        {
            _aviWriter.AddFrame(frameImage);
        }

        public void Refresh()
        {
            Close();
            Init(_aviWriter.FrameRate);
        }

        public void Close()
        {
            _aviWriter.Close();
        }

        public override string ToString()
        {
            return $"Avi-FrameRate=[{FrameRate}]";
        }

        public void Dispose()
        {
            Close();
            _aviWriter.Dispose();
        }
    }
}
