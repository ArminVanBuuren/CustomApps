using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utils.WinForm.CustomProgressBar
{
    public static class ProgressBarUtils
    {
        public static void SetProgressNoAnimation(this IProgressBar pb, int currentValue)
        {
            // To get around the progressive animation, we need to move the progress bar backwards.
            if (currentValue == pb.Maximum)
            {
                // Special case as value can't be set greater than Maximum.
                pb.Maximum = currentValue + 1;     // Temporarily Increase Maximum
                pb.Value = currentValue + 1;       // Move past
                pb.Maximum = currentValue;         // Reset maximum
            }
            else
            {
                pb.Value = currentValue + 1;       // Move past
            }
            pb.Value = currentValue; // Move to correct value
        }

        public static void SetProgressNoAnimation(this ProgressBar pb, int currentValue)
        {
            // To get around the progressive animation, we need to move the progress bar backwards.
            if (currentValue == pb.Maximum)
            {
                // Special case as value can't be set greater than Maximum.
                pb.Maximum = currentValue + 1;     // Temporarily Increase Maximum
                pb.Value = currentValue + 1;       // Move past
                pb.Maximum = currentValue;         // Reset maximum
            }
            else
            {
                pb.Value = currentValue + 1;       // Move past
            }
            pb.Value = currentValue; // Move to correct value
        }
    }
}
