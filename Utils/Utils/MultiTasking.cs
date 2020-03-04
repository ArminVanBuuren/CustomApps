using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utils.CollectionHelper;

namespace Utils
{
    public class MultiTasking
    {
        public static async Task<DoubleDictionary<Action, object>> RunAsync(IEnumerable<Action> actions, int maxThreads = 2, CancellationToken? cancel = null)
        {
            return await Task<DoubleDictionary<Action, object>>.Factory.StartNew(() => Run(actions, maxThreads, cancel));
        }

        public static DoubleDictionary<Action, object> Run(IEnumerable<Action> actions, int maxThreads = 2, CancellationToken? cancel = null)
        {
            var pool = new Semaphore(maxThreads, maxThreads, $"IEnumerable<Action>_{actions.GetHashCode() + 3}");
            var result = new DoubleDictionary<Action, object>(actions.Count());
            var listOfTasks = new List<Task>();

            foreach (var action in actions)
            {
                if (cancel != null && cancel.Value.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    var inputAction = (Action) input;
                    try
                    {
                        if (cancel != null && cancel.Value.IsCancellationRequested)
                            return;

                        inputAction.Invoke();
                        result.Add(inputAction, true);
                    }
                    catch (Exception ex)
                    {
                        result.Add(inputAction, ex);
                    }
                    finally
                    {
                        pool.Release();
                    }
                }, action));
            }

            Task.WaitAll(listOfTasks.ToArray());
            return result;
        }

        public static async Task<DoubleDictionary<Func<T>, object>> RunAsync<T>(IEnumerable<Func<T>> funcs, int maxThreads = 2, CancellationToken? cancel = null)
        {
            return await Task<DoubleDictionary<Func<T>, object>>.Factory.StartNew(() => Run(funcs, maxThreads, cancel));
        }

        public static DoubleDictionary<Func<T>, object> Run<T>(IEnumerable<Func<T>> funcs, int maxThreads = 2, CancellationToken? cancel = null)
        {
            var pool = new Semaphore(maxThreads, maxThreads, $"IEnumerable<Func<T>>_{typeof(T).GetHashCode() + funcs.GetHashCode() + 3}");
            var result = new DoubleDictionary<Func<T>, object>(funcs.Count());
            var listOfTasks = new List<Task>();
            
            foreach (var func in funcs)
            {
                if (cancel != null && cancel.Value.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    var inputFunc = (Func<T>) input;
                    try
                    {
                        if (cancel != null && cancel.Value.IsCancellationRequested)
                            return;

                        var res = inputFunc.Invoke();
                        result.Add(inputFunc, res);
                    }
                    catch (Exception ex)
                    {
                        result.Add(inputFunc, ex);
                    }
                    finally
                    {
                        pool.Release();
                    }
                }, func));
            }

            Task.WaitAll(listOfTasks.ToArray());
            return result;
        }

        public static async Task<DoubleDictionary<T, object>> RunAsync<T>(Action<T> action, IEnumerable<T> data, int maxThreads = 2, CancellationToken? cancel = null)
        {
            return await Task<DoubleDictionary<T, object>>.Factory.StartNew(() => Run(action, data, maxThreads, cancel));
        }

        public static DoubleDictionary<T, object> Run<T>(Action<T> action, IEnumerable<T> data, int maxThreads = 2, CancellationToken? cancel = null)
        {
            var pool = new Semaphore(maxThreads, maxThreads, $"Action<T>_{typeof(T).GetHashCode() + action.GetHashCode() + 3}");
            var result = new DoubleDictionary<T, object>(data.Count());
            var listOfTasks = new List<Task>();

            foreach (var item in data)
            {
                if (cancel != null && cancel.Value.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    var inputItem = (T) input;
                    try
                    {
                        if (cancel != null && cancel.Value.IsCancellationRequested)
                            return;

                        action.Invoke(inputItem);
                        result.Add(inputItem, true);
                    }
                    catch (Exception ex)
                    {
                        result.Add(inputItem, ex);
                    }
                    finally
                    {
                        pool.Release();
                    }
                }, item));
            }

            Task.WaitAll(listOfTasks.ToArray());
            return result;
        }

        public static async Task<DoubleDictionary<TIn, object>> RunAsync<TIn, TOut>(Func<TIn, TOut> func, IEnumerable<TIn> data, int maxThreads = 2, CancellationToken? cancel = null)
        {
            return await Task<DoubleDictionary<TIn, object>>.Factory.StartNew(() => Run(func, data, maxThreads, cancel));
        }

        public static DoubleDictionary<TIn, object> Run<TIn, TOut>(Func<TIn, TOut> func, IEnumerable<TIn> data, int maxThreads = 2, CancellationToken? cancel = null)
        {
            var pool = new Semaphore(maxThreads, maxThreads, $"Func<TIn, TOut>_{typeof(TIn).GetHashCode() + typeof(TOut).GetHashCode() + func.GetHashCode() + 3}");
            var result = new DoubleDictionary<TIn, object>(data.Count());
            var listOfTasks = new List<Task>();

            foreach (var item in data)
            {
                if (cancel != null && cancel.Value.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    var inputItem = (TIn) input;
                    try
                    {
                        if (cancel != null && cancel.Value.IsCancellationRequested)
                            return;

                        var res = func.Invoke(inputItem);
                        result.Add(inputItem, res);
                    }
                    catch (Exception ex)
                    {
                        result.Add(inputItem, ex);
                    }
                    finally
                    {
                        pool.Release();
                    }
                }, item));
            }

            Task.WaitAll(listOfTasks.ToArray());
            return result;
        }
    }
}