using System;
using System.Diagnostics;
using System.Reflection;

namespace Utils
{
	public class AssemblyInfo
	{
		public string ApplicationName => CurrentAssembly.GetName().Name;
		public string ApplicationPath => CurrentAssembly.Location;
		public static string ApplicationDirectory => AppDomain.CurrentDomain.BaseDirectory; //Path.GetDirectoryName(ApplicationPath);
		public static string ProcessFilePath => Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
		public string ApplicationFilePath => CurrentAssembly.GetDirectory();
		public string Version => CurrentAssembly.GetName().Version.ToString();
		public string Company => ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(CurrentAssembly, typeof(AssemblyCompanyAttribute), false))?.Company ?? string.Empty;
		public Assembly CurrentAssembly { get; }
		public Assembly ParentAssembly { get; }

		//public static Assembly CurrentAssembly => Assembly.GetCallingAssembly(); //Assembly.GetEntryAssembly(); //Assembly.GetExecutingAssembly(); 

		public AssemblyInfo(object input):this(input.GetType()) { }

		public AssemblyInfo(Type type):this(type.Assembly) { }

		public AssemblyInfo(Assembly assembly)
		{
			CurrentAssembly = assembly;
			ParentAssembly = Assembly.GetEntryAssembly();
		}
	}
}
