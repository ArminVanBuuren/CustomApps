using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video.VFW;

namespace Utils.WinForm.MediaCapture
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
