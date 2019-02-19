using System;
using System.Text.RegularExpressions;
using Microsoft.Exchange.WebServices.Data;

namespace TFSAssist.Control
{
    public class TraceListener : ITraceListener
    {
        public static Regex regReplace = new Regex(@"<.?Trace.*?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private Action<string, bool> _writeLog;

        public TraceListener(Action<string, bool> writeLog)
        {
            _writeLog = writeLog;
        }

        public void Trace(string traceType, string traceMessage)
        {
            _writeLog?.Invoke(string.Format("{0} - {1}", traceType, regReplace.Replace(traceMessage, "").Trim()), true);
        }
    }
}
