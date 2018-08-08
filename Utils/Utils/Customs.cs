using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Utils
{
	public static class Customs
	{
	    public static string ApplicationName = Assembly.GetEntryAssembly().GetName().Name;
	    public static string ApplicationPath = Assembly.GetEntryAssembly().Location; 
		public static string AccountFilePath => Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), Assembly.GetEntryAssembly().GetName().Name);
		public static void AddAllAccessPermissions(string filePath)
		{
			DirectoryInfo dInfo = new DirectoryInfo(filePath);
			DirectorySecurity dSecurity = dInfo.GetAccessControl();
			dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl,
				InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
			dInfo.SetAccessControl(dSecurity);

			FileSecurity access = File.GetAccessControl(filePath);
			SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
			access.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.ReadAndExecute, AccessControlType.Allow));
			//access.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
			File.SetAccessControl(filePath, access);
		}

	    private static Random random = new Random();
        /// <summary>
        /// Возвращает рандомные буквы
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
	    public static string RandomString(int length)
	    {
	        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
	        return ReturnRandomObject(chars, length);
        }
        /// <summary>
        /// Возвращает раномные цифры
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
	    public static string RandomNumbers(int length)
	    {
	        const string chars = "1234567890";
	        return ReturnRandomObject(chars, length);
        }
        /// <summary>
        /// Возвращает рандомные цифры и буквы
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
	    public static string RandomStringNumbers(int length)
	    {
	        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";
	        return ReturnRandomObject(chars, length);
	    }

	    static string ReturnRandomObject(string chars, int length)
	    {
	        return new string(Enumerable.Repeat(chars, length)
	                                    .Select(s => s[random.Next(s.Length)]).ToArray());

        }
        /// <summary>
        /// Создание или получение записи в реестре с сгенеринным ключем и данными о приложении
        /// </summary>
        /// <param name="applicationName"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static string GetOrSetRegedit(string applicationName, string description)
		{
			RegistryKey software = Registry.CurrentUser.OpenSubKey("SOFTWARE", true);
			if (software == null)
				throw new Exception(@"Can't find HKEY_CURRENT_USER\SOFTWARE");

			RegistryKey myProject = software.OpenSubKey(applicationName, true) ?? software.CreateSubKey(applicationName);

		    string currentDesc = (string) myProject.GetValue("Description") ?? string.Empty;
            if (!currentDesc.Equals(description))
		        myProject.SetValue("Description", description);

			string result = (string)myProject.GetValue("Key");
			if (result == null)
			{
				result = Guid.NewGuid().ToString("D");
				myProject.SetValue("Key", result);
			}

			software.Close();
			myProject.Close();
			return result;
		}

	    public static bool EnabledBootRun(string applicationName)
	    {
	        RegistryKey software = GetBootRunKey();
            string applicatinItemValue = (string)software.GetValue(applicationName);
	        if (!applicatinItemValue.IsNullOrEmpty())
	            applicatinItemValue = applicatinItemValue.Trim();

            return !applicatinItemValue.IsNullOrEmpty();
	    }

	    public static bool SetBootStartup(string applicationName, string applicationPath, bool addToStartup)
	    {
	        RegistryKey software = GetBootRunKey(true);
	        string applicatinItemValue = (string)software.GetValue(applicationName);
	        if (!applicatinItemValue.IsNullOrEmpty())
	            applicatinItemValue = applicatinItemValue.Trim();


            string setValue = string.Format("\"{0}\" /autostart", applicationPath);
	        if (addToStartup && setValue.Equals(applicatinItemValue))
	            return true;

            if (addToStartup)
	            software.SetValue(applicationName, setValue);
	        else if (applicatinItemValue != null) // если в загрузке уже стоит данное приложение, если нет, то удалять нечего
	            software.DeleteValue(applicationName);

	        return true;
        }

	    static RegistryKey GetBootRunKey(bool writeble = false)
	    {
	        RegistryKey software = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", writeble);
	        if (software == null)
	            throw new Exception(@"Can't find HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run");

	        return software;
	    }

		static Regex getNotNumber = new Regex(@"[^0-9]", RegexOptions.Compiled);
		/// <summary>
		/// Обработчик который корректно провыеряет поле TextBox из окна приложения чтобы ввод был строго чисел, также вставляется позиция корретки
		/// </summary>
		/// <param name="oldValue"></param>
		/// <param name="caretIndex"></param>
		/// <param name="maxLength"></param>
		public static void GetOnlyNumberWithCaret(ref string oldValue, ref int caretIndex, int maxLength)
		{
			string correctValue = getNotNumber.Replace(oldValue, "");
			if (correctValue.Length > maxLength)
				correctValue = correctValue.Substring(0, maxLength);

			int newCoretIndex = 0;
			if (oldValue.Length > correctValue.Length)
				newCoretIndex = caretIndex - (oldValue.Length - correctValue.Length);
			else
				newCoretIndex = caretIndex;

			caretIndex = newCoretIndex < 0 ? 0 : newCoretIndex > correctValue.Length ? correctValue.Length : newCoretIndex;
			oldValue = correctValue;
		}

    }
}
