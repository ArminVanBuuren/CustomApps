using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utils.CollectionHelper;

namespace Utils
{
    public class MultiTasking
    {
        public static async Task RunAsync(IEnumerable<Action> actions, CancellationTokenSource cancel, int maxThreads = 2)
        {
            await Task.Factory.StartNew(() => Run(actions, cancel, maxThreads));
        }

        public static void Run(IEnumerable<Action> actions, CancellationTokenSource cancel, int maxThreads = 2)
        {
            var pool = new Semaphore(maxThreads, maxThreads, actions.GetHashCode().ToString());
            var listOfTasks = new List<Task>();

            foreach (var action in actions)
            {
                if (cancel != null && cancel.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    if (cancel != null && cancel.IsCancellationRequested)
                    {
                        pool.Release();
                        return;
                    }

                    ((Action)input).Invoke();
                    pool.Release();
                }, action));
            }

            Task.WaitAll(listOfTasks.ToArray());
        }

        public static async Task RunAsync<T>(Action<T> action, IEnumerable<T> data, CancellationTokenSource cancel, int maxThreads = 2)
        {
            await Task.Factory.StartNew(() => Run(action, data, cancel, maxThreads));
        }

        public static void Run<T>(Action<T> action, IEnumerable<T> data, CancellationTokenSource cancel, int maxThreads = 2)
        {
            var pool = new Semaphore(maxThreads, maxThreads, typeof(T).GetHashCode().ToString() + action.GetHashCode().ToString());
            var listOfTasks = new List<Task>();

            foreach (var item in data)
            {
                if (cancel != null && cancel.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    if (cancel != null && cancel.IsCancellationRequested)
                    {
                        pool.Release();
                        return;
                    }

                    action.Invoke((T)input);
                    pool.Release();
                }, item));
            }

            Task.WaitAll(listOfTasks.ToArray());
        }

        public static async Task<HashTable<TIn, TOut>> RunAsync<TIn, TOut>(Func<TIn, TOut> func, IEnumerable<TIn> data, CancellationTokenSource cancel, int maxThreads = 2)
        {
            return await Task<HashTable<TIn, TOut>>.Factory.StartNew(() => Run(func, data, cancel, maxThreads));
        }

        public static HashTable<TIn, TOut> Run<TIn, TOut>(Func<TIn, TOut> func, IEnumerable<TIn> data, CancellationTokenSource cancel, int maxThreads = 2)
        {
            var pool = new Semaphore(maxThreads, maxThreads, (typeof(TIn).GetHashCode() + typeof(TOut).GetHashCode()) + func.GetHashCode().ToString());
            var result = new HashTable<TIn, TOut>(data.Count());
            var listOfTasks = new List<Task>();

            foreach (var item in data)
            {
                if (cancel != null && cancel.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    if (cancel != null && cancel.IsCancellationRequested)
                    {
                        pool.Release();
                        return;
                    }

                    var input1 = (TIn) input;
                    result.Add(input1, func.Invoke(input1));
                    pool.Release();
                }, item));
            }

            Task.WaitAll(listOfTasks.ToArray());
            return result;
        }
    }
}
