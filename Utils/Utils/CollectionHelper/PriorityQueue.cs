using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils.CollectionHelper
{
    /// <inheritdoc />
    /// <summary>Очередь с приоритетом элементов</summary>
    public class PriorityQueue<T, TKey> : IEnumerable<T> where TKey : IComparable
    {
        private readonly Func<T, TKey> _keySelector;
        private readonly List<T> _heap;
        private readonly bool _ascending;

        /// <summary>Количество элементов в очереди</summary>
        public int Count
        {
            get
            {
                lock (_heap)
                {
                    return _heap.Count;
                }
            }
        }

        /// <summary>
        /// Очередь с приоритетом элементов
        /// </summary>
        /// <param name="keySelector">Ключ</param>
        /// <param name="ascending"></param>
        public PriorityQueue(Func<T, TKey> keySelector, bool ascending = true)
        {
            _heap = new List<T>();
            _keySelector = keySelector;
            _ascending = ascending;
        }

        /// <summary>Заталкивает элемент в очередь</summary>
        public void Enqueue(T item)
        {
            lock (_heap)
            {
                _heap.Add(item);
                PropagateUp(_heap.Count);
            }
        }

        /// <summary>Извлекает элемент из очереди (с наивысшим приоритетом)</summary>
        public T Dequeue()
        {
            lock (_heap)
            {
                if (IsEmpty())
                    return default(T);

                var max = AtIndex(1);
                Exchange(1, Count);
                _heap.RemoveAt(Count - 1);
                PropagateDown(1);
                return max;
            }
        }

        public T Top()
        {
            if (IsEmpty())
                throw new InvalidOperationException("queue is empty");

            lock (_heap)
                return AtIndex(1);
        }

        public bool IsEmpty()
        {
            return Count == 0;
        }



        private void PropagateUp(int k)
        {
            while (k > 1 && Less(AtIndex(k / 2), AtIndex(k)))
            {
                Exchange(k / 2, k);
                k = k / 2;
            }
        }

        private void PropagateDown(int k)
        {
            while (2 * k <= Count)
            {
                var j = 2 * k;

                if (j < Count && Less(AtIndex(j), AtIndex(j + 1)))
                    j++;

                if (!Less(AtIndex(k), AtIndex(j)))
                    break;

                Exchange(k, j);
                k = j;
            }
        }

        private T AtIndex(int index)
        {
            return _heap[index - 1];
        }

        private bool Less(T a, T b)
        {
            var aKey = _keySelector(a);
            var bKey = _keySelector(b);

            var order = aKey.CompareTo(bKey);

            return _ascending ? order > 0 : order < 0;
        }

        private void Exchange(int aIdx, int bIdx)
        {
            if (aIdx == bIdx)
                return;

            var temp = _heap[aIdx - 1];
            _heap[aIdx - 1] = _heap[bIdx - 1];
            _heap[bIdx - 1] = temp;
        }


        #region IEnumerable members

        public IEnumerator<T> GetEnumerator()
        {
            var result = Dump();
            return result.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            System.Collections.IEnumerable result = Dump();
            return result.GetEnumerator();
        }

        private IEnumerable<T> Dump()
        {
            lock (_heap)
            {
                return _ascending
                    ? _heap.OrderBy(_keySelector).ToList()
                    : _heap.OrderByDescending(_keySelector).ToList();
            }
        }

        #endregion
    }
}