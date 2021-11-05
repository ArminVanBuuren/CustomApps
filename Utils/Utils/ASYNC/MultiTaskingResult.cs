using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Utils
{
	[Serializable]
	public abstract class MultiTaskingResult<TSource, TResult> : MultiTaskingTemplate
	{
		public abstract event EventHandler IsCompeted;
		public ReadOnlyCollection<TSource> Source { get; }
		public MTCallBackList<TSource, TResult> Result { get; protected set; }

		public bool IsCompleted => Source.Count() == Result.Count || CancelToken.IsCancellationRequested;
		public int PercentOfComplete => (Result.Count * 100) / Source.Count();

		protected MultiTaskingResult(IEnumerable<TSource> source, MultiTaskingTemplate mtTemplate) : this(source, mtTemplate.MaxThreads, mtTemplate.Priority, mtTemplate.CancelAfterMilliseconds) { }

		protected MultiTaskingResult(IEnumerable<TSource> source, int maxThreads, ThreadPriority priority, int cancelAfterMilliseconds) : base(maxThreads, priority, cancelAfterMilliseconds)
		{
			Source = new ReadOnlyCollection<TSource>(source.ToList());
			Result = new MTCallBackList<TSource, TResult>(Source.Count());
		}

		public async Task StartAsync()
		{
			await Task.Factory.StartNew(Start);
		}

		public virtual void Start()
		{
			Result.Clear();
			ReinitCancellationToken();
		}

		protected void CallBack(MTCallBack<TSource, TResult> result)
		{
			Result.Add(result);
		}

		public override string ToString()
		{
			return $"IsCompleted=[{IsCompleted}] PercentOfComplete={PercentOfComplete}";
		}
	}
}
