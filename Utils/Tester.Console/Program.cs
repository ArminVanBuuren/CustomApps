using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using Utils;

namespace Tester.Console
{
	public interface ICustomFunction
	{
		string Invoke(string[] args);
	}

	public delegate void DelegateTest(string value);
	public class TestingEvents
	{
		public event DelegateTest DelegateTest;

		public void Test(string value)
		{
			DelegateTest?.Invoke(value);
		}
	}

	class  testClass
	{
		private static int _test = 0;
		public testClass()
		{
			CountTest = ++_test;
		}
		public int CountTest { get; }
	}

	class Program
	{
		static void Main(string[] args)
		{
			System.Console.WriteLine($"Start = {DateTime.Now:HH:mm:ss.fff}");
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			
			try
			{
				var list = new List<testClass>
				{
					new testClass(),
					new testClass(),
					new testClass(),
					new testClass(),
					new testClass()
				};
				var res = list.Sum(x => x.CountTest / 5);

				var limit = 4;
				var ss1 = Math.Max(0, limit == 0 ? list.Count : list.Count - limit / 2);
				var ss2 = list.Skip(ss1).ToList();
			}
			catch (Exception e)
			{
				System.Console.WriteLine(e);
			}

			stopWatch.Stop();
			System.Console.WriteLine($"Complete = {DateTime.Now:HH:mm:ss.fff} Elapsed = {stopWatch.Elapsed}");
			System.Console.ReadLine();
		}

		static void Sort()
		{
			var array = new[] { 1, 5, 7, 5, 2, 0 };
			var dd = array.OrderBy(x => x);
			var result = new int[array.Length];
			for (var i = 0; i > array.Length; i++)
			{
				var res = array[i].CompareTo(array[i + 1]);
			}

			System.Console.WriteLine(string.Join(",", result));
		}

		static void Test_GetLastDigit()
		{
			for (int i = 0; i < 51; i++)
			{
				var lastNumber = Math.Abs(i) % 10;
				var time = new TimeSpan(0, 0, 0, i);
				System.Console.WriteLine($"Number is {i} | Last number is {lastNumber} | Время выполнения: {time.ToReadableString()}");
			}

			System.Console.WriteLine(@"Enter a number:");
			while (true)
			{
				var num = System.Console.ReadLine();
				if (!int.TryParse(num, out var num1))
					break;
				var lastNumber = Math.Abs(num1) % 10;
				var time = new TimeSpan(0, 0, 0, num1);

				System.Console.WriteLine($"Last number is {lastNumber} | Время выполнения: {time.ToReadableString()}");
				System.Console.WriteLine(@"For repeat test Enter a number:");
			}
		}

		static void Test_DelegateAllEventsTo()
		{
			var test_1 = new TestingEvents();
			test_1.DelegateTest += (arg) => { System.Console.WriteLine(arg); };

			var test_2 = new TestingEvents();
			test_1.DelegateAllEventsTo(test_2);

			test_1.Test("Invoked Test_1");
			test_2.Test("Invoked Test_2");
		}

		static void Test_Lookup()
		{
			var parameters = new[]
			{
				new  { PropType = "Type_2", PropValue = "Value_3" },
				new  { PropType = "Type_3", PropValue = "Value_2" },
				new  { PropType = "Type_3", PropValue = "Value_2" },
				new  { PropType = "Type_3", PropValue = "Value_3" },
				new  { PropType = "Type_1", PropValue = "Value_1" },
				new  { PropType = "Type_1", PropValue = "Value_2" },
			};
			var data = parameters.Select(x => new { x.PropType, x.PropValue })
					.Distinct()
					.OrderBy(x => x.PropValue)
					.ToLookup(x => x.PropType, x => x.PropValue)
				//
				;
		}

		void Test_SpecifyOrderBy()
		{
			var data = new string[]
			{
				"SERVICE_CODE",
				"SourceTypeCode",
				"ExternalServiceCode",
				"DateFrom",
				"ProductStatusCode",
				"Test"
			};
			var preferences = new HashSet<string> { "DateFrom", "SERVICE_CODE", "SourceTypeCode", "ProductStatusCode", "ExternalServiceCode" };
			var orderedData = data.OrderBy(item => preferences.Concat(data).ToList().IndexOf(item));
		}

		static void Test_CustomFunction()
		{
			var codeFunc = @"public class Test1 : ICustomFunction { public string Invoke(string[] args) { return ""[SUCCESS]="" + string.Join("","", args); } }" + Environment.NewLine;
			codeFunc += @"public class Test2 : ICustomFunction { public string Invoke(string[] args) { return ""[SUCCESS]="" + string.Join("","", args); } }";
			//var codeFunc = "";
			var customFunc = new CustomFunctions
			{
				Assemblies = new CustomFunctionAssemblies
				{
					Childs = new[]
					{
						new XmlNodeValueText("System.dll")
					}
				},
				Namespaces = new XmlNodeValueText("using System;"),
				Functions = new CustomFunctionCode()
				{
					Function = new XmlNodeCDATAText[]
					{
						new XmlNodeCDATAText(codeFunc)
					}
				}
			};

			var compiler = new CustomFunctionsCompiler<ICustomFunction>(customFunc);
		}

