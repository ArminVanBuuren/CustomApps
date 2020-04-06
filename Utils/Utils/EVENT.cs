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
}
