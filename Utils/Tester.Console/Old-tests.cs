using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using Utils.Crypto;
//using Utils.Messaging.Telegram;

namespace Tester.ConsoleTest
{
	static class Old_tests
	{
		
		//(int current, int previous, int dd) Fib(int i)
		//{
		//    if (i == 0) return (1, 0);
		//    var (p, pp, ppp) = Fib(i - 1);
		//    return (p + pp, p);
		//}

		class test
		{
			public int Id;
			public string name;
		}

		static void OLD()
		{

			try
			{
				var f11f = Directory.GetDirectories(@"C:\test1");
			}
			catch (DirectoryNotFoundException e)
			{

			}
			catch (IOException e)
			{
				var type = e.GetType();
			}
			catch (Exception e)
			{

			}

			//
			test:
			try
			{
				try
				{
					using (var dd1111 = new NetworkConnection(@"\\f6-mg01\c$", new NetworkCredential(@"foris6\vhovanskij", "IO%11111")))
					{

					}

					//var dd111= Directory.GetAccessControl(@"\\f6-spa-sa01\c$");
					//var exist = Directory.Exists(@"\\f6-spa-sa01\c$");
					//var files1 = Directory.GetFiles(@"\\f6-spa-sa01\c$\forislog\", "*", SearchOption.AllDirectories);
				}

				catch (InvalidOperationException ex)
				{
					var type = ex.GetType();
				}
				catch (Exception e)
				{
				}

				using (var dd111 = new NetworkConnection(@"\\f6-mg05\c$", new NetworkCredential(@"foris6\vhovanskij", "IO%11111")))
				{

				}

				var files = Directory.GetFiles(@"\\f6-mg05\c$\forislog\mg", "*", SearchOption.AllDirectories);



				Ping x = new Ping();
				PingReply reply = x.Send("f6-mg02", 10000);

				if (reply.Status == IPStatus.Success)
					System.Console.WriteLine("Address is accessible");
			}
			catch (Exception e)
			{
				var type = e.GetType();
				var res1111 = NetworkShare.ConnectToShare(@"\\f6-mg02\c$\forislog\mg", @"foris6\vhovanskij", "IO%11111");
				goto test;
			}

			if (Directory.Exists(@"\\f6-mg02\c$\forislog\mg"))
			{
				var res1111 = NetworkShare.ConnectToShare(@"\\f6-mg02\c$\forislog\mg", @"foris6\vhovanskij", "IO%11111");
				var files = Directory.GetFiles(@"\\f6-mg02\c$\forislog\mg", "*", SearchOption.AllDirectories);
			}
			else
			{

			}

			var sfewfwe = IO.SafeReadFile(@"C:\SMSTemplateProcessor.01.xml");
			//var sfewfwe = IO.SafeReadFile(@"C:\1.xml");
			var res111 = XPATH.Select(sfewfwe, @"//template");


			var list = new List<test>();


			var dssdd = list.ToDictionary(x => x.Id, x => x.name);


			//var list = new List<string>() {"1","2","3","4", "5"};
			//var eeee = list.Skip(2).Take(2);

			//var sseee = @"[All] C:\ddd\ddd";
			//var dsed = sseee.Substring(5, sseee.Length - 5);
			//var dsed2 = sseee.Substring(0, 5).Equals("[ALl]", StringComparison.InvariantCultureIgnoreCase);
			//var doc = new XmlDocument();
			//doc.LoadXml(@"<XML><UserInfo><SpaReqId>-9223372036854775808</SpaReqId></UserInfo></XML>");
			//var oo = doc.InnerXml;

			//var doc2 = new XmlDocument();
			//doc2.LoadXml(@"<XML><UserInfo><SpaReqId>-9223372036854775808</SpaReqId></UserInfo></XML>");
			//var oo2 = doc2.InnerXml;

			//if (oo == oo2)
			//{

			//}

			//var code = AES.EncryptStringAES("18739", nameof(TLControl));

			//CMD.OverwriteAndStartApplication(@"C:\test\CMD\NEW\versions.xml", @"C:\test\CMD\OLD\versions.xml", 0, 10);
			//return;


			var res11111 = getTimeInRangeFunction.Invoke(null, new[] {"0", "16:00", "20:00"}, null);

			var _result = new SortedDictionary<DateTime, string>();
			_result.Add(DateTime.Parse("17.03.2020 11:54:53.880"), "Empty1");
			_result.Add(DateTime.Parse("17.03.2020 11:54:53.880"), "Empty3");
			_result.Add(DateTime.Parse("17.03.2020 11:54:53.880"), "Empty2");


			//var method = CODE.CreateMethod(@"public static void Test(){System.Console.WriteLine(""Test"");}", "Test");
			//method.Invoke(null, null);



			var stop = new Stopwatch();

			string context = string.Empty;
			using (var sr = new StreamReader(@"C:\test\1_1.txt"))
			{
				context = sr.ReadToEnd();
			}

			var test2 = new Regex(@"(\d+[-]\d+[-]\d+\s+\d+[:]\d+[:]\d+[,]\d+)\s+\((\w+)\)\s+\[\d+\]\n[-]{49,}(.+?)(\<.+?\>\s*)($|\n\s*[-]{49,})", RegexOptions.Singleline);
			var ddd = test2.Matches(context);

			//double.TryParse("100.032332", out var test11);

			var qq = IO.IsFileReady(@"C:\test4\1_0.txt");

			var ff = false;
			//using (var regControl = new RegeditControl("TEST123"))
			//{
			//    //var user = Environment.UserName;
			//    //regControl[$"{user}_prevSearch"] = "test";

			//    using (var schemeControl = new RegeditControl("Inner1", regControl))
			//    {
			//        schemeControl["Prop1"] = "111";
			//    }

			//    //regControl[$"{Environment.UserName}_useRegex", RegistryValueKind.String] = true;
			//    //regControl.Add("Prop1", "test");
			//    foreach (var key in regControl)
			//    {

			//    }



			//    //if (!description.Equals(regControl["Description"]))
			//    //{
			//    //    regControl["Description"] = description;
			//    //}

			//    //RegeditKey = regControl["Key"]?.ToString();
			//    //if (RegeditKey.IsNullOrEmptyTrim())
			//    //{
			//    //    RegeditKey = Guid.NewGuid().ToString("D");
			//    //    regControl["Key"] = RegeditKey;
			//    //}
			//}


			//string sss1;
			//using (StreamReader stream = new StreamReader(@"C:\test3\1_1.txt", new UTF8Encoding(false)))
			//{
			//    var str = stream.ReadToEnd();
			//    if (str.IsXml(out var doc))
			//    {
			//        sss1 = doc.PrintXml();
			//    }
			//}

			//string s1 = null;
			//string s2 = null;
			//var www1 = s1.Like(s2);
			//var www2 = s1.Like("");
			//var www3 = "".Like(s2);

			//var testttt = @"<DateFrom>          <Value />        </DateFrom>";
			//var ewwe = XML.RemoveUnallowable(testttt);
			//System.Console.ReadKey();
			//return;

			//var patt = System.Console.ReadLine();
			//var regex = new Regex(patt, RegexOptions.Compiled | RegexOptions.Multiline);
			//using (StreamReader stream = new StreamReader(@"C:\test3\1_0.txt", new UTF8Encoding(false)))
			//{
			//    var str = stream.ReadToEnd();
			//    var match = regex.Matches(str);
			//    string result = string.Empty;
			//    foreach (Match mtch in match)
			//    {
			//        if (mtch.Success)
			//            result += "{" + $"\"{mtch.Groups[2]}\", \"{mtch.Groups[1]}\"" + "}," + Environment.NewLine;
			//    }
			//}

			//return;

			//var rrrr = new Regex(@"(?<DISC>\w{1})(\$|\:)(?<FULL>([^\\/]*[\\/])*(?<LAST>[^\\/]*))", RegexOptions.IgnoreCase);
			//var match = rrrr.Match(@"C:\test\1\2.txt");
			//System.Console.ReadKey();


			//var sss11 = new DoubleDictionary<string, string>();
			//for (int i = 0; i < 10000000; i++)
			//{
			//    for (int j = 0; j < 5; j++)
			//    {
			//        sss11.Add(i.ToString(), j.ToString());
			//    }
			//}
			//var tsttt = sss11.CountValues;
			//System.Console.ReadKey();
			//var list = new List<byte[]>();
			//while (true)
			//{
			//    list.Add(new byte[2000000]);
			//    System.Console.WriteLine(SERVER.GetMemUsage(Process.GetCurrentProcess()));
			//    Thread.Sleep(1000);
			//}



			stop.Start();
			var lines = IO.CountLinesReader(@"C:\test\1_1.txt");
			stop.Stop();
			System.Console.WriteLine($"lines={lines} - {stop.ElapsedMilliseconds}");
			stop.Reset();

			stop.Start();
			lines = IO.CountLinesSmarter(@"C:\test\1_1.txt");
			stop.Stop();
			System.Console.WriteLine($"lines={lines} - {stop.ElapsedMilliseconds}");
			stop.Reset();

			//var d111 = new MTFuncCallBackList<string, int>();
			//d111.Add(new MTFuncCallBack<string, int>("1", 1));
			//d111.Add(new MTFuncCallBack<string, int>("1", 2));
			//d111.Add(new MTFuncCallBack<string, int>("1", 3));
			//d111.Add(new MTFuncCallBack<string, int>("1", 4));
			//d111.Add(new MTFuncCallBack<string, int>("1", 5));
			//d111.Add(new MTFuncCallBack<string, int>("2", 1));
			//d111.Add(new MTFuncCallBack<string, int>("2", 2));
			//foreach (var item in d111)
			//{

			//}

			var sss = "<TEST><node/><!-- test11 --></TEST>";
			if (sss.IsXml(out var documnet))
			{
				var fff = documnet.PrintXml(15);
			}

			var obj = new object();
			var iter = 0;
			var actions = new List<int>();
			Action<int> func = (int input) =>
			{
				Thread.Sleep(1000);
				System.Console.Write($"[{input}];");
			};
			for (var i = 0; i < 1000; i++)
			{
				actions.Add(i);
			}

			stop.Start();
			//cancel.CancelAfter(15000);
			//MultiTasking.Run(actions, 10, cancel.Token);
			var mt = new MTActionResult<int>(func, actions, 100);
			mt.Start();
			stop.Stop();
			System.Console.WriteLine();
			//mt.Result.Values.Select(x => x.)
			System.Console.WriteLine(
				$"Complete=[{stop.ElapsedMilliseconds}] Keys=[{mt.Result.SourceList.Count()}] Values=[{mt.Result.CallBackList.Count()}] Exception=[{string.Join(";\r\n", mt.Result.CallBackList.Where(x => x.Error != null).Select(x => x.Error.Message))}]");
			System.Console.ReadKey();
			return;

			var ss = "Ïðîöåññ íå ìîæåò ïîëó÷èòü äîñòóï ê ôàéëó".GetEncoding("[А-я]{8,}");
			var ss1 = "Ïðîöåññ íå ìîæåò ïîëó÷èòü äîñòóï ê ôàéëó".StringConvert(Encoding.GetEncoding("windows-1252"), Encoding.GetEncoding("windows-1251"));

			var dd = new FormatFunction();
			var dd12 = dd.Invoke(null, new[] {@"{0}-{1}-{2}\r\n", "22;33;44"}, null);
			var res = FormatFunction.get_codelist("1111", @"@@ \@      =     ^^ \^", "", ";");
			//(string s, string s2) = LookupName(6);


			var stw = new Stopwatch();
			//while (true)
			//{
			//    stw.Start();
			//    string concat = string.Empty;
			//    for (int j = 0; j < 5; j++)
			//    {
			//        concat += STRING.RandomString(1);
			//    }
			//    stw.Stop();
			//    System.Console.WriteLine($"Concat=[{concat}]. Miliseconds={stw.ElapsedMilliseconds} Ticks={stw.ElapsedTicks}");
			//    stw.Reset();

			//    stw.Start();
			//    StringBuilder builder = new StringBuilder();
			//    for (int j = 0; j < 5; j++)
			//    {
			//        builder.Append(STRING.RandomString(1));
			//    }
			//    stw.Stop();
			//    System.Console.WriteLine($"Builder=[{builder.ToString()}]. Miliseconds={stw.ElapsedMilliseconds} Ticks={stw.ElapsedTicks}");
			//    stw.Reset();

			//    System.Console.WriteLine(new string('=',20));
			//    Thread.Sleep(1000);
			//}

			//string ss1 = XML.NormalizeXmlValueFast("&amp;quot;&amp;lt;&amp;gt; &lt;=  &gt;=");
			//string ss2 = XML.NormalizeXmlValueFast("&&&quot; fefe &quot;&apos; & 111&quot");
			//string ss3 = XML.NormalizeXmlValueFast("& fefe \"' 111", XML.XMLValueEncoder.Encode);

			//IO.EvaluateFirstMatchPath("..\\..\\..\\123\\123", "C:\\123\\455");

			//var dd1 = 197.IsParity();
			//var dd2 = 266.IsParity();

			//var temp = new List<long>();

			//var i = 0;
			////while (i < 200)
			////{
			////    stw.Start();




			////    stw.Stop();
			////    temp.Add(stw.ElapsedMilliseconds);
			////    stw.Reset();


			////    i++;
			////}

			//var dynObj = new DynamicObject(GetResult);
			////DuplicateDictionary<string, bool> res = new DuplicateDictionary<string, bool>();

			//while (true)
			//{
			//    stw.Start();

			//    var res = false;
			//    var res1 = ExpressionBuilder.Calculate("('1'='1' and '2'>'${random}' and '5'>='4' and 'fff'='fff' and 'sasDDDddd'^='DDD' and 'rrrrr'!='aaaaa' and '((1.2+2+3)*12)/12'>'6')", dynObj);
			//    //if (1 == 1 && "2" == GetResult("${random}") && 5>=4 && "fff"=="fff" && "sasDDDddd".Like("DDD") && "rrrrr"!= "aaaaa" && MATH.Calculate("((1.2+2+3)*12)/12")>6)
			//    //{
			//    //    res = true;
			//    //}

			//    //res.Add(res1.StringResult, res1.ConditionResult);
			//    //dynObj.Elapsed();

			//    stw.Stop();
			//    temp.Add(stw.ElapsedMilliseconds);

			//    System.Console.WriteLine($"[M:{stw.ElapsedMilliseconds} T:{stw.ElapsedTicks}] [{res1.StringResult}] {res1.ConditionResult}");
			//    //System.Console.WriteLine($"[M:{stw.ElapsedMilliseconds} T:{stw.ElapsedTicks}] [{res}]");
			//    stw.Reset();

			//    i++;
			//    if (System.Console.ReadKey().Key == ConsoleKey.Escape)
			//    {
			//        break;
			//    }
			//}


			//double dd = 855555;
			//using (var responceBody = WEB.GetHttpWebResponse("https://www.whatismyip.com/ip-address-lookup/"))
			//{
			//    if (responceBody != null && responceBody.StatusCode == HttpStatusCode.OK)
			//    {
			//        var httpDoc = responceBody.GetHtmlDocument();
			//    }
			//}



			//using (RegeditControl regedit = new RegeditControl("TFSAssist"))
			//{
			//    using (var stream = new FileStream(nameof(TLControl) + "Session", FileMode.CreateNew))
			//    {
			//        byte[] bytes = (byte[])regedit[nameof(TLControl) + "Session", RegistryValueKind.Binary];
			//        stream.Write(bytes, 0, bytes.Length);
			//    }
			//}


			//!!!!!!
			//string dd = TLControl.SessionName + ".code";
			//if(File.Exists(dd))
			//    File.Delete(dd);

			//string ss = AES.EncryptStringAES("InCommon-2.0", nameof(TLControl));
			//using (var stream = new FileStream(TLControl.SessionName + ".code", FileMode.OpenOrCreate))
			//{
			//    byte[] logsBytes = new UTF8Encoding(true).GetBytes(ss);
			//    stream.Write(logsBytes, 0, logsBytes.Length);
			//}

			//DisplayData();

		}

