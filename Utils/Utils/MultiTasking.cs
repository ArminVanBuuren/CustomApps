using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utils.CollectionHelper;

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
            for (int i = 0; i < _threads.Length; i++)
            {
                _threads[i] = new Thread(() =>
                {
                    foreach (Task t in _tasks.GetConsumingEnumerable())
                        base.TryExecuteTask(t);
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

    [Serializable]
    public class MTFuncResult<TResult> : MultiTaskingResult<Func<TResult>, TResult>
    {
        public override event EventHandler IsCompeted;
        public MTFuncResult(IEnumerable<Func<TResult>> funcs, int maxThreads = 2, ThreadPriority priority = ThreadPriority.Normal, int cancelAfterMilliseconds = -1) : base(funcs, maxThreads, priority, cancelAfterMilliseconds) { }
        public MTFuncResult(IEnumerable<Func<TResult>> funcs, MultiTaskingTemplate mtTemplate) : base(funcs, mtTemplate) { }

        public override void Start()
        {
            base.Start();
            MultiTasking.Run(Source, CallBack, this);
            IsCompeted?.Invoke(this, EventArgs.Empty);
        }
    }

    [Serializable]
    public class MTFuncResult<TSource, TResult> : MultiTaskingResult<TSource, TResult>
    {
        public override event EventHandler IsCompeted;
        private Func<TSource, TResult> _func;
        public MTFuncResult(Func<TSource, TResult> func, IEnumerable<TSource> data, int maxThreads = 2, ThreadPriority priority = ThreadPriority.Normal, int cancelAfterMilliseconds = -1) : base(data, maxThreads, priority, cancelAfterMilliseconds)
        {
            _func = func;
        }

        public MTFuncResult(Func<TSource, TResult> func, IEnumerable<TSource> data, MultiTaskingTemplate mtTemplate) : base(data, mtTemplate)
        {
            _func = func;
        }

        public override void Start()
        {
            base.Start();
            MultiTasking.Run(_func, Source, CallBack, this);
            IsCompeted?.Invoke(this, EventArgs.Empty);
        }
    }
    
    [Serializable]
    public abstract class MultiTaskingResult<TSource, TResult> : MultiTaskingTemplate
    {
        public abstract event EventHandler IsCompeted;
        public ReadOnlyCollection<TSource> Source { get; }
        public MTCallBackList<TSource, TResult> Result { get; protected set; }

        public bool IsCompleted => Source.Count() == Result.Count || CancelToken.IsCancellationRequested;
        public int PercentOfComplete => (Result.Count * 100) / Source.Count();

        protected MultiTaskingResult(IEnumerable<TSource> source, MultiTaskingTemplate mtTemplate) : this(source,  mtTemplate.MaxThreads, mtTemplate.Priority, mtTemplate.CancelAfterMilliseconds) { }

        protected MultiTaskingResult(IEnumerable<TSource> source, int maxThreads, ThreadPriority priority, int cancelAfterMilliseconds) : base( maxThreads, priority, cancelAfterMilliseconds)
        {
            Source = new ReadOnlyCollection<TSource>(source.ToList());
            Result = new MTCallBackList<TSource, TResult>(Source.Count());
        }

        public async Task StartAsync()
        {
            await Task.Factory.StartNew(Start);
        }

        public virtual void Start()
        {
            Result.Clear();
            ReinitCancellationToken();
        }

        protected void CallBack(MTCallBack<TSource, TResult> result)
        {
            Result.Add(result);
        }

        public override string ToString()
        {
            return $"IsCompleted=[{IsCompleted}] PercentOfComplete={PercentOfComplete}";
        }
    }

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

    [Serializable]
    public class MTCallBackList<TSource, TResult>
    {
        private readonly DoubleDictionary<TSource, MTCallBack<TSource, TResult>> _values;

        public int Count => _values.CountValues;

        public IEnumerable<TSource> SourceList => _values.Keys;
        public IEnumerable<MTCallBack<TSource, TResult>> CallBackList => _values.Values;

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

    [Serializable]
    public class MTCallbackTimeoutException : Exception
    {
        public MTCallbackTimeoutException():base("Exception when wait callback")
        {

        }
        public MTCallbackTimeoutException(Exception ex) : base("Exception when wait callback", ex)
        {

        }
    }

    [Serializable]
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

        public static async Task<MTCallBackList<Action, bool>> RunAsync(IEnumerable<Action> actions, MultiTaskingTemplate mtTemplate = null)
        {
            return await Task<MTCallBackList<Action, bool>>.Factory.StartNew(() => Run(actions, mtTemplate));
        }

        public static MTCallBackList<Action, bool> Run(IEnumerable<Action> actions, MultiTaskingTemplate mtTemplate = null)
        {
            var mt = mtTemplate ?? new MultiTaskingTemplate();
            var pool = new Semaphore(mt.MaxThreads, mt.MaxThreads, $"MultiTasking<Action, bool>_{actions.GetHashCode() + 3}");
            var result = new MTCallBackList<Action, bool>(actions.Count());
            var listOfTasks = new List<Task>();

            foreach (var action in actions)
            {
                if (mt.CancelToken.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    var inputAction = (Action) input;
                    try
                    {
                        if (mt.CancelToken.IsCancellationRequested)
                            return;

                        if (mt.Priority != ThreadPriority.Normal)
                            Thread.CurrentThread.Priority = mt.Priority;

                        inputAction.Invoke();

                        result.Add(inputAction,true);
                    }
                    catch (Exception ex)
                    {
                        result.Add(inputAction, ex);
                    }
                    finally
                    {
                        if (mt.Priority != ThreadPriority.Normal)
                            Thread.CurrentThread.Priority = ThreadPriority.Normal;

                        pool.Release();
                    }
                }, action));
            }

            Task.WaitAll(listOfTasks.ToArray());
            return result;
        }

        public static async Task RunAsync(IEnumerable<Action> actions, Action<MTCallBackList<Action, bool>> callback, MultiTaskingTemplate mtTemplate)
        {
            await Task.Factory.StartNew(() => Run(actions, callback, mtTemplate));
        }

        public static void Run(IEnumerable<Action> actions, Action<MTCallBackList<Action, bool>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            var result = Run(actions, mtTemplate);

            if (callback != null)
                Task.Factory.StartNew((callbackItem) => callback.Invoke((MTCallBackList<Action, bool>)callbackItem), result);
        }

        public static async Task RunAsync(IEnumerable<Action> actions, Action<MTCallBack<Action, bool>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            await Task.Factory.StartNew(() => Run(actions, callback, mtTemplate));
        }

        public static void Run(IEnumerable<Action> actions, Action<MTCallBack<Action, bool>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            var mt = mtTemplate ?? new MultiTaskingTemplate();
            var pool = new Semaphore(mt.MaxThreads, mt.MaxThreads, $"MultiTasking_{actions.GetHashCode() + 3}");
            var listOfTasks = new List<Task>();
            //var listOfCallBack = new List<Task>();

            foreach (var action in actions)
            {
                if (mt.CancelToken.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    //Task taskCallback = null;
                    MTCallBack<Action, bool> callbackItem = null;
                    var inputAction = (Action) input;
                    try
                    {
                        if (mt.CancelToken.IsCancellationRequested)
                            return;

                        if (mt.Priority != ThreadPriority.Normal)
                            Thread.CurrentThread.Priority = mt.Priority;

                        inputAction.Invoke();

                        if (callback != null)
                        {
                            callbackItem = new MTCallBack<Action, bool>(inputAction, true);
                            //taskCallback = new Task((callbackItem2) => callback.Invoke((MTCallBack<Action, bool>) callbackItem2), callbackItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (callback != null)
                        {
                            callbackItem = new MTCallBack<Action, bool>(inputAction, ex);
                            //taskCallback = new Task((callbackItem2) => callback.Invoke((MTCallBack<Action, bool>) callbackItem2), callbackItem);
                        }
                    }
                    finally
                    {
                        //if (taskCallback != null)
                        //{
                        //    try
                        //    {
                        //        listOfCallBack.Add(taskCallback);
                        //        taskCallback.Start();
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        callbackItem.Error = callbackItem.Error == null ? new MTCallbackException(ex) : new MTCallbackException(ex, callbackItem.Error);
                        //    }
                        //}

                        //if (taskCallback != null && !taskCallback.Wait(CallbackTimeout))
                        //{
                        //    callbackItem.Error = callbackItem.Error == null ? new MTCallbackTimeoutException() : new MTCallbackTimeoutException(callbackItem.Error);
                        //}

                        try
                        {
                            callback?.Invoke(callbackItem);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        if (mt.Priority != ThreadPriority.Normal)
                            Thread.CurrentThread.Priority = ThreadPriority.Normal;

                        pool.Release();
                    }
                }, action));
            }

            Task.WaitAll(listOfTasks.ToArray());
            //Task.WaitAll(listOfCallBack.ToArray());
        }


        public static async Task<MTCallBackList<Func<TResult>, TResult>> RunAsync<TResult>(IEnumerable<Func<TResult>> funcs, MultiTaskingTemplate mtTemplate = null)
        {
            return await Task<MTCallBackList<Func<TResult>, TResult>>.Factory.StartNew(() => Run(funcs, mtTemplate));
        }

        public static MTCallBackList<Func<TResult>, TResult> Run<TResult>(IEnumerable<Func<TResult>> funcs, MultiTaskingTemplate mtTemplate = null)
        {
            var mt = mtTemplate ?? new MultiTaskingTemplate();
            var pool = new Semaphore(mt.MaxThreads, mt.MaxThreads, $"MultiTasking<Func<TResult>>_{typeof(TResult).GetHashCode() + funcs.GetHashCode() + 3}");
            var result = new MTCallBackList<Func<TResult>, TResult>(funcs.Count());
            var listOfTasks = new List<Task>();
            
            foreach (var func in funcs)
            {
                if (mt.CancelToken.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    var inputFunc = (Func<TResult>) input;
                    try
                    {
                        if (mt.CancelToken.IsCancellationRequested)
                            return;

                        if (mt.Priority != ThreadPriority.Normal)
                            Thread.CurrentThread.Priority = mt.Priority;

                        var res = inputFunc.Invoke();

                        result.Add(inputFunc, res);
                    }
                    catch (Exception ex)
                    {
                        result.Add(inputFunc, ex);
                    }
                    finally
                    {
                        if (mt.Priority != ThreadPriority.Normal)
                            Thread.CurrentThread.Priority = ThreadPriority.Normal;

                        pool.Release();
                    }
                }, func));
            }

            Task.WaitAll(listOfTasks.ToArray());
            return result;
        }

        public static async Task RunAsync<TResult>(IEnumerable<Func<TResult>> funcs, Action<MTCallBackList<Func<TResult>, TResult>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            await Task.Factory.StartNew(() => Run(funcs, callback, mtTemplate));
        }

        public static void Run<TResult>(IEnumerable<Func<TResult>> funcs, Action<MTCallBackList<Func<TResult>, TResult>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            var result = Run(funcs, mtTemplate);

            if (callback != null)
                Task.Factory.StartNew((callbackItem) => callback.Invoke((MTCallBackList<Func<TResult>, TResult>)callbackItem), result);
        }

        public static async Task RunAsync<TResult>(IEnumerable<Func<TResult>> funcs, Action<MTCallBack<Func<TResult>, TResult>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            await Task.Factory.StartNew(() => Run(funcs, callback, mtTemplate));
        }

        public static void Run<TResult>(IEnumerable<Func<TResult>> funcs, Action<MTCallBack<Func<TResult>, TResult>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            var mt = mtTemplate ?? new MultiTaskingTemplate();
            var pool = new Semaphore(mt.MaxThreads, mt.MaxThreads, $"MultiTasking_{typeof(TResult).GetHashCode() + funcs.GetHashCode() + 3}");
            var listOfTasks = new List<Task>();
            //var listOfCallBack = new List<Task>();

            foreach (var func in funcs)
            {
                if (mt.CancelToken.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    //Task taskCallback = null;
                    MTCallBack<Func<TResult>, TResult> callbackItem = null;
                    var inputFunc = (Func<TResult>)input;
                    try
                    {
                        if (mt.CancelToken.IsCancellationRequested)
                            return;

                        if (mt.Priority != ThreadPriority.Normal)
                            Thread.CurrentThread.Priority = mt.Priority;

                        var res = inputFunc.Invoke();

                        if (callback != null)
                        {
                            callbackItem = new MTCallBack<Func<TResult>, TResult>(inputFunc, res);
                            //taskCallback = new Task((callbackItem2) => callback.Invoke((MTCallBack<Func<TResult>, TResult>) callbackItem2), callbackItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (callback != null)
                        {
                            callbackItem = new MTCallBack<Func<TResult>, TResult>(inputFunc, ex);
                            //taskCallback = new Task((callbackItem2) => callback.Invoke((MTCallBack<Func<TResult>, TResult>) callbackItem2), callbackItem);
                        }
                    }
                    finally
                    {
                        //if (taskCallback != null)
                        //{
                        //    try
                        //    {
                        //        listOfCallBack.Add(taskCallback);
                        //        taskCallback.Start();
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        callbackItem.Error = callbackItem.Error == null ? new MTCallbackException(ex) : new MTCallbackException(ex, callbackItem.Error);
                        //    }
                        //}

                        //if (taskCallback != null && !taskCallback.Wait(CallbackTimeout))
                        //{
                        //    callbackItem.Error = callbackItem.Error == null ? new MTCallbackTimeoutException() : new MTCallbackTimeoutException(callbackItem.Error);
                        //}

                        try
                        {
                            callback?.Invoke(callbackItem);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        if (mt.Priority != ThreadPriority.Normal)
                            Thread.CurrentThread.Priority = ThreadPriority.Normal;

                        pool.Release();
                    }
                }, func));
            }

            Task.WaitAll(listOfTasks.ToArray());
            //Task.WaitAll(listOfCallBack.ToArray());
        }

        public static async Task<MTCallBackList<TSource, bool>> RunAsync<TSource>(Action<TSource> action, IEnumerable<TSource> data, MultiTaskingTemplate mtTemplate = null)
        {
            return await Task<MTCallBackList<TSource, bool>>.Factory.StartNew(() => Run(action, data, mtTemplate));
        }

        public static MTCallBackList<TSource, bool> Run<TSource>(Action<TSource> action, IEnumerable<TSource> data, MultiTaskingTemplate mtTemplate = null)
        {
            var mt = mtTemplate ?? new MultiTaskingTemplate();
            var pool = new Semaphore(mt.MaxThreads, mt.MaxThreads, $"MultiTasking<TSource, bool>_{typeof(TSource).GetHashCode() + action.GetHashCode() + 3}");
            var result = new MTCallBackList<TSource, bool>(data.Count());
            var listOfTasks = new List<Task>();

            foreach (var item in data)
            {
                if (mt.CancelToken.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    var inputItem = (TSource) input;
                    try
                    {
                        if (mt.CancelToken.IsCancellationRequested)
                            return;

                        if (mt.Priority != ThreadPriority.Normal)
                            Thread.CurrentThread.Priority = mt.Priority;

                        action.Invoke(inputItem);

                        result.Add(inputItem, true);
                    }
                    catch (Exception ex)
                    {
                        result.Add(inputItem, ex);
                    }
                    finally
                    {
                        if (mt.Priority != ThreadPriority.Normal)
                            Thread.CurrentThread.Priority = ThreadPriority.Normal;

                        pool.Release();
                    }
                }, item));
            }

            Task.WaitAll(listOfTasks.ToArray());
            return result;
        }

        public static async Task RunAsync<TSource>(Action<TSource> action, IEnumerable<TSource> data, Action<MTCallBackList<TSource, bool>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            await Task.Factory.StartNew(() => Run(action, data, callback, mtTemplate));
        }

        public static void Run<TSource>(Action<TSource> action, IEnumerable<TSource> data, Action<MTCallBackList<TSource, bool>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            var result = Run(action, data, mtTemplate);
            
            if (callback != null)
                Task.Factory.StartNew((callbackItem) => callback.Invoke((MTCallBackList<TSource, bool>)callbackItem), result);
        }

        public static async Task RunAsync<TSource>(Action<TSource> action, IEnumerable<TSource> data, Action<MTCallBack<TSource, bool>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            await Task.Factory.StartNew(() => Run(action, data, callback, mtTemplate));
        }

        public static void Run<TSource>(Action<TSource> action, IEnumerable<TSource> data, Action<MTCallBack<TSource, bool>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            var mt = mtTemplate ?? new MultiTaskingTemplate();
            var pool = new Semaphore(mt.MaxThreads, mt.MaxThreads, $"MultiTasking_{typeof(TSource).GetHashCode() + action.GetHashCode() + 3}");
            var listOfTasks = new List<Task>();
            //var listOfCallBack = new List<Task>();

            foreach (var item in data)
            {
                if (mt.CancelToken.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    //Task taskCallback = null;
                    MTCallBack<TSource, bool> callbackItem = null;
                    var inputItem = (TSource)input;
                    try
                    {
                        if (mt.CancelToken.IsCancellationRequested)
                            return;

                        if (mt.Priority != ThreadPriority.Normal)
                            Thread.CurrentThread.Priority = mt.Priority;

                        action.Invoke(inputItem);

                        if (callback != null)
                        {
                            callbackItem = new MTCallBack<TSource, bool>(inputItem, true);
                            //taskCallback = new Task((callbackItem2) => callback.Invoke((MTCallBack<TSource, bool>)callbackItem2), callbackItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (callback != null)
                        {
                            callbackItem = new MTCallBack<TSource, bool>(inputItem, ex);
                            //taskCallback = new Task((callbackItem2) => callback.Invoke((MTCallBack<TSource, bool>)callbackItem2), callbackItem);
                        }
                    }
                    finally
                    {
                        //if (taskCallback != null)
                        //{
                        //    try
                        //    {
                        //        listOfCallBack.Add(taskCallback);
                        //        taskCallback.Start();
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        callbackItem.Error = callbackItem.Error == null ? new MTCallbackException(ex) : new MTCallbackException(ex, callbackItem.Error);
                        //    }
                        //}

                        //taskCallback?.RunSynchronously();
                        //if (taskCallback != null && !taskCallback.Wait(CallbackTimeout))
                        //{
                        //    callbackItem.Error = callbackItem.Error == null ? new MTCallbackTimeoutException() : new MTCallbackTimeoutException(callbackItem.Error);
                        //}

                        try
                        {
                            callback?.Invoke(callbackItem);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        if (mt.Priority != ThreadPriority.Normal)
                            Thread.CurrentThread.Priority = ThreadPriority.Normal;

                        pool.Release();
                    }
                }, item));
            }

            Task.WaitAll(listOfTasks.ToArray());
            //Task.WaitAll(listOfCallBack.ToArray());
        }

        public static async Task<MTCallBackList<TSource, TResult>> RunAsync<TSource, TResult>(Func<TSource, TResult> func, IEnumerable<TSource> data, MultiTaskingTemplate mtTemplate = null)
        {
            return await Task<MTCallBackList<TSource, TResult>>.Factory.StartNew(() => Run(func, data, mtTemplate));
        }

        public static MTCallBackList<TSource, TResult> Run<TSource, TResult>(Func<TSource, TResult> func, IEnumerable<TSource> data, MultiTaskingTemplate mtTemplate = null)
        {
            var mt = mtTemplate ?? new MultiTaskingTemplate();
            var pool = new Semaphore(mt.MaxThreads, mt.MaxThreads, $"MultiTasking<TSource, TResult>_{typeof(TSource).GetHashCode() + typeof(TResult).GetHashCode() + func.GetHashCode() + 3}");
            var result = new MTCallBackList<TSource, TResult>(data.Count());
            var listOfTasks = new List<Task>();

            foreach (var item in data)
            {
                if (mt.CancelToken.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    var inputItem = (TSource) input;
                    try
                    {
                        if (mt.CancelToken.IsCancellationRequested)
                            return;

                        if (mt.Priority != ThreadPriority.Normal)
                            Thread.CurrentThread.Priority = mt.Priority;

                        var res = func.Invoke(inputItem);

                        result.Add(inputItem, res);
                    }
                    catch (Exception ex)
                    {
                        result.Add(inputItem, ex);
                    }
                    finally
                    {
                        if (mt.Priority != ThreadPriority.Normal)
                            Thread.CurrentThread.Priority = ThreadPriority.Normal;

                        pool.Release();
                    }
                }, item));
            }

            Task.WaitAll(listOfTasks.ToArray());
            return result;
        }

        public static async Task RunAsync<TSource, TResult>(Func<TSource, TResult> func, IEnumerable<TSource> data, Action<MTCallBackList<TSource, TResult>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            await Task.Factory.StartNew(() => Run(func, data, callback, mtTemplate));
        }

        public static void Run<TSource, TResult>(Func<TSource, TResult> func, IEnumerable<TSource> data, Action<MTCallBackList<TSource, TResult>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            var result = Run(func, data, mtTemplate);
            
            if (callback != null)
                Task.Factory.StartNew((callbackItem) => callback.Invoke((MTCallBackList<TSource, TResult>) callbackItem), result);
        }

        public static async Task RunAsync<TSource, TResult>(Func<TSource, TResult> func, IEnumerable<TSource> data, Action<MTCallBack<TSource, TResult>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            await Task.Factory.StartNew(() => Run(func, data, callback, mtTemplate));
        }

        public static void Run<TSource, TResult>(Func<TSource, TResult> func, IEnumerable<TSource> data, Action<MTCallBack<TSource, TResult>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            var mt = mtTemplate ?? new MultiTaskingTemplate();
            var pool = new Semaphore(mt.MaxThreads, mt.MaxThreads, $"MultiTasking<TSource, TResult>_{typeof(TSource).GetHashCode() + typeof(TResult).GetHashCode() + func.GetHashCode() + 3}");
            var listOfTasks = new List<Task>();
            //var listOfCallBack = new List<Task>();

            foreach (var item in data)
            {
                if (mt.CancelToken.IsCancellationRequested)
                    break;

                pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((input) =>
                {
                    //Task taskCallback = null;
                    
                    MTCallBack<TSource, TResult> callbackItem = null;
                    var inputItem = (TSource)input;
                    try
                    {
                        if (mt.CancelToken.IsCancellationRequested)
                            return;

                        if (mt.Priority != ThreadPriority.Normal)
                            Thread.CurrentThread.Priority = mt.Priority;

                        var res = func.Invoke(inputItem);

                        if (callback != null)
                        {
                            callbackItem = new MTCallBack<TSource, TResult>(inputItem, res);
                            //taskCallback = new Task((callbackItem2) => callback.Invoke((MTCallBack<TSource, TResult>) callbackItem2), callbackItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (callback != null)
                        {
                            callbackItem = new MTCallBack<TSource, TResult>(inputItem, ex);
                            //taskCallback = new Task((callbackItem2) => callback.Invoke((MTCallBack<TSource, TResult>) callbackItem2), callbackItem);
                        }
                    }
                    finally
                    {
                        //if (taskCallback != null)
                        //{
                        //    try
                        //    {
                        //        listOfCallBack.Add(taskCallback);
                        //        taskCallback.Start();
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        callbackItem.Error = callbackItem.Error == null ? new MTCallbackException(ex) : new MTCallbackException(ex, callbackItem.Error);
                        //    }
                        //}

                        //if (taskCallback != null && !taskCallback.Wait(CallbackTimeout))
                        //{
                        //    callbackItem.Error = callbackItem.Error == null ? new MTCallbackTimeoutException() : new MTCallbackTimeoutException(callbackItem.Error);
                        //}

                        try
                        {
                            callback?.Invoke(callbackItem);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        if (mt.Priority != ThreadPriority.Normal)
                            Thread.CurrentThread.Priority = ThreadPriority.Normal;

                        pool.Release();
                    }
                }, item));
            }

            Task.WaitAll(listOfTasks.ToArray());
            //Task.WaitAll(listOfCallBack.ToArray());
        }
    }
}