using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WinForm.AForge
{
    public delegate void AForgeEventHandler(object sender, AForgeEventArgs args);
    public class AForgeEventArgs : EventArgs
    {
        public bool Result { get; }
        public Exception Error { get; }

        internal AForgeEventArgs(Exception ex, bool result = false)
        {
            Error = ex;
            Result = result;
        }
    }
}