		static void LogFile()
		{
			var syncLock = new object();
			Random random = new Random();
			var date = DateTime.Now.AddDays(-30);
			using (StreamWriter sw = new StreamWriter(@"C:\test\1_5.txt", false, Encoding.UTF8))
			{
				Parallel.For(1, 10000000, (i) =>
				{
					string traceName;
					string description;
					string message;
					int item = 0;
					lock (syncLock)
						item = random.Next(1, 4);
					switch (item)
					{
						case 1:
							lock (syncLock)
							{
								date = date.AddSeconds(random.Next(0, 5));
								description = STRING.RandomString(random.Next(10, 20));
								message = $"<xmlTest>{STRING.RandomStringSimbols(random.Next(100, 1000))}</xmlTest>";
							}

							traceName = "Info";
							break;
						case 2:
							lock (syncLock)
							{
								date = date.AddSeconds(random.Next(5, 10));
								description = STRING.RandomString(random.Next(10, 20));
								message = $"{STRING.RandomStringSimbols(random.Next(100, 1000))}";
							}

							traceName = "Normal";
							break;
						default:
							//System.Console.Write($"\r{i}");
							lock (syncLock)
							{
								date = date.AddSeconds(random.Next(10, 20));
								message = $"<xmlTest>{STRING.RandomStringSimbols(random.Next(100, 1000))}</xmlTest>";
							}

							traceName = "Debug";
							description = string.Empty;
							break;
					}

					lock (syncLock)
						sw.WriteLine($"{date:dd.MM.yyyy HH:mm:ss.fff} [{traceName}] {description} {message}");
				});
			}

			date = DateTime.Now.AddDays(-30);
			using (StreamWriter sw = new StreamWriter(@"C:\test\1_6.txt", false, Encoding.UTF8))
			{
				for (int i = 0; i < 10000000; i++)
				{
					string traceName;
					string description;
					string message;
					int item = 0;

					item = random.Next(1, 4);
					switch (item)
					{
						case 1:
							date = date.AddSeconds(random.Next(0, 5));
							description = STRING.RandomString(random.Next(10, 20));
							message = $"<xmlTest>{STRING.RandomStringSimbols(random.Next(100, 1000))}</xmlTest>";
							traceName = "Info";
							break;
						case 2:
							date = date.AddSeconds(random.Next(5, 10));
							description = STRING.RandomString(random.Next(10, 20));
							message = $"{STRING.RandomStringSimbols(random.Next(100, 1000))}";
							traceName = "Normal";
							break;
						default:
							date = date.AddSeconds(random.Next(10, 20));
							message = $"<xmlTest>{STRING.RandomStringSimbols(random.Next(100, 1000))}</xmlTest>";
							traceName = "Debug";
							description = string.Empty;
							break;
					}

					sw.WriteLine($"{date:dd.MM.yyyy HH:mm:ss.fff} [{traceName}] {description} {message}");
				}
			}

			System.Console.ReadKey();
		}

