using System;
using System.Collections.Generic;
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
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }

            System.Console.WriteLine(@"Complete");
            System.Console.ReadLine();
        }

	    static void WriteData(KeyValuePair<string, string> data)
        {
            //System.Console.WriteLine(data.Key);
            Thread.Sleep(10000);
        }
    }
}
