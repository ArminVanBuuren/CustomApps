using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Utils
{
    public static class ASYNC
    {
        private static readonly TaskFactory factory = new TaskFactory(default,
                TaskCreationOptions.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        public static void RunSync(this Func<Task> task)
            => factory.StartNew(task).Unwrap().GetAwaiter().GetResult();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        public static TResult RunSync<TResult>(this Func<Task<TResult>> task)
            => factory.StartNew(task).Unwrap().GetAwaiter().GetResult();

        /// <summary T="method which has a void return value synchronously">
        /// Execute an async Task
        /// </summary>
        public static void RunSync2(this Func<Task> task)
        {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            synch.Post(async _ =>
            {
                try
                {
                    await task();
                }
                catch (Exception e)
                {
                    synch.InnerException = e;
                    throw;
                }
                finally
                {
                    synch.EndMessageLoop();
                }
            }, null);
            synch.BeginMessageLoop();

            SynchronizationContext.SetSynchronizationContext(oldContext);
        }

        /// <summary>
        /// Execute an async Task&lt;T&gt; method which has a T return type synchronously
        /// </summary>
        /// <typeparam name="T">Return Type</typeparam>
        /// <param name="task">Task&lt;T&gt; method to execute</param>
        /// <returns></returns>
        public static T RunSync2<T>(Func<Task<T>> task)
        {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            var ret = default(T);
            synch.Post(async _ =>
            {
                try
                {
                    ret = await task();
                }
                catch (Exception e)
                {
                    synch.InnerException = e;
                    throw;
                }
                finally
                {
                    synch.EndMessageLoop();
                }
            }, null);
            synch.BeginMessageLoop();
            SynchronizationContext.SetSynchronizationContext(oldContext);
            return ret;
        }

        public static async Task<T> ExecuteWithTimeoutAsync<T>(Task<T> task, int millisecondsTimeout = 1000)
        {
            if (await Task.WhenAny(task, Task.Delay(millisecondsTimeout)) == task)
            {
                // task completed within timeout
                return await task.ConfigureAwait(false);
            }

            // timeout logic
            return default;
        }

        public static async Task ExecuteWithTimeoutAsync(Task task, int millisecondsTimeout = 1000)
        {
            if (await Task.WhenAny(task, Task.Delay(millisecondsTimeout)) == task)
            {
                // task completed within timeout
            }

            // timeout logic
        }

        private class ExclusiveSynchronizationContext : SynchronizationContext
        {
            private bool done;
            public Exception InnerException { get; set; }
            readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);
            readonly Queue<Tuple<SendOrPostCallback, object>> items =
                new Queue<Tuple<SendOrPostCallback, object>>();

            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotSupportedException("We cannot send to our same thread");
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                lock (items)
                {
                    items.Enqueue(Tuple.Create(d, state));
                }
                workItemsWaiting.Set();
            }

            public void EndMessageLoop()
            {
                Post(_ => done = true, null);
            }

            public void BeginMessageLoop()
            {
                while (!done)
                {
                    Tuple<SendOrPostCallback, object> task = null;
                    lock (items)
                    {
                        if (items.Count > 0)
                        {
                            task = items.Dequeue();
                        }
                    }
                    if (task != null)
                    {
                        task.Item1(task.Item2);
                        if (InnerException != null) // the method threw an exeption
                        {
                            throw new AggregateException("AsyncHelpers.Run method threw an exception.", InnerException);
                        }
                    }
                    else
                    {
                        workItemsWaiting.WaitOne();
                    }
                }
            }

            public override SynchronizationContext CreateCopy()
            {
                return this;
            }
        }

        public static T Run<T>(TimeSpan timeout, Func<T> operation)
        {
            Exception error = null;
            var result = default(T);

            var mre = new ManualResetEvent(false);
            ThreadPool.QueueUserWorkItem(
                delegate
                {
                    try
                    {
                        result = operation();
                    }
                    catch (Exception e)
                    {
                        error = e;
                    }
                    finally
                    {
                        mre.Set();
                    }
                }
            );

            if (!mre.WaitOne(timeout, true))
                throw new TimeoutException();
            if (error != null)
                throw new TargetInvocationException(error);

            return result;
        }

        /// <summary>
        /// Запускает несколько задач параллельно с барьерной синхронизацией в конце.
        /// </summary>
        /// <remarks>
        ///  Для первой из задач задействуется основной поток приложения.
        /// </remarks>
        public static void RunAndWait(params Task[] tasks)
        {
	        if (tasks == null)
		        throw new ArgumentNullException(nameof(tasks));

	        if (tasks.Length == 0)
		        return;

	        for (var i = 1; i < tasks.Length; i++)
		        tasks[i].Start();

	        tasks[0].RunSynchronously();

	        Task.WaitAll(tasks);
        }
    }
}
