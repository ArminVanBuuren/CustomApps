using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utils.Handles
{
    public class SemaphoreHelper<T> : IDisposable
    {
        private bool _isCancelledOperation;
        private Action<T> _action;
        private Semaphore _pool;

        public SemaphoreHelper(Action<T> action, int maxThreads = 2)
        {
            _action = action;
            _pool = new Semaphore(maxThreads, maxThreads, "SemaphoreHelper");
            _isCancelledOperation = false;
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
                if (_isCancelledOperation)
                    break;

                _pool?.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((token) =>
                {
                    if (_isCancelledOperation)
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
            _isCancelledOperation = true;
        }

        public void Reload()
        {
            _isCancelledOperation = false;
        }

        public void Dispose()
        {
            _action = null;
            _pool = null;
            _isCancelledOperation = true;
        }
    }
}
