using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Utils
{
    public static class ASYNC
    {
        /// <summary T="method which has a void return value synchronously">
        /// Execute an async Task
        /// </summary>
        /// <param name="task" T="method to execute">Task</param>
        public static void RunSync(Func<Task> task)
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
        /// Execute an async Task<T> method which has a T return type synchronously
        /// </summary>
        /// <typeparam name="T">Return Type</typeparam>
        /// <param name="task">Task<T> method to execute</param>
        /// <returns></returns>
        public static T RunSync<T>(Func<Task<T>> task)
        {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            T ret = default(T);
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
                return task.Result;
            }

            // timeout logic
            return default(T);
        }

        public static async Task ExecuteWithTimeoutAsync(Task task, int millisecondsTimeout = 1000)
        {
            if (await Task.WhenAny(task, Task.Delay(millisecondsTimeout)) == task)
            {
                // task completed within timeout
                return;
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
            T result = default(T);

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
    }
}
