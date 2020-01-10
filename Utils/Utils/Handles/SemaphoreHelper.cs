using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utils.Handles
{
    public class SemaphoreHelper<T>
    {
        private readonly Action<T> _action;
        private readonly Semaphore pool;

        public SemaphoreHelper(Action<T> action, int maxThreads = 2)
        {
            _action = action;
            pool = new Semaphore(maxThreads, maxThreads, "SemaphoreHelper");
        }

        public void Performer(IEnumerable<T> data)
        {
            foreach (var item in data)
            {
                pool.WaitOne();
                var task = Task.Factory.StartNew(() =>
                {
                    _action.Invoke(item);
                    pool.Release();
                });
            }
        }
    }
}
