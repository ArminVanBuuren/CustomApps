using System.Windows.Forms;

namespace Utils.WinForm.CustomProgressBar
{
    public interface IProgressBar
    {
        Control ProgressBar { get; }
        bool Visible { get; set; }
        int Maximum { get; set; }
        int Value { get; set; }
    }
}