		public class getTimeInRangeFunction
		{

			public static object Invoke(string xsltContext, object[] args, string docContext)
			{
				try
				{
					string sTimeZone = args[0].ToString();
					Int32 timeZone;
					if (Int32.TryParse(sTimeZone, out timeZone))
					{
						timeZone = Convert.ToInt32(sTimeZone);
					}
					else
					{
						timeZone = 0;
					}

					string sFrom = args[1].ToString();
					string sTo = args[2].ToString();
					int spreadMinutes = (args.Length > 3) ? Convert.ToInt32(args[3].ToString()) : 120;
					//default value of spread is 120 minutes

					DateTime now = DateTime.Now;
					DateTime dFrom = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
					dFrom = dFrom + stringToTimeSpan(sFrom);
					dFrom = dFrom + TimeSpan.FromMinutes(timeZone);
					DateTime dTo = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
					dTo = dTo + stringToTimeSpan(sTo);
					dTo = dTo + TimeSpan.FromMinutes(timeZone);
					var ааа = TimeSpan.FromMinutes(1);

					if (now >= dFrom && now <= dTo) return "";

					Random randomShift = new Random();
					if (now < dFrom)
						return timespanToString(dFrom - now + TimeSpan.FromMinutes(randomShift.Next(spreadMinutes)));
					if (now > dFrom)
						return
							timespanToString(dFrom.AddDays(1) - now + TimeSpan.FromMinutes(randomShift.Next(spreadMinutes)));
					return "";
				}
				catch (Exception e)
				{
					return e.Message;
				}
			}

