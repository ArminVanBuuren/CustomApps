using System;
using System.Collections.Generic;
using System.Linq;

namespace Tester.Console
{
	internal class SpaCalculator
	{
		private readonly List<ParametersRequest> _requests = new List<ParametersRequest>();

		internal void Require<T>(Context context, Action<T> setter) where T : ParameterBase
		{
			Require(new[] { new[] { typeof(T) } },
				context,
				parameters => setter(parameters[0].As<T>()));
		}

		private void Require(
			Type[][] possibleTypeSets,
			Context context,
			Action<ParameterBase[]> setter,
			bool isOptional = false,
			bool isPrioritized = false,
			string externalParameterCode = null)
		{
			_requests.Add(
				new ParametersRequest
				{

				});
		}
	}
	public class ParameterBase
	{
		/// <summary>
		/// Приводим к типу
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T As<T>() where T : ParameterBase
		{
			return (T)As(typeof(T));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="targetType"></param>
		/// <returns></returns>
		public ParameterBase As(Type targetType)
		{
			var parameterType = GetType();
			return this;
		}
	}
	public class Context
	{

	}

	public class ParametersRequest
	{

	}

	internal sealed class ParameterizedService
	{
		internal object Product { get; set; }
	}

	internal sealed class ResultedTerminalDeviceServices : SingleValueParameter<List<ParameterizedService>>
	{
		internal ResultedTerminalDeviceServices(List<ParameterizedService> value) : base(value)
		{
		}

		protected override string ValueToString()
		{
			return "[" + string.Join(",", Value.Select(x => x.Product)) + "]";
		}
	}
	/// <summary>
	/// Класс с одним значением
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class SingleValueParameter<T> : ParameterBase
	{
		/// <summary>
		/// Значение
		/// </summary>
		public T Value { get; }

		/// <summary>
		/// Конструктор
		/// </summary>
		protected SingleValueParameter(T value)
		{
			Value = value;
		}

		/// <summary>
		/// Приведение типов
		/// </summary>
		/// <param name="parameter"></param>
		/// <returns></returns>
		public static implicit operator T(SingleValueParameter<T> parameter)
		{
			return parameter.Value;
		}

		protected virtual string ValueToString()
		{
			return Value == null ? "" : Value.ToString();
		}

		/// <summary>
		/// ToString
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return base.ToString() + "=" + ValueToString();
		}
	}

}
