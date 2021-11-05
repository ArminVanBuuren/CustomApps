using System;
using System.Collections.Generic;
using System.Threading;

namespace Utils
{
	[Serializable]
	public class MTActionResult : MultiTaskingResult<Action, bool>
	{
		public override event EventHandler IsCompeted;
		public MTActionResult(IEnumerable<Action> actions, int maxThreads = 2, ThreadPriority priority = ThreadPriority.Normal, int cancelAfterMilliseconds = -1) : base(actions, maxThreads, priority, cancelAfterMilliseconds) { }
		public MTActionResult(IEnumerable<Action> actions, MultiTaskingTemplate mtTemplate) : base(actions, mtTemplate) { }

		public override void Start()
		{
			base.Start();
			MultiTasking.Run(Source, CallBack, this);
			IsCompeted?.Invoke(this, EventArgs.Empty);
		}
	}

	[Serializable]
	public class MTActionResult<TSource> : MultiTaskingResult<TSource, bool>
	{
		public override event EventHandler IsCompeted;
		private Action<TSource> _action;
		public MTActionResult(Action<TSource> action, IEnumerable<TSource> data, int maxThreads = 2, ThreadPriority priority = ThreadPriority.Normal, int cancelAfterMilliseconds = -1) : base(data, maxThreads, priority, cancelAfterMilliseconds)
		{
			_action = action;
		}

		public MTActionResult(Action<TSource> action, IEnumerable<TSource> data, MultiTaskingTemplate mtTemplate) : base(data, mtTemplate)
		{
			_action = action;
		}

		public override void Start()
		{
			base.Start();
			MultiTasking.Run(_action, Source, CallBack, this);
			IsCompeted?.Invoke(this, EventArgs.Empty);
		}
	}
}
