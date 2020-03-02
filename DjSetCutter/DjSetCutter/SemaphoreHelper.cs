using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DjSetCutter
{
    public class SemaphoreHelper<T> : IDisposable
    {
        public bool IsCancellationRequested { get; private set; } = false;
        private Action<T> _action;
        private Semaphore _pool;

        public SemaphoreHelper(Action<T> action, int maxThreads = 2)
        {
            _action = action;
            _pool = new Semaphore(maxThreads, maxThreads, "SemaphoreHelper");
            IsCancellationRequested = false;
        }

        public async Task ExecuteAsync(IEnumerable<T> data)
        {
            await Task.Factory.StartNew((token) => { Execute((IEnumerable<T>)token); }, data);
        }

        public void Execute(IEnumerable<T> data)
        {
            var listOfTasks = new List<Task>();
            foreach (var item in data)
            {
                if (IsCancellationRequested)
                    break;

                _pool?.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((token) =>
                {
                    if (IsCancellationRequested)
                    {
                        _pool?.Release();
                        return;
                    }

                    _action?.Invoke((T)token);
                    _pool?.Release();
                }, item));
            }

            Task.WaitAll(listOfTasks.ToArray());
        }

        public void Abort()
        {
            IsCancellationRequested = true;
        }

        public void Reload()
        {
            IsCancellationRequested = false;
        }

        public void Dispose()
        {
            _action = null;
            _pool = null;
            IsCancellationRequested = true;
        }
    }
}
