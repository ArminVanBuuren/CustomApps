using System;
using System.Collections.Generic;
using Utils.CollectionHelper;

namespace Utils
{
	[Serializable]
	public class MTCallBackList<TSource, TResult>
	{
		private readonly DoubleDictionary<TSource, MTCallBack<TSource, TResult>> _values;

		public int Count => _values.CountValues;

		public IEnumerable<TSource> SourceList => _values.Keys;
		public IEnumerable<MTCallBack<TSource, TResult>> CallBackList => _values.Values;

		internal MTCallBackList(int capacity = 4)
		{
			_values = new DoubleDictionary<TSource, MTCallBack<TSource, TResult>>(capacity);
		}

		internal void Add(TSource source, TResult result)
		{
			Add(new MTCallBack<TSource, TResult>(source, result));
		}

		internal void Add(TSource source, Exception error)
		{
			Add(new MTCallBack<TSource, TResult>(source, error));
		}

		internal void Add(MTCallBack<TSource, TResult> item)
		{
			_values.Add(item.Source, item);
		}

		internal void Clear()
		{
			_values.Clear();
		}

		public bool TryGetValue(TSource source, out List<MTCallBack<TSource, TResult>> value)
		{
			if (_values.TryGetValue(source, out var res))
			{
				value = res;
				return true;
			}

			value = null;
			return false;
		}

		public IEnumerator<KeyValuePair<TSource, List<MTCallBack<TSource, TResult>>>> GetEnumerator()
		{
			return _values.GetEnumerator();
		}

		public override string ToString()
		{
			return GetType().ToString();
		}
	}
}
