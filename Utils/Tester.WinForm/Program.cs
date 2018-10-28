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
                Application.Run(new XmlNotepad(@"C:\@Repos\CustomApp\Utils\Tester.WinForm\bin\Debug\!text.xml"));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            
        }
    }
}
