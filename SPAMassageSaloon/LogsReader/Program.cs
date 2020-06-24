using System;
using System.Reflection;
using System.Windows.Forms;
using SPAMassageSaloon.Common;

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
                Application.Run(new LogsReaderMainForm());
            }
            catch (Exception ex)
            {
                ReportMessage.Show(ex.ToString(), MessageBoxIcon.Error, @"Run");
            }
        }

        public static T Clone<T>(this T controlToClone)
	        where T : Control
        {
	        var controlProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

	        var instance = Activator.CreateInstance<T>();

	        foreach (var propInfo in controlProperties)
	        {
		        if (propInfo.CanWrite)
		        {
			        if (propInfo.Name != "WindowTarget")
				        propInfo.SetValue(instance, propInfo.GetValue(controlToClone, null), null);
		        }
	        }

	        return instance;
        }
    }
}