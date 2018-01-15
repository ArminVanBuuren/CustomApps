using System.Collections;
using System.Collections.Generic;

namespace Protas.Components.Functions
{
    public class MyCollection<T> : IList<T>, ICollection<T>, IEnumerable
    {
        static int minArrayElemnets = 4;
        T[] contents;
        int count;

        public MyCollection()
        {
            Clear();
        }

        #region IList Members

        /// <summary>
        /// Добавляет элемент в список IList.
        /// </summary>
        /// <param name="value">Элемент который требуется поместить в коллекцию.</param>
        /// <returns>Индекс элемента который помещен в коллекцию.</returns>
        public void Add(T value)
        {
            if (count < contents.Length)
            {
                contents[count] = value;
                count++;
            }
            else
            {
                var newArray = new T[contents.Length + minArrayElemnets]; // Создание нового массива (на 1 больше старого).
                contents.CopyTo(newArray, 0);              // Копирование старого массива в новый.
                newArray[newArray.Length - 1] = value;      // Помещение нового значения в конец массива.
                contents = newArray;                       // Замена старого массива на новый.
            }

        }

        // Удаляет все элементы из коллекции IList.
        public void Clear()
        {
            count = 0;
            contents = new T[minArrayElemnets];
        }

        // Определяет, содержится ли указанное значение в списке IList.
        public bool Contains(T value)
        {
            for (int i = 0; i < Count; i++)
            {
                if (contents[i].Equals(value))
                    return true;
            }
            return false;
        }

        // Определяет индекс заданного элемента в списке IList.
        public int IndexOf(T value)
        {
            for (int i = 0; i < Count; i++)
                if (contents[i].Equals(value))
                    return i;
            return -1;
        }


        // Вставляет элемент в коллекцию IList с заданным индексом.
        public void Insert(int index, T value)
        {
            if ((count + 1 <= contents.Length) && (index < Count) && (index >= 0))
            {
                count++;

                for (int i = Count - 1; i > index; i--)
                {
                    contents[i] = contents[i - 1];
                }
                contents[index] = value;
            }
        }

        // Получает значение, показывающее, имеет ли список IList фиксированный размер.
        public bool IsFixedSize => true;

        // Получает значение, указывающее, доступна ли коллекция IList только для чтения.
        public bool IsReadOnly => true;

        // Удаляет первое вхождение указанного объекта из списка IList.
        public bool Remove(T value)
        {
            RemoveAt(IndexOf(value));
            return true;
        }


        // Удаляет элемент IList, расположенный по указанному индексу.
        public void RemoveAt(int index)
        {
            if ((index >= 0) && (index < Count))
            {
                for (int i = index; i < Count - 1; i++)
                    contents[i] = contents[i + 1];

                count--;
            }
        }
        public T this[int index]
        {
            get
            {
                return contents[index];
            }
            set
            {
                contents[index] = value;
            }
        }

        #endregion

        #region ICollection Members

        // Копирует элементы ICollection в Array, начиная с конкретного индекса Array.
        public void CopyTo(T[] array, int index)
        {
            int j = index;
            for (int i = 0; i < Count; i++)
            {
                array.SetValue(contents[i], j);
                j++;
            }
        }

        // Возвращает число элементов, содержащихся в коллекции ICollection.
        public int Count
        {
            get { return count; }
        }

        // Получает значение, позволяющее определить, является ли доступ к коллекции ICollection синхронизированным (потокобезопасным).
        public bool IsSynchronized
        {
            get { return false; }
        }

        // Получает объект, который можно использовать для синхронизации доступа к ICollection.
        public T SyncRoot
        {
            get { return default(T); }
        }

        #endregion

        #region IEnumerable Members


        /// <summary>
        /// Возвращает перечислитель, выполняющий перебор элементов в коллекции.  (Унаследовано от IEnumerable<T>.)
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)contents).GetEnumerator();
        }

        /// <summary>
        /// Возвращает перечислитель, который выполняет итерацию по элементам коллекции. (Унаследовано от IEnumerable)
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<T>).GetEnumerator();
        }


        #endregion


    }
}
