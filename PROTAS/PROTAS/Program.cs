using Protas.Control;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;
using System.Xml.Xsl;
using Protas.Components.Functions;
using Protas.Control.Resource;
using Timer = System.Timers.Timer;
using Protas.Components.PerformanceLog;

namespace PROTAS
{
    class Program
    {
        static void Main(string[] args)
        {            //trap t = new trap();
                     //machine machineA = new machine();
                     //machineA.subscribe(t);
                     //t.run();

            //while (Console.Read() != 'q')
            //{
            //    Thread.Sleep(1000);
            //}


            //Log3Net _log3 = new Log3Net(Path.Combine(FStatic.LocalPath, "Config.xml"));
            //ShellLog3Net _log = new ShellLog3Net(_log3);
            //ResourcePack resp = new ResourcePack(_log);

            //IfTarget cExBlock = new IfCondition(_log).ExpressionEx("(('2'>'1'>'0' and '1'='1') or '2'='2') or '1'!='1' and '1'='1'");
            //bool rescond = cExBlock.ResultCondition;
            //int i = 0;
            //new CleanUp(_log3);
            //string ss3 = _log3.ConfigPackage.XPackage.VxmlView;
            //int length2 = ss.Length - ss2.Length;
            //int length3 = ss.Length - ss3.Length;
            //DateTime dNow = DateTime.Now;
            //ResourceObject res = resp.GetResource("{now('ss:ffffff')} {now('ss:ffffff')} {now('ss:ffffff')}");
            //ResourceObject res = resp.GetResource("{now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')} {now('ffffff')}", null);
            //ResourceObject res = resp.GetResource("{random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random} {random}");
            //ResourceObject res = resp.GetResource("{random('1','3')} {dynamic efwe}", new List<IResourceContext>());
            //ResourceObject res = resp.GetResource(@"{if('{now('ss')}'>='24' or '{now('ss')}'<='6','Night','Day')}");
            //ResourceObject res = resp.GetResource(@"Good {if('7'<='{now('ss')}'<='11','Morning','{if('12'<='{now('ss')}'<='17','Day','{if('18'<='{now('ss')}'<='23','Evening','Night')}')}')} !", null);
            //ResourceObject res = resp.GetResource("{{random} {random}('{random}','{random}')}");
            //ResourceObject res = resp.GetResource("{newguid('{random('2','3')}','{random('4','5')}')}");
            //ResourceObject res = resp.GetResource("{math('{random('{random('{random('20','30')}','40')}','50')}+6')} {math('{random('{random('{random('20','30')}','40')}','50')}+6')} {math('{random('{random('{random('20','30')}','40')}','50')}+6')} {math('{random('{random('{random('20','30')}','40')}','50')}+6')} {math('{random('{random('{random('20','30')}','40')}','50')}+6')} {math('{random('{random('{random('20','30')}','40')}','50')}+6')}", null);
            //ResourceObject res = resp.GetResource("{math('{random('{random('{random('2','3')}','4')}','5')}+6')}");
            //ResourceObject res = resp.GetResource("{random('{random('1','2')}','3')}");
            //ResourceObject res = resp.GetResource("{random('{random('100','200')}','300')}", null);
            //ResourceObject res = resp.GetResource("Total Memory Used:{LOCALRAM$used$mb} MB Avail:{LOCALRAM$avail$mb} MB Total:{LOCALRAM$total$mb} MB", null);

            //string temp = res.Result;
            //res.ResourcesChanged += (Res_ComponentChanged);
            //res.StartCoreMode();
            //Console.ReadKey();
            //resp.Dispose();
            //Console.ReadKey();

            //XPack result = new XPack("main", "Main", null);
            //result.ChildPacks.Add(new XPack("123", "Child", null));
            //Dictionary<string, string> dynamicObjects = new Dictionary<string, string>();
            //dynamicObjects.Add("name", @"Monitoring CPU \\{Static server} {0} {0$123}");
            //ContextDynamic ccc = new ContextDynamic(dynamicObjects, result);
            //DynamicResource dnm = (DynamicResource)ccc.GetResource("name");
            //string result2 = dnm.GetResult().Value;



            string config = "Config.xml";
            if (args.Length > 0 && !string.IsNullOrEmpty(args[0]))
                config = args[0];

            MainProcessor mainProc = new MainProcessor(Path.Combine(FStatic.LocalPath, config));
            while (Console.Read() != 'q')
            {
                Thread.Sleep(1000);
            }
        }

