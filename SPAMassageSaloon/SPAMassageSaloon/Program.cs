using System;
using System.Windows.Forms;
using SPAMassageSaloon.Common;

namespace SPAMassageSaloon
{
	internal static class Program
	{
		/// <summary>
		///     The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main()
		{
			try
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new MainForm());
			}
			catch (Exception ex)
			{
				ReportMessage.Show(ex.ToString());
			}
		}
	}
}