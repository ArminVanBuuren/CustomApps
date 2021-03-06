using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Utils
{
	[Serializable]
	public class PriorityScheduler : TaskScheduler
	{
		public static PriorityScheduler AboveNormal = new PriorityScheduler(ThreadPriority.AboveNormal);
		public static PriorityScheduler BelowNormal = new PriorityScheduler(ThreadPriority.BelowNormal);
		public static PriorityScheduler Lowest = new PriorityScheduler(ThreadPriority.Lowest);

		private readonly BlockingCollection<Task> _tasks = new BlockingCollection<Task>();
		private Thread[] _threads;
		private readonly ThreadPriority _priority;
		private readonly int _maximumConcurrencyLevel = Math.Max(1, Environment.ProcessorCount);

		public PriorityScheduler(ThreadPriority priority)
		{
			_priority = priority;
		}

		public override int MaximumConcurrencyLevel => _maximumConcurrencyLevel;

		protected override IEnumerable<Task> GetScheduledTasks()
		{
			return _tasks;
		}

		protected override void QueueTask(Task task)
		{
			_tasks.Add(task);

			if (_threads != null)
				return;

			_threads = new Thread[_maximumConcurrencyLevel];
			for (var i = 0; i < _threads.Length; i++)
			{
				_threads[i] = new Thread(() =>
				{
					foreach (var t in _tasks.GetConsumingEnumerable())
						TryExecuteTask(t);
				})
				{
					Name = $"PriorityScheduler: {i}",
					Priority = _priority,
					IsBackground = true
				};
				_threads[i].Start();
			}
		}

		protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
		{
			return false; // we might not want to execute task that should schedule as high or low priority inline
		}
	}
}
