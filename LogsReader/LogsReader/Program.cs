using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

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
            Application.Run(new MainForm());
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
