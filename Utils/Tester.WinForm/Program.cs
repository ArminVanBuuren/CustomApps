using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils.WinForm;
using Utils.WinForm.Notepad;

namespace Tester.WinForm
{
    class Test
    {
        public Test()
        {
            var source = CultureInfo.DefaultThreadCurrentCulture;
            var source1 = CultureInfo.DefaultThreadCurrentUICulture;
            var source2 = Thread.CurrentThread.CurrentCulture;
            var source3 = Thread.CurrentThread.CurrentUICulture;
        }
    }
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
	            //var filePath = @"C:\Soft\RegExTester.exe";
	            //var theIcon = DRAWING.IconFromFilePath(filePath);

	            //if (theIcon != null)
	            //{
		           // // Save it to disk, or do whatever you want with it.
		           // using (var stream = new System.IO.FileStream(@"C:\Soft\RegExTester.ico", System.IO.FileMode.CreateNew))
		           // {
			          //  theIcon.Save(stream);
		           // }
	            //}


                //var source = CultureInfo.DefaultThreadCurrentCulture;
                //var source1 = CultureInfo.DefaultThreadCurrentUICulture;
                //var source2 = Thread.CurrentThread.CurrentCulture;
                //var source3 = Thread.CurrentThread.CurrentUICulture;

                //var culture = new CultureInfo("en-US");

                //CultureInfo.DefaultThreadCurrentCulture = culture;
                //CultureInfo.DefaultThreadCurrentUICulture = culture;
                //Thread.CurrentThread.CurrentCulture = culture;
                //Thread.CurrentThread.CurrentUICulture = culture;

                //Task.Factory.StartNew(() => new Test());
                //var test = new Test();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
	            Application.Run(new Form1());

	            //Application.Run(new Notepad { AllowUserCloseItems = true, SizingGrip = true });
            }
            catch (Exception e)
            {
                var ex = e;
                var exMess = ex?.ToString();
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
