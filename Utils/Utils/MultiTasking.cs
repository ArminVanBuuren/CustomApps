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
        public static async Task RunAsync(IEnumerable<Action> actions, int maxThreads = 2, CancellationTokenSource cancel = null)
        {
            await Task.Factory.StartNew(() => Run(actions, maxThreads, cancel));
        }

        public static void Run(IEnumerable<Action> actions, int maxThreads = 2, CancellationTokenSource cancel = null)
        {
            var pool = new Semaphore(maxThreads, maxThreads, $"IEnumerable<Action>_{actions.GetHashCode()}");
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

        public static async Task<HashTable<Func<T>, T>> RunAsync<T>(IEnumerable<Func<T>> funcs, int maxThreads = 2, CancellationTokenSource cancel = null)
        {
            return await Task.Factory.StartNew(() => Run(funcs, maxThreads, cancel));
        }

        public static HashTable<Func<T>, T> Run<T>(IEnumerable<Func<T>> funcs, int maxThreads = 2, CancellationTokenSource cancel = null)
        {
            var pool = new Semaphore(maxThreads, maxThreads, $"IEnumerable<Func<T>>_{typeof(T).GetHashCode() + funcs.GetHashCode()}");
            var result = new HashTable<Func<T>, T>(funcs.Count());
            var listOfTasks = new List<Task>();

            foreach (var func in funcs)
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

                    var func1 = (Func<T>) input;
                    result.Add(func1, func1.Invoke());
                    pool.Release();
                }, func));
            }

            Task.WaitAll(listOfTasks.ToArray());
            return result;
        }

        public static async Task RunAsync<T>(Action<T> action, IEnumerable<T> data, int maxThreads = 2, CancellationTokenSource cancel = null)
        {
            await Task.Factory.StartNew(() => Run(action, data, maxThreads, cancel));
        }

        public static void Run<T>(Action<T> action, IEnumerable<T> data, int maxThreads = 2, CancellationTokenSource cancel = null)
        {
            var pool = new Semaphore(maxThreads, maxThreads, $"Action<T>_{typeof(T).GetHashCode() + action.GetHashCode()}");
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

        public static async Task<HashTable<TIn, TOut>> RunAsync<TIn, TOut>(Func<TIn, TOut> func, IEnumerable<TIn> data, int maxThreads = 2, CancellationTokenSource cancel = null)
        {
            return await Task<HashTable<TIn, TOut>>.Factory.StartNew(() => Run(func, data, maxThreads, cancel));
        }

        public static HashTable<TIn, TOut> Run<TIn, TOut>(Func<TIn, TOut> func, IEnumerable<TIn> data, int maxThreads = 2, CancellationTokenSource cancel = null)
        {
            var pool = new Semaphore(maxThreads, maxThreads, $"Func<TIn, TOut>_{typeof(TIn).GetHashCode() + typeof(TOut).GetHashCode() + func.GetHashCode()}");
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
