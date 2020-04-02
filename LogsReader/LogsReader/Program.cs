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
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var mainForm = new MainForm();
                mainForm.ApplySettings();
                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                Util.MessageShow(ex.ToString(), @"Run");
            }
        }
    }
}