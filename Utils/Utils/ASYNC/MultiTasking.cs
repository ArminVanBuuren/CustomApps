using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Utils
{
	/// <summary>
    /// Многопоточная обработка данных
    /// </summary>
	public class MultiTasking
    {
        /// <summary>
        /// Многопоточная обработка коллекции элементов.
        /// Ограничения по умолчания - 2 потока.
        /// </summary>
        /// <param name="actions">Коллекция методов по асинхронной обработке</param>
        /// <param name="mtTemplate">Настройки многопоточного выполнения. Ограничения по количеству потоков, отмена выполнения, приоретизация потоков.</param>
        /// <returns>Коллекция завершенных элементов</returns>
        public static async Task<MTCallBackList<Action, bool>> RunAsync(IEnumerable<Action> actions, MultiTaskingTemplate mtTemplate = null)
        {
            return await Task<MTCallBackList<Action, bool>>.Factory.StartNew(() => Run(actions, mtTemplate));
        }

        /// <summary>
        /// Многопоточная обработка коллекции элементов.
        /// Настройка ограничения по количеству потоков, работают только в контексте вызывающего потока. Ограничения по умолчания - 2 потока.
        /// </summary>
        /// <param name="actions">Коллекция методов по асинхронной обработке</param>
        /// <param name="mtTemplate">Настройки многопоточного выполнения. Ограничения по количеству потоков, отмена выполнения, приоретизация потоков.</param>
        /// <returns>Коллекция завершенных элементов</returns>
        public static MTCallBackList<Action, bool> Run(IEnumerable<Action> actions, MultiTaskingTemplate mtTemplate = null)
        {
            var mt = mtTemplate ?? new MultiTaskingTemplate();
            using (var pool = new Semaphore(mt.MaxThreads, mt.MaxThreads, $"MultiTasking<Action, bool>_{Thread.CurrentThread.ManagedThreadId}"))
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

        /// <summary>
        /// Многопоточная обработка коллекции элементов.
        /// Ограничения по умолчания - 2 потока.
        /// </summary>
        /// <param name="actions">Коллекция методов по асинхронной обработке</param>
        /// <param name="callback">Отправляет коллекцию завершенных элементов</param>
        /// <param name="mtTemplate">Настройки многопоточного выполнения. Ограничения по количеству потоков, отмена выполнения, приоретизация потоков.</param>
        public static async Task RunAsync(IEnumerable<Action> actions, Action<MTCallBackList<Action, bool>> callback, MultiTaskingTemplate mtTemplate)
        {
            await Task.Factory.StartNew(() => Run(actions, callback, mtTemplate));
        }

        /// <summary>
        /// Многопоточная обработка коллекции элементов.
        /// Настройка ограничения по количеству потоков, работают только в контексте вызывающего потока. Ограничения по умолчания - 2 потока.
        /// </summary>
        /// <param name="actions">Коллекция методов по асинхронной обработке</param>
        /// <param name="callback">Отправляет коллекцию завершенных элементов</param>
        /// <param name="mtTemplate">Настройки многопоточного выполнения. Ограничения по количеству потоков, отмена выполнения, приоретизация потоков.</param>
        public static void Run(IEnumerable<Action> actions, Action<MTCallBackList<Action, bool>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            var result = Run(actions, mtTemplate);

            if (callback != null)
                Task.Factory.StartNew((callbackItem) => callback.Invoke((MTCallBackList<Action, bool>)callbackItem), result);
        }

        /// <summary>
        /// Многопоточная обработка коллекции элементов.
        /// Ограничения по умолчания - 2 потока.
        /// </summary>
        /// <param name="actions">Коллекция методов по асинхронной обработке</param>
        /// <param name="callback">Метод CallBack при завершении выполнения одного из элемента коллекции</param>
        /// <param name="mtTemplate">Настройки многопоточного выполнения. Ограничения по количеству потоков, отмена выполнения, приоретизация потоков.</param>
        public static async Task RunAsync(IEnumerable<Action> actions, Action<MTCallBack<Action, bool>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            await Task.Factory.StartNew(() => Run(actions, callback, mtTemplate));
        }

        /// <summary>
        /// Многопоточная обработка коллекции элементов.
        /// Настройка ограничения по количеству потоков, работают только в контексте вызывающего потока. Ограничения по умолчания - 2 потока.
        /// </summary>
        /// <param name="actions">Коллекция методов по асинхронной обработке</param>
        /// <param name="callback">Метод CallBack при завершении выполнения одного из элемента коллекции</param>
        /// <param name="mtTemplate">Настройки многопоточного выполнения. Ограничения по количеству потоков, отмена выполнения, приоретизация потоков.</param>
        public static void Run(IEnumerable<Action> actions, Action<MTCallBack<Action, bool>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            var mt = mtTemplate ?? new MultiTaskingTemplate();
            using (var pool = new Semaphore(mt.MaxThreads, mt.MaxThreads, $"MultiTasking_{Thread.CurrentThread.ManagedThreadId}"))
            {
                var listOfTasks = new List<Task>();
                foreach (var action in actions)
                {
                    if (mt.CancelToken.IsCancellationRequested)
                        break;

                    pool.WaitOne();

                    listOfTasks.Add(Task.Factory.StartNew((input) =>
                    {
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
                            }
                        }
                        catch (Exception ex)
                        {
                            if (callback != null)
                            {
                                callbackItem = new MTCallBack<Action, bool>(inputAction, ex);
                            }
                        }
                        finally
                        {
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
            }
        }

        /// <summary>
        /// Многопоточная обработка коллекции элементов.
        /// Ограничения по умолчания - 2 потока.
        /// </summary>
        /// <typeparam name="TResult">Результат обработки элемента функции</typeparam>
        /// <param name="funcs">Коллекция функций по асинхронной обработке</param>
        /// <param name="mtTemplate">Настройки многопоточного выполнения. Ограничения по количеству потоков, отмена выполнения, приоретизация потоков.</param>
        /// <returns>Коллекция завершенных элементов</returns>
        public static async Task<MTCallBackList<Func<TResult>, TResult>> RunAsync<TResult>(IEnumerable<Func<TResult>> funcs, MultiTaskingTemplate mtTemplate = null)
        {
            return await Task<MTCallBackList<Func<TResult>, TResult>>.Factory.StartNew(() => Run(funcs, mtTemplate));
        }

        /// <summary>
        /// Многопоточная обработка коллекции элементов.
        /// Настройка ограничения по количеству потоков, работают только в контексте вызывающего потока. Ограничения по умолчания - 2 потока.
        /// </summary>
        /// <typeparam name="TResult">Результат обработки элемента функции</typeparam>
        /// <param name="funcs">Коллекция функций по асинхронной обработке</param>
        /// <param name="mtTemplate">Настройки многопоточного выполнения. Ограничения по количеству потоков, отмена выполнения, приоретизация потоков.</param>
        /// <returns>Коллекция завершенных элементов</returns>
        public static MTCallBackList<Func<TResult>, TResult> Run<TResult>(IEnumerable<Func<TResult>> funcs, MultiTaskingTemplate mtTemplate = null)
        {
            var mt = mtTemplate ?? new MultiTaskingTemplate();
            using (var pool = new Semaphore(mt.MaxThreads, mt.MaxThreads, $"MultiTasking<Func<TResult>>_{Thread.CurrentThread.ManagedThreadId}"))
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

        /// <summary>
        /// Многопоточная обработка коллекции элементов.
        /// Ограничения по умолчания - 2 потока.
        /// </summary>
        /// <typeparam name="TResult">Результат обработки элемента функции</typeparam>
        /// <param name="funcs">Коллекция функций по асинхронной обработке</param>
        /// <param name="callback">Отправляет коллекцию завершенных элементов</param>
        /// <param name="mtTemplate">Настройки многопоточного выполнения. Ограничения по количеству потоков, отмена выполнения, приоретизация потоков.</param>
        public static async Task RunAsync<TResult>(IEnumerable<Func<TResult>> funcs, Action<MTCallBackList<Func<TResult>, TResult>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            await Task.Factory.StartNew(() => Run(funcs, callback, mtTemplate));
        }

        /// <summary>
        /// Многопоточная обработка коллекции элементов.
        /// Настройка ограничения по количеству потоков, работают только в контексте вызывающего потока. Ограничения по умолчания - 2 потока.
        /// </summary>
        /// <typeparam name="TResult">Результат обработки элемента функции</typeparam>
        /// <param name="funcs">Коллекция функций по асинхронной обработке</param>
        /// <param name="callback">Отправляет коллекцию завершенных элементов</param>
        /// <param name="mtTemplate">Настройки многопоточного выполнения. Ограничения по количеству потоков, отмена выполнения, приоретизация потоков.</param>
        public static void Run<TResult>(IEnumerable<Func<TResult>> funcs, Action<MTCallBackList<Func<TResult>, TResult>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            var result = Run(funcs, mtTemplate);

            if (callback != null)
                Task.Factory.StartNew((callbackItem) => callback.Invoke((MTCallBackList<Func<TResult>, TResult>)callbackItem), result);
        }

        /// <summary>
        /// Многопоточная обработка коллекции элементов.
        /// Ограничения по умолчания - 2 потока.
        /// </summary>
        /// <typeparam name="TResult">Результат обработки элемента функции</typeparam>
        /// <param name="funcs">Коллекция функций по асинхронной обработке</param>
        /// <param name="callback">Метод CallBack при завершении выполнения одного из элемента коллекции</param>
        /// <param name="mtTemplate">Настройки многопоточного выполнения. Ограничения по количеству потоков, отмена выполнения, приоретизация потоков.</param>
        public static async Task RunAsync<TResult>(IEnumerable<Func<TResult>> funcs, Action<MTCallBack<Func<TResult>, TResult>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            await Task.Factory.StartNew(() => Run(funcs, callback, mtTemplate));
        }

        /// <summary>
        /// Многопоточная обработка коллекции элементов.
        /// Настройка ограничения по количеству потоков, работают только в контексте вызывающего потока. Ограничения по умолчания - 2 потока.
        /// </summary>
        /// <typeparam name="TResult">Результат обработки элемента функции</typeparam>
        /// <param name="funcs">Коллекция функций по асинхронной обработке</param>
        /// <param name="callback">Метод CallBack при завершении выполнения одного из элемента коллекции</param>
        /// <param name="mtTemplate">Настройки многопоточного выполнения. Ограничения по количеству потоков, отмена выполнения, приоретизация потоков.</param>
        public static void Run<TResult>(IEnumerable<Func<TResult>> funcs, Action<MTCallBack<Func<TResult>, TResult>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            var mt = mtTemplate ?? new MultiTaskingTemplate();
            using (var pool = new Semaphore(mt.MaxThreads, mt.MaxThreads, $"MultiTasking_{Thread.CurrentThread.ManagedThreadId}"))
            {
                var listOfTasks = new List<Task>();
                foreach (var func in funcs)
                {
                    if (mt.CancelToken.IsCancellationRequested)
                        break;

                    pool.WaitOne();

                    listOfTasks.Add(Task.Factory.StartNew((input) =>
                    {
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
                            }
                        }
                        catch (Exception ex)
                        {
                            if (callback != null)
                            {
                                callbackItem = new MTCallBack<Func<TResult>, TResult>(inputFunc, ex);
                            }
                        }
                        finally
                        {
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
            }
        }

        /// <summary>
        /// Многопоточная обработка коллекции элементов.
        /// Ограничения по умолчания - 2 потока.
        /// </summary>
        /// <typeparam name="TSource">Тип элементов коллекции</typeparam>
        /// <param name="action">Метод по асинхронной обработке коллекции</param>
        /// <param name="data">Коллекция которую нужно обработать асинхронно</param>
        /// <param name="mtTemplate">Настройки многопоточного выполнения. Ограничения по количеству потоков, отмена выполнения, приоретизация потоков.</param>
        /// <returns>Коллекция завершенных элементов</returns>
        public static async Task<MTCallBackList<TSource, bool>> RunAsync<TSource>(Action<TSource> action, IEnumerable<TSource> data, MultiTaskingTemplate mtTemplate = null)
        {
            return await Task<MTCallBackList<TSource, bool>>.Factory.StartNew(() => Run(action, data, mtTemplate));
        }

        /// <summary>
        /// Многопоточная обработка коллекции элементов.
        /// Настройка ограничения по количеству потоков, работают только в контексте вызывающего потока. Ограничения по умолчания - 2 потока.
        /// </summary>
        /// <typeparam name="TSource">Тип элементов коллекции</typeparam>
        /// <param name="action">Метод по асинхронной обработке коллекции</param>
        /// <param name="data">Коллекция которую нужно обработать асинхронно</param>
        /// <param name="mtTemplate">Настройки многопоточного выполнения. Ограничения по количеству потоков, отмена выполнения, приоретизация потоков.</param>
        /// <returns>Коллекция завершенных элементов</returns>
        public static MTCallBackList<TSource, bool> Run<TSource>(Action<TSource> action, IEnumerable<TSource> data, MultiTaskingTemplate mtTemplate = null)
        {
            var mt = mtTemplate ?? new MultiTaskingTemplate();
            using (var pool = new Semaphore(mt.MaxThreads, mt.MaxThreads, $"MultiTasking<TSource, bool>_{Thread.CurrentThread.ManagedThreadId}"))
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

        /// <summary>
        /// Многопоточная обработка коллекции элементов.
        /// Ограничения по умолчания - 2 потока.
        /// </summary>
        /// <typeparam name="TSource">Тип элементов коллекции</typeparam>
        /// <param name="action">Метод по асинхронной обработке коллекции</param>
        /// <param name="data">Коллекция которую нужно обработать асинхронно</param>
        /// <param name="callback">Отправляет коллекцию завершенных элементов</param>
        /// <param name="mtTemplate">Настройки многопоточного выполнения. Ограничения по количеству потоков, отмена выполнения, приоретизация потоков.</param>
        public static async Task RunAsync<TSource>(Action<TSource> action, IEnumerable<TSource> data, Action<MTCallBackList<TSource, bool>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            await Task.Factory.StartNew(() => Run(action, data, callback, mtTemplate));
        }

        /// <summary>
        /// Многопоточная обработка коллекции элементов.
        /// Настройка ограничения по количеству потоков, работают только в контексте вызывающего потока. Ограничения по умолчания - 2 потока.
        /// </summary>
        /// <typeparam name="TSource">Тип элементов коллекции</typeparam>
        /// <param name="action">Метод по асинхронной обработке коллекции</param>
        /// <param name="data">Коллекция которую нужно обработать асинхронно</param>
        /// <param name="callback">Отправляет коллекцию завершенных элементов</param>
        /// <param name="mtTemplate">Настройки многопоточного выполнения. Ограничения по количеству потоков, отмена выполнения, приоретизация потоков.</param>
        public static void Run<TSource>(Action<TSource> action, IEnumerable<TSource> data, Action<MTCallBackList<TSource, bool>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            var result = Run(action, data, mtTemplate);
            
            if (callback != null)
                Task.Factory.StartNew((callbackItem) => callback.Invoke((MTCallBackList<TSource, bool>)callbackItem), result);
        }

        /// <summary>
        /// Многопоточная обработка коллекции элементов.
        /// Ограничения по умолчания - 2 потока.
        /// </summary>
        /// <typeparam name="TSource">Тип элементов коллекции</typeparam>
        /// <param name="action">Метод по асинхронной обработке коллекции</param>
        /// <param name="data">Коллекция которую нужно обработать асинхронно</param>
        /// <param name="callback">Метод CallBack при завершении выполнения одного из элемента коллекции</param>
        /// <param name="mtTemplate">Настройки многопоточного выполнения. Ограничения по количеству потоков, отмена выполнения, приоретизация потоков.</param>
        public static async Task RunAsync<TSource>(Action<TSource> action, IEnumerable<TSource> data, Action<MTCallBack<TSource, bool>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            await Task.Factory.StartNew(() => Run(action, data, callback, mtTemplate));
        }

        /// <summary>
        /// Многопоточная обработка коллекции элементов.
        /// Настройка ограничения по количеству потоков, работают только в контексте вызывающего потока. Ограничения по умолчания - 2 потока.
        /// </summary>
        /// <typeparam name="TSource">Тип элементов коллекции</typeparam>
        /// <param name="action">Метод по асинхронной обработке коллекции</param>
        /// <param name="data">Коллекция которую нужно обработать асинхронно</param>
        /// <param name="callback">Метод CallBack при завершении выполнения одного из элемента коллекции</param>
        /// <param name="mtTemplate">Настройки многопоточного выполнения. Ограничения по количеству потоков, отмена выполнения, приоретизация потоков.</param>
        public static void Run<TSource>(Action<TSource> action, IEnumerable<TSource> data, Action<MTCallBack<TSource, bool>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            var mt = mtTemplate ?? new MultiTaskingTemplate();
            using (var pool = new Semaphore(mt.MaxThreads, mt.MaxThreads, $"MultiTasking_{Thread.CurrentThread.ManagedThreadId}"))
            {
                var listOfTasks = new List<Task>();
                foreach (var item in data)
                {
                    if (mt.CancelToken.IsCancellationRequested)
                        break;

                    pool.WaitOne();

                    listOfTasks.Add(Task.Factory.StartNew((input) =>
                    {
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
                            }
                        }
                        catch (Exception ex)
                        {
                            if (callback != null)
                            {
                                callbackItem = new MTCallBack<TSource, bool>(inputItem, ex);
                            }
                        }
                        finally
                        {
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
            }
        }

        /// <summary>
        /// Многопоточная обработка коллекции элементов.
        /// Ограничения по умолчания - 2 потока.
        /// </summary>
        /// <typeparam name="TSource">Тип элементов коллекции</typeparam>
        /// <typeparam name="TResult">Результат обработки элемента функции</typeparam>
        /// <param name="func">Функция по асинхронной обработке коллекции</param>
        /// <param name="data">Коллекция которую нужно обработать асинхронно</param>
        /// <param name="mtTemplate">Настройки многопоточного выполнения. Ограничения по количеству потоков, отмена выполнения, приоретизация потоков.</param>
        /// <returns>Коллекция завершенных элементов</returns>
        public static async Task<MTCallBackList<TSource, TResult>> RunAsync<TSource, TResult>(Func<TSource, TResult> func, IEnumerable<TSource> data, MultiTaskingTemplate mtTemplate = null)
        {
            return await Task<MTCallBackList<TSource, TResult>>.Factory.StartNew(() => Run(func, data, mtTemplate));
        }

        /// <summary>
        /// Многопоточная обработка коллекции элементов.
        /// Настройка ограничения по количеству потоков, работают только в контексте вызывающего потока. Ограничения по умолчания - 2 потока.
        /// </summary>
        /// <typeparam name="TSource">Тип элементов коллекции</typeparam>
        /// <typeparam name="TResult">Результат обработки элемента функции</typeparam>
        /// <param name="func">Функция по асинхронной обработке коллекции</param>
        /// <param name="data">Коллекция которую нужно обработать асинхронно</param>
        /// <param name="mtTemplate">Настройки многопоточного выполнения. Ограничения по количеству потоков, отмена выполнения, приоретизация потоков.</param>
        /// <returns>Коллекция завершенных элементов</returns>
        public static MTCallBackList<TSource, TResult> Run<TSource, TResult>(Func<TSource, TResult> func, IEnumerable<TSource> data, MultiTaskingTemplate mtTemplate = null)
        {
            var mt = mtTemplate ?? new MultiTaskingTemplate();
            using (var pool = new Semaphore(mt.MaxThreads, mt.MaxThreads, $"MultiTasking<TSource, TResult>_{Thread.CurrentThread.ManagedThreadId}"))
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

        /// <summary>
        /// Многопоточная обработка коллекции элементов.
        /// Ограничения по умолчания - 2 потока.
        /// </summary>
        /// <typeparam name="TSource">Тип элементов коллекции</typeparam>
        /// <typeparam name="TResult">Результат обработки элемента функции</typeparam>
        /// <param name="func">Функция по асинхронной обработке коллекции</param>
        /// <param name="data">Коллекция которую нужно обработать асинхронно</param>
        /// <param name="callback">Коллекция завершенных элементов</param>
        /// <param name="mtTemplate">Настройки многопоточного выполнения. Ограничения по количеству потоков, отмена выполнения, приоретизация потоков.</param>
        public static async Task RunAsync<TSource, TResult>(Func<TSource, TResult> func, IEnumerable<TSource> data, Action<MTCallBackList<TSource, TResult>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            await Task.Factory.StartNew(() => Run(func, data, callback, mtTemplate));
        }

        /// <summary>
        /// Многопоточная обработка коллекции элементов.
        /// Настройка ограничения по количеству потоков, работают только в контексте вызывающего потока. Ограничения по умолчания - 2 потока.
        /// </summary>
        /// <typeparam name="TSource">Тип элементов коллекции</typeparam>
        /// <typeparam name="TResult">Результат обработки элемента функции</typeparam>
        /// <param name="func">Функция по асинхронной обработке коллекции</param>
        /// <param name="data">Коллекция которую нужно обработать асинхронно</param>
        /// <param name="callback">Отправляет коллекцию завершенных элементов</param>
        /// <param name="mtTemplate">Настройки многопоточного выполнения. Ограничения по количеству потоков, отмена выполнения, приоретизация потоков.</param>
        public static void Run<TSource, TResult>(Func<TSource, TResult> func, IEnumerable<TSource> data, Action<MTCallBackList<TSource, TResult>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            var result = Run(func, data, mtTemplate);
            
            if (callback != null)
                Task.Factory.StartNew((callbackItem) => callback.Invoke((MTCallBackList<TSource, TResult>) callbackItem), result);
        }

        /// <summary>
        /// Многопоточная обработка коллекции элементов.
        /// Ограничения по умолчания - 2 потока.
        /// </summary>
        /// <typeparam name="TSource">Тип элементов коллекции</typeparam>
        /// <typeparam name="TResult">Результат обработки элемента функции</typeparam>
        /// <param name="func">Функция по асинхронной обработке коллекции</param>
        /// <param name="data">Коллекция которую нужно обработать асинхронно</param>
        /// <param name="callback">Метод CallBack при завершении выполнения одного из элемента коллекции</param>
        /// <param name="mtTemplate">Настройки многопоточного выполнения. Ограничения по количеству потоков, отмена выполнения, приоретизация потоков.</param>
        public static async Task RunAsync<TSource, TResult>(Func<TSource, TResult> func, IEnumerable<TSource> data, Action<MTCallBack<TSource, TResult>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            await Task.Factory.StartNew(() => Run(func, data, callback, mtTemplate));
        }

        /// <summary>
        /// Многопоточная обработка коллекции элементов.
        /// Настройка ограничения по количеству потоков, работают только в контексте вызывающего потока. Ограничения по умолчания - 2 потока.
        /// </summary>
        /// <typeparam name="TSource">Тип элементов коллекции</typeparam>
        /// <typeparam name="TResult">Результат обработки элемента функции</typeparam>
        /// <param name="func">Функция по асинхронной обработке коллекции</param>
        /// <param name="data">Коллекция которую нужно обработать асинхронно</param>
        /// <param name="callback">Метод CallBack при завершении выполнения одного из элемента коллекции</param>
        /// <param name="mtTemplate">Настройки многопоточного выполнения. Ограничения по количеству потоков, отмена выполнения, приоретизация потоков.</param>
        public static void Run<TSource, TResult>(Func<TSource, TResult> func, IEnumerable<TSource> data, Action<MTCallBack<TSource, TResult>> callback, MultiTaskingTemplate mtTemplate = null)
        {
            var mt = mtTemplate ?? new MultiTaskingTemplate();
            using (var pool = new Semaphore(mt.MaxThreads, mt.MaxThreads, $"MultiTasking<TSource, TResult>_{Thread.CurrentThread.ManagedThreadId}"))
            {
                var listOfTasks = new List<Task>();
                foreach (var item in data)
                {
                    if (mt.CancelToken.IsCancellationRequested)
                        break;

                    pool.WaitOne();

                    listOfTasks.Add(Task.Factory.StartNew((input) =>
                    {
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
                            }
                        }
                        catch (Exception ex)
                        {
                            if (callback != null)
                            {
                                callbackItem = new MTCallBack<TSource, TResult>(inputItem, ex);
                            }
                        }
                        finally
                        {
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
            }
        }
    }
}