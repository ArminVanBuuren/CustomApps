using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;
using Utils.CollectionHelper;

namespace Utils
{
    [Serializable]
    public class MTFuncCallBackList<TSource, TResult>
    {
        private readonly DoubleDictionary<TSource, MTFuncCallBack<TSource, TResult>> _values;
        internal MTFuncCallBackList(int capacity = 4)
        {
            _values = new DoubleDictionary<TSource, MTFuncCallBack<TSource, TResult>>(capacity);
        }

        internal void Add(TSource source, TResult result)
        {
            Add(new MTFuncCallBack<TSource, TResult>(source, result));
        }

        internal void Add(TSource source, Exception exception)
        {
            Add(new MTFuncCallBack<TSource, TResult>(source, exception));
        }

        internal void Add(MTFuncCallBack<TSource, TResult> item)
        {
            _values.Add(item.Source, item);
        }

        public bool TryGetValue(TSource source, out List<MTFuncCallBack<TSource, TResult>> value)
        {
            if (_values.TryGetValue(source, out var res))
            {
                value = res;
                return true;
            }

            value = null;
            return false;
        }

        public int Count => _values.Count;
        public IEnumerable<TSource> Keys => _values.Keys;
        public IEnumerable<MTFuncCallBack<TSource, TResult>> Values => _values.Values;

        public IEnumerator<KeyValuePair<TSource, List<MTFuncCallBack<TSource, TResult>>>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        public override string ToString()
        {
            return this.GetType().ToString();
        }
    }

    [Serializable]
    public class MTFuncCallBack<TSource, TResult>
    {
        internal MTFuncCallBack(TSource source, TResult result)
        {
            IsSuccess = true;
            Source = source;
            Result = result;
        }

        internal MTFuncCallBack(TSource source, Exception error)
        {
            IsSuccess = false;
            Source = source;
            Error = error;
        }

        public bool IsSuccess { get; }
        public TSource Source { get; }
        public TResult Result { get; }
        public Exception Error { get; }

        public override string ToString()
        {
            return IsSuccess ? Result.ToString() : Error.Message;
        }
    }

    [Serializable]
    public class MTActionCallBackList<TSource>
    {
        private readonly DoubleDictionary<TSource, MTActionCallBack<TSource>> _values;
        internal MTActionCallBackList(int capacity = 4)
        {
            _values = new DoubleDictionary<TSource, MTActionCallBack<TSource>>(capacity);
        }

        internal void Add(TSource source)
        {
            Add(new MTActionCallBack<TSource>(source));
        }

        internal void Add(TSource source, Exception exception)
        {
            Add(new MTActionCallBack<TSource>(source, exception));
        }

        internal void Add(MTActionCallBack<TSource> item)
        {
            _values.Add(item.Source, item);
        }

        public bool TryGetValue(TSource source, out List<MTActionCallBack<TSource>> value)
        {
            if (_values.TryGetValue(source, out var res))
            {
                value = res;
                return true;
            }

            value = null;
            return false;
        }

        public int Count => _values.Count;
        public IEnumerable<TSource> Keys => _values.Keys;
        public IEnumerable<MTActionCallBack<TSource>> Values => _values.Values;

        public IEnumerator<KeyValuePair<TSource, List<MTActionCallBack<TSource>>>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        public override string ToString()
        {
            return this.GetType().ToString();
        }
    }

    [Serializable]
    public class MTActionCallBack<TSource>
    {
        internal MTActionCallBack(TSource source)
        {
            IsSuccess = true;
            Source = source;
        }

        internal MTActionCallBack(TSource source, Exception error)
        {
            IsSuccess = false;
            Source = source;
            Error = error;
        }

        public bool IsSuccess { get; }
        public TSource Source { get; }
        public Exception Error { get; }

        public override string ToString()
        {
            return IsSuccess ? Source.ToString() : Error.Message;
        }
    }

    public class MultiTasking
    {
        public static async Task<MTActionCallBackList<Action>> RunAsync(IEnumerable<Action> actions, int maxThreads = 2, CancellationToken? cancel = null)
        {
            return await Task<MTActionCallBackList<Action>>.Factory.StartNew(() => Run(actions, maxThreads, cancel));
        }

