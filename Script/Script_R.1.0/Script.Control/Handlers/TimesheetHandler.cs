using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using Script.Control.Handlers.Arguments;
using Script.Control.Handlers.Timesheet;
using Script.Control.Handlers.Timesheet.Project;
using Script.Control.Handlers.Timesheet.Stats;
using Script.Control.Handlers.Timesheet.WriteData;
using XPackage;

namespace Script.Control.Handlers
{
    [IdentifierClass(typeof(TimesheetHandler), "Выполняет сбор статистики табеля рабочего времени указанного за определенный период и эскпортит его в файл Word или текстовый файл")]
    public class TimesheetHandler : ScriptTemplate
    {
        internal const string TIMESHEET_NAME = "TimesheetStat";
        const string DEFAULT_SRC = @"https://timesheets.bss.nvision-group.com/surv/_layouts/WSS/WSSC.CST.SIT.SURV/v2.0.0.0/Tabel.aspx?";
        const string DEFAULT_FID = @"fid=";
        private string[] _deserializeFilesPath;
        private const string _groupBYconst = "Vacation[I00001];Other[]";
        private const string _exportConst = "docx";
        /// <summary>
        /// Если необходимо сохранить результат обработки в виде сериализуемого объекта TFSProjectCollection, для того чтобы десериализовать
        /// </summary>
        public static string SerializationDir => Path.Combine(Functions.LocalPath, TIMESHEET_NAME);
        private ExecutionTimeSheet _execute;
        public TFSProjectCollection TFSProjects { get; }
        public Statistic Matches { get; private set; }

        [Identifier("Src", "Адрес Timesheet страцины до параметра fid=. Например: https://......./v2.0.0.0/Tabel.aspx?", "Условно Обязательный. Если аттрибута не будет, то должны быть созданы сериализуемые файлы")]
        public Uri Src { get; }

        [Identifier("UserName", "Домен с логином", "Обязательный парамтер")]
        public string UserName { get; } = string.Empty;

        [Identifier("Password", "Пароль к учетке", "Обязательный парамтер")]
        public string Password { get; } = string.Empty;

        [Identifier("Serialization", "Если необходимо сохранить результат обработки в виде сериализации, для того чтобы при повторной обработки заново не проходить цикл обращения к TimeSheet. При выполнении этого объекта, происходит десериализация предыдущих результатов.", false)]
        public bool SerializationResult { get; } = false;

        [Identifier("GroupBy", "Для гурппировки регионов по названиям проектов. Например GroupBy='БЕБ[B13001,U11861,U14043_MNP];БЕУз[R13962,R13016,T14002];БЕР[R14007,R14008];Отпуск и Обучение[I00001,I00003];Остальное[]'. Значение Остальное[] означает что все остальные проекты которые не попали в предыдущие условия будут в регионе Остальное", _groupBYconst)]
        public string GroupBy { get; }

        [Identifier("Export", "Мод экспорта результата. Тектовый вид или WORD документ. Eсли указать просто doc (export='doc') то локально с программой создаст папку %TIMESHEET_NAME% и там создаст файл с названием %username%.bin.", _exportConst)]
        public string ExportMode { get; }

        [Identifier("Fid_Start", "Первая страница цикла обработки. Например https://......./v2.0.0.0/Tabel.aspx?fid=[значение].", "Обязательное; Числовое значение")]
        public int FidStart { get; }

        [Identifier("Fid_End", "Последняя страница цикла обработки. Например https://......./v2.0.0.0/Tabel.aspx?fid=[значение].", "Обязательное; Числовое значение")]
        public int FidEnd { get; }

        public TimesheetHandler(XPack parentPack, XmlNode node, LogFill logFill) : base(parentPack, node, logFill)
        {
            FidStart = int.Parse(Attributes[GetXMLAttributeName(nameof(FidStart))]);
            FidEnd = int.Parse(Attributes[GetXMLAttributeName(nameof(FidEnd))]);
            if(FidStart > FidEnd)
                throw new HandlerInitializationException("FidStart=[{0}] Most Lage Then FidEnd=[{1}]", FidStart, FidEnd);
            GroupBy = Attributes[GetXMLAttributeName(nameof(GroupBy))] ?? _groupBYconst;
            ExportMode = Attributes[GetXMLAttributeName(nameof(ExportMode))] ?? _exportConst;
            string src_path = Attributes[GetXMLAttributeName(nameof(Src))];


            if (src_path == null)
            {
                if (!Directory.Exists(SerializationDir))
                    throw new HandlerInitializationException("Directory=[{0}] Doesn't Exist! Unable To Get Already Finished Result.", SerializationDir);
                _deserializeFilesPath = Directory.GetFiles(SerializationDir, "*.bin", SearchOption.TopDirectoryOnly);
                if (_deserializeFilesPath.Length == 0)
                    throw new HandlerInitializationException("Files=[{0}] Doesn't Exist! Unable To Get Already Finished Result.", SerializationDir);

                _execute = DeSerializeExists;
                return;
            }

            Src = new Uri(src_path);
            bool temp_serialization = false;
            if (bool.TryParse(Attributes[GetXMLAttributeName(nameof(SerializationResult))], out temp_serialization) && temp_serialization)
                SerializationResult = true;
            TFSProjects = new TFSProjectCollection(Attributes[GetXMLAttributeName(nameof(UserAutorization.UserName))], Attributes[GetXMLAttributeName(nameof(UserAutorization.Password))], GroupBy);

            _execute = GetHTMLBodySource;

            //CookieContainer myContainer = new CookieContainer();
            //myContainer.Add(new Cookie("__utma", "#########.###########.##########.##########.##########.#"));
            //Cookie chocolateChip = new Cookie("WSS_KeepSessionAuthenticated", "443") { Domain = "bss" };
            //gaCookies.Add(new Cookie("WSS_KeepSessionAuthenticated", "443") { Domain = target.Host });
            //gaCookies.Add(new Cookie("jreject-close", "true") { Domain = target.Host });
            //string encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(UserName + ":" + Password));
            //Ping png = new Ping();
            //var result = png.Send(Src);
            //if (result.Status != IPStatus.Success)
            //    return;


        }

