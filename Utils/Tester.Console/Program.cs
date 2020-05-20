﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Utils;

namespace Tester.Console
{
    class Program
    {
	    static void Main(string[] args)
	    {
		    try
		    {
			    Test_DateTime();
		    }
		    catch (Exception e)
		    {
			    System.Console.WriteLine(e);
		    }

		    System.Console.WriteLine(@"Complete");
		    System.Console.ReadLine();
	    }

		static void Test_GetReplacement()
		{
			var str1 = "20.05.2020 00:12:23.246 [ProcessingContent] SMSCON.MainThread[1]: Submit_sm ()  0870->375336923017 (smsc '172.24.224.6:900') messageId:RU:[1d646bc0-33e3-488b-83f8-9eb2e15876f6 / 0],message:,request:(submit: (pdu: 0 4 0 249243) (addr: 0 1 0870)  (addr: 1 1 375336923017)  (sm: enc: 8 len: 140 msg: ???Баланс Вашего лицевого счета на дату 20.05.2020 0:11:41 составляет:)  (regDelivery: 0) (validTime: ) (schedTime: 000000092436000R) (priority: 1) (opt: ) ) ";
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
			Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
			Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU");
			var dateStr = "  2020-03-26 18:45:21.009   ";

			if (DateTime.TryParseExact(dateStr, "dd.MM.yyyy HH:mm:ss.fff", null, DateTimeStyles.AllowWhiteSpaces, out var result))
			{
				dateStr = result.ToString("O");
			}

			if (TIME.TryParseAnyDate(dateStr, DateTimeStyles.AllowWhiteSpaces, out var result2))
			{

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
			_multiTaskingHandler.IsCompeted += (sender, eventArgs) =>
			{
				System.Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} - Multitask completed");
			};
			_multiTaskingHandler.StartAsync();

			Task.Factory.StartNew(() =>
			{
				var action = (Action<KeyValuePair<string, string>>)WriteData;
				var priority = ThreadPriority.Lowest;
				var listOfTask = new List<Task>();
				foreach (var data in listOfData2)
				{
					var task = Task.Factory.StartNew((input) =>
					{
						Thread.CurrentThread.Priority = priority;
						action.Invoke((KeyValuePair<string, string>)input);
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
