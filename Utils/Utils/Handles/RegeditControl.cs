using Microsoft.Win32;
using System;

namespace Utils.Handles
{
    public class RegeditControl : IDisposable
    {
        private readonly RegistryKey software;
        private readonly RegistryKey myProject;
        public RegeditControl(string applicationName)
        {
            software = Registry.CurrentUser.OpenSubKey("SOFTWARE", true);
            if (software == null)
                throw new Exception(@"Can't find HKEY_CURRENT_USER\SOFTWARE");

            myProject = software.OpenSubKey(applicationName, true) ?? software.CreateSubKey(applicationName);
        }

        ///// <summary>
        ///// Создание или получение записи в реестре с сгенеринным ключем и данными о приложении
        ///// </summary>
        ///// <param name="applicationName"></param>
        ///// <param name="description"></param>
        ///// <returns></returns>
        //public static string GetOrSetRegedit(string applicationName, string description)
        //{
        //    string currentDesc = (string) myProject.GetValue("Description") ?? string.Empty;
        //    if (!currentDesc.Equals(description))
        //        myProject.SetValue("Description", description);

        //    string result = (string) myProject.GetValue("Key");
        //    if (result == null)
        //    {
        //        result = Guid.NewGuid().ToString("D");
        //        myProject.SetValue("Key", result);
        //    }

        //    software.Close();
        //    myProject.Close();
        //    return result;
        //}

        public object this[string propertyName, RegistryValueKind type = RegistryValueKind.String]
        {
            get => myProject.GetValue(propertyName);
            set => myProject.SetValue(propertyName, value, type);
        }

        public static bool EnabledBootRun(string applicationName)
        {
            var software = GetBootRunKey();
            var applicatinItemValue = (string) software.GetValue(applicationName);
            if (!applicatinItemValue.IsNullOrEmpty())
                applicatinItemValue = applicatinItemValue.Trim();

            return !applicatinItemValue.IsNullOrEmpty();
        }

        public static bool SetBootStartup(string applicationName, string applicationPath, bool addToStartup)
        {
            var software = GetBootRunKey(true);
            var applicatinItemValue = (string) software.GetValue(applicationName);
            if (!applicatinItemValue.IsNullOrEmpty())
                applicatinItemValue = applicatinItemValue.Trim();


            var setValue = $"\"{applicationPath}\" /autostart";
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
            var software = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", writeble);
            if (software == null)
                throw new Exception(@"Can't find HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run");

            return software;
        }

        public void Dispose()
        {
            software.Close();
            myProject.Close();
        }
    }
}