using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace Script.Control.Handlers.Timesheet.Project
{
    [Serializable]
    public class TFSProjectCollection 
    {
        public double TotalTimeByAnyDay => Items.Sum(x => x.Sum(p => p.TotalTimeByAnyDay));
        public double TotalTimeByDay => Items.Sum(x => x.Sum(p => p.TotalTimeByWorkDay.TotalHours));
        public double TotalTimeByShortDay => Items.Sum(x => x.Sum(p => p.TotalTimeByShortDay.TotalHours));
        public double TotalTimeByHolidayDay => Items.Sum(x => x.Sum(p => p.TotalTimeByHolidayDay.TotalHours));
        public UserAutorization Autorization { get; }
        public string GroupBy { get; }
        //public Dictionary<string, List<TFS>> Items { get; }
        public List<TFSProject> Items { get; }

        ///// <summary>
        ///// конструктор для десериализации объекта Dictionary[string, List[TFS]]
        ///// </summary>
        ///// <param name="info"></param>
        ///// <param name="context"></param>
        //public TFSProjectsCollection(SerializationInfo info, StreamingContext context) : base(info, context)
        //{

        //}
        public TFSProjectCollection(string userName, string password, string groupBy)
        {
            //Items = new Dictionary<string, List<TFS>>(StringComparer.CurrentCultureIgnoreCase);
            Items = new List<TFSProject>();
            Autorization = new UserAutorization(userName, password);
            GroupBy = groupBy;
        }

        public void Load(string htmlBody, int fid)
        {
            string monthPeriod = new Regex(@"My Timesheet.+?\((.+?)\)", RegexOptions.IgnoreCase).Match(htmlBody).Groups[1].Value;
            Autorization.FullName = new Regex(@"<u.+" + Regex.Escape(Autorization.DomainUserName) + ".+?>(.+?)<", RegexOptions.IgnoreCase).Match(htmlBody).Groups[1].Value;
            htmlBody = ReplaceByRegex(htmlBody);
            LoadXml(htmlBody, fid, monthPeriod);
        }
        string ReplaceByRegex(string str)
        {
            string _pattern1 = ".*(<div class=\"SIT_TAB_tbldivsrl\".+?</div>).*";
            string _pattern2 = "<img.+?>";
            string result = Regex.Replace(str, _pattern1, "$1", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            result = Regex.Replace(result, _pattern2, "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            result = result.Replace("<br>", "").Replace("&", "&amp;");
            return result;
        }
        void LoadXml(string strXml, int fid, string monthPeriod)
        {
            XmlDocument xmlSetting = new XmlDocument();
            xmlSetting.LoadXml(strXml);
            foreach (XmlNode findedNode in xmlSetting.SelectNodes("//tr[td/@class='SIT_TAB_tblcelid']"))
            {
                XmlNode newNode = findedNode.Clone();
                var temp_prjName = newNode.SelectSingleNode("//td[@class='SIT_TAB_tblcelprj']/text()") ?? newNode.SelectSingleNode("//td[@class='SIT_TAB_tblcelprj SIT_TAB_tblcel_filling']/text()");
                if (temp_prjName != null)
                {
                    string prjName = temp_prjName.Value.Trim();
                    TFSProject tfsList;
                    if (TryGetValue(prjName, out tfsList))
                    {
                        tfsList.Add(new TFS(newNode, fid, monthPeriod));
                    }
                    else
                    {
                        tfsList = new TFSProject(prjName);
                        tfsList.Add(new TFS(newNode, fid, monthPeriod));
                        Add(tfsList);
                    }
                }
            }
        }

        public bool TryGetValue(string name, out TFSProject stat)
        {
            stat = null;
            foreach (TFSProject st in Items)
            {
                if (st.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                {
                    stat = st;
                    return true;
                }
            }
            return false;
        }

        public void Add(TFSProject project)
        {
            Items.Add(project);
        }

        //public Dictionary<string, List<TFS>>.Enumerator GetEnumerator() => Items.GetEnumerator();
        public List<TFSProject>.Enumerator GetEnumerator() => Items.GetEnumerator();
    }
}
