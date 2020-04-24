using System.Diagnostics;
using System.IO;

namespace Utils
{
    public static class CMD
    {
        const string argument_start = "/C choice /C Y /N /D Y /T 4 {0} & Start \"\" /D \"{1}\" \"{2}\"";
        const string argument_update = "/C choice /C Y /N /D Y /T 4 {0} & Del /F /Q \"{1}\" & choice /C Y /N /D Y /T 2 & Move /Y \"{2}\" \"{3}\"";
        const string argument_update_start = argument_update + " {4} & Start \"\" /D \"{5}\" \"{6}\" {7}";
        const string argument_add = "/C choice /C Y /N /D Y /T 4 {0} & Move /Y \"{1}\" \"{2}\"";
        const string argument_remove = "/C choice /C Y /N /D Y /T 4 {0} & Del /F /Q \"{1}\"";

        /// <summary>
        /// Запустить приложение
        /// </summary>
        /// <param name="fileDestination">запустить приложение</param>
        /// <param name="delayBeforeRun">задержка в секундах перед запуском приложения  (ограниение 1 час)</param>
        public static void StartApplication(string fileDestination, int delayBeforeRun = 0)
        {
            var argument_complete = string.Format(argument_start, 
                GetCommandTimeout(delayBeforeRun), 
                Path.GetDirectoryName(fileDestination), 
                Path.GetFileName(fileDestination));

            StartProcess(argument_complete);
        }

        /// <summary>
        /// Заменить файл и запустить его на новом месте. Время замены должно быть меньше задержки запуска.
        /// </summary>
        /// <param name="fileSource">изначальное местоположение файла</param>
        /// <param name="fileDestination">скопировать куда и запустить</param>
        /// <param name="delayBeforeMove">задержка в секундах перед запуском переноса файла (ограниение 1 час)</param>
        /// <param name="delayAfterMoveAndRunDestination">задержка в секундах перед запуском приложения после переноса файла (ограниение 1 час). Учесть размер файла и примерное время замены и удаления изначального местоположения файла</param>
        public static void OverwriteAndStartApplication(string fileSource, string fileDestination, int delayBeforeMove = 0, int delayAfterMoveAndRunDestination = 0)
        {
            var delayDelMove = GetCommandTimeout(delayBeforeMove);
            var delayStart = GetCommandTimeout(delayAfterMoveAndRunDestination);
            var argument_complete = string.Format(argument_update_start,
                delayDelMove,
                fileDestination,
                fileSource,
                fileDestination,
                delayStart,
                Path.GetDirectoryName(fileDestination),
                Path.GetFileName(fileDestination),
                string.Empty);

            StartProcess(argument_complete);
        }

        /// <summary>
        /// Перезаписать файл
        /// </summary>
        /// <param name="fileSource">изначальное местоположение файла</param>
        /// <param name="fileDestination">путь назначения файла</param>
        /// <param name="delayBeforeMove">перезаписать после оперделенного времени в секундах (ограниение 1 час)</param>
        public static void OverwriteFile(string fileSource, string fileDestination, int delayBeforeMove = 0)
        {
            var argument_complete = string.Format(argument_update, 
                GetCommandTimeout(delayBeforeMove), 
                fileDestination, 
                fileSource, 
                fileDestination);

            StartProcess(argument_complete);
        }

        /// <summary>
        /// Скопировать файл
        /// </summary>
        /// <param name="fileSource">местоположение файла</param>
        /// <param name="fileDestination">путь назначения файла</param>
        /// <param name="delayBeforeCopy">скопировать файл после оперделенного времени в секундах (ограниение 1 час)</param>
        public static void CopyFile(string fileSource, string fileDestination, int delayBeforeCopy = 0)
        {
            var argument_complete = string.Format(argument_add, 
                GetCommandTimeout(delayBeforeCopy), 
                fileSource, 
                fileDestination);

            StartProcess(argument_complete);
        }

        /// <summary>
        /// Файл удалиться асинхронным процессом, после того когда держать перестанут. Но не гарантированно.
        /// </summary>
        /// <param name="fileDestination">путь назначения файла</param>
        /// <param name="delayBeforeDelete">скопировать файл после оперделенного времени в секундах (ограниение 1 час)</param>
        public static void DeleteFile(string fileDestination, int delayBeforeDelete = 0)
        {
            var argument_complete = string.Format(argument_remove, 
                GetCommandTimeout(delayBeforeDelete), 
                fileDestination);

            StartProcess(argument_complete);
        }

        /// <summary>
        /// Максимальное значение 3600 секунд и значение не должно быть отрицательным или равным 0
        /// </summary>
        /// <param name="secondsDelay"></param>
        /// <returns></returns>
        static string GetCommandTimeout(int secondsDelay)
        {
            var currectSeconds = secondsDelay;
            // поставим ограниение на задержку выполнение процесса на один час
            if (currectSeconds > 3600)
                currectSeconds = 3600;
            else if (secondsDelay <= 0)
                return string.Empty;

            // & timeout /t 10 /nobreak - таймаут 10 секунд перед запуском приложения

            return $"& timeout /t {currectSeconds} /nobreak";
        }

        static void StartProcess(string arguments)
        {
            var cmd = new ProcessStartInfo
            {
                Arguments = arguments,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe"
            };

            Process.Start(cmd);
        }
    }
}
