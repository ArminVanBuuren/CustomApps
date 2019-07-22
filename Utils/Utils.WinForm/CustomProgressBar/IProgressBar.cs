using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WinForm.CustomProgressBar
{
    public interface IProgressBar
    {
        bool Visible { get; set; }
        int Maximum { get; set; }
        int Value { get; set; }
    }
}
