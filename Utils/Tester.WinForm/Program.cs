using System;
using System.Windows.Forms;
using Utils.WinForm.Notepad;

namespace Tester.WinForm
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

            try
            {
                Application.Run(new XmlNotepad(@"C:\Users\vhovanskij\Desktop\2018.09.06\ROBP\Processes\Create Subscriber in All platform.xml"));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            
        }
    }
}
