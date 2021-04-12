using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.XmlDiffPatch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using Org.XmlUnit;
using Org.XmlUnit.Builder;
using Org.XmlUnit.Diff;
//using TeleSharp.TL;
//using TLSharp.Core.MTProto.Crypto;
using Utils;
using Utils.Crypto;


namespace Tester.ConsoleTest
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

	class testClass
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

		public class OrderDocumentData : IEquatable<OrderDocumentData>
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

			public bool Equals(OrderDocumentData other)
			{
				if (ReferenceEquals(null, other)) return false;
				if (ReferenceEquals(this, other)) return true;
				return OrderId == other.OrderId && DeliveryDate.Equals(other.DeliveryDate) && RecipientFullName == other.RecipientFullName && DeliveryStatusCode == other.DeliveryStatusCode && ReasonNoDeliveryCode == other.ReasonNoDeliveryCode;
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != this.GetType()) return false;
				return Equals((OrderDocumentData) obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					var hashCode = OrderId.GetHashCode();
					hashCode = (hashCode * 397) ^ DeliveryDate.GetHashCode();
					hashCode = (hashCode * 397) ^ (RecipientFullName != null ? RecipientFullName.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (DeliveryStatusCode != null ? DeliveryStatusCode.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (ReasonNoDeliveryCode != null ? ReasonNoDeliveryCode.GetHashCode() : 0);
					return hashCode;
				}
			}
		}

		//private static void CompareXml(string file1, string file2)
		//{
		//	XmlReader reader1 = XmlReader.Create(new StringReader(file1));
		//	XmlReader reader2 = XmlReader.Create(new StringReader(file2));

		//	string diffFile = @"C:\tmp\1.txt";
		//	StringBuilder differenceStringBuilder = new StringBuilder();

		//	FileStream fs = new FileStream(diffFile, FileMode.Create);
		//	XmlWriter diffGramWriter = XmlWriter.Create(fs);

		//	XmlDiff xmldiff = new XmlDiff(
		//		XmlDiffOptions.IgnoreChildOrder
		//		| XmlDiffOptions.IgnoreNamespaces
		//		| XmlDiffOptions.IgnorePrefixes
		//		| XmlDiffOptions.IgnoreWhitespace
		//		| XmlDiffOptions.IgnoreComments
		//		| XmlDiffOptions.IgnoreDtd);
		//	bool bIdentical = xmldiff.Compare(file1, file2, false, diffGramWriter);

		//	diffGramWriter.Close();
		//}

		//public static void CompareXml2(string file1, string file2)
		//{
		//	ISource control = Input.FromFile(file1).Build();
		//	ISource test = Input.FromFile(file2).Build();
		//	IDifferenceEngine diff = new DOMDifferenceEngine();
		//	diff.DifferenceListener += (comparison, outcome) =>
		//	{
		//		try
		//		{
		//			Console.WriteLine("found a difference: \"{0}\" \"{1}\"", comparison, outcome);
		//		}
		//		catch (Exception ex)
		//		{
		//			Console.WriteLine(ex);
		//		}
		//	};
		//	diff.Compare(control, test);
		//}




		class TestBase
		{
			//static int i = 0;
			private readonly int _curI = 0;
			//public Testbase() => _curI = i++;


			public override bool Equals(object obj)
			{
				Console.WriteLine($"({_curI}) - Equals");
				return base.Equals(obj);
			}

			public override int GetHashCode()
			{
				Console.WriteLine($"({_curI}) - GetHashCode");
				return 1;
			}

			public static bool operator ==(TestBase left, TestBase right)
			{
				Console.WriteLine($"==");
				return Equals(left, right);
			}

			public static bool operator !=(TestBase left, TestBase right)
			{
				Console.WriteLine($"!=");
				return !Equals(left, right);
			}

			public override string ToString() => "Testbase";
		}

		struct Testing
		{
			public static bool operator ==(Testing c1, Testing c2)
			{
				return c1.Equals(c2);
			}

			public static bool operator !=(Testing c1, Testing c2)
			{
				return !c1.Equals(c2);
			}

			public override bool Equals(object obj)
			{
				var eq = base.Equals(obj);
				Console.WriteLine($"Equals - {eq}");
				return eq;
			}

			public override int GetHashCode()
			{
				var hash = base.GetHashCode();
				Console.WriteLine($"Hash - {hash}");
				return hash;
			}
		}

		//class Tesitng2 : Testing
		//{
		//	public List<string> Prop { get; set; }
		//}

		//class Tesitng3 : Tesitng2
		//{
		//	public List<string> Prop2 { get; set; }
		//}

		public class ChangeBillAttributesRequest
		{
			public ChangeBillAttributesOperationRules OperationRules { get; set; }
		}

		public enum ChangeBillAttributesOperationRules
		{
			/// <summary>
			/// Нет правил
			/// </summary>
			[EnumMember]
			None = 0,

			/// <summary>
			/// Использовать переменные по умолчанию
			/// </summary>
			[EnumMember]
			UseDefaultValues = 1,

			/// <summary>
			/// Игнорируем ошибку изменения не нужны
			/// </summary>
			[EnumMember]
			IgnoreErrorChangesNotNeed = 2,
		}

		class PersonalAccount
		{
			public string PersonalAccountNumber { get; set;  }
		}

		public class FieldRules
		{
			/// <summary>
			/// Точная длина входных данных.
			/// </summary>
			[DataMember]
			public int ExactLength { get; set; }

			/// <summary>
			/// Минимальная длина входных данных.
			/// </summary>
			[DataMember]
			public int MinLength { get; set; }

			/// <summary>
			/// Максимальная длина входных данных.
			/// </summary>
			[DataMember]
			public int MaxLength { get; set; }

			/// <summary>
			/// Регулярное выражение для проверки входных данных.
			/// </summary>
			[DataMember]
			public string Regexp { get; set; }

			public override string ToString()
				=> $"ExactLength={ExactLength} MinLength={MinLength} MaxLength={MaxLength}";
		}

		class InputDataRestrictions
		{
			public int? FieldLength { get; set; }
			public int? MinFieldLength { get; set; }
			public int? MaxFieldLength { get; set; }


		}

		static FieldRules MapToFieldRules(this InputDataRestrictions dataRestrictions)
		{
			int exactLength = dataRestrictions.FieldLength ?? 0;
			int minLength = dataRestrictions.MinFieldLength ?? 0;
			int maxLength = dataRestrictions.MaxFieldLength ?? 0;

			return new FieldRules
			{
				ExactLength = exactLength,
				MinLength = dataRestrictions.MinFieldLength ?? Math.Min(exactLength, maxLength),
				MaxLength = dataRestrictions.MaxFieldLength ?? Math.Max(minLength, exactLength),
			};
		}


		class TerminalDevicePricesRequest
		{
			public List<TarifficationContextBase> TarifficationRequests { get; set; }

			public TarifficationContextService[] TarifficationContextServices { get; set; }
		}

		class TarifficationContextBase
		{

		}

		class TarifficationContextService : TarifficationContextBase
		{

		}

		class ActionOnPeriodicalProductTarifficationRequest : TarifficationContextBase
		{

		}

		class PeriodicalProductActionTarifficationRequest : TarifficationContextBase
		{

		}

		class PeriodicalProductTarifficationRequest : TarifficationContextBase
		{
			
		}

		private static List<object> GetNewContextServices
		(
			List<ActionOnPeriodicalProductTarifficationRequest> terminalDevicePricesRequests,
			List<PeriodicalProductActionTarifficationRequest> periodicalProductActionTarifficationRequests,
			TarifficationContextService[] tarifficationContextServices
		)
			=> tarifficationContextServices == null
				   ? terminalDevicePricesRequests.Select(MapToOtspContextServiceWithActionType).Concat(periodicalProductActionTarifficationRequests.Select(MapToOtspContextServiceWithActionType)).ToList()
				   : tarifficationContextServices.Select(MapToOtspContextServiceWithActionType).ToList();

		private static object MapToOtspContextServiceWithActionType(ActionOnPeriodicalProductTarifficationRequest actionOnServicePriceRequest)
		{
			return new object();
		}

		private static object MapToOtspContextServiceWithActionType(PeriodicalProductActionTarifficationRequest periodicalProductActionTarifficationRequest)
		{
			return new object();
		}

		private static object MapToOtspContextServiceWithActionType(TarifficationContextService tarifficationContextService)
		{
			return new object();
		}

		static string ReturnTypeString(object arg)
		{
			Thread.Sleep(3000);
			Console.WriteLine($"{arg} is int?  - {arg is int}");
			Console.WriteLine($"{arg} is long? - {arg is long}");
			Console.WriteLine($"{arg} is decimal? - {arg is decimal}");
			Console.WriteLine($"{arg} is double? - {arg is double}");
			//Console.WriteLine($"Casting<int> - {(int)(double)arg}");
			//Console.WriteLine($"Casting<long> - {(long)arg}");
			//Console.WriteLine($"Casting<decimal> - {(decimal)arg}");
			//Console.WriteLine($"Casting<double> - {(decimal)arg}");
			return arg.ToString();
		}

		class Person
		{
			public int Age { get; set; }
			public string Name { get; set; }

			// A person is uniquely identified by name, so let's use it for equality.
			public override bool Equals(object obj) => obj is Person person && person.Name == Name;

			// For lazyness reasons we (incorrectly) use the age as the hash code.
			//public override int GetHashCode() => Age;

			public override string ToString() => $"Age = {Age}; Name = {Name}";
		}

		public abstract class Shape { }
		public class Circle : Shape { }

		public interface IContainer<out T>
		{
			T Figure { get; }
		}
		public class Container<T> : IContainer<T>
		{
			private T figure;
			public Container(T figure)
			{
				this.figure = figure;
			}
			public T Figure // реализуем абстрактное свойсво из интерфейса
			{
				get { return figure; } // свойство должно быть только для чтения.
			}
		}

		delegate string testDeleg(object arg);

		[Flags]
		public enum TerminalDeviceProductStateFlags
		{
			/// <summary>Никаких</summary>
			[EnumMember] None = 0,
			/// <summary>Активная ли услуга у абонента</summary>
			[EnumMember] IsActivated = 1,
			/// <summary>Было ли разовое списание</summary>
			[EnumMember] IsOneTimeCharged = 2,
			/// <summary>Было ли периодическое списание за пользование</summary>
			[EnumMember] IsPeriodicalChargingStarted = 4,
			/// <summary>Была ли заблокирована услуга</summary>
			[EnumMember] IsBlocked = 8,
		}

		class MyClass
		{
			public List<Product> Products { get; set; }
		}

		class Product
		{
			public List<Service> Services { get; set; }
		}

		class Service
		{
			public string ServiceCode { get; set; }
		}

		private static bool IsValidJson(string strInput)
		{
			if (string.IsNullOrWhiteSpace(strInput)) { return false; }
			strInput = strInput.Trim();
			if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
				(strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
			{
				try
				{
					var obj = JToken.Parse(strInput);
					return true;
				}
				catch (JsonReaderException jex)
				{
					//Exception in parsing json
					Console.WriteLine(jex.Message);
					return false;
				}
				catch (Exception ex) //some other exception
				{
					Console.WriteLine(ex.ToString());
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		static void Main(string[] args)
		{
			repeat:
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			Console.WriteLine($"Start = {DateTime.Now:HH:mm:ss.fff}");

			try
			{
				var logger = new Logger("TestTestTest1");
				logger.LogWriteInfo("11111111111111");
				logger.LogWriteError(new Exception("22222222222222222"));
				logger.LogWriteError("3333333333");
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

			stopWatch.Stop();
			Console.WriteLine($"Complete = {DateTime.Now:HH:mm:ss.fff} Elapsed = {stopWatch.Elapsed}");
			Console.WriteLine("Press Enter for repeat");
			if (Console.ReadKey().Key == ConsoleKey.Enter)
				goto repeat;
			Console.ReadLine();
		}

		static void Old_2021_04_09()
		{
			var instance = new { Name = "Alex", Age = 27 };

			var product = new
			{
				IsActivated = true,
				IsOneTimeCharged = true,
				IsPeriodicalChargingStarted = true,
				IsBlocked = true
			};

			TerminalDeviceProductStateFlags state = TerminalDeviceProductStateFlags.None;

			if (product.IsActivated)
				state |= TerminalDeviceProductStateFlags.IsActivated;
			if (product.IsOneTimeCharged)
				state |= TerminalDeviceProductStateFlags.IsOneTimeCharged;
			if (product.IsPeriodicalChargingStarted)
				state |= TerminalDeviceProductStateFlags.IsPeriodicalChargingStarted;
			if (product.IsBlocked)
				state |= TerminalDeviceProductStateFlags.IsBlocked;




			Console.WriteLine(state.HasFlag(TerminalDeviceProductStateFlags.IsActivated));
			Console.WriteLine(state.HasFlag(TerminalDeviceProductStateFlags.IsOneTimeCharged));
			Console.WriteLine(state.HasFlag(TerminalDeviceProductStateFlags.IsPeriodicalChargingStarted));
			Console.WriteLine(state.HasFlag(TerminalDeviceProductStateFlags.IsBlocked));


			var fff1 = new Product
			{
				Services = new List<Service>
					{
						new Service
						{
							ServiceCode = "111"
						}
					}
			};
			var fff2 = new MyClass
			{
				Products = new List<Product>
					{
						new Product
						{
							Services = new List<Service>
							{
								new Service
								{
									ServiceCode = "222"
								}
							}
						},
						null,
						new Product()
					}
			};

			IEnumerable<Service> GetServiceListFromCmServicesFull(IEnumerable<Service> terminalDeviceServices)
			{
				return terminalDeviceServices == null
					? new List<Service>()
					: terminalDeviceServices;
			}

			var result = GetServiceListFromCmServicesFull(fff1?.Services)
				.Concat(fff2?.Products?.SelectMany(p => GetServiceListFromCmServicesFull(p?.Services)))
				.ToList();

			int xx = 11;
			//ref int xRef = ref xx;

			//Console.WriteLine(xRef is int);



			////Shape shape = new Circle(); // предварительный UpCast
			////IContainer<Circle> container = new Container<Shape>(shape);

			//Circle circle = new Circle();
			//IContainer<Shape> container = new Container<Circle>(circle);




			//ReturnTypeString(55);
			//ReturnTypeString(67.555m);


			//var terminalDevicePricesRequest = new TerminalDevicePricesRequest
			//{
			//	TarifficationRequests = new List<TarifficationContextBase>
			//	{
			//		new PeriodicalProductTarifficationRequest(),
			//		new PeriodicalProductActionTarifficationRequest(),
			//		new ActionOnPeriodicalProductTarifficationRequest()
			//	}
			//};

			//var actionsOnPeriodicalProductTarifficationRequests = terminalDevicePricesRequest.TarifficationRequests
			//	.OfType<ActionOnPeriodicalProductTarifficationRequest>().ToList();

			//var periodicalProductActionsTarifficationRequests = terminalDevicePricesRequest.TarifficationRequests
			//	.OfType<PeriodicalProductActionTarifficationRequest>().ToList();

			//var newServices = GetNewContextServices(actionsOnPeriodicalProductTarifficationRequests, periodicalProductActionsTarifficationRequests, terminalDevicePricesRequest.TarifficationContextServices);




			//Console.WriteLine(string.IsNullOrWhiteSpace("  "));
			//Console.WriteLine(string.IsNullOrWhiteSpace(""));
			//Console.WriteLine(string.IsNullOrWhiteSpace(null));

			//List<(InputDataRestrictions, InputDataRestrictions)> restrictions = new List<(InputDataRestrictions, InputDataRestrictions)>
			//{
			//	(new InputDataRestrictions
			//		{
			//			FieldLength = 111
			//		}, new InputDataRestrictions
			//		{
			//			FieldLength = 222
			//		})
			//};

			//var dict1 = new Dictionary<InputDataRestrictions, InputDataRestrictions>
			//{
			//	{
			//		new InputDataRestrictions
			//		{
			//			MinFieldLength = 111,
			//		},
			//		new InputDataRestrictions
			//		{
			//			MinFieldLength = 111,
			//		}
			//	}
			//};

			//var dict2 = new Dictionary<InputDataRestrictions, InputDataRestrictions>
			//{
			//	{
			//		new InputDataRestrictions
			//		{
			//			MinFieldLength = 111,
			//		},
			//		new InputDataRestrictions
			//		{
			//			MinFieldLength = 111,
			//		}
			//	}
			//};

			//if (dict1.TryGetValue(dict2.First().Key, out var res))
			//{

			//}

			//var rs =  restrictions.FirstOrDefault(x => x.Item1.FieldLength == 112).Item2;

			//var s1 = "Blue";
			//var sb = new StringBuilder("Bl");
			//sb.Append("ue");
			//var s2 = sb.ToString();

			//Console.WriteLine(s1 == s2); // True
			//Console.WriteLine(object.ReferenceEquals(s1, s2)); // False

			//unsafe
			//{
			//	int* x; // определение указателя
			//	int y = 10; // определяем переменную
			//}

			object gg1 = "0";
			object gg2 = "0";
			Console.WriteLine(gg1 == gg2);                          // true
			Console.WriteLine(gg1.Equals(gg2));                     // true
			Console.WriteLine(object.ReferenceEquals(gg1, gg2));    // true

			//var dict = new Dictionary<object, object>
			//{
			//	{gg1, gg1},
			//	{gg2, gg2}
			//};

			//object fff = 11;


			//object test1 = new TestBase();
			//object test2 = new TestBase();
			//var dict = new Dictionary<object, object>
			//{
			//	{test1, test1},
			//	{test2, test2}
			//};
			//Console.WriteLine("Start");
			//Console.WriteLine(test1 == test2);
			//Console.WriteLine("Pause");
			//Console.WriteLine(test1.Equals(test2));


			//Console.WriteLine("111" == "111");
			//Console.WriteLine("111".Equals("111"));


			//var test1 = new Testing();
			//var test2 = new Testing();
			//Console.WriteLine(test1 == test2);
			//Console.WriteLine(test1.Equals(test2));


			//var childRequest = (Testing)new Tesitng3();
			//var test = "[MgSendingUseEmailOcatTempate] Child request: DocRequestId=" + childRequest.ToString() +
			//	(childRequest is Tesitng2 bilReq ? $"; Bill={bilReq.Prop}" : string.Empty) +
			//	(childRequest is Tesitng3 bilReq2 ? $"; Bill={bilReq2.Prop2}" : string.Empty);

			//var ddd = true;
			//var favColours = new Dictionary<Person, string>();

			//var p = new Person
			//{
			//	Age = 1,
			//	Name = "Alice"
			//};

			//favColours[p] = "Blue";

			//// Happy birthday Alice!
			//p.Age = 2;
			//favColours[p] = "Green";

			//Console.WriteLine(favColours.Count); // 2

			//var keys = favColours.Keys.ToArray();
			//Console.WriteLine(object.ReferenceEquals(keys[0], keys[1])); // True
			//Console.WriteLine(favColours[p]);

			//foreach (var test in dict)
			//{
			//	Console.WriteLine(test);
			//}

			//Console.WriteLine(nameof(ChangeBillAttributesOperationRules.IgnoreErrorChangesNotNeed));

			//var dd = default(int).ToString(CultureInfo.InvariantCulture);
			//var ddd = Convert.ToDateTime(default(DateTime).ToString(CultureInfo.InvariantCulture));

			//var dddd = new List<PersonalAccount> { new PersonalAccount() { PersonalAccountNumber = "111" } }.SingleOrDefault(x => x == new PersonalAccount());

			////Console.WriteLine(test1 == test2);
			//var Request = new ChangeBillAttributesRequest
			//{
			//	OperationRules = ChangeBillAttributesOperationRules.UseDefaultValues
			//};

			//var CheckNeedChanges = new Func<bool>(() => true);

			//var pans = new List<PersonalAccount>
			//{
			//	new PersonalAccount {PersonalAccountNumber = "11111"},
			//	new PersonalAccount {PersonalAccountNumber = "22222"},
			//	new PersonalAccount {PersonalAccountNumber = "33333"},
			//	new PersonalAccount {PersonalAccountNumber = "44444"},
			//	new PersonalAccount {PersonalAccountNumber = "55555"},
			//};

			//var pans2 = new List<string>
			//{
			//	"33333",
			//	"55555",
			//};

			//var dd1 = pans.Any(x => x.PersonalAccountNumber == "11111" || x.PersonalAccountNumber == "22222");

			//if (pans.Any(x => x.PersonalAccountNumber == "11111" || x.PersonalAccountNumber == "22222"))
			//{

			//}
			//else
			//{
			//	Console.WriteLine("111");
			//}

			//pans.Select(x => x.PersonalAccountNumber).Except(pans2).ForEach(Console.WriteLine);



			//var buttons = new List<Button> { new Button(), new Button()};
			//for (var b = 0; b < buttons.Count; b++)
			//{
			//	var elem = buttons[b];
			//	var b1 = b;
			//	elem.onclick += (s, arg) => { Console.WriteLine(b1); };
			//	// все три обработчика берут b из одной области видимости!
			//}

			//foreach (var VARIABLE in buttons)
			//{
			//	VARIABLE.Send();
			//}

			//if (Request.OperationRules.HasFlag(ChangeBillAttributesOperationRules.IgnoreErrorChangesNotNeed) || CheckNeedChanges())
			//{
			//	Console.WriteLine($"222");
			//}
			//else
			//{
			//	Console.WriteLine($"111");
			//}

			//var result1 = !Request.OperationRules.HasFlag(ChangeBillAttributesOperationRules.IgnoreErrorChangesNotNeed) && !CheckNeedChanges();
			//var result2 = !(Request.OperationRules.HasFlag(ChangeBillAttributesOperationRules.IgnoreErrorChangesNotNeed) || CheckNeedChanges());

			//Console.WriteLine($"result1: = {result1}");
			//Console.WriteLine($"result2: = {result2}");

			//Console.WriteLine(Testtest());
		}

		class Button
		{
			public event EventHandler onclick;

			public void Send()
			{
				onclick?.Invoke(this, EventArgs.Empty);
			}
		}

		static void DivMod()
		{
			var test = Console.ReadLine();
			var a1 = int.Parse(test.Split(' ')[0]);
			var a2 = int.Parse(test.Split(' ')[1]);
			var dd1 = a1 / a2;
			var dd2 = a1 % a2;

			Console.WriteLine(dd1);
			Console.WriteLine(dd2);
		}

		public enum BalanceAdjustmentType
		{
			/// <summary>Неизвестно</summary>
			[EnumMember] Unknown,
			/// <summary>Обычный обещанный платеж.</summary>
			[EnumMember] PromisedPayment,
			/// <summary>Обещанный платеж в обеспечении рассрочки.</summary>
			[EnumMember] Instalment,
		}

		static string Testtest()
		{
			Console.WriteLine(nameof(BalanceAdjustmentType.PromisedPayment));

			var documentTypeId = 415;
			switch (documentTypeId)
			{
				case 273:
				case 274:
				case 901:
					return "Edo";
				case 324:
				case 415:
					return "Pdf";
				case 357:
				case 610:
				case 611:
					return "Xml";
				default:
					return "-1";
			}
		}

		static void Test()
		{
			

			var test = "05-10-20 10:55 - 05.10.21 10:55";

			//foreach (var time in test.Split('-'))
			//{
			//	if (DateTime.TryParse(time, CultureInfo.InvariantCulture, DateTimeStyles.None, out var res))
			//		Console.WriteLine(res.ToString("G"));

			//	if (DateTime.TryParseExact(time, "dd.mm.yyyy hh24:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var res2))
			//		Console.WriteLine("second: " + res2.ToString("G"));
			//}


			//var periods = Regex.Split(test, @"(\d+.)+");

			var periods = Regex.Matches(test, @"(\d+.)+");
			if (periods.Count >= 1 && DateTime.TryParse(periods[0].Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var start))
				Console.WriteLine(start.ToString("G"));
			if (periods.Count > 1 && DateTime.TryParse(periods[1].Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var end))
				Console.WriteLine(end.ToString("G"));

			return;


			//var cells = new string[] { "1", "2", "3", "4" };
			//Console.WriteLine($"\"{string.Join("\", \"", cells.Select(x => x))}\"");
			//return;

			//var sss = new List<Tesitng2> { new Tesitng2 { } };
			//Console.WriteLine("1. - " + (sss.FirstOrDefault(x => !x.Prop.IsEmpty()) ?? sss.First()).Prop?.FirstOrDefault());
			////Console.WriteLine("2. - " + (sss.FirstOrDefault(x => x.Prop == "1112") ?? sss.First()).Prop?.FirstOrDefault());

			//var asynnc = new Func<Task<bool>>(async () =>
			//{
			//	Console.Write("Working");
			//	for (var i = 0; i < 10; i++)
			//	{
			//		Thread.Sleep(500);
			//		Console.Write(".");
			//	}

			//	Console.WriteLine("\r\nFinished");
			//	return false;
			//});

			////var func = Task.Factory.StartNew<bool>(asynnc);
			//asynnc.RunSync();
			//asynnc.RunSync2();

			//var subsOnMe = IO.SafeReadFile(@"C:\tmp\подписаныНаМеня.txt");
			//var mySubs = IO.SafeReadFile(@"C:\tmp\моиПодписки.txt");


			//var subsOnMe1 = subsOnMe.Split('\n').Select(x => x.Trim()).ToList();
			//var mySubs1 = mySubs.Split('\n').Select(x => x.Trim()).ToList();

			//Console.WriteLine($"На меня не подписаны:");
			//mySubs1.Except(subsOnMe1).ForEach(x => Console.WriteLine(x));

			//Console.WriteLine($"\r\nЯ не подписан на:");
			//subsOnMe1.Except(mySubs1).ForEach(x => Console.WriteLine(x));

			//CompareXml2(@"C:\tmp\expected.txt", @"C:\tmp\sent.txt");
			//return;	
			//Thread.Sleep(1000);

			//var i = 0;
			//var test11 = (from dd in new string[] { "test", "ste", "efef" } 
			//			  select (i++, dd)).ToArray();

			//new string[] { "test", "ste", "efef" }.Select(x => (i++, x)).ToArray();

			//OrderDocumentData datatest = null;
			//if (datatest is OrderDocumentData datatest11)
			//{
			//	Console.WriteLine("true!!");
			//}

			//object test = "test";
			//string test2 = "test";
			//var list = new List<string>();
			//var sss = list.SingleOrDefault(x => x == "");

			//Console.WriteLine(test2.Equals(test));

			//startString = DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture);
			//Console.WriteLine($"Stop. - {DateTime.Parse(startString, CultureInfo.InvariantCulture, DateTimeStyles.None)}\r\n...........................");
			//Console.WriteLine($"Stop. - {DateTime.Parse("2020-11-11T17:00:00")}\r\n...........................");

			//Thread.Sleep(5000);
			//goto test;

			//var testtt  = $"{DateTime.Now:dd.MM.yyyy_HH-mm-ss-ff}";


			//var tesss1 = new List<OrderDocumentRequestTemp>
			//            {
			//                new OrderDocumentRequestTemp
			//                {
			//                    DocumentTypeId = 1,
			//                    ContractNumber = "6"
			//                },
			//                new OrderDocumentRequestTemp
			//                {
			//                    DocumentTypeId = 3,
			//                    ContractNumber = "222"
			//                },
			//                new OrderDocumentRequestTemp
			//                {
			//                    DocumentTypeId = 1,
			//                    ContractNumber = "333"
			//                },
			//                new OrderDocumentRequestTemp
			//                {
			//                    DocumentTypeId = 2,
			//                    ContractNumber = "444"
			//                }
			//            };


			//var ssss = tesss1.Max(x => x.ContractNumber);

			//var testttt = tesss1.GroupBy(x => x.DocumentTypeId).Select(x => x.ToList());



			//var psList = GetOrderDocumentByPersonalAccountRequests();
			//            var cList = GetOrderDocumentByContractRequests();

			//            void tesss(IEnumerable<OrderDocumentRequestTemp> items)
			//            {
			//                foreach (var data in items)
			//                {
			//		Console.WriteLine($"\r\n\r\nContractNumber={data.ContractNumber}");
			//		foreach (var table in data.AdditionalParameters.Tables.OfType<DataTable>())
			//                    {
			//                        Console.WriteLine($"Table Name='{table.TableName}' Count={table.Rows.Count}");
			//                        foreach (var row in table.Rows.OfType<DataRow>())
			//                        {
			//                            Console.WriteLine($"\tRow Count={table.Columns.Count}");

			//                            foreach (var column in table.Columns.OfType<DataColumn>())
			//                            {
			//                                Console.WriteLine($"\t\tName='{column.ColumnName}' Value='{row[column.ColumnName]}'");
			//                            }
			//                        }
			//                    }
			//                }
			//}

			//            void Tessst2(Dictionary<string, List<DocParameter>> items)
			//            {
			//                Console.WriteLine(string.Join("\r\n", items.Select(x => $"ContractNumber={x.Key}\r\n{string.Join("\r\n", x.Value.Select(x2 => $"{x2.ToString2()}"))}\r\n")));
			//}

			//            tesss(psList);
			//            Console.WriteLine(new string('-', 25));

			//tesss(cList);
			//            Console.WriteLine(new string('-', 25));

			//var psParams = new Dictionary<string, List<DocParameter>>();
			//            foreach (var fff in psList)
			//            {
			//                psParams.Add(fff.ContractNumber, fff.AdditionalParameters.ConvertToListParameters());
			//            }
			//            Tessst2(psParams);
			//            Console.WriteLine(new string('-', 25));

			//var cParams = new Dictionary<string, List<DocParameter>>();
			//foreach (var fff2 in cList)
			//            {
			//                cParams.Add(fff2.ContractNumber, fff2.AdditionalParameters.ConvertToListParameters());
			//}
			//            Tessst2(cParams);
			//Console.WriteLine(new string('-', 25));


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


			//Console.WriteLine($"Is Changed: {test.IsTableValueChanged("EdmDeliveryStatus", "EdmDeliveryStatusCode", "44")}");
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
			[DataMember] public Guid DataItemId { get; set; }

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
			var list = new List<string> {"1", "2", "3", "4", "5", "6"};

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
						Properties = columns.Select(col => new DocProperty {Name = col.ColumnName, Value = row[col.ColumnName]}).ToArray()
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
			var array = new[] {4, 1, 5, 7, 5, 2, 0, 0, 2, 4, 1, 5, 7, 5, 2, 0, 0, 2};

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

			Console.WriteLine(string.Join(",", array));
		}

		static void Test_GetLastDigit()
		{
			for (int i = 0; i < 251; i++)
			{
				var lastNumber = Math.Abs(i) % 10;
				var time = new TimeSpan(0, 0, 0, i);
				Console.WriteLine($"Number is {i} | Last number is {lastNumber} | Время выполнения: {time.ToReadableString()}");
			}

			Console.WriteLine(@"Enter a number:");
			while (true)
			{
				var num = Console.ReadLine();
				if (!int.TryParse(num, out var num1))
					break;
				var lastNumber = Math.Abs(num1) % 10;
				var time = new TimeSpan(0, 0, 0, num1);

				Console.WriteLine($"Last number is {lastNumber} | Время выполнения: {time.ToReadableString()}");
				Console.WriteLine(@"For repeat test Enter a number:");
			}
		}

		static void Test_DelegateAllEventsTo()
		{
			var test_1 = new TestingEvents();
			test_1.DelegateTest += (arg) => { Console.WriteLine(arg); };

			var test_2 = new TestingEvents();
			test_1.DelegateAllEventsTo(test_2);

			test_1.Test("Invoked Test_1");
			test_2.Test("Invoked Test_2");
		}

		static void Test_Lookup()
		{
			var parameters = new[]
			{
				new {PropType = "Type_2", PropValue = "Value_3"},
				new {PropType = "Type_3", PropValue = "Value_2"},
				new {PropType = "Type_3", PropValue = "Value_2"},
				new {PropType = "Type_3", PropValue = "Value_3"},
				new {PropType = "Type_1", PropValue = "Value_1"},
				new {PropType = "Type_1", PropValue = "Value_2"},
			};
			var data = parameters.Select(x => new {x.PropType, x.PropValue})
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
			var preferences = new HashSet<string> {"DateFrom", "SERVICE_CODE", "SourceTypeCode", "ProductStatusCode", "ExternalServiceCode"};
			var orderedData = data.OrderBy(item => preferences.Concat(data).ToList().IndexOf(item));
		}

		static void Test_CustomFunction()
		{
			var codeFunc = @"public class Test1 : ICustomFunction { public string Invoke(string[] args) { return ""[SUCCESS]="" + string.Join("","", args); } }" +
			               Environment.NewLine;
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
			//	Console.WriteLine(func.Invoke(regex.Match(virtualArg)));

			Console.WriteLine(func.Invoke(regex.Match(virtualArg)));
			Thread.Sleep(1000);
			Console.WriteLine(func.Invoke(regex.Match(virtualArg)));
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
					Console.WriteLine(command.ExecuteScalar());
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
			Console.WriteLine(match.GetValueByReplacement("[ $1:{dd.MM.yyyy HH:mm:ss.fff $1:{dd.MM.yyyy HH:mm:ss.fff} ]", (value, format) =>
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
			Console.WriteLine(match2.GetValueByReplacement("$4.$6", (value, format) =>
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
			Console.WriteLine(match.GetValueByReplacement("[ $1 : {dd.MM.yyyy} ]", (value, format) =>
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
				Console.WriteLine(result);
			}

			if (TIME.TryParseAnyDate(invariant, DateTimeStyles.AllowWhiteSpaces, out var result2))
			{
				Console.WriteLine(result2);
			}


			//         DateTime.Now.ToString("")
			//var test = DateTime.TryParse(" 01.05.2020   15:30:05 ".Replace(",", "."), out var date1);
			//         var test2 = DateTime.TryParseExact("01.05.2020 15:30:05".Replace(",", "."), "dd.MM.yyyy HH:mm:ss.fff", null, DateTimeStyles.AllowWhiteSpaces, out var date2);
			//         var date3 = Convert.ToDateTime("  01.05\\2020   15:30:05  ");

			//Console.WriteLine(date1);
			//Console.WriteLine(date2);
			//Console.WriteLine(date3);
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
			Console.WriteLine(stop.Elapsed);

			stop.Reset();
			stop.Start();
			for (var i = 0; i < 10000; i++)
				listForIgnore2.Contains("USER_INFO");
			stop.Stop();
			Console.WriteLine(stop.Elapsed);
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

			Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} - Started");

			var _multiTaskingHandler = new MTActionResult<KeyValuePair<string, string>>(
				WriteData,
				listOfData,
				listOfData.Count,
				ThreadPriority.Lowest);
			_multiTaskingHandler.IsCompeted += (sender, eventArgs) => { Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} - Multitask completed"); };
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
				Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} - Raw completed");
			});
		}

		static void WriteData(KeyValuePair<string, string> data)
		{
			//Console.WriteLine(data.Key);
			Thread.Sleep(10000);
		}
	}
}