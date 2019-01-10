using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace SPAFilter
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

            SPAFilterForm mainControl = null;
            if (File.Exists(SPAFilterForm.SerializationDataPath))
            {
                try
                {
                    using (Stream stream = new FileStream(SPAFilterForm.SerializationDataPath, FileMode.Open, FileAccess.Read))
                    {
                        mainControl = new BinaryFormatter().Deserialize(stream) as SPAFilterForm;
                    }
                }
                catch (Exception ex)
                {
                    File.Delete(SPAFilterForm.SerializationDataPath);
                }
            }

            if (mainControl == null)
                mainControl = new SPAFilterForm();

            Application.Run(mainControl);
        }
    }
}
