using System;
using System.Windows.Forms;

namespace DjSetCutter
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
            Application.Run(new Form1());
        }

        public static bool IsNullOrEmpty(this string data)
        {
            return string.IsNullOrEmpty(data);
        }
    }
}