        public static MTActionCallBackList<Action> Run(IEnumerable<Action> actions, int maxThreads = 2, CancellationToken? cancel = null)
        {
            var pool = new Semaphore(maxThreads, maxThreads, $"IEnumerable<Action>_1_{actions.GetHashCode() + 3}");
            var result = new MTActionCallBackList<Action>(actions.Count());
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

                        result.Add(inputAction);
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

        public static async Task RunAsync(IEnumerable<Action> actions, Action<MTActionCallBackList<Action>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            await Task.Factory.StartNew(() => Run(actions, callback, maxThreads, cancel));
        }

        public static void Run(IEnumerable<Action> actions, Action<MTActionCallBackList<Action>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            var pool = new Semaphore(maxThreads, maxThreads, $"IEnumerable<Action>_2_{actions.GetHashCode() + 3}");
            var result = new MTActionCallBackList<Action>(actions.Count());
            var listOfTasks = new List<Task>();

            foreach (var action in actions)
            {
                if (cancel != null && cancel.Value.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    var inputAction = (Action)input;
                    try
                    {
                        if (cancel != null && cancel.Value.IsCancellationRequested)
                            return;

                        inputAction.Invoke();

                        result.Add(inputAction);
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

            if (callback != null)
                Task.Factory.StartNew((callbackItem) => callback.Invoke((MTActionCallBackList<Action>)callbackItem), result);
        }

        public static async Task RunAsync(IEnumerable<Action> actions, Action<MTActionCallBack<Action>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            await Task.Factory.StartNew(() => Run(actions, callback, maxThreads, cancel));
        }

        public static void Run(IEnumerable<Action> actions, Action<MTActionCallBack<Action>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            var pool = new Semaphore(maxThreads, maxThreads, $"IEnumerable<Action>_3_{actions.GetHashCode() + 3}");
            var listOfTasks = new List<Task>();

            foreach (var action in actions)
            {
                if (cancel != null && cancel.Value.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    var inputAction = (Action)input;
                    try
                    {
                        if (cancel != null && cancel.Value.IsCancellationRequested)
                            return;

                        inputAction.Invoke();

                        if (callback != null)
                            Task.Factory.StartNew((callbackItem) => callback.Invoke((MTActionCallBack<Action>) callbackItem), new MTActionCallBack<Action>(inputAction));
                    }
                    catch (Exception ex)
                    {
                        if (callback != null)
                            Task.Factory.StartNew((callbackItem) => callback.Invoke((MTActionCallBack<Action>) callbackItem), new MTActionCallBack<Action>(inputAction, ex));
                    }
                    finally
                    {
                        pool.Release();
                    }
                }, action));
            }

            Task.WaitAll(listOfTasks.ToArray());
        }


        public static async Task<MTFuncCallBackList<Func<T>, T>> RunAsync<T>(IEnumerable<Func<T>> funcs, int maxThreads = 2, CancellationToken? cancel = null)
        {
            return await Task<MTFuncCallBackList<Func<T>, T>>.Factory.StartNew(() => Run(funcs, maxThreads, cancel));
        }

        public static MTFuncCallBackList<Func<T>, T> Run<T>(IEnumerable<Func<T>> funcs, int maxThreads = 2, CancellationToken? cancel = null)
        {
            var pool = new Semaphore(maxThreads, maxThreads, $"IEnumerable<Func<T>>_1_{typeof(T).GetHashCode() + funcs.GetHashCode() + 3}");
            var result = new MTFuncCallBackList<Func<T>, T>(funcs.Count());
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

        public static async Task RunAsync<T>(IEnumerable<Func<T>> funcs, Action<MTFuncCallBackList<Func<T>, T>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            await Task.Factory.StartNew(() => Run(funcs, callback, maxThreads, cancel));
        }

        public static void Run<T>(IEnumerable<Func<T>> funcs, Action<MTFuncCallBackList<Func<T>, T>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            var pool = new Semaphore(maxThreads, maxThreads, $"IEnumerable<Func<T>>_2_{typeof(T).GetHashCode() + funcs.GetHashCode() + 3}");
            var result = new MTFuncCallBackList<Func<T>, T>(funcs.Count());
            var listOfTasks = new List<Task>();

            foreach (var func in funcs)
            {
                if (cancel != null && cancel.Value.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    var inputFunc = (Func<T>)input;
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

            if (callback != null)
                Task.Factory.StartNew((callbackItem) => callback.Invoke((MTFuncCallBackList<Func<T>, T>)callbackItem), result);
        }

        public static async Task RunAsync<T>(IEnumerable<Func<T>> funcs, Action<MTFuncCallBack<Func<T>, T>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            await Task.Factory.StartNew(() => Run(funcs, callback, maxThreads, cancel));
        }

        public static void Run<T>(IEnumerable<Func<T>> funcs, Action<MTFuncCallBack<Func<T>, T>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            var pool = new Semaphore(maxThreads, maxThreads, $"IEnumerable<Func<T>>_3_{typeof(T).GetHashCode() + funcs.GetHashCode() + 3}");
            var listOfTasks = new List<Task>();

            foreach (var func in funcs)
            {
                if (cancel != null && cancel.Value.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    var inputFunc = (Func<T>)input;
                    try
                    {
                        if (cancel != null && cancel.Value.IsCancellationRequested)
                            return;

                        var res = inputFunc.Invoke();

                        if (callback != null)
                            Task.Factory.StartNew((callbackItem) => callback.Invoke((MTFuncCallBack<Func<T>, T>)callbackItem), new MTFuncCallBack<Func<T>, T>(inputFunc, res));
                    }
                    catch (Exception ex)
                    {
                        if (callback != null)
                            Task.Factory.StartNew((callbackItem) => callback.Invoke((MTFuncCallBack<Func<T>, T>)callbackItem), new MTFuncCallBack<Func<T>, T>(inputFunc, ex));
                    }
                    finally
                    {
                        pool.Release();
                    }
                }, func));
            }

            Task.WaitAll(listOfTasks.ToArray());
        }

        public static async Task<MTActionCallBackList<T>> RunAsync<T>(Action<T> action, IEnumerable<T> data, int maxThreads = 2, CancellationToken? cancel = null)
        {
            return await Task<MTActionCallBackList<T>>.Factory.StartNew(() => Run(action, data, maxThreads, cancel));
        }

        public static MTActionCallBackList<T> Run<T>(Action<T> action, IEnumerable<T> data, int maxThreads = 2, CancellationToken? cancel = null)
        {
            var pool = new Semaphore(maxThreads, maxThreads, $"Action<T>_1_{typeof(T).GetHashCode() + action.GetHashCode() + 3}");
            var result = new MTActionCallBackList<T>(data.Count());
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

                        result.Add(inputItem);
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

        public static async Task RunAsync<T>(Action<T> action, IEnumerable<T> data, Action<MTActionCallBackList<T>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            await Task.Factory.StartNew(() => Run(action, data, callback, maxThreads, cancel));
        }

        public static void Run<T>(Action<T> action, IEnumerable<T> data, Action<MTActionCallBackList<T>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            var pool = new Semaphore(maxThreads, maxThreads, $"Action<T>_2_{typeof(T).GetHashCode() + action.GetHashCode() + 3}");
            var result = new MTActionCallBackList<T>(data.Count());
            var listOfTasks = new List<Task>();

            foreach (var item in data)
            {
                if (cancel != null && cancel.Value.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    var inputItem = (T)input;
                    try
                    {
                        if (cancel != null && cancel.Value.IsCancellationRequested)
                            return;

                        action.Invoke(inputItem);

                        result.Add(inputItem);
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

            if (callback != null)
                Task.Factory.StartNew((callbackItem) => callback.Invoke((MTActionCallBackList<T>)callbackItem), result);
        }

        public static async Task RunAsync<T>(Action<T> action, IEnumerable<T> data, Action<MTActionCallBack<T>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            await Task.Factory.StartNew(() => Run(action, data, callback, maxThreads, cancel));
        }

        public static void Run<T>(Action<T> action, IEnumerable<T> data, Action<MTActionCallBack<T>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            var pool = new Semaphore(maxThreads, maxThreads, $"Action<T>_3_{typeof(T).GetHashCode() + action.GetHashCode() + 3}");
            var listOfTasks = new List<Task>();

            foreach (var item in data)
            {
                if (cancel != null && cancel.Value.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    var inputItem = (T)input;
                    try
                    {
                        if (cancel != null && cancel.Value.IsCancellationRequested)
                            return;

                        action.Invoke(inputItem);

                        if (callback != null)
                            Task.Factory.StartNew((callbackItem) => callback.Invoke((MTActionCallBack<T>)callbackItem), new MTActionCallBack<T>(inputItem));
                    }
                    catch (Exception ex)
                    {
                        if (callback != null)
                            Task.Factory.StartNew((callbackItem) => callback.Invoke((MTActionCallBack<T>)callbackItem), new MTActionCallBack<T>(inputItem, ex));
                    }
                    finally
                    {
                        pool.Release();
                    }
                }, item));
            }

            Task.WaitAll(listOfTasks.ToArray());
        }

        public static async Task<MTFuncCallBackList<TIn, TOut>> RunAsync<TIn, TOut>(Func<TIn, TOut> func, IEnumerable<TIn> data, int maxThreads = 2, CancellationToken? cancel = null)
        {
            return await Task<MTFuncCallBackList<TIn, TOut>>.Factory.StartNew(() => Run(func, data, maxThreads, cancel));
        }

        public static MTFuncCallBackList<TIn, TOut> Run<TIn, TOut>(Func<TIn, TOut> func, IEnumerable<TIn> data, int maxThreads = 2, CancellationToken? cancel = null)
        {
            var pool = new Semaphore(maxThreads, maxThreads, $"Func<TIn, TOut>_1_{typeof(TIn).GetHashCode() + typeof(TOut).GetHashCode() + func.GetHashCode() + 3}");
            var result = new MTFuncCallBackList<TIn, TOut>(data.Count());
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

        public static async Task RunAsync<TIn, TOut>(Func<TIn, TOut> func, IEnumerable<TIn> data, Action<MTFuncCallBackList<TIn, TOut>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            await Task.Factory.StartNew(() => Run(func, data, callback, maxThreads, cancel));
        }

        public static void Run<TIn, TOut>(Func<TIn, TOut> func, IEnumerable<TIn> data, Action<MTFuncCallBackList<TIn, TOut>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            var pool = new Semaphore(maxThreads, maxThreads, $"Func<TIn, TOut>_2_{typeof(TIn).GetHashCode() + typeof(TOut).GetHashCode() + func.GetHashCode() + 3}");
            var result = new MTFuncCallBackList<TIn, TOut>(data.Count());
            var listOfTasks = new List<Task>();

            foreach (var item in data)
            {
                if (cancel != null && cancel.Value.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    var inputItem = (TIn)input;
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

            if (callback != null)
                Task.Factory.StartNew((callbackItem) => callback.Invoke((MTFuncCallBackList<TIn, TOut>) callbackItem), result);
        }

        public static async Task RunAsync<TIn, TOut>(Func<TIn, TOut> func, IEnumerable<TIn> data, Action<MTFuncCallBack<TIn, TOut>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            await Task.Factory.StartNew(() => Run(func, data, callback, maxThreads, cancel));
        }

        public static void Run<TIn, TOut>(Func<TIn, TOut> func, IEnumerable<TIn> data, Action<MTFuncCallBack<TIn, TOut>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            var pool = new Semaphore(maxThreads, maxThreads, $"Func<TIn, TOut>_3_{typeof(TIn).GetHashCode() + typeof(TOut).GetHashCode() + func.GetHashCode() + 3}");
            var listOfTasks = new List<Task>();

            foreach (var item in data)
            {
                if (cancel != null && cancel.Value.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    var inputItem = (TIn)input;
                    try
                    {
                        if (cancel != null && cancel.Value.IsCancellationRequested)
                            return;

                        var res = func.Invoke(inputItem);

                        if (callback != null)
                            Task.Factory.StartNew((callbackItem) => callback.Invoke((MTFuncCallBack<TIn, TOut>)callbackItem), new MTFuncCallBack<TIn, TOut>(inputItem, res));
                    }
                    catch (Exception ex)
                    {
                        if (callback != null)
                            Task.Factory.StartNew((callbackItem) => callback.Invoke((MTFuncCallBack<TIn, TOut>)callbackItem), new MTFuncCallBack<TIn, TOut>(inputItem, ex));
                    }
                    finally
                    {
                        pool.Release();
                    }
                }, item));
            }

            Task.WaitAll(listOfTasks.ToArray());
        }
    }
}