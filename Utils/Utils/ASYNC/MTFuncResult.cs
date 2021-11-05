using System;
using System.Collections.Generic;
using System.Threading;

namespace Utils
{
	[Serializable]
	public class MTFuncResult<TResult> : MultiTaskingResult<Func<TResult>, TResult>
	{
		public override event EventHandler IsCompeted;
		public MTFuncResult(IEnumerable<Func<TResult>> funcs, int maxThreads = 2, ThreadPriority priority = ThreadPriority.Normal, int cancelAfterMilliseconds = -1) : base(funcs, maxThreads, priority, cancelAfterMilliseconds) { }
		public MTFuncResult(IEnumerable<Func<TResult>> funcs, MultiTaskingTemplate mtTemplate) : base(funcs, mtTemplate) { }

		public override void Start()
		{
			base.Start();
			MultiTasking.Run(Source, CallBack, this);
			IsCompeted?.Invoke(this, EventArgs.Empty);
		}
	}

	[Serializable]
	public class MTFuncResult<TSource, TResult> : MultiTaskingResult<TSource, TResult>
	{
		public override event EventHandler IsCompeted;
		private Func<TSource, TResult> _func;
		public MTFuncResult(Func<TSource, TResult> func, IEnumerable<TSource> data, int maxThreads = 2, ThreadPriority priority = ThreadPriority.Normal, int cancelAfterMilliseconds = -1) : base(data, maxThreads, priority, cancelAfterMilliseconds)
		{
			_func = func;
		}

		public MTFuncResult(Func<TSource, TResult> func, IEnumerable<TSource> data, MultiTaskingTemplate mtTemplate) : base(data, mtTemplate)
		{
			_func = func;
		}

		public override void Start()
		{
			base.Start();
			MultiTasking.Run(_func, Source, CallBack, this);
			IsCompeted?.Invoke(this, EventArgs.Empty);
		}
	}
}
