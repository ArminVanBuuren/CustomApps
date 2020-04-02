using System;
using System.Windows.Forms;

namespace LogsReader
{
    public static class Util
    {
        public static void AssignValue<T>(this Control textBox, T value, EventHandler handler)
        {
            try
            {
                textBox.TextChanged -= handler;
                textBox.Text = value.ToString();
            }
            catch (Exception)
            {
                //ignored
            }
            finally
            {
                textBox.TextChanged += handler;
            }
        }

        public static void MessageShow(string msg, string caption, bool isError = true)
        {
            MessageBox.Show(msg, caption, MessageBoxButtons.OK, isError ? MessageBoxIcon.Error : MessageBoxIcon.Asterisk);
        }
    }
}
