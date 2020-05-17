using System;

namespace Utils
{
	[Serializable]
	public class MTCallBack<TSource, TResult>
	{
		public TSource Source { get; }
		public TResult Result { get; }
		public Exception Error { get; internal set; }

		internal MTCallBack(TSource source, TResult result)
		{
			Source = source;
			Result = result;
		}

		internal MTCallBack(TSource source, Exception error)
		{
			Source = source;
			Error = error;
		}

		public override string ToString()
		{
			return Error == null ? $"{Source}=[{Result}]" : Error.Message;
		}
	}
}
