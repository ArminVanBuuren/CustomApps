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

        public static void SetProgressNoAnimation(this ProgressBar pb, int value)
        {
            // To get around the progressive animation, we need to move the 
            // progress bar backwards.
            if (value == pb.Maximum)
            {
                // Special case as value can't be set greater than Maximum.
                pb.Maximum = value + 1;     // Temporarily Increase Maximum
                pb.Value = value + 1;       // Move past
                pb.Maximum = value;         // Reset maximum
            }
            else
            {
                pb.Value = value + 1;       // Move past
            }
            pb.Value = value;               // Move to correct value
        }
    }
}
