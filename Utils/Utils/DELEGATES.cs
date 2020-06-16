using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Utils
{
	public static class DELEGATES
	{
		public static void DelegateAllEventsTo(this object source, object target)
		{
			var sourceType = source.GetType();

			while (sourceType != null)
			{
				var targetType = target.GetType();

				while (sourceType != targetType)
				{
					if (targetType.BaseType == null)
						break;
					targetType = targetType.BaseType;
				}

				var sourceEventList = sourceType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).Where(x => IsTargetType(typeof(Delegate), x.FieldType)).ToDictionary(x => x.Name, x => x);

				try
				{
					if (sourceEventList.Count == 0)
						continue;

					var targetEventList = targetType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).Where(x => IsTargetType(typeof(Delegate), x.FieldType)).ToDictionary(x => x.Name, x => x);
					foreach (var (name, field) in sourceEventList)
					{
						var objSource = (Delegate)field.GetValue(source);
						var eventListeners = objSource?.GetInvocationList();
						if (eventListeners == null || !eventListeners.Any() || !targetEventList.TryGetValue(name, out var targetFiled))
							continue;

						var listOfAllDelegates = new List<Delegate>();

						var objTarget = (Delegate)targetFiled.GetValue(target);
						var targetSourceDelegates = objTarget?.GetInvocationList();

						if (targetSourceDelegates != null && targetSourceDelegates.Any())
							listOfAllDelegates.AddRange(targetSourceDelegates);
						listOfAllDelegates.AddRange(eventListeners);

						var newDelegates = Delegate.Combine(listOfAllDelegates.ToArray());
						targetFiled.SetValue(target, newDelegates);
					}
				}
				finally
				{
					sourceType = sourceType.BaseType;
				}
			}
		}

		static bool IsTargetType(Type source, Type target)
		{
			var target2 = target;

			while (target2 != null && source != target2)
				target2 = target2.BaseType;

			return target2 != null;
		}

		public static void RemoveAllEvents(this object input)
		{
			var fieldList = input.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).Where(x => x.FieldType == typeof(Delegate));
			foreach (var field in fieldList)
			{
				var obj = (Delegate)field.GetValue(input);
				var eventListeners = obj?.GetInvocationList();
				if (eventListeners == null || !eventListeners.Any())
					continue;

				field.SetValue(input, null);
			}
		}

		/// <summary>
		/// Выполняем действие над входным объектом.
		/// </summary>
		public static T Do<T>(this T src, Action<T> fn) where T : class
		{
			fn(src);
			return src;
		}
	}
	//public static class EVENT
	//{
	//	public static Dictionary<string, KeyValuePair<FieldInfo, Delegate[]>> GetSubscribedEvents(this object source, bool onlyAssigned = false)
	//	{
	//		FieldInfo Ei2Fi(EventInfo ei)
	//		{
	//			var sourceType = source.GetType();

	//			while (sourceType != null)
	//			{
	//				var fieldInfo = sourceType.GetField(ei.Name);
	//				if (fieldInfo != null)
	//					return fieldInfo;
	//				sourceType = sourceType.BaseType;
	//			}

	//			sourceType = source.GetType();
	//			while (sourceType != null)
	//			{
	//				var propertyInfo = sourceType.GetProperty(ei.Name);
	//				if (propertyInfo != null)
	//					return null;
	//				sourceType = sourceType.BaseType;
	//			}

	//			//sourceType = source.GetType();
	//			//while (sourceType != null)
	//			//{
	//			//	var propertyInfo = sourceType.GetMember(ei.Name);
	//			//	if (propertyInfo != null)
	//			//		return null;
	//			//	sourceType = sourceType.BaseType;
	//			//}

	//			return null;
	//		}

	//		Func<Delegate[], bool> onlyAssignedFunc = (delegates) => true;
	//		if (onlyAssigned)
	//			onlyAssignedFunc = delegates => delegates != null && delegates.Length > 0;

	//		var test  = source.GetType().GetEvents();

	//		foreach (var @event in test)
	//		{

	//			var dd = @event;
	//			if (dd != null)
	//			{

	//			}
	//		}

	//		return source.GetType()
	//			.GetEvents()
	//			.Select(Ei2Fi)
	//			.Select(x => new { Field = x, Delegates = ((Delegate)x?.GetValue(source))?.GetInvocationList() })
	//			.Where(x => onlyAssignedFunc(x.Delegates))
	//			.ToDictionary(x => x.Field.Name, x => new KeyValuePair<FieldInfo, Delegate[]>(x.Field, x.Delegates));
	//	}

	//	public static void DelegateAllEventsTo<T>(this object source, object to) where T : Delegate
	//	{
	//		var sourceEventList = source.GetSubscribedEvents(true);
	//		if (sourceEventList.Count == 0)
	//			return;

	//		var toEventList = to.GetSubscribedEvents(false);
	//		foreach (var (fieldName, (field, delegates)) in sourceEventList)
	//		{
	//			if (!toEventList.TryGetValue(fieldName, out var toFiled))
	//				continue;

	//			var listOfAllDelegates = new List<Delegate>();

	//			var objTo = (Delegate)toFiled.Key.GetValue(to);
	//			var targetExistedDelegates = objTo?.GetInvocationList();

	//			if (targetExistedDelegates != null && targetExistedDelegates.Any())
	//				listOfAllDelegates.AddRange(targetExistedDelegates);
	//			listOfAllDelegates.AddRange(delegates);

	//			toFiled.Key.SetValue(to, Delegate.Combine(listOfAllDelegates.ToArray()));
	//		}


	//		//while (sourceType != null)
	//		//{
	//		//	toType = to.GetType();
	//		//	while (sourceType != toType)
	//		//	{
	//		//		if (toType.BaseType == null)
	//		//			break;
	//		//		toType = toType.BaseType;
	//		//	}


	//		//	try
	//		//	{

	//		//	}
	//		//	finally
	//		//	{
	//		//		sourceType = sourceType.BaseType;
	//		//	}
	//		//}
	//	}

	//	static bool IsTargetType(Type source, Type target)
	//	{
	//		var target2 = target;

	//		while (target2 != null && source != target2)
	//			target2 = target2.BaseType;

	//		return target2 != null;
	//	}

	//	public static void RemoveAllEvents<T>(this object input) where T : Delegate
	//	{
	//		var eventType = typeof(T);
	//		var fieldList = input.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).Where(x => x.FieldType == eventType);
	//		foreach (var field in fieldList)
	//		{
	//			var obj = (T)field.GetValue(input);
	//			var eventListeners = obj?.GetInvocationList();
	//			if (eventListeners == null || !eventListeners.Any())
	//				continue;

	//			field.SetValue(input, null);
	//		}
	//	}
	//}
}
