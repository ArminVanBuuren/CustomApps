using Microsoft.Exchange.WebServices.Data;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace TFSGeneration.Control.Utils
{
	public static class CustomFunc
	{
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

		/// <summary>
		/// Создание или получение записи в реестре с сгенеринным ключем и данными о приложении
		/// </summary>
		/// <param name="applicationName"></param>
		/// <returns></returns>
		public static string GetOrSetRegedit(string applicationName)
		{
			RegistryKey software = Registry.CurrentUser.OpenSubKey("SOFTWARE", true);
			if (software == null)
				throw new Exception(@"Can't find HKEY_CURRENT_USER\SOFTWARE");

			RegistryKey myProject = software.OpenSubKey(applicationName, true) ?? software.CreateSubKey(applicationName);

			if ((string)myProject.GetValue("Description") == null)
				myProject.SetValue("Description", "This application implements reading mail items and creation of TFS.");

			string result = (string)myProject.GetValue("Key");
			if (result == null)
			{
				result = Guid.NewGuid().ToString();
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

		/// <summary>
		/// Получаем все существующие папки на почте exchange
		/// </summary>
		/// <param name="service"></param>
		/// <returns></returns>
		public static List<Folder> GetAllFolders(ExchangeService service)
		{
			List<Folder> completeListOfFolderIds = new List<Folder>();
			FolderView folderView = new FolderView(int.MaxValue);
			//FindFoldersResults findFolderResults = service.FindFolders(WellKnownFolderName.PublicFoldersRoot, folderView);
			FindFoldersResults findFolderResults = service.FindFolders(WellKnownFolderName.MsgFolderRoot, folderView);
			foreach (Folder folder in findFolderResults)
			{
				completeListOfFolderIds.Add(folder);
				FindAllSubFolders(service, folder.Id, completeListOfFolderIds);
			}
			return completeListOfFolderIds;
		}

		private static void FindAllSubFolders(ExchangeService service, FolderId parentFolderId, List<Folder> completeListOfFolderIds)
		{
			//search for sub folders
			FolderView folderView = new FolderView(int.MaxValue);
			FindFoldersResults foundFolders = service.FindFolders(parentFolderId, folderView);

			// Add the list to the growing complete list
			completeListOfFolderIds.AddRange(foundFolders);

			// Now recurse
			foreach (Folder folder in foundFolders)
			{
				FindAllSubFolders(service, folder.Id, completeListOfFolderIds);
			}
		}

    }
}
