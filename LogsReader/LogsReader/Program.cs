using System;
using System.Windows.Forms;

namespace LogsReader
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var mainForm  = new MainForm();
            mainForm.ApplySettings();
            Application.Run(mainForm);
        }

        public static void AssignValue<T>(this TextBox textBox, T value, EventHandler handler)
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

        public static void MessageShow(string msg, string caption)
        {
            MessageBox.Show(msg, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}