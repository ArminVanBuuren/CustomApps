using System;
using System.Windows.Forms;
using SPAMessageSaloon.Common;

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
                ReportMessage.Show(ex.ToString(), MessageBoxIcon.Error, @"Run");
            }
        }
    }
}