        static int _prevSecond = -1;
        static int _count = 0;
        static string _prevres = string.Empty;
        private static void Res_ComponentChanged(object sender, EventArgs e)
        {
            _count++;
            string res = ((ResourceKernel)sender).Result;
            if (res == _prevres)
                Console.WriteLine("{1} {0}", "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", DateTime.Now.ToString("HH:mm:ss.ffffff"));
            Console.WriteLine("{1} {0}", res, DateTime.Now.ToString("HH:mm:ss.ffffff"));
            _prevres = res;
            int currentSecond = int.Parse(DateTime.Now.ToString("ss"));
            if (currentSecond != _prevSecond)
            {
                Console.WriteLine(_count);
                _prevSecond = currentSecond;
                _count = 0;
            }
        }
        class CleanUp : ShellLog3Net
        {
            ResourceKernel _res;
            public CleanUp(Log3Net log) : base(log)
            {
                _res = new ResourceBase().GetResource("{LOCALRAM('used','b')}", null);
                _res.ResultMode = FinalResultMode.Unique;
                _res.ProcessingMode = ResourceMode.Core;
                Timer timerDispose = new Timer { Interval = 30000 };
                timerDispose.Elapsed += StartAllDispose;
                timerDispose.Enabled = true;
            }
            void StartAllDispose(object sender, System.Timers.ElapsedEventArgs e)
            {
                AddLogForm(Log3NetSeverity.Normal, "Start Clear Memory. Current:{0}", _res.Result);
                //GC.Collect();
                SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
                AddLogForm(Log3NetSeverity.Normal, "Memory Cleared.     Current:{0}", _res.Result);
            }

            [DllImport("kernel32.dll")]
            static extern bool SetProcessWorkingSetSize(IntPtr handle, int minimumWorkingSetSize, int maximumWorkingSetSize);
        }
        public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            try
            {
                string sTimeZone = args[0].ToString();
                Int32 timeZone = 0;
                if (sTimeZone != "")
                {
                    timeZone = Convert.ToInt32(sTimeZone);
                }
                string sFrom = args[1].ToString();
                string sTo = args[2].ToString();
                int spreadMinutes = (args.Length > 3) ? Convert.ToInt32(args[3].ToString()) : 120;  //default value of spread is 120 minutes

                DateTime now = DateTime.Now;
                DateTime dFrom = new DateTime(now.Year, now.Month, now.Day,
                        Convert.ToInt32(sFrom.Substring(0, 2)), Convert.ToInt32(sFrom.Substring(3, 2)), 0);
                dFrom = dFrom + TimeSpan.FromMinutes(timeZone);
                DateTime dTo = new DateTime(now.Year, now.Month, now.Day,
                        Convert.ToInt32(sTo.Substring(0, 2)), Convert.ToInt32(sTo.Substring(3, 2)), 0);
                dTo = dTo + TimeSpan.FromMinutes(timeZone);

                if (now >= dFrom && now <= dTo) return "";

                Random randomShift = new Random();
                if (now < dFrom) return TimespanToString(dFrom - now + TimeSpan.FromMinutes(randomShift.Next(spreadMinutes)));
                if (now > dFrom) return TimespanToString(dFrom.AddDays(1) - now + TimeSpan.FromMinutes(randomShift.Next(spreadMinutes)));
                return "";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        static string TimespanToString(TimeSpan timespan)
        {
            return string.Format("0000{0:00}{1:00}{2:00}{3:00}000R", timespan.Days, timespan.Hours, timespan.Minutes, timespan.Seconds);
        }
    }
}
