using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utils.CollectionHelper;

namespace Utils
{
    [Serializable]
    public class MTActionResult : MultiTaskingResult<Action, bool>
    {
        public override event EventHandler IsCompeted;
        public MTActionResult(IEnumerable<Action> actions, int maxThreads = 2, int millisecondsDalay = -1) : base(actions, maxThreads, millisecondsDalay)
        {

        }

        public override void Start()
        {
            base.Start();
            MultiTasking.Run(Source, CallBack, MaxThreads, CancelToken.Token);
            IsCompeted?.Invoke(this, EventArgs.Empty);
        }
    }

    [Serializable]
    public class MTActionResult<TSource> : MultiTaskingResult<TSource, bool>
    {
        public override event EventHandler IsCompeted;
        private Action<TSource> _action;
        public MTActionResult(Action<TSource> action, IEnumerable<TSource> data, int maxThreads = 2, int millisecondsDalay = -1) : base(data, maxThreads, millisecondsDalay)
        {
            _action = action;
        }

        public override void Start()
        {
            base.Start();
            MultiTasking.Run(_action, Source, CallBack, MaxThreads, CancelToken.Token);
            IsCompeted?.Invoke(this, EventArgs.Empty);
        }
    }

    [Serializable]
    public class MTFuncResult<TResult> : MultiTaskingResult<Func<TResult>, TResult>
    {
        public override event EventHandler IsCompeted;
        public MTFuncResult(IEnumerable<Func<TResult>> funcs, int maxThreads = 2, int millisecondsDalay = -1) : base(funcs, maxThreads, millisecondsDalay)
        {

        }

        public override void Start()
        {
            base.Start();
            MultiTasking.Run(Source, CallBack, MaxThreads, CancelToken.Token);
            IsCompeted?.Invoke(this, EventArgs.Empty);
        }
    }

    [Serializable]
    public class MTFuncResult<TSource, TResult> : MultiTaskingResult<TSource, TResult>
    {
        public override event EventHandler IsCompeted;
        private Func<TSource, TResult> _func;
        public MTFuncResult(Func<TSource, TResult> func, IEnumerable<TSource> data, int maxThreads = 2, int millisecondsDalay = -1) : base(data, maxThreads, millisecondsDalay)
        {
            _func = func;
        }

        public override void Start()
        {
            base.Start();
            MultiTasking.Run(_func, Source, CallBack, MaxThreads, CancelToken.Token);
            IsCompeted?.Invoke(this, EventArgs.Empty);
        }
    }
    
    [Serializable]
    public abstract class MultiTaskingResult<TSource, TResult>
    {
        public abstract event EventHandler IsCompeted;
        public int MaxThreads { get; }
        public int MillisecondsDalay { get; }
        public IEnumerable<TSource> Source { get; protected set; }
        public MTCallBackList<TSource, TResult> Result { get; protected set; }
        public bool IsCompleted => Source.Count() == Result.Values.Count() || (CancelToken != null && CancelToken.IsCancellationRequested);
        public int PercentOfComplete => (Result.Values.Count() * 100) / Source.Count();
        protected CancellationTokenSource CancelToken { get; private set; }

        protected MultiTaskingResult(IEnumerable<TSource> source, int maxThreads, int millisecondsDalay)
        {
            Source = source;
            Result = new MTCallBackList<TSource, TResult>(Source.Count());
            MaxThreads = maxThreads;
            MillisecondsDalay = millisecondsDalay;
        }

        public async Task StartAsync()
        {
            await Task.Factory.StartNew(Start);
        }

        public virtual void Start()
        {
            Result.Clear();
            CancelToken = new CancellationTokenSource();

            if (MillisecondsDalay > 0)
                CancelToken.CancelAfter(MillisecondsDalay);
        }

        protected void CallBack(MTCallBack<TSource, TResult> result)
        {
            Result.Add(result);
        }

        public void Stop()
        {
            CancelToken?.Cancel();
        }

        public override string ToString()
        {
            return $"IsCompleted=[{IsCompleted}] PercentOfComplete={PercentOfComplete}";
        }
    }

    [Serializable]
    public class MTCallBackList<TSource, TResult>
    {
        private readonly DoubleDictionary<TSource, MTCallBack<TSource, TResult>> _values;

        public int Count => _values.Count;
        public IEnumerable<TSource> Keys => _values.Keys;
        public IEnumerable<MTCallBack<TSource, TResult>> Values => _values.Values;

