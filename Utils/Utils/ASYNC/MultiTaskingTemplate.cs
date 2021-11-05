using System;
using System.Threading;

namespace Utils
{
	[Serializable]
	public class MultiTaskingTemplate
	{
		private CancellationTokenSource _cancelSource;

		public int MaxThreads { get; }

		/// <summary>
		/// Schedules a cancel operation on this <see cref="T:Utils.MultiTaskingTemplate" /> after the specified number of milliseconds.
		/// Value less or equal 0 disabled option.
		/// </summary>
		public int CancelAfterMilliseconds { get; }

		public ThreadPriority Priority { get; }

		internal CancellationToken CancelToken => _cancelSource.Token;

		public MultiTaskingTemplate(int maxThreads = 2, ThreadPriority priority = ThreadPriority.Normal, int cancelAfterMilliseconds = -1)
		{
			MaxThreads = maxThreads;
			CancelAfterMilliseconds = cancelAfterMilliseconds;
			Priority = priority;
			_cancelSource = new CancellationTokenSource();
		}

		public void Stop()
		{
			_cancelSource.Cancel();
		}

		protected internal void ReinitCancellationToken()
		{
			_cancelSource = new CancellationTokenSource();

			if (CancelAfterMilliseconds > 0)
				_cancelSource.CancelAfter(CancelAfterMilliseconds);
		}

		public override string ToString()
		{
			return $"MaxThreads=[{MaxThreads}] CancelAfterMilliseconds=[{CancelAfterMilliseconds}] IsCancelled=[{CancelToken.IsCancellationRequested}]";
		}
	}
}
