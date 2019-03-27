using Microsoft.TeamFoundation.Common;
using System;

namespace TFSAssist.Control
{
    public delegate void WriteLogHandler(WarnSeverity severity, DateTime dateLog, string message, string stackMessage, bool lockProcess);
    public enum WarnSeverity
    {
        Error = 0,
        Warning = 1,
        Attention = 2,
        Status = 4,
        StatusRegular = 8,
        Normal = 16,
        Debug = 32
    }

    public class LogPerformer
    {
        public event WriteLogHandler WriteLog;
        internal uint LogItemId { get; set; } = 0;

        /// <summary>
        /// Уведомление пользователя по статусу работы приложения (нижняя строка приложения)
        /// </summary>
        /// <param name="status"></param>
        /// <param name="severity"></param>
        internal void OnStatusChanged(string status, WarnSeverity severity = WarnSeverity.Status)
        {
            OnWriteLog(severity, status);
        }

        /// <summary>
        /// Нотифицируем в случае ошибки при выполнении - выскакивает окно с ошибкой
        /// </summary>
        /// <param name="severity"></param>
        /// <param name="trace"></param>
        /// <param name="ex"></param>
        /// <param name="lockProcess"></param>
        internal void OnWriteLog(WarnSeverity severity, string trace, Exception ex, bool lockProcess = false)
        {
            string stackInnerMessage = string.Empty;
            if (ex.InnerException != null)
                GetInnerException(ex.InnerException, ref stackInnerMessage);
            OnWriteLog(severity, $"{trace}\r\n{ex.Message}", $"{trace} {GetFormatException(ex)}\r\n{ex.StackTrace}\r\n{stackInnerMessage}".Trim(), lockProcess);
        }

        void GetInnerException(Exception ex, ref string innerMessage)
        {
            while (true)
            {
                innerMessage = $"{(innerMessage.IsNullOrEmpty() ? string.Empty : innerMessage + "\r\n")}{GetFormatException(ex)}";

                if (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    continue;
                }
                break;
            }
        }

        string GetFormatException(Exception ex)
        {
            return $"{ex.GetType().Name}=[{ex.Message}]";
        }

        internal void OnWriteLog(string trace)
        {
            OnWriteLog(WarnSeverity.Normal, trace);
        }

        internal void OnWriteLog(string trace, bool isDebug)
        {
            OnWriteLog(isDebug ? WarnSeverity.Debug : WarnSeverity.Normal, trace);
        }

        internal void OnWriteLog(WarnSeverity severity, string trace, string stackMessage = null, bool lockProcess = false)
        {
            WriteLog?.Invoke(severity, DateTime.Now, severity == WarnSeverity.Status || severity == WarnSeverity.StatusRegular ? trace : $"[{LogItemId}] {trace}", stackMessage, lockProcess);
        }
    }
}
