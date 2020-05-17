using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Utils
{
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
            using (var pool = new Semaphore(mt.MaxThreads, mt.MaxThreads, $"MultiTasking<Action, bool>_{actions.GetHashCode() + 3}"))
            {
                var result = new MTCallBackList<Action, bool>();
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

                            result.Add(inputAction, true);
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
            using (var pool = new Semaphore(mt.MaxThreads, mt.MaxThreads, $"MultiTasking_{actions.GetHashCode() + 3}"))
            {
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
                                if (callback != null && callbackItem != null)
                                    callback.Invoke(callbackItem);
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
        }


        public static async Task<MTCallBackList<Func<TResult>, TResult>> RunAsync<TResult>(IEnumerable<Func<TResult>> funcs, MultiTaskingTemplate mtTemplate = null)
        {
            return await Task<MTCallBackList<Func<TResult>, TResult>>.Factory.StartNew(() => Run(funcs, mtTemplate));
        }

        public static MTCallBackList<Func<TResult>, TResult> Run<TResult>(IEnumerable<Func<TResult>> funcs, MultiTaskingTemplate mtTemplate = null)
        {
            var mt = mtTemplate ?? new MultiTaskingTemplate();
            using (var pool = new Semaphore(mt.MaxThreads, mt.MaxThreads, $"MultiTasking<Func<TResult>>_{typeof(TResult).GetHashCode() + funcs.GetHashCode() + 3}"))
            {
                var result = new MTCallBackList<Func<TResult>, TResult>();
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
            using (var pool = new Semaphore(mt.MaxThreads, mt.MaxThreads, $"MultiTasking_{typeof(TResult).GetHashCode() + funcs.GetHashCode() + 3}"))
            {
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
                        var inputFunc = (Func<TResult>) input;
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
                                if (callback != null && callbackItem != null)
                                    callback.Invoke(callbackItem);
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
        }

        public static async Task<MTCallBackList<TSource, bool>> RunAsync<TSource>(Action<TSource> action, IEnumerable<TSource> data, MultiTaskingTemplate mtTemplate = null)
        {
            return await Task<MTCallBackList<TSource, bool>>.Factory.StartNew(() => Run(action, data, mtTemplate));
        }

        public static MTCallBackList<TSource, bool> Run<TSource>(Action<TSource> action, IEnumerable<TSource> data, MultiTaskingTemplate mtTemplate = null)
        {
            var mt = mtTemplate ?? new MultiTaskingTemplate();
            using (var pool = new Semaphore(mt.MaxThreads, mt.MaxThreads, $"MultiTasking<TSource, bool>_{typeof(TSource).GetHashCode() + action.GetHashCode() + 3}"))
            {
                var result = new MTCallBackList<TSource, bool>();
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
            using (var pool = new Semaphore(mt.MaxThreads, mt.MaxThreads, $"MultiTasking_{typeof(TSource).GetHashCode() + action.GetHashCode() + 3}"))
            {
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
                        var inputItem = (TSource) input;
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
                                if (callback != null && callbackItem != null)
                                    callback.Invoke(callbackItem);
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

        public static async Task<MTCallBackList<TSource, TResult>> RunAsync<TSource, TResult>(Func<TSource, TResult> func, IEnumerable<TSource> data, MultiTaskingTemplate mtTemplate = null)
        {
            return await Task<MTCallBackList<TSource, TResult>>.Factory.StartNew(() => Run(func, data, mtTemplate));
        }

        public static MTCallBackList<TSource, TResult> Run<TSource, TResult>(Func<TSource, TResult> func, IEnumerable<TSource> data, MultiTaskingTemplate mtTemplate = null)
        {
            var mt = mtTemplate ?? new MultiTaskingTemplate();
            using (var pool = new Semaphore(mt.MaxThreads, mt.MaxThreads, $"MultiTasking<TSource, TResult>_{typeof(TSource).GetHashCode() + typeof(TResult).GetHashCode() + func.GetHashCode() + 3}"))
            {
                var result = new MTCallBackList<TSource, TResult>();
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
            using (var pool = new Semaphore(mt.MaxThreads, mt.MaxThreads, $"MultiTasking<TSource, TResult>_{typeof(TSource).GetHashCode() + typeof(TResult).GetHashCode() + func.GetHashCode() + 3}"))
            {
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
                        var inputItem = (TSource) input;
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
                                if (callback != null && callbackItem != null)
                                    callback.Invoke(callbackItem);
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
}