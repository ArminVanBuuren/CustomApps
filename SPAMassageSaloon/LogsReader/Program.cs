using System;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SPAMassageSaloon.Common;

namespace LogsReader
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
				Application.Run(new LogsReaderMainForm());
			}
			catch (Exception ex)
			{
				ReportMessage.Show(ex.ToString(), MessageBoxIcon.Error, @"Run");
			}
		}

		public static bool IsValidJson(this string strInput)
		{
			if (string.IsNullOrWhiteSpace(strInput))
				return false;

			strInput = strInput.Trim();
			if ((!strInput.StartsWith("{") || !strInput.EndsWith("}")) && (!strInput.StartsWith("[") || !strInput.EndsWith("]")))
				return false;

			try
			{
				JToken.Parse(strInput);
				return true;
			}
			catch (JsonReaderException jex)
			{
				//Exception in parsing json
				return false;
			}
			catch (Exception ex) //some other exception
			{
				return false;
			}
		}

		public static bool TryGetJson<T>(this string str, out T result)
		{
			result = default;

			if (!str.IsValidJson())
				return false;

			try
			{
				result = JsonConvert.DeserializeObject<T>(str);
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}
	}
}