			static string timespanToString(TimeSpan timespan)
			{
				return String.Format("0000{0:00}{1:00}{2:00}{3:00}000R", timespan.Days, timespan.Hours, timespan.Minutes, timespan.Seconds);
			}

			static TimeSpan stringToTimeSpan(string span)
			{
				Regex rx = new Regex(@"((?<days>(\d+))\.)?(?<hours>(\d{1,2})):(?<mins>(\d{1,2}))");
				Match match = rx.Match(span);
				GroupCollection groups = match.Groups;
				Int32 days = 0;
				try
				{
					days = Convert.ToInt32(groups["days"].Value);
				}
				catch
				{
					days = 0;
				}

				TimeSpan timeSpan = new TimeSpan(days, Convert.ToInt32(groups["hours"].Value), Convert.ToInt32(groups["mins"].Value), 0);
				return timeSpan;
			}
		}

		static string GetResult(string input)
		{
			if (input.IndexOf("${random}", StringComparison.InvariantCultureIgnoreCase) == -1)
				return input;

			var res = new Random().Next(0, 9);
			return input.Replace("${random}", res.ToString());
		}

		//static void DisplayData()
		//{
		//    var reader = OleDbEnumerator.GetRootEnumerator();

		//    var list = new List<string>();
		//    while (reader.Read())
		//    {
		//        for (var i = 0; i < reader.FieldCount; i++)
		//        {
		//            //if (reader.GetName(i) == "SOURCES_NAME")
		//            {
		//                list.Add(reader.GetValue(i).ToString());
		//            }
		//        }
		//        System.Console.WriteLine("{0} = {1}", reader.GetName(0), reader.GetValue(0));
		//    }
		//    reader.Close();
		//}