        public override void Execute()
        {
            _execute.Invoke();
        }
        /// <summary>
        /// получение тела HTML по URI
        /// </summary>
        void GetHTMLBodySource()
        {
            for (int i = FidStart; i <= FidEnd; i++)
            {
                string htmlBody = GetSiteData(string.Format("{0}{1}{2}", Src.AbsoluteUri, DEFAULT_FID, i), TFSProjects.Autorization.GetNetworkCredential());
                if (string.IsNullOrEmpty(htmlBody))
                    continue;
                TFSProjects.Load(htmlBody, i);
            }

            if(TFSProjects.Items.Count == 0)
                return;

            if (SerializationResult)
                Serialize(TFSProjects);

            Matches = ExportData(TFSProjects, TFSProjects.GroupBy, ExportMode);
        }

        /// <summary>
        /// десиарелизация предыдущих результатов обработки
        /// </summary>
        void DeSerializeExists()
        {
            Matches = new Statistic("Collection");
            foreach (string file in _deserializeFilesPath)
            {
                using (Stream stream = File.Open(file, FileMode.Open))
                {
                    var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    TFSProjectCollection projects = (TFSProjectCollection)bformatter.Deserialize(stream);
                    Matches.AddChild(ExportData(projects, GroupBy ?? projects.GroupBy, ExportMode));
                }
            }
        }
        static string GetSiteData(string src, NetworkCredential autorization)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(src);
            request.Headers.Add("AUTHORIZATION", "Basic YTph");
            request.ContentType = "text/html";
            request.Credentials = autorization;
            //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            request.PreAuthenticate = true;
            request.Method = "GET";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream;

                if (response.CharacterSet == null)
                    readStream = new StreamReader(receiveStream);
                else
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

                string data = readStream.ReadToEnd();
                response.Close();
                readStream.Close();

                return data;
            }
            return null;
        }

        /// <summary>
        /// Экспорт результатов в файл docx или txt
        /// </summary>
        /// <param name="projects"></param>
        /// <param name="groupByString"></param>
        /// <param name="exportPathOrType"></param>
        Statistic ExportData(TFSProjectCollection projects, string groupByString, string exportPathOrType)
        {
            string[] groupBy = new string[] { };
            if (!string.IsNullOrEmpty(groupByString))
                groupBy = groupByString.Split(';');

            List<TFSProject> prjs = new List<TFSProject>();
            foreach (TFSProject project in projects.Items)
            {
                List<TFS> tfss = project.Where(tfs => tfs.Fid >= FidStart && tfs.Fid <= FidEnd).ToList();
                if (tfss.Any())
                {
                    TFSProject prj = new TFSProject(project.Name);
                    prj.AddRange(tfss);
                    prjs.Add(prj);
                }
            }

            Statistic stats = new Statistic(projects.Autorization.FullName, groupBy);
            foreach (TFSProject proj in prjs)
            {
                Statistic childProject = new Statistic(proj.Name);
                var ff = proj.GroupBy(s => s.Id).Select(grp => grp.ToList()).ToList();
                foreach (List<TFS> tf in ff)
                {
                    StatTFS childTFS = new StatTFS(tf[0].Id, tf[0].Title, tf.Sum(x => x.TotalTimeByAnyDay), tf.Min(m => m.PeriodStart), tf.Max(m => m.PeriodEnd));
                    childProject.Add(childTFS);
                }
                stats.Add(childProject);
            }
            stats.OrderByGroups();

            WriteDataPerformer wdp = new WriteDataPerformer(exportPathOrType, projects.Autorization.UserName);
            wdp.Export(stats);
            return stats;
        }



        /// <summary>
        /// сериализировать весь результат обработки
        /// </summary>
        /// <param name="projects"></param>
        static void Serialize(TFSProjectCollection projects)
        {
            if (!Directory.Exists(SerializationDir))
                Directory.CreateDirectory(SerializationDir);
            using (Stream stream = File.Open(Path.Combine(SerializationDir, string.Format("{0}.bin", projects.Autorization.UserName)), FileMode.Create))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bformatter.Serialize(stream, projects);
            }
        }
    }
}