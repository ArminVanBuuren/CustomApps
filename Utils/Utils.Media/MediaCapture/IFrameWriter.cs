using System.Drawing;

namespace Utils.Media.MediaCapture
{
    public interface IFrameWriter
    {
        int FrameRate { get; set; }
        string VideoExtension { get; }
        void Open(string fileName, int width, int height);
        void AddFrame(Bitmap frameImage);
        void Refresh();
        void Close();
    }
}