		//static void ParceOptions()
		//{
		//    string isCommand = "30609:";
		//    //string tlMessage = "30609:cam ";
		//    string tlMessage = "30609:cam ( par1 =d '1111,=(2222)' , par2 =q '5555,=(6666)' ) ";

		//    string command = tlMessage.Substring(isCommand.Length, tlMessage.Length - isCommand.Length);
		//    Dictionary<string, string> options = null;
		//    int optStart = command.IndexOf('(');
		//    int optEnd = command.IndexOf(')');
		//    if (optStart != -1 && optEnd != -1 && optEnd > optStart)
		//    {
		//        string strOptions = command.Substring(optStart, command.Length - optStart);
		//        options = ReadOptionParams(strOptions);
		//        command = command.Substring(0, optStart);
		//    }

		//    command = command.ToLower().Trim();
		//}

		//static void TelegramTester()
		//{
		//    //using (var stream = new FileStream(@"C:\!MyRepos\CustomApp\Utils\Tester.Console\bin\Debug\session.dat", FileMode.Open))
		//    //{
		//    //    var buffer = new byte[2048];
		//    //    stream.Read(buffer, 0, 2048);
		//    //    using (RegeditControl regedit = new RegeditControl(ASSEMBLY.ApplicationName))
		//    //    {
		//    //        regedit[nameof(TLControl) + "Session", RegistryValueKind.Binary] = buffer;
		//    //    }
		//    //}


