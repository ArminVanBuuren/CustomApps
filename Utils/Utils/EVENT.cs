using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class EVENT
    {
        private static readonly Type EventType = typeof(EventHandler);

        public static void RemoveAllEvents(this object input)
        {
            var fields = input.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var f1 in fields)
            {
                if (f1.FieldType != EventType)
                    continue;

                var obj = (EventHandler) f1.GetValue(input);
                var eventListeners = obj?.GetInvocationList();
                if (eventListeners == null || !eventListeners.Any())
                    continue;

                foreach (EventHandler del in eventListeners)
                    obj -= del;
            }
        }
    }
}
