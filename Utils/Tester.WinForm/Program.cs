using System;
using System.Windows.Forms;
//using Utils.WinForm.Notepad;

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
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            catch (Exception e)
            {
                Exception ex = e;
                string exMess = ex?.ToString();
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    exMess += "\r\n" + ex?.ToString();
                }

                MessageBox.Show(exMess);
            }


            //try
            //{
            //    Application.Run(new XmlNotepad(@"C:\Users\vhovanskij\Desktop\2018.09.06\ROBP\Processes\test.xml"));
            //    //Application.Run(new XmlNotepad(@"C:\@MyRepos\FORIS_WORK\UZ-REPOS\SPA\PROD-2018.09.06\SPA.SA\Scenarios\HLR\ZTE\ChangeMSISDNOnHLR.xml"));
            //}
            //catch (Exception e)
            //{
            //    MessageBox.Show(e.Message);
            //}
            
        }
    }
}