		//    //DateTime d1 = DateTime.ParseExact("20.03.2019 15:28:00", "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
		//    //DateTime d2 = DateTime.ParseExact("20.03.2019 21:34:00", "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
		//    //var dd = d2.Subtract(d1);
		//    //TimeSpan ss = d1.Subtract(mdt);

		//    var mdt = new DateTime(1970, 1, 1, 0, 0, 0);
		//    DateTime newDt1 = mdt.AddSeconds(1553084912).ToLocalTime();
		//    DateTime newDt1Temp = mdt.AddSeconds(1553084912);
		//    var strUs = newDt1Temp.ToString(new CultureInfo("en-US"));
		//    var strGb = newDt1Temp.ToString(new CultureInfo("en-GB"));
		//    DateTime newDt2 = mdt.AddSeconds(1553106864).ToLocalTime();

		//    var ddddd = DateTime.Now;
		//    var ddddd1 = DateTime.Now.ToUniversalTime();


		//    DateTime newd1 = DateTime.ParseExact("20.03.2019 15:28:32", "dd.MM.yyyy HH:mm:ss", CultureInfo.CurrentCulture);
		//    DateTime newd2 = DateTime.ParseExact("20.03.2019 21:34:24", "dd.MM.yyyy HH:mm:ss", CultureInfo.CurrentCulture);
		//    newd1 = newd1.ToUniversalTime();
		//    newd2 = newd2.ToUniversalTime();
		//    double start = newd1.Subtract(mdt).TotalSeconds;
		//    double end = newd2.Subtract(mdt).TotalSeconds;


