using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace Script.Control.Handlers.Timesheet.Project
{
    [Serializable]
    public class TFS
    {
        public string PeriodInterval { get; }
        public DateTime PeriodStart { get; }
        public DateTime PeriodEnd { get; }
        public TFS(XmlNode xm, int fid, string monthPeriod)
        {
            Fid = fid;
            PeriodInterval = monthPeriod;
            string[] pers = monthPeriod.Split('-');
            if (pers.Count() > 1)
            {
                PeriodStart = DateTime.Parse(pers[0].Trim());
                PeriodEnd = DateTime.Parse(pers[1].Trim());
            }

            var temp_id = xm.SelectSingleNode("//td[@class='SIT_TAB_tblceltfs']/a/text()");
            if (temp_id != null)
                Id = int.Parse(temp_id.Value);
            else
                Id = 0;

            var obj_tfsData = xm.SelectSingleNode("//td[@class='SIT_TAB_tblceltsk']") ?? xm.SelectSingleNode("//td[@class='SIT_TAB_tblceltsk SIT_TAB_tblcel_filling']");
            if (obj_tfsData != null)
            {
                string temp_tfsData = obj_tfsData.OuterXml;
                Regex r = new Regex("(Requirement|LeadTask|Type|Title).+?</span>(.+?)<", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                foreach (Match mch in r.Matches(temp_tfsData))
                {
                    if (mch.Groups[1].Value.Equals("Requirement", StringComparison.CurrentCultureIgnoreCase))
                        Requirement = mch.Groups[2].Value.Trim();
                    else if (mch.Groups[1].Value.Equals("LeadTask", StringComparison.CurrentCultureIgnoreCase))
                        LeadTask = mch.Groups[2].Value.Trim();
                    else if (mch.Groups[1].Value.Equals("Type", StringComparison.CurrentCultureIgnoreCase))
                        Type = mch.Groups[2].Value.Trim();
                    else if (mch.Groups[1].Value.Equals("Title", StringComparison.CurrentCultureIgnoreCase))
                        Title = mch.Groups[2].Value.Trim();
                }
                
            }

            TotalTimeByWorkDay = GetSumTime(xm, "//td[@class='SIT_TAB_tblcelday'][text()!='']/text()");
            TotalTimeByShortDay = GetSumTime(xm, "//td[@class='SIT_TAB_tblceldayshort'][text()!='']/text()");
            TotalTimeByHolidayDay = GetSumTime(xm, "//td[@class='SIT_TAB_tblceldayhol'][text()!='']/text()");
            //_totalTimeByAnyDay = _totalTimeByAnyDay.Add(TotalTimeByWorkDay);
            //_totalTimeByAnyDay = _totalTimeByAnyDay.Add(TotalTimeByShortDay);
            //_totalTimeByAnyDay = _totalTimeByAnyDay.Add(TotalTimeByHolidayDay);
        }

        static TimeSpan GetSumTime(XmlNode xm, string xpath)
        {
            TimeSpan ts = new TimeSpan(0, 0, 0);
            foreach (XmlNode timeofday in xm.SelectNodes(xpath))
            {
                if (timeofday.Value != null)
                {
                    string[] temp_timeofDay = timeofday.Value.Split(':');
                    TimeSpan appendTime;
                    if (temp_timeofDay.Count() > 1)
                        appendTime = new TimeSpan(int.Parse(temp_timeofDay[0]), int.Parse(temp_timeofDay[1]), 0);
                    else
                        appendTime = new TimeSpan(int.Parse(timeofday.Value), 0, 0);
                    ts = ts.Add(appendTime);

                }
            }
            return ts;
        }
        public int Fid { get; }
        public string Requirement { get; }
        public string LeadTask { get; }
        public int Id { get; }
        public string Type { get; }
        public string Title { get; }
        //TimeSpan _totalTimeByAnyDay { get; }
        public double TotalTimeByAnyDay => TotalTimeByWorkDay.TotalHours + TotalTimeByShortDay.TotalHours + TotalTimeByHolidayDay.TotalHours;
        public TimeSpan TotalTimeByWorkDay { get; }
        public TimeSpan TotalTimeByShortDay { get; }
        public TimeSpan TotalTimeByHolidayDay { get; }

        public override string ToString()
        {
            return string.Format("TFS_ID=[{0}]; FID={1}", Id, Fid);
        }
    }
}
