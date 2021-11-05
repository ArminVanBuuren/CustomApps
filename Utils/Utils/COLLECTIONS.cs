using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Utils
{
    public static class COLLECTIONS
    {
	    public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
	    {
		    return source == null || !source.Any<T>();
	    }

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
            var typeParams = new ParameterExpression[] { Expression.Parameter(typeof(T), "") };
            var pi = typeof(T).GetProperty(memberName);
            if(pi == null)
                throw new Exception($"Property \"{memberName}\" not found in type \"{typeof(T)}\".");

            return (IOrderedQueryable<T>)query.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable),
                    "OrderBy",
                    new[] { typeof(T), pi.PropertyType },
                    query.Expression,
                    Expression.Lambda(Expression.Property(typeParams[0], pi), typeParams))
            );
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> query, string memberName)
        {
            var typeParams = new ParameterExpression[] { Expression.Parameter(typeof(T), "") };
            var pi = typeof(T).GetProperty(memberName);
            if (pi == null)
                throw new Exception($"Property \"{memberName}\" not found in type \"{typeof(T)}\".");

            return (IOrderedQueryable<T>)query.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable),
                    "ThenBy",
                    new[] { typeof(T), pi.PropertyType },
                    query.Expression,
                    Expression.Lambda(Expression.Property(typeParams[0], pi), typeParams))
            );
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> query, string memberName)
        {
            var typeParams = new ParameterExpression[] { Expression.Parameter(typeof(T), "") };
            var pi = typeof(T).GetProperty(memberName);
            if (pi == null)
	            throw new Exception($"Property \"{memberName}\" not found in type \"{typeof(T)}\".");

            return (IOrderedQueryable<T>)query.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable),
                    "OrderByDescending",
                    new[] { typeof(T), pi.PropertyType },
                    query.Expression,
                    Expression.Lambda(Expression.Property(typeParams[0], pi), typeParams))
            );
        }

        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> query, string memberName)
        {
            var typeParams = new ParameterExpression[] { Expression.Parameter(typeof(T), "") };
            var pi = typeof(T).GetProperty(memberName);
            if (pi == null)
	            throw new Exception($"Property \"{memberName}\" not found in type \"{typeof(T)}\".");

            return (IOrderedQueryable<T>)query.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable),
                    "ThenByDescending",
                    new[] { typeof(T), pi.PropertyType },
                    query.Expression,
                    Expression.Lambda(Expression.Property(typeParams[0], pi), typeParams))
            );
        }

        /// <summary>
        /// Добавляет к <paramref name="items"/> операцию <paramref name="action"/>,
        /// которая вызывается для каждого элемента при его перечислении.
        /// </summary>
        public static IEnumerable<T> Decorate<T>(this IEnumerable<T> items, Action<T> action)
        {
	        return items.Select(
		        item =>
		        {
			        action(item);
			        return item;
		        });
        }

        /// <summary>
        /// Добавляет к <paramref name="items"/> операцию <paramref name="action"/>,
        /// которая вызывается для каждого элемента при его перечислении.
        /// </summary>
        public static IEnumerable<T> Decorate<T>(this IEnumerable<T> items, Action<T, int> action)
        {
	        return items.Select(
		        (item, i) =>
		        {
			        action(item, i);
			        return item;
		        });
        }

        ///<summary>
        /// Добавляет в начало последовательности заданный элемент.
        ///</summary>
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> sequence, T item)
        {
	        yield return item;
	        foreach (var sequenceItem in sequence)
	        {
		        yield return sequenceItem;
	        }
        }

        ///<summary>
        /// Добавляет в начало массива заданный элемент.
        ///</summary>
        public static T[] Prepend<T>(this T[] array, T item)
        {
	        var result = new T[array.Length + 1];
	        result[0] = item;
	        array.CopyTo(result, 1);
	        return array;
        }

        ///<summary>
        /// Добавляет в конец последовательности заданный элемент.
        ///</summary>
        public static IEnumerable<T> Append<T>(this IEnumerable<T> sequence, T item)
        {
	        foreach (var sequenceItem in sequence)
	        {
		        yield return sequenceItem;
	        }
	        yield return item;
        }

        ///<summary>
        /// Добавляет в конец массива заданный элемент.
        ///</summary>
        public static T[] Append<T>(this T[] array, T item)
        {
	        var result = new T[array.Length + 1];
	        array.CopyTo(result, 0);
	        result[result.Length - 1] = item;
	        return result;
        }

        /// <summary>
        /// Объединяет два массива в один.
        /// </summary>
        public static T[] Concat<T>(this T[] first, T[] second)
        {
	        if (first == null)
	        {
		        throw new ArgumentNullException(nameof(first));
	        }
	        if (second == null)
	        {
		        throw new ArgumentNullException(nameof(second));
	        }
	        var result = new T[first.Length + second.Length];
	        first.CopyTo(result, 0);
	        second.CopyTo(result, first.Length);
	        return result;
        }

        ///<summary>
		/// Парсит строку в значение типа-перечисления.
		///</summary>
		public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value);
        }

        ///<summary>
        /// Парсит строку в значение типа-перечисления.
        ///</summary>
        public static T ParseEnum<T>(string value, bool ignoreCase)
        {
            return (T)Enum.Parse(typeof(T), value, ignoreCase);
        }

        ///<summary>
        /// Возвращает единственный ожидаемый элемент последовательности. Если последовательность пустая, или элементов больше одного - кидает исключение.
        ///</summary>
        public static T Single<T>(this IEnumerable<T> sequence, string sequenceName)
        {
            return sequence.NotNull(sequenceName).SingleImpl(sequenceName);
        }

        ///<summary>
        /// Возвращает единственный ожидаемый элемент последовательности. Если последовательность пустая, или элементов больше одного - кидает исключение.
        ///</summary>
        public static T SingleEx<T>(this IList<T> sequence)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            if (sequence.Count != 1)
                throw COMMON.CreateException("Single item expected in {0}.SingleEx(), but {1} items found.", sequence.GetType().FormatTypeName(), sequence.Count);

            return sequence[0];
        }

        ///<summary>
        /// Возвращает единственный ожидаемый элемент последовательности. Если последовательность пустая, или элементов больше одного - кидает исключение.
        ///</summary>
        public static T SingleEx<T>(this ICollection<T> sequence)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            if (sequence.Count != 1)
                throw COMMON.CreateException("Single item expected in {0}.SingleEx(), but {1} items found.", sequence.GetType().FormatTypeName(), sequence.Count);

            foreach (var item in sequence)
                return item;

            throw COMMON.CreateException("{1} items indicated in {0}.SingleEx() by ICollection<>.Count, but enumeration returns no items.", sequence.GetType().FormatTypeName(), sequence.Count);
        }

        ///<summary>
        /// Возвращает единственный ожидаемый элемент последовательности. Если последовательность пустая, или элементов больше одного - кидает исключение.
        ///</summary>
        public static T SingleEx<T>(this IEnumerable<T> sequence)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            using (var enumerator = sequence.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    throw COMMON.CreateException(
                        "Single item expected in {0}.SingleEx(), but no items found.", sequence.GetType().FormatTypeName());

                var result = enumerator.Current;

                if (enumerator.MoveNext())
                    throw COMMON.CreateException(
                        "Single item expected in {0}.SingleEx(), but two or more items found.", sequence.GetType().FormatTypeName());

                return result;
            }
        }

        ///<summary>
        /// Возвращает единственный ожидаемый элемент последовательности. Если последовательность пустая, или элементов больше одного - кидает исключение.
        ///</summary>
        public static T SingleEx<T>(this IEnumerable<T> sequence, Expression<Func<T, bool>> predicate)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            using (var enumerator = sequence.Where(predicate.Compile()).GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    throw COMMON.CreateException(
                        "Single item expected in {0}.SingleEx({1}), but no items found.", sequence.GetType().FormatTypeName(), predicate);

                var result = enumerator.Current;

                if (enumerator.MoveNext())
                    throw COMMON.CreateException(
                        "Single item expected in {0}.SingleEx({1}), but two or more items found.", sequence.GetType().FormatTypeName(), predicate);

                return result;
            }
        }

        ///<summary>
        /// Возвращает единственный ожидаемый элемент последовательности. Если последовательность пустая, или элементов больше одного - кидает исключение.
        ///</summary>
        public static T Single<T>(this IEnumerable<T> sequence, string sequenceName, Func<T, bool> predicate)
        {
            return sequence.NotNull(sequenceName).Where(predicate).SingleImpl(sequenceName);
        }

        /// <summary>
        /// Проверяет, что последовательность не является null. Если передан null, выкидывает исключение.
        /// </summary>
        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> sequence, string sequenceName)
        {
            if (sequence == null)
                throw COMMON.CreateException("Sequence '{0}' is null.", sequenceName);
            return sequence;
        }

        /// <summary>
        /// Проверяет, что объект не является null. Если передан null, выкидывает исключение с типом объекта.
        /// </summary>
        public static T NotNull<T>(this T obj) where T : class
        {
            if (obj == null)
                throw COMMON.CreateException("Not null value of type '{0}' is expected here.", typeof(T).Name);
            return obj;
        }

        /// <summary>
        /// Проверяет, что объект не является null. Если передан null, выкидывает исключение с типом объекта.
        /// </summary>
        public static T NotNull<T>(this T? obj) where T : struct
        {
            if (!obj.HasValue)
                throw COMMON.CreateException("Not null value of type '{0}' is expected here.", typeof(T).Name);
            return obj.Value;
        }

        private static T SingleImpl<T>(this IEnumerable<T> sequence, string sequenceName)
        {
            T result;
            if (!sequence.TrySingleImpl(sequenceName, out result))
            {
                throw COMMON.CreateException("Single item expected in the sequence '{0}', but no items found.", sequenceName);
            }
            return result;
        }

        ///<summary>
        /// Получает единственное ожидаемое значение из последовательности. Если значений много - возникает исключение.
        ///</summary>
        ///<returns>true, если значение единственное, если последовательность пуста - false</returns>
        public static bool TrySingle<T>(this IEnumerable<T> sequence, string sequenceName, out T result)
        {
            return sequence.NotNull(sequenceName).TrySingleImpl(sequenceName, out result);
        }

        ///<summary>
        /// Получает единственное ожидаемое значение из последовательности. Если значений много - возникает исключение.
        ///</summary>
        ///<returns>true, если значение единственное, если последовательность пуста - false</returns>
        public static bool TrySingle<T>(this IEnumerable<T> sequence, string sequenceName, Func<T, bool> predicate, out T result)
        {
            return sequence.NotNull(sequenceName).Where(predicate).TrySingleImpl(sequenceName, out result);
        }

        private static bool TrySingleImpl<T>(this IEnumerable<T> sequence, string sequenceName, out T result)
        {
            using (var enumerator = sequence.GetEnumerator())
                if (enumerator.MoveNext())
                {
                    result = enumerator.Current;
                    if (enumerator.MoveNext())
                    {
                        throw COMMON.CreateException("Single item expected in the sequence '{0}', but two or more items found.", sequenceName);
                    }
                    return true;
                }
            result = default(T);
            return false;
        }

        ///<summary>
        /// Возвращает единственный конкретный элемент последовательности или значение по умолчанию, если этот элемент не найден.
        /// Посзволяет задать исключение, выбрасываемое при неско
        ///</summary>
        /// <param name="source">Исходная коллекция элементов.</param>
        /// <param name="errorPredicate">Функция возрата тела исключения для нескольких элементов в коллекции.</param>
        ///<returns>Элемент входной коллекции, или значение по умолчанию для типа элемента.</returns>
        public static T SingleOrDefaultEx<T>(this IEnumerable<T> source, Func<Exception> errorPredicate)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var list = source as IList<T>;
            if (list != null)
            {
                switch (list.Count)
                {
                    case 0: return default(T);
                    case 1: return list[0];
                }
            }
            else
            {
                using (var e = source.GetEnumerator())
                {
                    if (!e.MoveNext()) return default(T);
                    var result = e.Current;
                    if (!e.MoveNext()) return result;
                }
            }

            throw errorPredicate.Invoke();
        }

        /// <summary>
		/// Формирует из заданной последовательности последовательность пар значений key-value.
		/// </summary>
		/// <typeparam name="TItem"></typeparam>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="items"></param>
		/// <param name="keySelector"></param>
		/// <param name="valueSelector"></param>
		/// <returns></returns>
		public static IEnumerable<KeyValuePair<TKey, TValue>> Select<TItem, TKey, TValue>(
            this IEnumerable<TItem> items,
            Func<TItem, TKey> keySelector,
            Func<TItem, TValue> valueSelector)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }
            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
            }
            return items.Select(item => new KeyValuePair<TKey, TValue>(keySelector(item), valueSelector(item)));
        }

        /// <summary>
        /// Добавляет к последовательности пар значений еще один элемент.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="sequence"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<TKey, TValue>> Append<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> sequence, TKey key, TValue value)
        {
            return sequence.Append(new KeyValuePair<TKey, TValue>(key, value));
        }

        /// <summary>
        /// Если исходная последовательность не равна null, возвращает ее, а иначе - пустую последовательность.
        /// </summary>
        public static IEnumerable<T> MayBeNull<T>(this IEnumerable<T> sequence)
        {
            return sequence ?? Enumerable.Empty<T>();
        }

        /// <summary>
        /// Если исходный список не равен null, возвращает его, а иначе - пустую последовательность.
        /// </summary>
        public static List<T> MayBeNull<T>(this List<T> sequence)
        {
            return sequence ?? new List<T>();
        }

        /// <summary>
        /// Если исходный список не равен null, возвращает его, а иначе - пустую последовательность.
        /// </summary>
        public static T[] MayBeNull<T>(this T[] input)
        {
            return input ?? new T[] { };
        }

        /// <summary>
        /// Если объект равен null, возвращает null, иначе вычисляет переданную функцию и возвращает результат.
        /// </summary>
        public static TResult With<TSource, TResult>(this TSource obj, Func<TSource, TResult> evaluator)
            where TSource : class
            where TResult : class
        {
            if (evaluator == null)
                throw new ArgumentNullException(nameof(evaluator));

            return obj == null ? null : evaluator(obj);
        }

        /// <summary>
        /// Добавляет в словарь пару ключ-значение, если до этого в словаре не было такого ключа.
        /// </summary>
        public static void AddIfNotContainsKey<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, value);
        }

        /// <summary>
        /// Возвращает значение, если ключ есть в словаре, иначе значение по умолчанию.
        /// </summary>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default)
        {
	        return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// Производит сравнение двух элементов.
        /// </summary>
        public static int CompareTo<TItem>(this TItem a, TItem b)
        {
	        return Comparer<TItem>.Default.Compare(a, b);
        }

        /// <summary>
        /// Производит сравнение ключей двух элементов.
        /// </summary>
        public static int CompareTo<TItem, TKey>(this TItem a, TItem b, Func<TItem, TKey> keySelector)
        {
	        return keySelector(a).CompareTo(keySelector(b));
        }

        /// <summary>
        /// Производит сравнение пар ключей двух элементов.
        /// </summary>
        public static int CompareTo<TItem, TMajorKey, TMinorKey>(
	        this TItem a, TItem b, Func<TItem, TMajorKey> majorKeySelector, Func<TItem, TMinorKey> minorKeySelector)
        {
	        var majorResult = majorKeySelector(a).CompareTo(majorKeySelector(b));
	        return majorResult == 0 ? minorKeySelector(a).CompareTo(minorKeySelector(b)) : majorResult;
        }

        /// <summary>
        /// Возвращает различающиеся элементы последовательности, опционально используя для сравнения значений указанный компаратор IEqualityComparer{T}.
        /// </summary>
        /// <typeparam name="TSource">Тип элементов последовательности <paramref name="source"/>.</typeparam>
        /// <typeparam name="TElement">Тип элементов для сравнения.</typeparam>
        /// <param name="source">Последовательность, из которой требуется удалить дубликаты элементов.</param>
        /// <param name="elementSelector">Функция, возвращающая объекты для сравнения.</param>
        /// <param name="comparer">Компаратор <see cref="IEqualityComparer&lt;T&gt;"/>, используемый для сравнения значений.</param>
        /// <returns>Объект <see cref="IEnumerable&lt;T&gt;"/>, содержащий различающиеся элементы из исходной последовательности.</returns>
        /// <remarks>Результирующая последовательность неупорядочена.</remarks>
        public static IEnumerable<TSource> Distinct<TSource, TElement>(
	        this IEnumerable<TSource> source,
	        Func<TSource, TElement> elementSelector,
	        IEqualityComparer<TElement> comparer = null)
        {
	        if (source == null)
		        throw new ArgumentNullException(nameof(source));
	        if (elementSelector == null)
		        throw new ArgumentNullException(nameof(elementSelector));
	        return DistinctImpl(source, elementSelector, comparer);
        }

        private static IEnumerable<TSource> DistinctImpl<TSource, TElement>(
	        IEnumerable<TSource> source,
	        Func<TSource, TElement> elementSelector,
	        IEqualityComparer<TElement> comparer)
        {
	        var set = new HashSet<TElement>(comparer);
	        return source.Where(item => set.Add(elementSelector(item)));
        }

        /// <summary>
        /// Добавляет в <param name="hashSet"/> элементы <param name="values"/>.
        /// </summary>
        public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> values)
        {
	        if (values != null)
		        foreach (var value in values)
		        {
			        hashSet.Add(value);
		        }
        }

        /// <summary>
        /// Добавляет элемент в группирующий словарь.
        /// </summary>
        public static void Add<TKey, TValue>(this IDictionary<TKey, List<TValue>> items, TKey key, TValue value)
        {
	        List<TValue> list;
	        if (items.TryGetValue(key, out list))
		        list.Add(value);
	        else
		        items.Add(key, new List<TValue> { value });
        }

        /// <summary>
        /// Меняет тип исходного массива на тип T.
        /// </summary>
        public static T[] ToArray<T>(this IList input)
        {
	        if (input == null)
		        return new T[] { };

	        var result = new T[input.Count];
	        unchecked
	        {
		        for (var i = 0; i < input.Count; i++)
			        result[i] = (T)input[i];
	        }
	        return result;
        }

        /// <summary>
        /// Преобразователь входной последовательности с доступом по индексу (например, массива или списка) в массив заданного типа с помощью лямбды.
        /// </summary>
        public static TOut[] ToArray<TIn, TOut>(this IList<TIn> input, Func<TIn, TOut> selector)
        {
	        if (input == null)
		        throw new ArgumentNullException(nameof(input));
	        if (selector == null)
		        throw new ArgumentNullException(nameof(selector));
	        unchecked
	        {
		        var result = new TOut[input.Count];
		        for (var i = 0; i < input.Count; i++)
			        result[i] = selector(input[i]);
		        return result;
	        }
        }

        /// <summary>
        /// Преобразователь входной последовательности с доступом по индексу (например, массива или списка) в список заданного типа с помощью лямбды.
        /// </summary>
        public static List<TOut> ToList<TIn, TOut>(this IList<TIn> input, Func<TIn, TOut> selector)
        {
	        if (input == null)
		        throw new ArgumentNullException(nameof(input));
	        if (selector == null)
		        throw new ArgumentNullException(nameof(selector));
	        var result = new List<TOut>(input.Count);
	        result.AddRange(input.Select(selector));
	        return result;
        }

        private sealed class Group<TValue> : List<TValue>
        {
	        internal bool Selected { get; set; }
        }

        /// <summary>
		/// Обеспечивает Outer Join двух последовательностей по совпадению ключей.
		/// </summary>
		public static IEnumerable<TResult> OuterJoin<T1, T2, TKey, TResult>(
            this IEnumerable<T1> sequence1,
            IEnumerable<T2> sequence2,
            Func<T1, TKey> keySelector1,
            Func<T2, TKey> keySelector2,
            Func<T1, T2, TResult> resultSelector)
        {
            if (sequence1 == null)
                throw new ArgumentNullException(nameof(sequence1));
            if (sequence2 == null)
                throw new ArgumentNullException(nameof(sequence2));
            if (keySelector1 == null)
                throw new ArgumentNullException(nameof(keySelector1));
            if (keySelector2 == null)
                throw new ArgumentNullException(nameof(keySelector2));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));

            var lookup2 = new Dictionary<TKey, Group<T2>>();
            foreach (var item2 in sequence2)
            {
                var key = keySelector2(item2);

                Group<T2> group2;
                if (!lookup2.TryGetValue(key, out group2))
                    lookup2.Add(key, group2 = new Group<T2>());

                group2.Add(item2);
            }

            foreach (var item1 in sequence1)
            {
                var key = keySelector1(item1);

                Group<T2> group2;
                if (!lookup2.TryGetValue(key, out group2))
                    yield return resultSelector(item1, default(T2));
                else
                {
                    foreach (var item2 in group2)
                        yield return resultSelector(item1, item2);

                    group2.Selected = true;
                }
            }

            foreach (var group2 in lookup2.Values)
            {
                if (!group2.Selected)
                    foreach (var item2 in group2)
                        yield return resultSelector(default(T1), item2);
            }
        }

        /// <summary>
        /// Группирует элементы по ключу, возвращает словарь со списками элементов.
        /// </summary>
        public static Dictionary<TKey, List<TItem>> ToListDictionary<TItem, TKey>(
            this IEnumerable<TItem> items,
            Func<TItem, TKey> keySelector,
            IEqualityComparer<TKey> comparer = null)
        {
            return ToListDictionary(items, keySelector, item => item, comparer);
        }

        /// <summary>
        /// Группирует элементы по ключу, возвращает словарь со списками элементов.
        /// </summary>
        public static Dictionary<TKey, List<TValue>> ToListDictionary<TItem, TKey, TValue>(
            this IEnumerable<TItem> items,
            Func<TItem, TKey> keySelector,
            Func<TItem, TValue> valueSelector,
            IEqualityComparer<TKey> comparer = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            if (valueSelector == null)
                throw new ArgumentNullException(nameof(valueSelector));

            var result = new Dictionary<TKey, List<TValue>>(comparer ?? EqualityComparer<TKey>.Default);
            foreach (var item in items)
            {
                var key = keySelector(item);
                var value = valueSelector(item);
                List<TValue> list;
                if (!result.TryGetValue(key, out list))
                {
                    list = new List<TValue> { value };
                    result.Add(key, list);
                }
                else
                {
                    list.Add(value);
                }
            }
            return result;
        }

        /// <summary>
        /// Выполняет Left Join двух последовательностей по совпадению ключей.
        /// </summary>
        public static void LeftJoin<T1, T2, TKey>(
            this IEnumerable<T1> sequence1,
            IEnumerable<T2> sequence2,
            Func<T1, TKey> keySelector1,
            Func<T2, TKey> keySelector2,
            Action<T1, T2> action,
            IEqualityComparer<TKey> comparer = null)
        {
            if (sequence1 == null)
                throw new ArgumentNullException(nameof(sequence1));
            if (sequence2 == null)
                throw new ArgumentNullException(nameof(sequence2));
            if (keySelector1 == null)
                throw new ArgumentNullException(nameof(keySelector1));
            if (keySelector2 == null)
                throw new ArgumentNullException(nameof(keySelector2));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            sequence1.ForeachGroupJoinInner(
                sequence2,
                keySelector1,
                keySelector2,
                (item1, items2) =>
                {
                    var hasJoin = false;
                    foreach (var item2 in items2)
                    {
                        hasJoin = true;
                        action(item1, item2);
                    }
                    if (!hasJoin)
                        action(item1, default(T2));
                },
                comparer);
        }

        /// <summary>
        /// Выполняет Left Join двух последовательностей по совпадению ключей.
        /// </summary>
        public static IEnumerable<TResult> LeftJoin<T1, T2, TKey, TResult>(
            this IEnumerable<T1> sequence1,
            IEnumerable<T2> sequence2,
            Func<T1, TKey> keySelector1,
            Func<T2, TKey> keySelector2,
            Func<T1, T2, TResult> selector,
            IEqualityComparer<TKey> comparer = null)
        {
            if (sequence1 == null)
                throw new ArgumentNullException(nameof(sequence1));
            if (sequence2 == null)
                throw new ArgumentNullException(nameof(sequence2));
            if (keySelector1 == null)
                throw new ArgumentNullException(nameof(keySelector1));
            if (keySelector2 == null)
                throw new ArgumentNullException(nameof(keySelector2));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return sequence1.LeftJoinInner(sequence2, keySelector1, keySelector2, selector, comparer);
        }

        private static IEnumerable<TResult> LeftJoinInner<T1, T2, TKey, TResult>(
            this IEnumerable<T1> sequence1,
            IEnumerable<T2> sequence2,
            Func<T1, TKey> keySelector1,
            Func<T2, TKey> keySelector2,
            Func<T1, T2, TResult> selector,
            IEqualityComparer<TKey> comparer)
        {
            return sequence1.GroupJoin(sequence2, keySelector1, keySelector2, (item1, items2) => new { item1, items2 }, comparer)
                                            .SelectMany(item => LeftJoinInner(item.item1, item.items2, selector));
        }

        private static IEnumerable<TResult> LeftJoinInner<T1, T2, TResult>(T1 item1, IEnumerable<T2> items2, Func<T1, T2, TResult> selector)
        {
            var hasJoin = false;
            foreach (var item2 in items2)
            {
                hasJoin = true;
                yield return selector(item1, item2);
            }
            if (!hasJoin)
                yield return selector(item1, default(T2));
        }

        /// <summary>
		/// Выполняет Join двух последовательностей по совпадению ключей.
		/// </summary>
		public static void ForeachJoin<T1, T2, TKey>(
            this IEnumerable<T1> sequence1,
            IEnumerable<T2> sequence2,
            Func<T1, TKey> keySelector1,
            Func<T2, TKey> keySelector2,
            Action<T1, T2> action,
            IEqualityComparer<TKey> comparer = null)
        {
            if (sequence1 == null)
                throw new ArgumentNullException(nameof(sequence1));
            if (sequence2 == null)
                throw new ArgumentNullException(nameof(sequence2));
            if (keySelector1 == null)
                throw new ArgumentNullException(nameof(keySelector1));
            if (keySelector2 == null)
                throw new ArgumentNullException(nameof(keySelector2));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            sequence1.ForeachGroupJoinInner(
                sequence2,
                keySelector1,
                keySelector2,
                (item1, items2) =>
                {
                    foreach (var item2 in items2)
                    {
                        action(item1, item2);
                    }
                },
                comparer);
        }

        /// <summary>
        /// Выполняет GroupJoin двух последовательностей по совпадению ключей.
        /// </summary>
        public static void ForeachGroupJoin<T1, T2, TKey>(
            this IEnumerable<T1> sequence1,
            IEnumerable<T2> sequence2,
            Func<T1, TKey> keySelector1,
            Func<T2, TKey> keySelector2,
            Action<T1, IEnumerable<T2>> action,
            IEqualityComparer<TKey> comparer = null)
        {
            if (sequence1 == null)
                throw new ArgumentNullException(nameof(sequence1));
            if (sequence2 == null)
                throw new ArgumentNullException(nameof(sequence2));
            if (keySelector1 == null)
                throw new ArgumentNullException(nameof(keySelector1));
            if (keySelector2 == null)
                throw new ArgumentNullException(nameof(keySelector2));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            sequence1.ForeachGroupJoinInner(sequence2, keySelector1, keySelector2, action, comparer ?? EqualityComparer<TKey>.Default);
        }

        private static void ForeachGroupJoinInner<T1, T2, TKey>(
            this IEnumerable<T1> sequence1,
            IEnumerable<T2> sequence2,
            Func<T1, TKey> keySelector1,
            Func<T2, TKey> keySelector2,
            Action<T1, IEnumerable<T2>> action,
            IEqualityComparer<TKey> comparer)
        {
            var lookup2 = sequence2.ToLookup(keySelector2, comparer);
            foreach (var item1 in sequence1)
            {
                action(item1, lookup2[keySelector1(item1)]);
            }
        }

        /// <summary>
        /// Безопасное преобразование списков.
        /// </summary>
        public static List<TOutput> ConvertAllOrNull<TInput, TOutput>(this List<TInput> input, Converter<TInput, TOutput> converter)
        {
	        return input?.ConvertAll(converter);
        }

        /// <summary>
        /// Безопасное преобразование массивов.
        /// </summary>
        public static TOutput[] ConvertAllOrNull<TInput, TOutput>(this TInput[] input, Converter<TInput, TOutput> converter)
        {
	        return input != null ? Array.ConvertAll(input, converter) : null;
        }

        /// <summary>
        /// Объединяет последовательность элементов в строку.
        /// </summary>
        public static string JoinBy<T>(this IEnumerable<T> sequence, string separator)
        {
	        return string.Join(separator, sequence);
        }

        /// <summary>
        /// Исключает из первой последовательности элементы второй, основываясь на сравнении ключей.
        /// </summary>
        public static IEnumerable<T1> Exclude<T1, T2, TKey>(
	        this IEnumerable<T1> items1,
	        IEnumerable<T2> items2,
	        Func<T1, TKey> keySelector1,
	        Func<T2, TKey> keySelector2,
	        IEqualityComparer<TKey> comparer = null)
        {
	        if (items1 == null)
		        throw new ArgumentNullException(nameof(items1));
	        if (items2 == null)
		        throw new ArgumentNullException(nameof(items2));
	        if (keySelector1 == null)
		        throw new ArgumentNullException(nameof(keySelector1));
	        if (keySelector2 == null)
		        throw new ArgumentNullException(nameof(keySelector2));

	        return items1.Exclude(keySelector1, items2.Select(keySelector2), comparer);
        }

        /// <summary>
        /// Исключает из первой последовательности элементы второй, основываясь на сравнении ключей.
        /// </summary>
        public static IEnumerable<T> Exclude<T, TKey>(
	        this IEnumerable<T> items,
	        Func<T, TKey> keySelector,
	        IEnumerable<TKey> excludeKeys,
	        IEqualityComparer<TKey> comparer = null)
        {
	        if (items == null)
		        throw new ArgumentNullException(nameof(items));
	        if (excludeKeys == null)
		        throw new ArgumentNullException(nameof(excludeKeys));
	        if (keySelector == null)
		        throw new ArgumentNullException(nameof(keySelector));

	        return ExcludeInner(items, keySelector, excludeKeys, comparer);
        }

        private static IEnumerable<T> ExcludeInner<T, TKey>(
	        this IEnumerable<T> items,
	        Func<T, TKey> keySelector,
	        IEnumerable<TKey> excludeKeys,
	        IEqualityComparer<TKey> comparer = null)
        {
	        var excludeSet = excludeKeys.ToHashSet(comparer);
	        return items.Where(item => !excludeSet.Contains(keySelector(item)));
        }

        ///<summary>
        /// Создает хэш-набор, содержащий последовательность значений.
        ///</summary>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> values, IEqualityComparer<T> comparer = null)
        {
	        return new HashSet<T>(values, comparer ?? EqualityComparer<T>.Default);
        }

        ///<summary>
        /// Создает хэш-набор, содержащий последовательность значений, выбранных с помощью указанной функции <paramref name="selector" />.
        ///</summary>
        public static HashSet<T2> ToHashSet<T1, T2>(this IEnumerable<T1> values, Func<T1, T2> selector, IEqualityComparer<T2> comparer = null)
        {
	        return values.Select(selector).ToHashSet(comparer);
        }

        /// <summary>
        /// Получает значение из словаря.
        /// </summary>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
	        TValue ret;
	        dictionary.TryGetValue(key, out ret);
	        return ret;
        }

        /// <summary>
		/// Returns first element with maximum selected value. Returns default value if sequence is empty.
		/// </summary>
		/// <typeparam name="TSource">Sequence type.</typeparam>
		/// <typeparam name="TArgument">Selected value type.</typeparam>
		/// <param name="items">Sequence.</param>
		/// <param name="selector">Selector function.</param>
		/// <returns>First element with maximum selected value.</returns>
		public static TSource FirstWithMax<TSource, TArgument>(this IEnumerable<TSource> items, Func<TSource, TArgument> selector)
        {
            return FirstWithMax(items, selector, true);
        }

        /// <summary>
        /// Returns first element with maximum selected value. Throws if sequence is empty.
        /// </summary>
        /// <typeparam name="TSource">Sequence type.</typeparam>
        /// <typeparam name="TArgument">Selected value type.</typeparam>
        /// <param name="items">Sequence.</param>
        /// <param name="selector">Selector function.</param>
        /// <returns>First element with maximum selected value.</returns>
        public static TSource FirstWithMaxOrDefault<TSource, TArgument>(this IEnumerable<TSource> items, Func<TSource, TArgument> selector)
        {
            return FirstWithMax(items, selector, false);
        }

        private static TSource FirstWithMax<TSource, TArgument>(this IEnumerable<TSource> items, Func<TSource, TArgument> selector, bool throwIfEmpty)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            var comparer = Comparer<TArgument>.Default;
            var result = default(TSource);
            var maxValue = default(TArgument);
            var hasValue = false;

            foreach (var item in items)
            {
                if (hasValue)
                {
                    var candidate = selector(item);
                    if (comparer.Compare(candidate, maxValue) > 0)
                    {
                        maxValue = candidate;
                        result = item;
                    }
                }
                else
                {
                    maxValue = selector(item);
                    result = item;
                    hasValue = true;
                }
            }

            if (!hasValue && throwIfEmpty)
                throw new ArgumentException("items can't be empty", nameof(items));

            return result;
        }

        /// <summary>
        /// Returns first element with minimum selected value. Throws if sequence is empty.
        /// </summary>
        /// <typeparam name="TSource">Sequence type.</typeparam>
        /// <typeparam name="TArgument">Selected value type.</typeparam>
        /// <param name="items">Sequence.</param>
        /// <param name="selector">Selector function.</param>
        /// <returns>First element with minimum selected value.</returns>
        public static TSource FirstWithMin<TSource, TArgument>(this IEnumerable<TSource> items, Func<TSource, TArgument> selector)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            var comparer = Comparer<TArgument>.Default;
            var result = default(TSource);
            var minValue = default(TArgument);
            var hasValue = false;

            foreach (var item in items)
            {
                if (hasValue)
                {
                    var candidate = selector(item);
                    if (comparer.Compare(candidate, minValue) < 0)
                    {
                        minValue = candidate;
                        result = item;
                    }
                }
                else
                {
                    minValue = selector(item);
                    result = item;
                    hasValue = true;
                }
            }

            if (!hasValue)
                throw new ArgumentException("items can't be empty", nameof(items));

            return result;
        }

        /// <summary>
        /// Splits sequence into pages.
        /// </summary>
        /// <typeparam name="T">Sequence type.</typeparam>
        /// <param name="items">Sequence.</param>
        /// <param name="pageSize">Page size.</param>
        /// <returns>Sequence of sequences, last one can have less than <paramref name="pageSize"/> items.</returns>
        public static IEnumerable<List<T>> PageBy<T>(this IEnumerable<T> items, int pageSize)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "pageSize must be positive");

            var page = new List<T>(pageSize);
            foreach (var item in items)
            {
                page.Add(item);
                if (page.Count == pageSize)
                {
                    yield return page;
                    page = new List<T>(pageSize);
                }
            }

            if (page.Count != 0)
                yield return page;
        }

        /// <summary>
        /// Приводит объект к типу перечисления. Если объект не определен, возвращается defaultValue.
        /// </summary>
        /// <param name="value">Значение.</param>
        /// <param name="defaultValue">Значение возвращаемое в случае неуспеха приведения. </param>
        /// <returns>Объект-элемент перечисления.</returns>
        /// <example>
        /// int result = ...
        /// ResultCode resultCode = GetEnumValue(typeof(ResultCode), result, ResultCode.Unknown);
        /// </example>
        public static T GetEnumValue<T>(object value, T defaultValue)
        {
            var enumType = typeof(T);
            if (Enum.IsDefined(enumType, value))
                return (T)Enum.ToObject(enumType, value);

            return defaultValue;
        }

        /// <summary>
        /// Приводит объект к типу перечисления. Если объект не определен, возвращается defaultValue.
        /// </summary>
        /// <param name="value">Значение.</param>
        /// <returns>Объект-элемент перечисления.</returns>
        /// <example>
        /// int result = ...
        /// ResultCode resultCode = GetEnumValue(typeof(ResultCode), result, ResultCode.Unknown);
        /// </example>
        public static T GetEnumValue<T>(object value)
        {
            var enumType = typeof(T);
            if (Enum.IsDefined(enumType, value))
                return (T)Enum.ToObject(enumType, value);

            throw new Exception("Value " + value + " not found");
        }

        /// <summary>
        /// Отложенное действие над каждым объектом коллекции.
        /// </summary>
        /// <typeparam name="T">Тип объектов в коллекции</typeparam>
        /// <param name="items">Коллекция объектов</param>
        /// <param name="action">Действие, выполняемое над каждым объектом коллекции</param>
        public static IEnumerable<T> ForEachLazy<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
            {
                action(item);
                yield return item;
            }
        }

        /// <summary>
        /// Расширяющий метод ForEach для IEnumerable'T
        /// </summary>
        /// <param name="items">Коллекция объектов</param>
        /// <param name="action">Действие, выполняемое над каждым объектом коллекции</param>
        /// <typeparam name="T">Тип объектов в коллекции</typeparam>
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
                action(item);
        }

        /// <summary>
        /// Возвращает значение из определенной колонки нужного типа
        /// </summary>
        /// <param name="row">Строка DataTable</param>
        /// <param name="columnName">Название колонки</param>
        /// <typeparam name="T">Тип значения колонки</typeparam>
        /// <returns>Значение колонки переданного типа</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static T GetColumnValue<T>(this DataRow row, string columnName)
        {
	        var value = row[columnName ?? throw new ArgumentNullException(nameof(columnName))];
	        return Convert.IsDBNull(value) ? COMMON.To<T>(null) : COMMON.To<T>(value);
        }

        /// <summary>
        /// Проверяет находится ли <paramref name="value"/> в допустимых значениях <paramref name="range"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static bool In<T>(this T value, params T[] range) => Array.IndexOf(range, value) != -1;

        /// <summary>
        /// Разбивает пару ключ-значение словаря на две переменные
        /// </summary>
        /// <typeparam name="T1">Тип ключа в словаре</typeparam>
        /// <typeparam name="T2">Тип значения в словаре</typeparam>
        /// <param name="pair">Пара ключ-значение</param>
        /// <param name="key">Ключ в словаре</param>
        /// <param name="value">Значение из словаря</param>
        public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> pair, out T1 key, out T2 value)
        {
	        key = pair.Key;
	        value = pair.Value;
        }
    }
}
