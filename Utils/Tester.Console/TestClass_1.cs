using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Tester.ConsoleTest
{
	public interface ITariffPlanDataStorage
	{
		/// <summary>
		/// Получение тарифного плана по идентификатору приложения обслуживания
		/// </summary>
		/// <param name="terminalDeviceId">Идентификатор приложения обслуживания</param>
		/// <returns>Идентификатор тарифного плана</returns>
		int GetTariffPlanId(long terminalDeviceId);

		/// <summary>
		/// Получение тарифного плана по идентификатору приложения обслуживания
		/// </summary>
		int GetTariffPlanId(long terminalDeviceId, DateTime dateOfSearch);

		/// <summary>
		/// Получение тарифного плана по идентификатору приложения обслуживания
		/// </summary>
		int GetTariffPlanId(long terminalDeviceId, DateTime? dateOfSearch);

		/// <summary>
		/// Получение тарифного плана по номеру телефона
		/// </summary>
		int GetTariffPlanIdByMsisdn(string msisdn);

		/// <summary>
		/// Полученик кода тарифного плана по номеру телефона.
		/// </summary>
		string GetTariffPlanCode(string msisdn);

		/// <summary>
		/// Возвращает код тарифного плана.
		/// </summary>
		/// <param name="tariffPlanId">Идентификатор тарифного плана.</param>
		/// <returns>Код тарифного плана.</returns>
		string GetTariffPlanCode(int tariffPlanId);

		/// <summary>
		/// Получение кода тарифного плана на IN-платформе
		/// </summary>
		/// <param name="tariffPlanId">Идентификатор тарифного плана</param>
		/// <exception cref="TariffPlanNotFoundException">В случае, если не задан идентификатор тарифного плана на IN-платформе.</exception>
		int GetINTariffPlanCode(int tariffPlanId);

		/// <summary>
		/// Проверяет, входит ли тарифный план в группу тарифных планов
		/// </summary>
		/// <param name="tariffPlanId">Идентификатор тарифного плана</param>
		/// <param name="tariffPlanGroupType">Код группы тарифных планов</param>
		bool IsTariffPlanInGroup(int tariffPlanId, string tariffPlanGroupType);

		/// <summary>
		/// Возвращает признак вхождения тарифного плана в группу тарифных планов.
		/// </summary>
		/// <param name="tariffPlanId">Идентификатор тарифного плана.</param>
		/// <returns>true, если тарифный план входит в группу тарифных планов, false в ином случае.</returns>
		bool IsTariffPlanIsAccumulator(int tariffPlanId);

		/// <summary>
		/// Получение идентификатора тарифного плана по коду тарифного плана.
		/// </summary>
		/// <param name="tariffPlanCode">Код тарифного плана.</param>
		/// <returns>Идентификатор тарифного плана.</returns>
		int GetTariffPlanIdByCode(string tariffPlanCode);

		/// <summary>
		/// Возвращает информацию о тарифном плане.
		/// </summary>
		/// <param name="tariffPlanId">Идентификатор тарифного плана.</param>
		DataTable GetTariffPlanById(int tariffPlanId);

		/// <summary>
		/// Возвращает идентификатор поставщика услуг для тарифного плана.
		/// </summary>
		/// <param name="tariffPlanCode">Код тарифного плана.</param>
		int GetTariffPlanServiceProviderId(string tariffPlanCode);
	}

	[DataContract(Namespace = "ContractsV001")]
	public class TariffPlanIdentifier
	{
		/// <summary/>
		public TariffPlanIdentifier()
			: this(null, null)
		{
		}

		/// <summary/>
		public TariffPlanIdentifier(long id)
		{
			Id = id;
		}

		/// <summary/>
		public TariffPlanIdentifier(string code)
		{
			Code = code;
		}

		/// <summary/>
		public TariffPlanIdentifier(long? id, string code)
		{
			Id = id;
			Code = code;
		}

		/// <summary>
		/// Идентификатор.
		/// </summary>
		[DataMember]
		public long? Id { get; set; }

		/// <summary>
		/// Уникальный код.
		/// </summary>
		[DataMember]
		public string Code { get; set; }

		/// <summary>
		/// Возвращает true, если указан Id или Code.
		/// </summary>
		public bool HasValue => Id.HasValue && Id.Value > 0 || !string.IsNullOrEmpty(Code);

		/// <summary>
		/// Преобразовывает значение типа <see cref="long"/> в экземпляр класса <see cref="TariffPlanIdentifier"/>.
		/// </summary>
		/// <param name="tariffPlanId">Идентификатор тарифного плана.</param>
		/// <returns>Экземпляр класса <see cref="TariffPlanIdentifier"/>.</returns>
		public static implicit operator TariffPlanIdentifier(long tariffPlanId)
		{
			return new TariffPlanIdentifier { Id = tariffPlanId };
		}

		/// <summary>
		/// Преобразовывает значение типа <see cref="string"/> в экземпляр класса <see cref="TariffPlanIdentifier"/>.
		/// </summary>
		/// <param name="tariffPlanCode">Код тарифного плана.</param>
		/// <returns>Экземпляр класса <see cref="TariffPlanIdentifier"/>.</returns>
		public static implicit operator TariffPlanIdentifier(string tariffPlanCode)
		{
			return new TariffPlanIdentifier { Code = tariffPlanCode };
		}
	}

	/// <summary>
	/// Методы-расширения для класса <see cref="TariffPlanIdentifier"/>.
	/// </summary>
	public static class TariffPlanIdentifierExtensions
	{
		/// <summary>
		/// Возвращает признак того, что идентификатор ПО не заполнен.
		/// </summary>
		public static bool IsEmpty(this TariffPlanIdentifier tariffPlan)
		{
			return tariffPlan == null ||
				   (!tariffPlan.Id.HasValue || tariffPlan.Id.Value <= 0) && string.IsNullOrEmpty(tariffPlan.Code);
		}
	}

	public static class TariffPlanDataStorageExtensions
	{
		/// <summary>
		/// Метод возвращает идентификатор тарифного плана
		/// </summary>
		/// <param name="tariffPlanDataStorage"><see cref="ITariffPlanDataStorage"/></param>
		/// <param name="tariffPlanIdentifier"><see cref="TariffPlanIdentifier"/></param>
		public static long GetTariffPlanId(this ITariffPlanDataStorage tariffPlanDataStorage, TariffPlanIdentifier tariffPlanIdentifier)
		{
			if (tariffPlanIdentifier == null)
				throw new ArgumentNullException("tariffPlanIdentifier");

			if (tariffPlanIdentifier.Id.HasValue && tariffPlanIdentifier.Id.Value > 0)
				return tariffPlanIdentifier.Id.Value;

			if (!string.IsNullOrEmpty(tariffPlanIdentifier.Code))
				return tariffPlanDataStorage.GetTariffPlanIdByCode(tariffPlanIdentifier.Code);

			throw new ArgumentException("Id or Code must be specified.", "tariffPlanIdentifier");
		}

		/// <summary>
		/// Метод возвращает код тарифного плана
		/// </summary>
		/// <param name="tariffPlanDataStorage"><see cref="ITariffPlanDataStorage"/></param>
		/// <param name="tariffPlanIdentifier"><see cref="TariffPlanIdentifier"/></param>
		/// <returns></returns>
		public static string GetTariffPlanCode(this ITariffPlanDataStorage tariffPlanDataStorage, TariffPlanIdentifier tariffPlanIdentifier)
		{
			if (tariffPlanIdentifier == null)
				throw new ArgumentNullException("tariffPlanIdentifier");

			if (!string.IsNullOrEmpty(tariffPlanIdentifier.Code))
				return tariffPlanIdentifier.Code;

			throw new ArgumentException("Id or Code must be specified.", "tariffPlanIdentifier");
		}
	}

	public class TariffPlanDataStorage : ITariffPlanDataStorage
	{
		public int GetINTariffPlanCode(int tariffPlanId)
		{
			throw new NotImplementedException();
		}

		public DataTable GetTariffPlanById(int tariffPlanId)
		{
			throw new NotImplementedException();
		}

		public string GetTariffPlanCode(string msisdn)
		{
			throw new NotImplementedException();
		}

		public string GetTariffPlanCode(int tariffPlanId)
		{
			throw new NotImplementedException();
		}

		public int GetTariffPlanId(long terminalDeviceId)
		{
			throw new NotImplementedException();
		}

		public int GetTariffPlanId(long terminalDeviceId, DateTime dateOfSearch)
		{
			throw new NotImplementedException();
		}

		public int GetTariffPlanId(long terminalDeviceId, DateTime? dateOfSearch)
		{
			throw new NotImplementedException();
		}

		public int GetTariffPlanIdByCode(string tariffPlanCode)
		{
			throw new NotImplementedException();
		}

		public int GetTariffPlanIdByMsisdn(string msisdn)
		{
			throw new NotImplementedException();
		}

		public int GetTariffPlanServiceProviderId(string tariffPlanCode)
		{
			throw new NotImplementedException();
		}

		public bool IsTariffPlanInGroup(int tariffPlanId, string tariffPlanGroupType)
		{
			throw new NotImplementedException();
		}

		public bool IsTariffPlanIsAccumulator(int tariffPlanId)
		{
			throw new NotImplementedException();
		}
	}

	public class Test01
	{
		public void Test()
		{
			var TariffPlanDb = new TariffPlanDataStorage();
			int TariffPlanIdNew = 9;
			int? TariffPlanIdNew2 = null;

			TariffPlanDb.GetTariffPlanCode(TariffPlanIdNew);
			TariffPlanDb.GetTariffPlanCode(TariffPlanIdNew2);
		}
	}
}
