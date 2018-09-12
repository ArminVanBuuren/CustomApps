using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ProcessFilter
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

            ProcessFilterForm mainControl = null;
            if (File.Exists(ProcessFilterForm.SerializationDataPath))
            {
                try
                {
                    using (Stream stream = new FileStream(ProcessFilterForm.SerializationDataPath, FileMode.Open, FileAccess.Read))
                    {
                        mainControl = new BinaryFormatter().Deserialize(stream) as ProcessFilterForm;
                    }
                }
                catch (Exception ex)
                {
                    File.Delete(ProcessFilterForm.SerializationDataPath);
                }
            }

            Application.Run(mainControl ?? new ProcessFilterForm());
        }
    }
}
