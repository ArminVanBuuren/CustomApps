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
        private readonly Semaphore _pool;

        public SemaphoreHelper(Action<T> action, int maxThreads = 2)
        {
            _action = action;
            _pool = new Semaphore(maxThreads, maxThreads, "SemaphoreHelper");
        }

        public void Performer(IEnumerable<T> data)
        {
            var listOfTasks = new List<Task>();
            foreach (var item in data)
            {
                _pool.WaitOne();

                listOfTasks.Add(Task.Factory.StartNew((token) =>
                {
                    _action.Invoke((T)token);
                    _pool.Release();
                }, item));
            }

            Task.WaitAll(listOfTasks.ToArray());
        }
    }
}
