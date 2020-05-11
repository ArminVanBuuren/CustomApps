using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Utils
{
    public static class COLLECTIONS
    {
        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }

        public static Dictionary<TKey, TValue> CloneDictionaryCloningValues<TKey, TValue>
	        (Dictionary<TKey, TValue> original) where TValue : ICloneable
        {
	        var ret = new Dictionary<TKey, TValue>(original.Count,
		        original.Comparer);
	        foreach (var entry in original)
	        {
		        ret.Add(entry.Key, (TValue)entry.Value.Clone());
	        }
	        return ret;
        }

        public static void RenameKey<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey fromKey, TKey toKey)
        {
	        var value = dic[fromKey];
	        dic.Remove(fromKey);
	        dic[toKey] = value;
        }

        public static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var cnt = new Dictionary<T, int>();
            foreach (var s in list1)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else
                {
                    cnt.Add(s, 1);
                }
            }
            foreach (var s in list2)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else
                {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }

        public static IEnumerable<Enum> GetFlags(this Enum input)
        {
            foreach (Enum value in Enum.GetValues(input.GetType()))
                if (input.HasFlag(value))
                    yield return value;
        }

        /// <summary>
        /// Distinct by property.
        /// Example:
        /// var query = people.DistinctBy(p => p.Id);
        /// var query = people.DistinctBy(p => new { p.Id, p.Name });
        ///
        /// Other options:
        /// List&lt;Person&gt; distinctPeople = allPeople.GroupBy(p => p.PersonId).Select(g => g.First()).ToList();
        /// List&lt;Person&gt; distinctPeople = allPeople.GroupBy(p => new {p.PersonId, p.FavoriteColor} ).Select(g => g.First()).ToList();
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> query, string memberName)
        {
            ParameterExpression[] typeParams = new ParameterExpression[] { Expression.Parameter(typeof(T), "") };
            var pi = typeof(T).GetProperty(memberName);
            if(pi == null)
                throw new Exception($"Property \"{memberName}\" not found in type \"{typeof(T)}\".");

            return (IOrderedQueryable<T>)query.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable),
                    "OrderBy",
                    new Type[] { typeof(T), pi.PropertyType },
                    query.Expression,
                    Expression.Lambda(Expression.Property(typeParams[0], pi), typeParams))
            );
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> query, string memberName)
        {
            ParameterExpression[] typeParams = new ParameterExpression[] { Expression.Parameter(typeof(T), "") };
            var pi = typeof(T).GetProperty(memberName);
            if (pi == null)
                throw new Exception($"Property \"{memberName}\" not found in type \"{typeof(T)}\".");

            return (IOrderedQueryable<T>)query.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable),
                    "ThenBy",
                    new Type[] { typeof(T), pi.PropertyType },
                    query.Expression,
                    Expression.Lambda(Expression.Property(typeParams[0], pi), typeParams))
            );
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> query, string memberName)
        {
            ParameterExpression[] typeParams = new ParameterExpression[] { Expression.Parameter(typeof(T), "") };
            var pi = typeof(T).GetProperty(memberName);

            return (IOrderedQueryable<T>)query.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable),
                    "OrderByDescending",
                    new Type[] { typeof(T), pi.PropertyType },
                    query.Expression,
                    Expression.Lambda(Expression.Property(typeParams[0], pi), typeParams))
            );
        }

        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> query, string memberName)
        {
            ParameterExpression[] typeParams = new ParameterExpression[] { Expression.Parameter(typeof(T), "") };
            var pi = typeof(T).GetProperty(memberName);

            return (IOrderedQueryable<T>)query.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable),
                    "ThenByDescending",
                    new Type[] { typeof(T), pi.PropertyType },
                    query.Expression,
                    Expression.Lambda(Expression.Property(typeParams[0], pi), typeParams))
            );
        }
    }
}