		//    //TimeSpan span = DateTime.Now.Subtract(DateTime.Parse("07.02.2018 00:00:00"));
		//    TLControl control = new TLControl(770122, "8bf0b952100c9b22fd92499fc329c27e");
		//    Process(control);
		//}

		//static async void Process(TLControl control)
		//{
		//   // await control.ConnectAsync();
		//    //var user1 = await control.GetUserAsync("+79113573202");
		//    //var user2 = await control.GetUserByUserNameAsync("MexicanCactus");
		//    //var chat1 = await control.GetChatAsync("Ацацоц");
		//    //var chat2 = await control.GetChatAsync("Тухлый сыр");
		//    //var user2 = await control.GetUserByUserNameAsync("TFSAssistbot");
		//    //await control.GetMessagesAsync(user2.Destination);

		//    //DateTime lastDate = DateTime.ParseExact("19.03.2019 00:00:00", "dd.MM.yyyy HH:mm:ss", CultureInfo.CurrentCulture);

		//    //!!!!!!!!!!!DateTime lastDate = DateTime.ParseExact("18.03.2019 20:17:00", "dd.MM.yyyy HH:mm:ss", CultureInfo.CurrentCulture);
		//    DateTime lastDate = DateTime.ParseExact("15.03.2019 00:00:00", "dd.MM.yyyy HH:mm:ss", CultureInfo.CurrentCulture);
		//    while (true)
		//    {
		//        //DateTime currentDate = DateTime.Now;
		//        List<TLMessage> newMessages = await control.GetDifference(control.UserHost.User, control.UserHost.Destination, lastDate);

		//        TLMessage message = newMessages?.LastOrDefault();
		//        if (message != null)
		//            lastDate = TLControl.ToDate(message.Date);
		//        //if (isChanged)
		//        //{
		//        //    var res = await control.GetMessagesAsync(control.CurrentUser.Destination, lastDate, null, 50);
		//        //    lastDate = currentDate;
		//        //}
		//        await Task.Delay(1000);
		//    }


		//}

	}
}