		static void Test_CustomTemplateCalculation()
		{
			var seq = 0;
			const string template = "$1|{ test ( '$1', 'ARG_BEAUTY_SECOND' ) }|$1";
			var funcs = new Dictionary<string, Func<string[], string>>()
			{
				{
					"test",
					(args2) => $"{seq++} Date={DateTime.Now:HH:mm:ss.fff} [SUCCESS]='{string.Join(",", args2)}'"
				}
			};

			var regex = new Regex("(.+)");
			const string virtualArg = "ARG_BEAUTY_FIRST";

			var func = CODE.Calculate<Match>(template, funcs, REGEX.GetValueByReplacement);

			//for (var i = 0; i < 10000; i++)
			//	System.Console.WriteLine(func.Invoke(regex.Match(virtualArg)));

			System.Console.WriteLine(func.Invoke(regex.Match(virtualArg)));
			Thread.Sleep(1000);
			System.Console.WriteLine(func.Invoke(regex.Match(virtualArg)));
		}

		static void Test_Join()
		{
			var list1 = new[] {"222", "333", "111"};
			var list2 = new[] {"111", "222"};
			var ddd = list1.Join(
				list2,
				sss1 => sss1,
				sss2 => sss2,
				(arg1, arg2) => { return $"{arg1} = {arg2}"; });
		}

