using System;
using XPackage;

namespace Script.Control.Handlers.Timesheet.Stats
{

    public class StatTFS : Statistic
    {
        public override DateTime PeriodStart { get; }
        public override DateTime PeriodEnd { get; }
        public override double TotalTimeByAnyDay { get; }
        public int TFSID { get; }
        public string Title { get; }

        public StatTFS(int tFSID, string title, double totalTimeByAnyDay, DateTime periodStart, DateTime periodEnd) : base(string.Empty)
        {
            TFSID = tFSID;
            title = title.Trim();
            if (title.Contains("&"))
                Title = Functions.ReplaceXmlSpecSymbls(title, 0);
            else
                Title = title;

            TotalTimeByAnyDay = totalTimeByAnyDay;
            PeriodStart = periodStart;
            PeriodEnd = periodEnd;
        }

        public override string ToString()
        {
            return string.Format("[{0}]; [{1}] [{2} Hours]; ", TFSID, Title, TotalTimeByAnyDay);
        }

        public override string GetStat()
        {
            return string.Format("    •  {0} - {1}", TFSID, Title);
        }
    }
}