        internal MTCallBackList(int capacity = 4)
        {
            _values = new DoubleDictionary<TSource, MTCallBack<TSource, TResult>>(capacity);
        }

        internal void Add(TSource source, TResult result)
        {
            Add(new MTCallBack<TSource, TResult>(source, result));
        }

        internal void Add(TSource source, Exception error)
        {
            Add(new MTCallBack<TSource, TResult>(source, error));
        }

        internal void Add(MTCallBack<TSource, TResult> item)
        {
            _values.Add(item.Source, item);
        }

        internal void Clear()
        {
            _values.Clear();
        }

        public bool TryGetValue(TSource source, out List<MTCallBack<TSource, TResult>> value)
        {
            if (_values.TryGetValue(source, out var res))
            {
                value = res;
                return true;
            }

            value = null;
            return false;
        }

        public IEnumerator<KeyValuePair<TSource, List<MTCallBack<TSource, TResult>>>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        public override string ToString()
        {
            return GetType().ToString();
        }
    }

    [Serializable]
    public class MTCallBack<TSource, TResult>
    {
        public TSource Source { get; }
        public TResult Result { get; }
        public Exception Error { get; internal set; }

        internal MTCallBack(TSource source, TResult result)
        {
            Source = source;
            Result = result;
        }

        internal MTCallBack(TSource source, Exception error)
        {
            Source = source;
            Error = error;
        }

        public override string ToString()
        {
            return Error == null ? $"{Source}=[{Result}]" : Error.Message;
        }
    }

    public class MTCallbackTimeoutException : Exception
    {
        public MTCallbackTimeoutException():base("Exception when wait callback")
        {

        }
        public MTCallbackTimeoutException(Exception ex) : base("Exception when wait callback", ex)
        {

        }
    }

    public class MTCallbackException : Exception
    {
        public Exception CallbackError { get; }

        public MTCallbackException(Exception callBackError) : base("Exception when processing callback")
        {
            CallbackError = callBackError;
        }
        public MTCallbackException(Exception taskError, Exception callBackError) : base("Exception when processing callback", taskError)
        {
            CallbackError = callBackError;
        }
    }

    public class MultiTasking
    {
        //public const int CallbackTimeout = 3000;

        public static async Task<MTCallBackList<Action, bool>> RunAsync(IEnumerable<Action> actions, int maxThreads = 2, CancellationToken? cancel = null)
        {
            return await Task<MTCallBackList<Action, bool>>.Factory.StartNew(() => Run(actions, maxThreads, cancel));
        }

