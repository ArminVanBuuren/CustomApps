using System;
using System.Linq;

namespace Utils
{
    public static class ATTRIBUTES
    {
        public static TValue GetAttributeValue<TAttribute, TValue>(this Type type, Func<TAttribute, TValue> valueSelector, bool inherit = true) where TAttribute : Attribute
        {
            if (type.GetCustomAttributes(typeof(TAttribute), inherit).FirstOrDefault() is TAttribute att)
            {
                return valueSelector(att);
            }

            return default(TValue);
        }
    }
}
