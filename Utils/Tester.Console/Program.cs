using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Policy;
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

	class test1
	{
		public bool IsWorking { get; set; } = true;
		public bool OnPause { get; set; } = true;
	}

	static class test
	{
		internal static bool MatchInteraction(this InteractionInfo x, InteractionInfo y)
		{
			return (!string.IsNullOrEmpty(x.FnsId) && x.FnsId == y.FnsId)
			       || (x.Operator.Code == y.Operator.Code && x.Counteragent.Inn == y.Counteragent.Inn);
		}
	}

	class DocumentItem
	{
		public List<DeliveryMethod> DeliveryMethods { get; set; }
	}

	class DeliveryMethod
	{
		public int Code { get; set; }
		public List<DocumentFormat> Formats { get; set; }
	}

	class DocumentFormat
	{
		public int FormatId { get; set; }
	}

    static class Program
	{


		public enum EquipmentAction
		{
			Test1,
			Test2,
			Test3
		}

		public enum SaEquipmentAction
		{
			Test1,
			Test2,
			Test3
		}

		class test1 : test
		{
			public string Commnet { get; set; }
		}

		class test2 : test
		{
			public string Commnet { get; set; }
		}

		class test
		{
			public EquipmentAction EquipmentAction { get; set; }
		}


		public class ordertest1 : Ordertest
		{
			public string PersonalAccount { get; set; }
			
			public string Test1 { get; set; }
		}

		public class ordertest2 : Ordertest
		{
			public string PersonalAccount { get; set; }
		
			public string Test2 { get; set; }
		}

		public interface iordertest
		{
			string PersonalAccount { get; set; }
			string PersonalAccountId { get; set; }
			string Uri { get; set; }
		}

		public class Ordertest
		{
			public string PersonalAccountId { get; set; }
			public string Uri { get; set; }
		}

		static void Testttt()
		{
			var list1 = new List<ordertest1>
				{
					new ordertest1
					{
						PersonalAccount = "1111",
						PersonalAccountId = "1111",
						Uri = "1111",
						Test1 = "1111"
					},
					new ordertest1
					{
						PersonalAccount = "2222",
						PersonalAccountId = "2222",
						Uri = "2222",
						Test1 = "2222"
					}
				};

			var list2 = new List<ordertest2>
				{
					new ordertest2
					{
						PersonalAccount = "3333",
						PersonalAccountId = "3333",
						Uri = "3333",
						Test2 = "3333"
					},
					new ordertest2
					{
						PersonalAccount = "4444",
						PersonalAccountId = "4444",
						Uri = "4444",
						Test2 = "4444"
					}
				};

			//var request = new test1();
			//request.EquipmentAction = (EquipmentAction)1;
			//var testest = (test)request;
			//var dddd = (test2)testest;

			//var test = (SaEquipmentAction)Enum.Parse(typeof(SaEquipmentAction), request.EquipmentAction.ToString());
		}

		public static DateTime GetLastExecutionDate(string lastExecutionDateStr)
		{
			if (string.IsNullOrEmpty(lastExecutionDateStr)
			   || !DateTime.TryParse(lastExecutionDateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var lastExecutionDate))
				return DateTime.Now.AddMinutes(-15);
			return lastExecutionDate.AddSeconds(-1);
		}

		public class OrderDocumentData
		{
			/// <summary>
			/// Номер родительской заявки
			/// </summary>
			
			public long OrderId { get; set; }

			/// <summary>
			/// Дата доставки 
			/// </summary>
			
			public DateTime DeliveryDate { get; set; }

			/// <summary>
			/// Получатель корреспонденции 
			/// </summary>
			
			public string RecipientFullName { get; set; }

			/// <summary>
			/// Статус доставки 
			/// </summary>
			
			public string DeliveryStatusCode { get; set; }

			/// <summary>
			/// Причина недоставки 
			/// </summary>
			
			public string ReasonNoDeliveryCode { get; set; }
		}

		static void Main(string[] args)
		{
			repeat:
			System.Console.WriteLine($"Start = {DateTime.Now:HH:mm:ss.fff}");
			var stopWatch = new Stopwatch();
			stopWatch.Start();

			try
			{

				//Thread.Sleep(1000);

				//var i = 0;
				//var test11 = (from dd in new string[] { "test", "ste", "efef" } 
				//			  select (i++, dd)).ToArray();

				//new string[] { "test", "ste", "efef" }.Select(x => (i++, x)).ToArray();

				//OrderDocumentData datatest = null;
				//if (datatest is OrderDocumentData datatest11)
				//{
				//	System.Console.WriteLine("true!!");
				//}

				//object test = "test";
				//string test2 = "test";
				//var list = new List<string>();
				//var sss = list.SingleOrDefault(x => x == "");

				//System.Console.WriteLine(test2.Equals(test));

				//startString = DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture);
				//System.Console.WriteLine($"Stop. - {DateTime.Parse(startString, CultureInfo.InvariantCulture, DateTimeStyles.None)}\r\n...........................");
				//System.Console.WriteLine($"Stop. - {DateTime.Parse("2020-11-11T17:00:00")}\r\n...........................");

				//Thread.Sleep(5000);
				//goto test;

                var tesss1 = new List<OrderDocumentRequestTemp>
                {
                    new OrderDocumentRequestTemp
                    {
                        DocumentTypeId = 1,
                        ContractNumber = "111"
                    },
                    new OrderDocumentRequestTemp
                    {
                        DocumentTypeId = 3,
                        ContractNumber = "222"
                    },
                    new OrderDocumentRequestTemp
                    {
                        DocumentTypeId = 1,
                        ContractNumber = "333"
                    },
                    new OrderDocumentRequestTemp
                    {
                        DocumentTypeId = 2,
                        ContractNumber = "444"
                    }
                };

                var testttt = tesss1.GroupBy(x => x.DocumentTypeId).Select(x => x.ToList());



				var psList = GetOrderDocumentByPersonalAccountRequests();
                var cList = GetOrderDocumentByContractRequests();

                void tesss(IEnumerable<OrderDocumentRequestTemp> items)
                {
                    foreach (var data in items)
                    {
						System.Console.WriteLine($"\r\n\r\nContractNumber={data.ContractNumber}");
						foreach (var table in data.AdditionalParameters.Tables.OfType<DataTable>())
                        {
                            System.Console.WriteLine($"Table Name='{table.TableName}' Count={table.Rows.Count}");
                            foreach (var row in table.Rows.OfType<DataRow>())
                            {
                                System.Console.WriteLine($"\tRow Count={table.Columns.Count}");

                                foreach (var column in table.Columns.OfType<DataColumn>())
                                {
                                    System.Console.WriteLine($"\t\tName='{column.ColumnName}' Value='{row[column.ColumnName]}'");
                                }
                            }
                        }
                    }
				}

                void Tessst2(Dictionary<string, List<DocParameter>> items)
                {
                    System.Console.WriteLine(string.Join("\r\n", items.Select(x => $"ContractNumber={x.Key}\r\n{string.Join("\r\n", x.Value.Select(x2 => $"{x2.ToString2()}"))}\r\n")));
				}

                tesss(psList);
                System.Console.WriteLine(new string('-', 25));

				tesss(cList);
                System.Console.WriteLine(new string('-', 25));

				var psParams = new Dictionary<string, List<DocParameter>>();
                foreach (var fff in psList)
                {
                    psParams.Add(fff.ContractNumber, fff.AdditionalParameters.ConvertToListParameters());
                }
                Tessst2(psParams);
                System.Console.WriteLine(new string('-', 25));

				var cParams = new Dictionary<string, List<DocParameter>>();
				foreach (var fff2 in cList)
                {
                    cParams.Add(fff2.ContractNumber, fff2.AdditionalParameters.ConvertToListParameters());
				}
                Tessst2(cParams);
				System.Console.WriteLine(new string('-', 25));


				//            long? test1 = int.MaxValue;

				//            int? test2 = test1 <= int.MaxValue ? (int?) test1 : null;

				//            test1 = null;
				//            int? test3 = test1 <= int.MaxValue ? (int?) test1 : null;


				//var test = new OrderAttributeContainer
				//            {
				//                TableAttributes = new Dictionary<string, TableValuePaged>()
				//                {
				//                    {
				//                        "EdmDeliveryStatus", new TableValuePaged
				//                        {
				//                            Columns = new Dictionary<string, List<object>>
				//                            {
				//                                {
				//                                    "EdmDeliveryStatusCode", new List<object>
				//                                    {
				//                                        "10", "44"
				//                                    }
				//                                }
				//                            }
				//                        }
				//                    }
				//                }
				//            };


				//System.Console.WriteLine($"Is Changed: {test.IsTableValueChanged("EdmDeliveryStatus", "EdmDeliveryStatusCode", "44")}");
			}
			catch (Exception e)
			{
				System.Console.WriteLine(e);
			}

			stopWatch.Stop();
			System.Console.WriteLine($"Complete = {DateTime.Now:HH:mm:ss.fff} Elapsed = {stopWatch.Elapsed}");
			System.Console.WriteLine("Press Enter for repeat");
			if (System.Console.ReadKey().Key == ConsoleKey.Enter)
				goto repeat;
			System.Console.ReadLine();
		}

        [Serializable]
        public class DocParameter
		{
            public DocParameter()
            {
            }

            public DocParameter(string name, DocParameterItem[] items)
            {
                this.Name = name;
                this.Items = items;
            }

            
            public string Name { get; set; }

            public DocParameterItem[] Items { get; set; }

            public override string ToString() => $"Table Name='{Name}' Count={Items.Length}";

            public string ToString2() => $"{ToString()}{string.Join("", Items.Select(x => $"\r\n\t{x.ToString2()}"))}";
		}

        [Serializable]
        public class DocParameterItem
		{
            public DocParameterItem()
            {
            }

            public DocParameterItem(DocProperty[] properties)
            {
                this.Properties = properties;
            }

            public DocProperty[] Properties { get; set; }

            public override string ToString() => $"Row Count={Properties.Length}";

            public string ToString2() => $"{ToString()}{string.Join("", Properties.Select(x => $"\r\n\t\t{x}"))}";
		}

        [Serializable]
        public class DocProperty
		{
            public DocProperty()
            {
            }

            public DocProperty(string name, object value)
            {
                this.Name = name;
                this.Value = value;
            }

            public string Name { get; set; }

            public object Value { get; set; }

            public override string ToString() => $"Name='{Name}' Value='{Value}'";
        }

        public class OrderDocumentRequestTemp
        {
            [DataMember]
            public Guid DataItemId { get; set; }

            /// <summary>
            /// Номер контракта
            /// </summary>
            [DataMember]
            public string ContractNumber { get; set; }

            /// <summary>
            /// Номер лицевого счета
            /// </summary>
            [DataMember]
            public string[] PersonalAccountNumbers { get; set; }

            /// <summary>
            /// Идентификатор наименования документа
            /// </summary>
            [DataMember]
            public int DocumentNameId { get; set; }

            /// <summary>
            /// Идентификатор типа документа
            /// </summary>
            [DataMember]
            public int DocumentTypeId { get; set; }

            /// <summary>
            /// Номер заявки Order Storage, с которой связан заказ документа в Doc
            /// </summary>
            [DataMember]
            public long OrderId { get; set; }

            /// <summary>
            /// Дополнительные параметры, передаваемые в Doc.
            /// Если бы сейчас был жив Сталин, он бы любителями DataSet'ов баню топил
            /// </summary>
            [DataMember]
            public DataSet AdditionalParameters { get; set; }

            /// <summary>
            /// Время жизни документа в днях
            /// </summary>
            [DataMember]
            public int DaysOfDocumentLife { get; set; }

            /// <summary>
            /// Идентификтор заявки на формирование документа, возвращаемый системой Doc
            /// </summary>
            [DataMember]
            public long DocRequestId { get; set; }

            /// <summary>
            /// Ссылка на заказанный документ
            /// </summary>
            [DataMember]
            public string Uri { get; set; }

			/// <summary>
			/// Идентификатор поставщика услуг.
			/// </summary>
			[DataMember]
			public long ServiceProviderId { get; set; }

            public override string ToString() => $"ContractNumber='{ContractNumber}'";
		}

		public static IEnumerable<OrderDocumentRequestTemp> GetOrderDocumentByPersonalAccountRequests()
        {
            var list = new List<string> {"1", "2", "3", "4", "5", "6"};
            var temp = new[] {"121212121212", "34343434343", "5454545454", "56565665565", "767676767676", "787878787878"};

			return list.Select(paGroup => new OrderDocumentRequestTemp
            {
                DataItemId = Guid.NewGuid(),
                PersonalAccountNumbers = temp,
                ContractNumber = paGroup,
                DaysOfDocumentLife = 999,
                DocumentNameId = 1111,
                DocumentTypeId = 1111,
                AdditionalParameters = GetParameters(temp.ToList(), DateTime.MinValue, DateTime.MaxValue),
                ServiceProviderId = 123456789
            });
        }

        public static IEnumerable<OrderDocumentRequestTemp> GetOrderDocumentByContractRequests()
        {
            var list = new List<string> { "1", "2", "3", "4", "5", "6" };

			return list.Select(c => new OrderDocumentRequestTemp
            {
                DataItemId = Guid.NewGuid(),
                ContractNumber = c,
                DaysOfDocumentLife = 999,
                DocumentNameId = 2222,
                DocumentTypeId = 2222,
                AdditionalParameters = GetParameters(long.Parse(c), c + "_12345", DateTime.MinValue, DateTime.MaxValue),
                ServiceProviderId = 123456789
			});
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000")]
        private static DataSet GetParameters(long contractId, string contractNumber, DateTime dateFrom, DateTime dateTo)
        {
            DataSet result = CreateDataSet();

            DataTable contractTable = result.Tables.Add("Contract");
            contractTable.Columns.Add("Id", typeof(long));
            contractTable.Columns.AddNumberColumn(typeof(string));
            contractTable.Rows.Add(contractId, contractNumber);

            result.AddPeriodsTable(dateFrom, dateTo);

            return result;
        }

        private static readonly DateTime _dateTimeNullValue = new DateTime(100, 1, 1);
        private static DataSet GetParameters(List<string> personalAccounts, DateTime dateFrom, DateTime dateTo)
        {
            DataSet result = CreateDataSet();

            var personalAccountWithPeriodListTable = result.Tables.Add("PersonalAccountWithPeriodList");
            personalAccountWithPeriodListTable.Columns.AddNumberColumn(typeof(long));
            personalAccountWithPeriodListTable.Columns.AddFromColumn();
            personalAccountWithPeriodListTable.Columns.AddToColumn();

            personalAccounts.ForEach(personalAccount =>
            {
                personalAccountWithPeriodListTable.Rows.Add
                (
                    long.Parse(personalAccount, CultureInfo.InvariantCulture),
                    DateTime.MinValue,
					DateTime.MaxValue
				);
            });

            result.AddPeriodsTable(dateFrom, dateTo);

            return result;
        }

        private static void AddPeriodsTable(this DataSet dataSet, DateTime dateFrom, DateTime dateTo)
        {
            DataTable periodTable = dataSet.Tables.Add("DocumentPeriod");
            periodTable.Columns.AddFromColumn();
            periodTable.Columns.AddToColumn();
            periodTable.Rows.Add(dateFrom, dateTo);
        }

		private static DataSet CreateDataSet() => new DataSet("I_Additional_Params");
        private static void AddNumberColumn(this DataColumnCollection columns, Type type) => columns.Add("Number", type);
        private static void AddFromColumn(this DataColumnCollection columns) => columns.Add("From", typeof(DateTime));
        private static void AddToColumn(this DataColumnCollection columns) => columns.Add("To", typeof(DateTime));

        private static List<DocParameter> ConvertToListParameters(this DataSet additionalParameters)
        {
            var result = new List<DocParameter>();

            if (additionalParameters.Tables.Count == 0)
                return result;

            var dd = additionalParameters.Tables.OfType<DataTable>();

            result.AddRange(
                from table in additionalParameters.Tables.OfType<DataTable>()
                from row in table.Rows.OfType<DataRow>()
                select CreateDocParameter(table.TableName, table.Columns.OfType<DataColumn>(), row));

            return result;
        }

        private static DocParameter CreateDocParameter(string name, IEnumerable<DataColumn> columns, DataRow row)
            => new DocParameter
            {
                Name = name,
                Items = new[]
                {
                    new DocParameterItem
                    {
                        Properties = columns.Select(col => new DocProperty{ Name = col.ColumnName, Value = row[col.ColumnName] } ).ToArray()
                    }
                }
            };

		class OrderAttributeContainer
        {
			public Dictionary<string, TableValuePaged> TableAttributes { get; set; }

		}

        class TableValuePaged
        {
            public Dictionary<string, List<object>> Columns { get; set; }
        }


		private static bool IsTableValueChanged<T>(this OrderAttributeContainer order, string tableName, string attrName, T newValue)
            where T : class
            => order.TableAttributes.IsNullOrEmpty() || !order.TableAttributes.TryGetValue(tableName, out var tableValue)
                                                     || tableValue.Columns.IsNullOrEmpty() || !tableValue.Columns.TryGetValue(attrName, out var attrValueList)
                                                     || attrValueList.IsNullOrEmpty() || !newValue.Equals(attrValueList.Last());

		static void Test_Linq()
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

		static void Test_Sort()
		{
			var array = new[] { 4, 1, 5, 7, 5, 2, 0, 0, 2, 4, 1, 5, 7, 5, 2, 0, 0, 2 };

			var iter = 0;

			test:
			iter++;

			for (var i = 0; i < array.Length - 1; i++)
			{
				var a = array[i];
				var b = array[i + 1];
				var res = a.CompareTo(b);

				if (res == 1)
				{
					array[i] = b;
					array[i + 1] = a;
					i = 0;
				}
			}

			if (iter <= 2)
				goto test;

			System.Console.WriteLine(string.Join(",", array));
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

		static void Test_SpecifyOrderBy()
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
			const string oradb = "Data Source=(DESCRIPTION =" 
			                     + "(ADDRESS = (PROTOCOL = TCP)(HOST = msk-ora-cd-02.mtsit.local)(PORT = 1521))" 
			                     + "(CONNECT_DATA ="
			                     + "(SERVER = DEDICATED)" 
			                     + "(SID = vip12)));" 
			                     + "User Id=tf2_cust;Password=cust;";
			const string connectionString = "Data Source=vip12;User ID=tf2_cust;Password=cust;Max Pool Size=1";
			var connection = new OracleConnection(oradb);
			try
			{
				var command = connection.CreateCommand();
				{
					command.CommandText = "SELECT sysdate - 100 FROM dual";
					connection.Open();
					System.Console.WriteLine(command.ExecuteScalar());
				}
			}
			finally
			{
				connection.Close();
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