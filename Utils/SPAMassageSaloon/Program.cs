using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utils;
using Utils.AppUpdater;

namespace SPAMassageSaloon
{
	class Program
	{
		internal static readonly string AppName = "SPA Massage Saloon";
		internal static ApplicationUpdater AppUpdater;

		static void Main(string[] args)
		{
			try
			{
				Console.WriteLine("Start");
				AppUpdater = new ApplicationUpdater(Assembly.GetExecutingAssembly(), 2);
				AppUpdater.OnUpdate += AppUpdater_OnUpdate;
				AppUpdater.OnSuccessfulUpdated += AppUpdater_OnSuccessfulUpdated;
				AppUpdater.Start();
				Console.WriteLine(AppUpdater);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}

			Console.ReadKey();
		}

		private static void AppUpdater_OnUpdate(object sender, ApplicationUpdaterArgs args)
		{
			try
			{
				AppUpdater.DoUpdate(args.Control);
			}
			catch (Exception)
			{
				AppUpdater.Refresh();
			}
		}

		private static void AppUpdater_OnSuccessfulUpdated(object sender, ApplicationUpdaterArgs args)
		{
			try
			{
				if (args.Control == null)
					return;

				var separator = $"\r\n{new string('-', 61)}\r\n";
				var description = separator.TrimStart()
				                + string.Join(separator,
				                              args.Control.Select(x
					                                                  => $"{x.RemoteFile.Location} Version = {x.RemoteFile.VersionString}\r\nDescription = {x.RemoteFile.Description}"))
				                        .Trim()
				                + separator.TrimEnd();
				Console.WriteLine(string.Format("{0} {1} {2} {3}", AppName, new AssemblyInfo(typeof(Program)).Version, "-", description).Trim());
			}
			catch (Exception)
			{
				// ignored
			}
		}
	}
}
