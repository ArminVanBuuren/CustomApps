using System.Text.RegularExpressions;
using Microsoft.Exchange.WebServices.Data;

namespace TFSAssist.Control
{
    public class TraceListener : ITraceListener
    {
        public static Regex regReplace = new Regex(@"<.?Trace.*?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private WriteLogHandler _makeWriteLog;

        public TraceListener(WriteLogHandler makeWriteLog)
        {
            _makeWriteLog = makeWriteLog;
        }

        public void Trace(string traceType, string traceMessage)
        {
            _makeWriteLog?.Invoke(string.Format("{0} - {1}", traceType, regReplace.Replace(traceMessage, "").Trim()));
        }
    }
}
