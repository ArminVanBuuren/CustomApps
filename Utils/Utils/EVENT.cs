using System;
using System.Linq;
using System.Reflection;

namespace Utils
{
    public static class EVENT
    {
        private static readonly Type EventType = typeof(EventHandler);

        public static void DelegateAllEvents(this object source, object to)
        {
            var sourceType = source.GetType();
            var toType = to.GetType();
            while (sourceType != toType)
            {
                if (toType.BaseType == null)
                    return;
                toType = toType.BaseType;
            }

            var sourceEventList = sourceType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).Where(x => x.FieldType == EventType).ToDictionary(x => x.Name, x => x);
            var toEventList = toType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).Where(x => x.FieldType == EventType).ToDictionary(x => x.Name, x => x);
            foreach (var sourceEvent in sourceEventList)
            {
                var objSource = (EventHandler)sourceEvent.Value.GetValue(source);
                var eventListeners = objSource?.GetInvocationList();
                if (eventListeners == null || !eventListeners.Any())
                    continue;

                if (toEventList.TryGetValue(sourceEvent.Key, out var toFiled))
                {
                    var objTo = (EventHandler) toFiled.GetValue(to);
                    foreach (EventHandler del in eventListeners)
                        objTo += del;
                }
            }
        }

        public static void RemoveAllEvents(this object input)
        {
            var fieldList = input.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).Where(x => x.FieldType == EventType);
            foreach (var field in fieldList)
            {
                var obj = (EventHandler) field.GetValue(input);
                var eventListeners = obj?.GetInvocationList();
                if (eventListeners == null || !eventListeners.Any())
                    continue;

                foreach (EventHandler del in eventListeners)
                    obj -= del;
            }
        }
    }

    //public static class EVENT
    //{
    //    //private static readonly Type EventType = typeof(EventHandler);

    //    public static void DelegateAllEvents<T>(this object source, object to)
    //    {
    //        var eventType = typeof(T);
    //        //var sourceType = source.GetType();
    //        //var toType = to.GetType();

    //        //Type CombineTypes(Type sourceType2, Type toType2)
    //        //{
    //        // var toType3 = toType2;
    //        // while (sourceType2 != toType3)
    //        // {
    //        //  if (toType3.BaseType == null)
    //        //   return null;
    //        //  toType3 = toType3.BaseType;
    //        // }

    //        // return toType3;
    //        //}

    //        var sourceType = source.GetType();

    //        while (sourceType != null)
    //        {
    //            var toType = to.GetType();

    //            while (sourceType != toType)
    //            {
    //                if (toType.BaseType == null)
    //                    break;
    //                toType = toType.BaseType;
    //            }

    //            if (toType == null)
    //            {
    //                sourceType = sourceType.BaseType;
    //                continue;
    //            }

    //            var sourceEventList = sourceType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).Where(x => IsTargetType(x.FieldType, eventType) || IsTargetType(eventType, x.FieldType)).ToDictionary(x => x.Name, x => x);
    //            var toEventList = toType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).Where(x => IsTargetType(x.FieldType, eventType) || IsTargetType(eventType, x.FieldType)).ToDictionary(x => x.Name, x => x);
    //            foreach (var sourceEvent in sourceEventList)
    //            {
    //                var objSource = (MulticastDelegate)sourceEvent.Value.GetValue(source);
    //                var eventListeners = objSource?.GetInvocationList();
    //                if (eventListeners == null || !eventListeners.Any())
    //                    continue;

    //                if (toEventList.TryGetValue(sourceEvent.Key, out var toFiled))
    //                {
    //                    var targetEvents = new List<Delegate>();
    //                    var objTo = (MulticastDelegate)toFiled.GetValue(to);
    //                    if (objTo != null)
    //                        targetEvents.AddRange(objTo.GetInvocationList());
    //                    targetEvents.AddRange(eventListeners);

    //                    foreach (var del in eventListeners)
    //                    {
    //                        objTo += eventListeners;
    //                    }
    //                    toFiled.SetValue(to, targetEvents.ToArray());
    //                }
    //            }

    //            sourceType = sourceType.BaseType;
    //        }
    //    }

    //    static bool IsTargetType(Type source, Type target)
    //    {
    //        var source1 = source;
    //        var target1 = target;

    //        while (target1 != null && source1 != target1)
    //            target1 = target1.BaseType;

    //        if (target1 != null)
    //            return true;

    //        return false;
    //    }

    //    public static void RemoveAllEvents<T>(this object input)
    //    {
    //        var eventType = typeof(T);
    //        var fieldList = input.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).Where(x => x.FieldType == eventType);
    //        foreach (var field in fieldList)
    //        {
    //            var obj = (EventHandler)field.GetValue(input);
    //            var eventListeners = obj?.GetInvocationList();
    //            if (eventListeners == null || !eventListeners.Any())
    //                continue;

    //            foreach (EventHandler del in eventListeners)
    //                obj -= del;
    //        }
    //    }
    //}
}
