using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace SPAFilter
{
    public enum ReportStatusType
    {
        Informaton,
        Warning,
        Error
    }
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

            SPAFilterForm mainControl = null;
            if (File.Exists(SPAFilterForm.SavedDataPath))
            {
                try
                {
                    using (Stream stream = new FileStream(SPAFilterForm.SavedDataPath, FileMode.Open, FileAccess.Read))
                    {
                        mainControl = new BinaryFormatter().Deserialize(stream) as SPAFilterForm;
                    }
                }
                catch (Exception)
                {
                    File.Delete(SPAFilterForm.SavedDataPath);
                }
            }

            if (mainControl == null)
                mainControl = new SPAFilterForm();

            Application.Run(mainControl);
        }

        public static void ReportMessage(string message, MessageBoxIcon type = MessageBoxIcon.Error, string caption = null)
        {
            MessageBox.Show(message,
                caption ?? type.ToString("G"), 
                MessageBoxButtons.OK, 
                type, 
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.DefaultDesktopOnly);
        }
    }
}
