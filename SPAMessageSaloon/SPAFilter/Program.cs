﻿using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using SPAMessageSaloon.Common;

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
            try
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
            catch (Exception ex)
            {
                ReportMessage.Show(ex.ToString(), MessageBoxIcon.Error, "Initialization Form", false);
            }
        }
    }
}
