using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Utils.Handles
{
    [Serializable]
    public class RegeditControl : IDictionary<string, object>, IDisposable
    {
        private RegeditControl Parent;
        public string ApplicationName { get; }
        private readonly RegistryKey software;

        public RegeditControl(string applicationName)
        {
            ApplicationName = applicationName;

            software = Registry.CurrentUser.OpenSubKey("SOFTWARE", true);
            if (software == null)
                throw new Exception(@"Can't find HKEY_CURRENT_USER\SOFTWARE");
        }

        public RegeditControl(string applicationName, RegeditControl parent)
        {
            if(parent == null)
                throw new ArgumentException(nameof(parent));

            ApplicationName = applicationName;
            Parent = parent;
            software = Parent.GetCurrentProject();
        }

        RegistryKey GetCurrentProject()
        {
            return software.OpenSubKey(ApplicationName, true) ?? software.CreateSubKey(ApplicationName);
        }

        public object this[string propertyName]
        {
            get => GetValue(propertyName);
            set => SetValue(propertyName, value, RegistryValueKind.String);
        }

        public object this[string propertyName, RegistryValueKind type]
        {
            get => GetValue(propertyName);
            set => SetValue(propertyName, value, type);
        }

        object GetValue(string propertyName)
        {
            try
            {
                using (var project = GetCurrentProject())
                    return project?.GetValue(propertyName);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        void SetValue(string propertyName, object value, RegistryValueKind type)
        {
            try
            {
                using (var project = GetCurrentProject())
                {
                    project?.DeleteValue(propertyName, false);
                    project?.SetValue(propertyName, value, type);
                }
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        string[] GetValueNames()
        {
            try
            {
                using (var project = GetCurrentProject())
                    return project?.GetValueNames();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        Dictionary<string, object> GetKeyValues()
        {
            var listOfValues = new Dictionary<string, object>();
            try
            {
                using (var project = GetCurrentProject())
                {
                    if (project == null)
                        return listOfValues;

                    foreach (var propertyName in project.GetValueNames())
                        listOfValues.Add(propertyName, project.GetValue(propertyName));

                    return listOfValues;
                }
            }
            catch (Exception ex)
            {
                return listOfValues;
            }
        }

        public ICollection<string> Keys => GetValueNames();

        public ICollection<object> Values => GetKeyValues().Values;

        public int Count
        {
            get
            {
                try
                {
                    using (var project = GetCurrentProject())
                        return project?.ValueCount ?? -1;       
                }
                catch (Exception ex)
                {
                    return -1;
                }
            }
        }

        public bool IsReadOnly => false;

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

        public void Add(string propertyName, object value)
        {
            SetValue(propertyName, value, RegistryValueKind.String);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            foreach (var key in Keys)
                Remove(key);
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return TryGetValue(item.Key, out _);
        }

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            foreach (var value in array)
                Add(value.Key, value.Value);
        }

        public bool Remove(string key)
        {
            try
            {
                using (var project = GetCurrentProject())
                    project?.DeleteValue(key, false);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return Remove(item.Key);
        }

        public bool TryGetValue(string propertyName, out object value)
        {
            value = GetValue(propertyName);
            return value != null;
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, object>>)this).GetEnumerator();
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        public void Dispose()
        {
            software.Close();
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
    }
}