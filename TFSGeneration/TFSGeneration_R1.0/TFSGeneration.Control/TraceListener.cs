using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Exchange.WebServices.Data;

namespace TFSGeneration.Control
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
