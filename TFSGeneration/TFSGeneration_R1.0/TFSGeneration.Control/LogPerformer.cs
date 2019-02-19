using Microsoft.TeamFoundation.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSAssist.Control
{
    public delegate void WriteLogHandler(WarnSeverity severity, DateTime dateLog, string message, string stackMessage, bool lockProcess);
    public class LogPerformer
    {
        public event WriteLogHandler WriteLog;

        /// <summary>
        /// Уведомление пользователя по статусу работы приложения (нижняя строка приложения)
        /// </summary>
        /// <param name="status"></param>
        internal void OnStatusChanged(string status)
        {
            OnWriteLog(WarnSeverity.Status, status);
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
            string stackMessage = string.Empty;
            GetInnerException(ex, ref stackMessage);
            OnWriteLog(severity, trace, string.Format("{0}{1}{2}", stackMessage, Environment.NewLine, ex.StackTrace), lockProcess);
        }

        void GetInnerException(Exception ex, ref string result)
        {
            while (true)
            {
                result = string.Format("{0}[{1}]:{2}", result.IsNullOrEmpty() ? string.Empty : result + Environment.NewLine, ex.GetType().Name, ex.Message);

                if (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    continue;
                }
                break;
            }
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
            WriteLog?.Invoke(severity, DateTime.Now, trace, stackMessage, lockProcess);
        }
    }
}
