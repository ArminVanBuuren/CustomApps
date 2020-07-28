using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tester.Console
{
	public static class OSRequest
	{
		public const string OrderTypeCode = "OM.EDM";
		public const string FnsId = "FnsId";
		public const string INN = "INN";
		public const string KPP = "KPP";
		public const string PersonalAccountNumber = "PersonalAccountNumber";
		public const string OpеratorEdoName = "OpеratorEdoName";
		public const string OperatorEdoCode = "OperatorEdoCode";
		public const string CustomerName = "CustomerName";
		public const string StartDate = "StartDate";
		public const string UserID = "UserID";
		public const string Status = "Status";
	}

	public class OrderAttr
	{
		public Dictionary<string, object> Attributes { get; set; }

		public long OrderId { get; set; }
	}


	public enum CounteragentStatus
	{
		/// <summary> Неизвестный статус </summary>

		UnknownCounteragentStatus,

		/// <summary> Является партнёром </summary>

		IsMyCounteragent,

		/// <summary> Участник пригласил МТС к партнёрству </summary>

		InvitesMe,

		/// <summary> МТС пригласили Участника к партнёрству </summary>

		IsInvitedByMe,

		/// <summary> Участник отказался от партнёрства </summary>

		RejectsMe,

		/// <summary> МТС отказался от партнёрства </summary>

		IsRejectedByMe,

		/// <summary> Не является Контрагентом</summary>

		NotInCounteragentList,

		/// <summary> Участник пригласил МТС к партнёрству </summary>

		InvitesMeSpecial
	}

	public class InteractionInfo
	{
		/// <summary>
		/// ФНС ИД ящика Участника ЭДО
		/// </summary>

		public string FnsId { get; set; }

		/// <summary>
		/// Статус взаимодействия
		/// </summary>

		public CounteragentStatus Status { get; set; }

		/// <summary>
		/// Код Оператора ЭДО
		/// </summary>

		public string EdmCode { get; set; }

		/// <summary>
		/// Наименование Оператора ЭДО
		/// </summary>

		public string EdmName { get; set; }

		/// <summary>
		/// Дата отправки запроса
		/// </summary>

		public DateTime StartDate { get; set; }
	}

	public class InteractionInfoOrder : InteractionInfo
	{
		public long OrderId { get; set; }
	}

	public class EdmTest
	{
		public static void Test()
		{
			var list = new List<string>();
			var dd = list.Select(x => x == "");

			var res = Enum.TryParse(null, true, out CounteragentStatus statu2s);

			var res2 = DateTime.TryParse(null, out var date);

			IEnumerable<InteractionInfo> intractions = new[]
			{
					new InteractionInfo()
					{
						FnsId = "111111",
						Status = CounteragentStatus.IsMyCounteragent,
						EdmCode = "111"
					},
					new InteractionInfo()
					{
						FnsId = "222222",
						Status = CounteragentStatus.InvitesMe,
						EdmCode = "222"
					},
					new InteractionInfo()
					{
						FnsId = "333333",
						Status = CounteragentStatus.InvitesMe,
						EdmCode = "333"
					},
					new InteractionInfo()
					{
						FnsId = "444444",
						Status = CounteragentStatus.UnknownCounteragentStatus,
						EdmCode = "444"
					},
				};

			var existOrdersByInn = new List<OrderAttr>
				{
					new OrderAttr()
					{
						Attributes = new Dictionary<string, object>
						{
							["FnsId"] = 111111,
							["OperatorEdoCode"] = 111,
							["Status"] = "UnknownCounteragentStatus",
							["StartDate"] = DateTime.Now.AddDays(-1).ToString("G")
						},
						OrderId = 1
					},
					new OrderAttr()
					{
						Attributes = new Dictionary<string, object>
						{
							["FnsId"] = null,
							["OperatorEdoCode"] = 222,
							["Status"] = "UnknownCounteragentStatus",
							["StartDate"] = DateTime.Now.AddDays(-2).ToString("G")
						},
						OrderId = 2
					},
					new OrderAttr()
					{
						Attributes = new Dictionary<string, object>
						{
							["FnsId"] = 333333,
							["OperatorEdoCode"] = 333,
							["Status"] = "UnknownCounteragentStatus",
							["StartDate"] = DateTime.Now.AddDays(-1).ToString("G")
						},
						OrderId = 3
					},
					new OrderAttr()
					{
						Attributes = new Dictionary<string, object>
						{
							["FnsId"] = null,
							["OperatorEdoCode"] = 444,
							["Status"] = "IsInvitedByMe",
							["StartDate"] = DateTime.Now.AddDays(-5).ToString("G")
						},
						OrderId = 4
					},
					new OrderAttr()
					{
						Attributes = new Dictionary<string, object>
						{
							["FnsId"] = null,
							["OperatorEdoCode"] = 444,
							["Status"] = "IsInvitedByMe",
							["StartDate"] = DateTime.Now.AddDays(-1).ToString("G")
						},
						OrderId = 5
					},
				};


			var existOrders = existOrdersByInn
				.Where(x => x.Attributes.ContainsKey(OSRequest.FnsId) || x.Attributes.ContainsKey(OSRequest.OperatorEdoCode))
				.Select(x => new InteractionInfoOrder
				{
					OrderId = x.OrderId,
					FnsId = x.Attributes.TryGetValue(OSRequest.FnsId, out var fnsId) && !string.IsNullOrWhiteSpace(fnsId?.ToString()) ? fnsId.ToString() : null,
					EdmCode = x.Attributes.TryGetValue(OSRequest.OperatorEdoCode, out var edmCode) && !string.IsNullOrWhiteSpace(edmCode?.ToString())
						? edmCode.ToString()
						: null,
					Status = x.Attributes.TryGetValue(OSRequest.Status, out var statusStr) && Enum.TryParse(statusStr?.ToString(), true, out CounteragentStatus status)
						? status
						: CounteragentStatus.UnknownCounteragentStatus,
					StartDate = x.Attributes.TryGetValue(OSRequest.StartDate, out var startDateStr) && DateTime.TryParse(startDateStr?.ToString(), out var startDate)
						? startDate
						: DateTime.MinValue
				});

			var existOrdersFull = existOrders.Where(x => x.FnsId != null && x.EdmCode != null).ToDictionary(x => new Tuple<string, string>(x.FnsId, x.EdmCode));
			// находим заявки где есть только код оператора ЭДО
			var existOrdersWithEdmCode = existOrders.Where(x => x.FnsId == null && x.EdmCode != null).GroupBy(x => x.EdmCode).ToDictionary(x => x.Key, x => x.ToList());


			var finishedOrders = new List<long>();


			// обновляем статус заявки или добавляем fnsId, если его изначально не было
			void ModifyOrders(InteractionInfo interaction, InteractionInfoOrder order)
			{
				if (interaction.Status == CounteragentStatus.IsMyCounteragent)
				{
					finishedOrders.Add(order.OrderId);
				}
				else if (order.FnsId == null || interaction.Status != order.Status)
				{
					System.Console.WriteLine("Modified: order={0} fnsId={1} status={2} startDate={3}", order.OrderId, interaction.FnsId, interaction.Status, interaction.StartDate);
				}
			}

			foreach (var interaction in intractions)
			{
				if (existOrdersFull.TryGetValue(new Tuple<string, string>(interaction.FnsId, interaction.EdmCode), out var order1))
				{
					interaction.Status = interaction.Status == CounteragentStatus.UnknownCounteragentStatus ? order1.Status : interaction.Status;
					interaction.StartDate = order1.StartDate;

					ModifyOrders(interaction, order1);
				}
				else if (existOrdersWithEdmCode.TryGetValue(interaction.EdmCode, out var orders2))
				{
					var withStatuses = orders2.Where(x => x.Status != CounteragentStatus.UnknownCounteragentStatus);
					var last = withStatuses.Any() ? withStatuses.OrderByDescending(x => x.StartDate).First() : orders2.OrderByDescending(x => x.StartDate).First();

					interaction.Status = interaction.Status == CounteragentStatus.UnknownCounteragentStatus ? last.Status : interaction.Status;
					interaction.StartDate = last.StartDate;

					foreach (var order in orders2.OrderBy(x => x.StartDate))
						ModifyOrders(interaction, order);
				}
			}

			// завершаем заявки в OS, если статус = IsMyCounteragent
			if (finishedOrders.Count > 0)
				foreach (var id in finishedOrders)
					System.Console.WriteLine("Finished: order={0}", id);
		}
	}
}