        public static MTCallBackList<Action, bool> Run(IEnumerable<Action> actions, int maxThreads = 2, CancellationToken? cancel = null)
        {
            var pool = new Semaphore(maxThreads, maxThreads, $"IEnumerable<Action>_1_{actions.GetHashCode() + 3}");
            var result = new MTCallBackList<Action, bool>(actions.Count());
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

                        result.Add(inputAction,true);
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

        public static async Task RunAsync(IEnumerable<Action> actions, Action<MTCallBackList<Action, bool>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            await Task.Factory.StartNew(() => Run(actions, callback, maxThreads, cancel));
        }

        public static void Run(IEnumerable<Action> actions, Action<MTCallBackList<Action, bool>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            var result = Run(actions, maxThreads, cancel);

            if (callback != null)
                Task.Factory.StartNew((callbackItem) => callback.Invoke((MTCallBackList<Action, bool>)callbackItem), result);
        }

        public static async Task RunAsync(IEnumerable<Action> actions, Action<MTCallBack<Action, bool>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            await Task.Factory.StartNew(() => Run(actions, callback, maxThreads, cancel));
        }

        public static void Run(IEnumerable<Action> actions, Action<MTCallBack<Action, bool>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            var pool = new Semaphore(maxThreads, maxThreads, $"IEnumerable<Action>_3_{actions.GetHashCode() + 3}");
            var listOfTasks = new List<Task>();
            var listOfCallBack = new List<Task>();

            foreach (var action in actions)
            {
                if (cancel != null && cancel.Value.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    Task taskCallback = null;
                    MTCallBack<Action, bool> callbackItem = null;
                    var inputAction = (Action)input;
                    try
                    {
                        if (cancel != null && cancel.Value.IsCancellationRequested)
                            return;

                        inputAction.Invoke();

                        if (callback != null)
                        {
                            callbackItem = new MTCallBack<Action, bool>(inputAction, true);
                            taskCallback = new Task((callbackItem2) => callback.Invoke((MTCallBack<Action, bool>) callbackItem2), callbackItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (callback != null)
                        {
                            callbackItem = new MTCallBack<Action, bool>(inputAction, ex);
                            taskCallback = new Task((callbackItem2) => callback.Invoke((MTCallBack<Action, bool>) callbackItem2), callbackItem);
                        }
                    }
                    finally
                    {
                        if (taskCallback != null)
                        {
                            try
                            {
                                listOfCallBack.Add(taskCallback);
                                taskCallback.Start();
                            }
                            catch (Exception ex)
                            {
                                callbackItem.Error = callbackItem.Error == null ? new MTCallbackException(ex) : new MTCallbackException(ex, callbackItem.Error);
                            }
                        }

                        //if (taskCallback != null && !taskCallback.Wait(CallbackTimeout))
                        //{
                        //    callbackItem.Error = callbackItem.Error == null ? new MTCallbackTimeoutException() : new MTCallbackTimeoutException(callbackItem.Error);
                        //}
                        pool.Release();
                    }
                }, action));
            }

            Task.WaitAll(listOfTasks.ToArray());
            Task.WaitAll(listOfCallBack.ToArray());
        }


        public static async Task<MTCallBackList<Func<TResult>, TResult>> RunAsync<TResult>(IEnumerable<Func<TResult>> funcs, int maxThreads = 2, CancellationToken? cancel = null)
        {
            return await Task<MTCallBackList<Func<TResult>, TResult>>.Factory.StartNew(() => Run(funcs, maxThreads, cancel));
        }

        public static MTCallBackList<Func<TResult>, TResult> Run<TResult>(IEnumerable<Func<TResult>> funcs, int maxThreads = 2, CancellationToken? cancel = null)
        {
            var pool = new Semaphore(maxThreads, maxThreads, $"IEnumerable<Func<TResult>>_1_{typeof(TResult).GetHashCode() + funcs.GetHashCode() + 3}");
            var result = new MTCallBackList<Func<TResult>, TResult>(funcs.Count());
            var listOfTasks = new List<Task>();
            
            foreach (var func in funcs)
            {
                if (cancel != null && cancel.Value.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    var inputFunc = (Func<TResult>) input;
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

        public static async Task RunAsync<TResult>(IEnumerable<Func<TResult>> funcs, Action<MTCallBackList<Func<TResult>, TResult>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            await Task.Factory.StartNew(() => Run(funcs, callback, maxThreads, cancel));
        }

        public static void Run<TResult>(IEnumerable<Func<TResult>> funcs, Action<MTCallBackList<Func<TResult>, TResult>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            var result = Run(funcs, maxThreads, cancel);

            if (callback != null)
                Task.Factory.StartNew((callbackItem) => callback.Invoke((MTCallBackList<Func<TResult>, TResult>)callbackItem), result);
        }

        public static async Task RunAsync<TResult>(IEnumerable<Func<TResult>> funcs, Action<MTCallBack<Func<TResult>, TResult>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            await Task.Factory.StartNew(() => Run(funcs, callback, maxThreads, cancel));
        }

        public static void Run<TResult>(IEnumerable<Func<TResult>> funcs, Action<MTCallBack<Func<TResult>, TResult>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            var pool = new Semaphore(maxThreads, maxThreads, $"IEnumerable<Func<TResult>>_3_{typeof(TResult).GetHashCode() + funcs.GetHashCode() + 3}");
            var listOfTasks = new List<Task>();
            var listOfCallBack = new List<Task>();

            foreach (var func in funcs)
            {
                if (cancel != null && cancel.Value.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    Task taskCallback = null;
                    MTCallBack<Func<TResult>, TResult> callbackItem = null;
                    var inputFunc = (Func<TResult>)input;
                    try
                    {
                        if (cancel != null && cancel.Value.IsCancellationRequested)
                            return;

                        var res = inputFunc.Invoke();

                        if (callback != null)
                        {
                            callbackItem = new MTCallBack<Func<TResult>, TResult>(inputFunc, res);
                            taskCallback = new Task((callbackItem2) => callback.Invoke((MTCallBack<Func<TResult>, TResult>) callbackItem2), callbackItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (callback != null)
                        {
                            callbackItem = new MTCallBack<Func<TResult>, TResult>(inputFunc, ex);
                            taskCallback = new Task((callbackItem2) => callback.Invoke((MTCallBack<Func<TResult>, TResult>) callbackItem2), callbackItem);
                        }
                    }
                    finally
                    {
                        if (taskCallback != null)
                        {
                            try
                            {
                                listOfCallBack.Add(taskCallback);
                                taskCallback.Start();
                            }
                            catch (Exception ex)
                            {
                                callbackItem.Error = callbackItem.Error == null ? new MTCallbackException(ex) : new MTCallbackException(ex, callbackItem.Error);
                            }
                        }

                        //if (taskCallback != null && !taskCallback.Wait(CallbackTimeout))
                        //{
                        //    callbackItem.Error = callbackItem.Error == null ? new MTCallbackTimeoutException() : new MTCallbackTimeoutException(callbackItem.Error);
                        //}
                        pool.Release();
                    }
                }, func));
            }

            Task.WaitAll(listOfTasks.ToArray());
            Task.WaitAll(listOfCallBack.ToArray());
        }

        public static async Task<MTCallBackList<TSource, bool>> RunAsync<TSource>(Action<TSource> action, IEnumerable<TSource> data, int maxThreads = 2, CancellationToken? cancel = null)
        {
            return await Task<MTCallBackList<TSource, bool>>.Factory.StartNew(() => Run(action, data, maxThreads, cancel));
        }

        public static MTCallBackList<TSource, bool> Run<TSource>(Action<TSource> action, IEnumerable<TSource> data, int maxThreads = 2, CancellationToken? cancel = null)
        {
            var pool = new Semaphore(maxThreads, maxThreads, $"Action<TSource>_1_{typeof(TSource).GetHashCode() + action.GetHashCode() + 3}");
            var result = new MTCallBackList<TSource, bool>(data.Count());
            var listOfTasks = new List<Task>();

            foreach (var item in data)
            {
                if (cancel != null && cancel.Value.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    var inputItem = (TSource) input;
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

        public static async Task RunAsync<TSource>(Action<TSource> action, IEnumerable<TSource> data, Action<MTCallBackList<TSource, bool>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            await Task.Factory.StartNew(() => Run(action, data, callback, maxThreads, cancel));
        }

        public static void Run<TSource>(Action<TSource> action, IEnumerable<TSource> data, Action<MTCallBackList<TSource, bool>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            var result = Run(action, data, maxThreads, cancel);
            
            if (callback != null)
                Task.Factory.StartNew((callbackItem) => callback.Invoke((MTCallBackList<TSource, bool>)callbackItem), result);
        }

        public static async Task RunAsync<TSource>(Action<TSource> action, IEnumerable<TSource> data, Action<MTCallBack<TSource, bool>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            await Task.Factory.StartNew(() => Run(action, data, callback, maxThreads, cancel));
        }

        public static void Run<TSource>(Action<TSource> action, IEnumerable<TSource> data, Action<MTCallBack<TSource, bool>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            var pool = new Semaphore(maxThreads, maxThreads, $"Action<TSource>_3_{typeof(TSource).GetHashCode() + action.GetHashCode() + 3}");
            var listOfTasks = new List<Task>();
            var listOfCallBack = new List<Task>();

            foreach (var item in data)
            {
                if (cancel != null && cancel.Value.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    Task taskCallback = null;
                    MTCallBack<TSource, bool> callbackItem = null;
                    var inputItem = (TSource)input;
                    try
                    {
                        if (cancel != null && cancel.Value.IsCancellationRequested)
                            return;

                        action.Invoke(inputItem);

                        if (callback != null)
                        {
                            callbackItem = new MTCallBack<TSource, bool>(inputItem, true);
                            taskCallback = new Task((callbackItem2) => callback.Invoke((MTCallBack<TSource, bool>)callbackItem2), callbackItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (callback != null)
                        {
                            callbackItem = new MTCallBack<TSource, bool>(inputItem, ex);
                            taskCallback = new Task((callbackItem2) => callback.Invoke((MTCallBack<TSource, bool>)callbackItem2), callbackItem);
                        }
                    }
                    finally
                    {
                        if (taskCallback != null)
                        {
                            try
                            {
                                listOfCallBack.Add(taskCallback);
                                taskCallback.Start();
                            }
                            catch (Exception ex)
                            {
                                callbackItem.Error = callbackItem.Error == null ? new MTCallbackException(ex) : new MTCallbackException(ex, callbackItem.Error);
                            }
                        }

                        //taskCallback?.RunSynchronously();
                        //if (taskCallback != null && !taskCallback.Wait(CallbackTimeout))
                        //{
                        //    callbackItem.Error = callbackItem.Error == null ? new MTCallbackTimeoutException() : new MTCallbackTimeoutException(callbackItem.Error);
                        //}
                        pool.Release();
                    }
                }, item));
            }

            Task.WaitAll(listOfTasks.ToArray());
            Task.WaitAll(listOfCallBack.ToArray());
        }

        public static async Task<MTCallBackList<TSource, TResult>> RunAsync<TSource, TResult>(Func<TSource, TResult> func, IEnumerable<TSource> data, int maxThreads = 2, CancellationToken? cancel = null)
        {
            return await Task<MTCallBackList<TSource, TResult>>.Factory.StartNew(() => Run(func, data, maxThreads, cancel));
        }

        public static MTCallBackList<TSource, TResult> Run<TSource, TResult>(Func<TSource, TResult> func, IEnumerable<TSource> data, int maxThreads = 2, CancellationToken? cancel = null)
        {
            var pool = new Semaphore(maxThreads, maxThreads, $"Func<TSource, TResult>_1_{typeof(TSource).GetHashCode() + typeof(TResult).GetHashCode() + func.GetHashCode() + 3}");
            var result = new MTCallBackList<TSource, TResult>(data.Count());
            var listOfTasks = new List<Task>();

            foreach (var item in data)
            {
                if (cancel != null && cancel.Value.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    var inputItem = (TSource) input;
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

        public static async Task RunAsync<TSource, TResult>(Func<TSource, TResult> func, IEnumerable<TSource> data, Action<MTCallBackList<TSource, TResult>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            await Task.Factory.StartNew(() => Run(func, data, callback, maxThreads, cancel));
        }

        public static void Run<TSource, TResult>(Func<TSource, TResult> func, IEnumerable<TSource> data, Action<MTCallBackList<TSource, TResult>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            var result = Run(func, data, maxThreads, cancel);
            
            if (callback != null)
                Task.Factory.StartNew((callbackItem) => callback.Invoke((MTCallBackList<TSource, TResult>) callbackItem), result);
        }

        public static async Task RunAsync<TSource, TResult>(Func<TSource, TResult> func, IEnumerable<TSource> data, Action<MTCallBack<TSource, TResult>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            await Task.Factory.StartNew(() => Run(func, data, callback, maxThreads, cancel));
        }

        public static void Run<TSource, TResult>(Func<TSource, TResult> func, IEnumerable<TSource> data, Action<MTCallBack<TSource, TResult>> callback, int maxThreads = 2, CancellationToken? cancel = null)
        {
            var pool = new Semaphore(maxThreads, maxThreads, $"Func<TSource, TResult>_3_{typeof(TSource).GetHashCode() + typeof(TResult).GetHashCode() + func.GetHashCode() + 3}");
            var listOfTasks = new List<Task>();
            var listOfCallBack = new List<Task>();

            foreach (var item in data)
            {
                if (cancel != null && cancel.Value.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    Task taskCallback = null;
                    MTCallBack<TSource, TResult> callbackItem = null;
                    var inputItem = (TSource)input;
                    try
                    {
                        if (cancel != null && cancel.Value.IsCancellationRequested)
                            return;

                        var res = func.Invoke(inputItem);

                        if (callback != null)
                        {
                            callbackItem = new MTCallBack<TSource, TResult>(inputItem, res);
                            taskCallback = new Task((callbackItem2) => callback.Invoke((MTCallBack<TSource, TResult>) callbackItem2), callbackItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (callback != null)
                        {
                            callbackItem = new MTCallBack<TSource, TResult>(inputItem, ex);
                            taskCallback = new Task((callbackItem2) => callback.Invoke((MTCallBack<TSource, TResult>) callbackItem2), callbackItem);
                        }
                    }
                    finally
                    {
                        if (taskCallback != null)
                        {
                            try
                            {
                                listOfCallBack.Add(taskCallback);
                                taskCallback.Start();
                            }
                            catch (Exception ex)
                            {
                                callbackItem.Error = callbackItem.Error == null ? new MTCallbackException(ex) : new MTCallbackException(ex, callbackItem.Error);
                            }
                        }

                        //if (taskCallback != null && !taskCallback.Wait(CallbackTimeout))
                        //{
                        //    callbackItem.Error = callbackItem.Error == null ? new MTCallbackTimeoutException() : new MTCallbackTimeoutException(callbackItem.Error);
                        //}
                        pool.Release();
                    }
                }, item));
            }

            Task.WaitAll(listOfTasks.ToArray());
            Task.WaitAll(listOfCallBack.ToArray());
        }
    }
}