		public static void OracleCommandTest()
		{
			const string connectionString = "Data Source=vip12;User ID=tf2_cust;Password=cust;Max Pool Size=1";
			var connection = new OracleConnection(connectionString);
			var command = connection.CreateCommand();
			{
				command.CommandText = "SELECT sysdate FROM dual";
				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		static void Test_SpaCalculator()
		{
			var calc = new SpaCalculator();
			calc.Require(new Context(), (ResultedTerminalDeviceServices parameter) => { });
		}

		static void Test_GetReplacement()
		{
			var pattern = Regex.Escape("wefefw-fefew]dwdw[(efe)");
			var dd = Regex.Match("2222w1efefw-fefew]dwdw[(efe)1111", pattern);
			var res = dd.Value;

			var str1 =
				"20.05.2020 00:12:23.246 [ProcessingContent] SMSCON.MainThread[1]: Submit_sm ()  0870->375336923017 (smsc '172.24.224.6:900') messageId:RU:[1d646bc0-33e3-488b-83f8-9eb2e15876f6 / 0],message:,request:(submit: (pdu: 0 4 0 249243) (addr: 0 1 0870)  (addr: 1 1 375336923017)  (sm: enc: 8 len: 140 msg: ???Баланс Вашего лицевого счета на дату 20.05.2020 0:11:41 составляет:)  (regDelivery: 0) (validTime: ) (schedTime: 000000092436000R) (priority: 1) (opt: ) ) ";
			var regex = new Regex(@"(.+?)\s*\[\s*(.+?)\s*\]\s*(.+?)\s+(.+)", RegexOptions.IgnoreCase);
			var match = regex.Match(str1);
			System.Console.WriteLine(match.GetValueByReplacement("[ $1:{dd.MM.yyyy HH:mm:ss.fff $1:{dd.MM.yyyy HH:mm:ss.fff} ]", (value, format) =>
			{
				if (DateTime.TryParseExact(value, format, null, DateTimeStyles.None, out var result))
				{
					return result.ToString("dd.MM.yyyy");
				}

				return value;
			}));


			var str2 = "4903TelnetSocket.TX_MESSAGE[[9] [CRM.BL:10976600638] -> [FORIS.SPA.BPM.03:773:5190532542775] -> [HL:16]:self]20.05.2020 3:19:3312345215";
			var regex2 = new Regex(@"(\d+?)\u0001(.+?)\u0001(.+?)\u0001(.+?)\u0001(.*?)\u0001(\d*)".ReplaceUTFCodeToSymbol(), RegexOptions.IgnoreCase);
			var match2 = regex2.Match(str2);
			System.Console.WriteLine(match2.GetValueByReplacement("$4.$6", (value, format) =>
			{
				if (DateTime.TryParseExact(value, format, null, DateTimeStyles.None, out var result))
				{
					return result.ToString("dd.MM.yyyy");
				}

				return value;
			}));
		}

		class Test1
		{
			public Test1(long id)
			{
				ID = id;
			}

			public long ID { get; }
		}

		static void Test_CustomOrderBy()
		{
			var dd = new List<Test1>()
			{
				new Test1(292000),
				new Test1(21),
				new Test1(23),
				new Test1(29),
				new Test1(30)
			};
			var result1 = dd.AsQueryable();
			var ddd = result1.OrderBy("ID").ToList();


			var regex = new Regex(@"(.+?)(\s+|$)", RegexOptions.IgnoreCase);
			var match = regex.Match("01.05.2020 15:30:05 word2 word3");
			System.Console.WriteLine(match.GetValueByReplacement("[ $1 : {dd.MM.yyyy} ]", (value, format) =>
			{
				if (DateTime.TryParseExact(value, format, null, DateTimeStyles.None, out var result))
				{
					return result.ToString("dd.MM.yyyy HH:mm:ss.fff");
				}

				return value;
			}));
		}

		static void Test_DateTime()
		{
			var invariant = DateTime.Now.ToString("dddd, yyyy 'с.' MMMM d 'күнэ'", CultureInfo.GetCultureInfo("sah-RU"));
			Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
			Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU");
			var dateStr = "  2020-03-26 18:45:21.009   ";

			if (DateTime.TryParseExact(invariant, "dd.MM.yyyy HH:mm:ss.fff", null, DateTimeStyles.AllowWhiteSpaces, out var result))
			{
				System.Console.WriteLine(result);
			}

			if (TIME.TryParseAnyDate(invariant, DateTimeStyles.AllowWhiteSpaces, out var result2))
			{
				System.Console.WriteLine(result2);
			}


			//         DateTime.Now.ToString("")
			//var test = DateTime.TryParse(" 01.05.2020   15:30:05 ".Replace(",", "."), out var date1);
			//         var test2 = DateTime.TryParseExact("01.05.2020 15:30:05".Replace(",", "."), "dd.MM.yyyy HH:mm:ss.fff", null, DateTimeStyles.AllowWhiteSpaces, out var date2);
			//         var date3 = Convert.ToDateTime("  01.05\\2020   15:30:05  ");

			//System.Console.WriteLine(date1);
			//System.Console.WriteLine(date2);
			//System.Console.WriteLine(date3);
		}

		static void Test_1()
		{
			var listForIgnore = new List<string>
			{
				"ProvisionList",
				"ModificationList",
				"WithdrawalList",
				"REGISTERED_LIST",
				"INITIAL_LIST",
				"USER_INFO"
			};

			var listForIgnore2 = new HashSet<string>
			{
				"ProvisionList",
				"ModificationList",
				"WithdrawalList",
				"REGISTERED_LIST",
				"INITIAL_LIST",
				"USER_INFO"
			};

			var stop = new Stopwatch();
			stop.Start();
			for (var i = 0; i < 10000; i++)
				listForIgnore.Contains("USER_INFO");
			stop.Stop();
			System.Console.WriteLine(stop.Elapsed);

			stop.Reset();
			stop.Start();
			for (var i = 0; i < 10000; i++)
				listForIgnore2.Contains("USER_INFO");
			stop.Stop();
			System.Console.WriteLine(stop.Elapsed);
		}

		static void Authorization()
		{
			var securedPassword = new SecureString();
			foreach (var ch in "IO%11111")
				securedPassword.AppendChar(ch);
			var creditail = new NetworkCredential(@"foris6\vhovanskij", securedPassword, "foris6");
			using (var dd1111 = new NetworkConnection(@"\\f6-crm-gui01\c$", creditail))
			{

			}
		}

		static void MultitaskTest()
		{
			var listOfData = new List<KeyValuePair<string, string>>();
			for (var i = 1; i <= 200; i++)
			{
				listOfData.Add(new KeyValuePair<string, string>(($"{i,+3}").Replace(" ", "0") + ".txt", $"Data created - {DateTime.Now:HH:mm:ss.fff}"));
			}

			var listOfData2 = new List<KeyValuePair<string, string>>(listOfData);

			System.Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} - Started");

			var _multiTaskingHandler = new MTActionResult<KeyValuePair<string, string>>(
				WriteData,
				listOfData,
				listOfData.Count,
				ThreadPriority.Lowest);
			_multiTaskingHandler.IsCompeted += (sender, eventArgs) => { System.Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} - Multitask completed"); };
			_multiTaskingHandler.StartAsync();

			Task.Factory.StartNew(() =>
			{
				var action = (Action<KeyValuePair<string, string>>) WriteData;
				var priority = ThreadPriority.Lowest;
				var listOfTask = new List<Task>();
				foreach (var data in listOfData2)
				{
					var task = Task.Factory.StartNew((input) =>
					{
						Thread.CurrentThread.Priority = priority;
						action.Invoke((KeyValuePair<string, string>) input);
					}, data);
					listOfTask.Add(task);
				}
				
				Task.WaitAll(listOfTask.ToArray());
				System.Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} - Raw completed");
			});
		}

		static void WriteData(KeyValuePair<string, string> data)
		{
			//System.Console.WriteLine(data.Key);
			Thread.Sleep(10000);
		}
	}
}