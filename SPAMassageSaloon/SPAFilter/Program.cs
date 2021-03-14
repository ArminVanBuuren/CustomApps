using System;
using System.Windows.Forms;
using SPAMassageSaloon.Common;

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
				Application.Run(SPAFilterForm.GetControl());
			}
			catch (Exception ex)
			{
				ReportMessage.Show(ex.ToString(), MessageBoxIcon.Error, "Initialization Form", false);
			}
		}
	}
}