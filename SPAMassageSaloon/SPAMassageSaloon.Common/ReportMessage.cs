using System;
using System.Windows.Forms;

namespace SPAMassageSaloon.Common
{
    public static class ReportMessage
    {
        public static void Show(Exception exception)
	        => Show(exception.ToString());

        public static void Show(string message, MessageBoxIcon type = MessageBoxIcon.Error, string caption = null, bool isForm = true)
        {
            if (isForm)
            {
                int num = (int)MessageBox.Show(message, caption ?? type.ToString("G"), MessageBoxButtons.OK, type);
            }
            else
            {
                int num = (int)MessageBox.Show(message, caption ?? type.ToString("G"), MessageBoxButtons.OK, type, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }
    }
}
