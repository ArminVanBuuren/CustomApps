using System;
using System.Linq;
using System.Reflection;

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

            return default;
        }

        /// <summary>
        /// Возвращает атрибут заданного типа <typeparamref name="T"/>, которым помечен указанный тип <paramref name="type"/>, или null в случае отсутствия.
        /// </summary>
        public static T GetAttribute<T>(this MemberInfo type, bool inherit = true)
	        where T : Attribute
        {
	        return (T)Attribute.GetCustomAttribute(type, typeof(T), inherit);
        }

        /// <summary>
        /// Возвращает атрибуты заданного типа <typeparamref name="T"/>, которым помечен указанный тип <paramref name="type"/>, или null в случае отсутствия.
        /// </summary>
        public static T[] GetAttributes<T>(this MemberInfo type, bool inherit = true) 
	        where T : Attribute
        {
	        return (T[])Attribute.GetCustomAttributes(type, typeof(T), inherit);
        }
	}
}
