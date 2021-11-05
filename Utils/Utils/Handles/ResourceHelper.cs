using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text;

namespace Utils.Handles
{
    /// <summary>Вспомогательный класс для работы со строковыми ресурсами</summary>
    public static class ResourceHelper
    {
        /// <summary>
        /// Возвращает форматированную строку, использующую ресурсы
        /// </summary>
        /// <param name="baseName">Название объекта, содержащего строковый ресурс</param>
        /// <param name="assembly">Сборка, содержащая объект</param>
        /// <param name="resID">Название ресурса</param>
        /// <param name="args">Аргументы, используемые для формирования строки</param>
        /// <returns>Строка, сформированная по шаблону ресурса <paramref name="resID"/> 
        /// и аргументам <paramref name="args"/></returns>
        public static string Format(string baseName, Assembly assembly, string resID, params object[] args)
        {
            if (baseName == null)
                throw new ArgumentNullException(nameof(baseName));
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));
            if (resID == null)
                throw new ArgumentNullException(nameof(resID));

            try
            {
                var rm = new ResourceManager(baseName, assembly);
                var formattedStr = rm.GetString(resID);
                return string.Format(formattedStr, args);
            }
            catch (Exception e)
            {
                var sb = new StringBuilder();
                sb.AppendFormat("Failed to get resource \"{0}\" ", resID);
                sb.AppendFormat("from base \"{0}\"", baseName);
                throw new Exception(sb.ToString(), e);
            }
        }

        /*
         * Загрузить из файла ресурса данные с требуемой культурой - "принудительно".
         * Example:
         * string defaultCulture = "en-US";
         * Thread.CurrentThread.CurrentCulture = new CultureInfo(defaultCulture);
         * Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
         * CultureInfo ci = new CultureInfo(Thread.CurrentThread.CurrentUICulture);
         * 
         * или
         * 
         * string defaultCulture = "en-US";
         * CultureInfo ci = new CultureInfo(defaultCulture);
         * String sz = Result.Description(Messaging.ResultType.SUCCESS, ci)
         */
        /// <summary>
        /// Возвращает форматированную строку, использующую ресурсы
        /// </summary>
        /// <param name="baseName">Название объекта, содержащего строковый ресурс</param>
        /// <param name="assembly">Сборка, содержащая объект</param>
        /// <param name="ci">Локализация ресурса</param>
        /// <param name="resID">Название ресурса</param>
        /// <param name="args">Аргументы, используемые для формирования строки</param>
        /// <returns>Строка, сформированная по шаблону ресурса <paramref name="resID"/> 
        /// и аргументам <paramref name="args"/></returns>
        public static string Format(string baseName, Assembly assembly, CultureInfo ci, String resID, params object[] args)
        {
            string formattedStr;
            try
            {
                var rm = new ResourceManager(baseName, assembly);
                formattedStr = rm.GetString(resID, ci);
                formattedStr = string.Format(formattedStr, args);
            }
            catch (Exception ex)
            {
                formattedStr = $"< ResourceHelper::Format (2)> {ex}";
                System.Diagnostics.Trace.WriteLine(formattedStr);
                throw new Exception(formattedStr);
            }
            return formattedStr;
        }

        /// <summary>
        /// Возвращает форматированную строку, использующую ресурсы
        /// </summary>
        /// <param name="resType">Название объекта, содержащего строковый ресурс</param>
        /// <param name="resID">Название ресурса</param>
        /// <param name="args">Аргументы, используемые для формирования строки</param>
        /// <returns>Строка, сформированная по шаблону ресурса <paramref name="resID"/> 
        /// и аргументам <paramref name="args"/>
        /// </returns>
        /// <exception cref="MissingManifestResourceException">Исключение генерируется если 
        /// указанный ресурс не найден</exception>
        public static string Format(Type resType, string resID, params object[] args)
        {
            var rm = new ResourceManager(resType);
            string formattedStr;
            try
            {
                formattedStr = rm.GetString(resID);
                if (formattedStr == null)
                {
                    var sb = new StringBuilder();
                    sb.AppendFormat("String for resourceId = '{0}' not found or null. ", resID);
                    sb.AppendFormat("Resource type is '{0}'", resType);
                    throw new MissingManifestResourceException(sb.ToString());
                }
            }
            catch (MissingManifestResourceException e)
            {
                if (resType.BaseType == null)
                {
                    var sb = new StringBuilder();
                    sb.AppendFormat("String for resourceId = '{0}' not found or null. ", resID);
                    sb.AppendFormat("Resource type is '{0}'", resType);
                    throw new MissingManifestResourceException(sb.ToString(), e);
                }

                var checkedTypes = new StringBuilder();
                checkedTypes.AppendFormat("{0}, ", resType);

                var msg = new StringBuilder();
                msg.AppendFormat("{0}{1}", e.Message, Environment.NewLine);
                return Format(resType.BaseType, resID, checkedTypes, msg, args);
            }

            return string.Format(formattedStr, args);
        }

        /// <summary>
        /// Возвращает форматированную строку, использующую ресурсы
        /// </summary>
        /// <param name="resType">Название объекта, содержащего строковый ресурс</param>
        /// <param name="resID">Название ресурса</param>
        /// <param name="checkedTypes">Список типов, в которых проводился поиск ресурса</param>
        /// <param name="msgErrors">Список ошибок, возникших при поиске ресурса</param>
        /// <param name="args">Аргументы, используемые для формирования строки</param>
        /// <returns>Строка, сформированная по шаблону ресурса <paramref name="resID"/> 
        /// и аргументам <paramref name="args"/>
        /// </returns>
        /// <exception cref="MissingManifestResourceException">Исключение генерируется если 
        /// указанный ресурс не найден</exception>
        private static string Format(Type resType, string resID, StringBuilder checkedTypes, StringBuilder msgErrors, params object[] args)
        {
            var rm = new ResourceManager(resType);
            string formattedStr;
            try
            {
                formattedStr = rm.GetString(resID);
                if (formattedStr == null)
                {
                    var sb = new StringBuilder();
                    sb.AppendFormat("String for resourceId = '{0}' not found or null. ", resID);
                    sb.AppendFormat("Resource type is '{0}'", resType);
                    throw new MissingManifestResourceException(sb.ToString());
                }
            }
            catch (MissingManifestResourceException e)
            {
                if (resType.BaseType == null)
                {
                    // сохраняем проверенный тип для указания в сообщении исключения
                    checkedTypes.Append(resType);
                    msgErrors.Append(e.Message);

                    var sb = new StringBuilder();
                    sb.AppendFormat("String for resourceId = '{0}' not found or null. ", resID);
                    sb.AppendFormat("Checked resource type(s): {0}{1}", checkedTypes, Environment.NewLine);
                    sb.AppendFormat("Error message(s): {0}{1}", Environment.NewLine, msgErrors);
                    throw new MissingManifestResourceException(sb.ToString(), e);
                }

                // и этот проверенный тип тоже сохраняем для указания в сообщении исключения
                if (checkedTypes == null)
                    checkedTypes = new StringBuilder();

                checkedTypes.AppendFormat("{0}, ", resType);

                if (msgErrors == null)
                    msgErrors = new StringBuilder();

                msgErrors.AppendFormat("{0}{1}", e.Message, Environment.NewLine);

                return Format(resType.BaseType, resID, checkedTypes, msgErrors, args);
            }

            return string.Format(formattedStr, args);
        }
    }
}
