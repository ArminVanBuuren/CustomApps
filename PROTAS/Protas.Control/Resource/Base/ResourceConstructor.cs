using System;
using System.Collections;
using System.Collections.Generic;
using Protas.Components.PerformanceLog;

namespace Protas.Control.Resource.Base
{
    /// <summary>
    /// Содержит в себе необходимые элементы
    /// 1) Определенгие лога
    /// 2) Конcтрукторы ресурса 
    /// например => {sys cpu('1111','2222')} в данном случае '1111','2222' это конструктор ресурса, который должен быть проинициализирован в самом контексте
    /// </summary>
    public class ResourceConstructor : ShellLog3Net, ICollection, IEnumerable //: IList<T>, ICollection<T>
    {
        private const int MinArrayElemnets = 4;
        string[] _contents;
        int _count;
        internal ResourceConstructor(ILog3NetMain log) : base(log)
        {
            Clear();
        }

        #region IList Members

        /// <summary>
        /// Добавляет элемент в список IList.
        /// </summary>
        /// <param name="value">Элемент который требуется поместить в коллекцию.</param>
        /// <returns>Индекс элемента который помещен в коллекцию.</returns>
        internal void Add(string value)
        {
            if (_count < _contents.Length)
            {
                _contents[_count] = value;
                _count++;
            }
            else
            {
                string[] newArray = new string[_contents.Length + MinArrayElemnets]; // Создание нового массива (на 1 больше старого).
                _contents.CopyTo(newArray, 0);              // Копирование старого массива в новый.
                newArray[newArray.Length - 1] = value;      // Помещение нового значения в конец массива.
                _contents = newArray;                       // Замена старого массива на новый.
            }

        }

        // Удаляет все элементы из коллекции IList.
        internal void Clear()
        {
            _count = 0;
            _contents = new string[MinArrayElemnets];
        }

        // Определяет, содержится ли указанное значение в списке IList.
        public bool Contains(string value)
        {
            for (int i = 0; i < Count; i++)
            {
                if (_contents[i].Equals(value))
                    return true;
            }
            return false;
        }

        // Определяет индекс заданного элемента в списке IList.
        public int IndexOf(string value)
        {
            for (int i = 0; i < Count; i++)
                if (_contents[i].Equals(value))
                    return i;
            return -1;
        }


        // Вставляет элемент в коллекцию IList с заданным индексом.
        internal void Insert(int index, string value)
        {
            if ((_count + 1 <= _contents.Length) && (index < Count) && (index >= 0))
            {
                _count++;

                for (int i = Count - 1; i > index; i--)
                {
                    _contents[i] = _contents[i - 1];
                }
                _contents[index] = value;
            }
        }

        // Получает значение, показывающее, имеет ли список IList фиксированный размер.
        //public bool IsFixedSize => true;

        // Получает значение, указывающее, доступна ли коллекция IList только для чтения.
        //public bool IsReadOnly => true;

        // Удаляет первое вхождение указанного объекта из списка IList.
        internal bool Remove(string value)
        {
            RemoveAt(IndexOf(value));
            return true;
        }


        // Удаляет элемент IList, расположенный по указанному индексу.
        internal void RemoveAt(int index)
        {
            if ((index >= 0) && (index < Count))
            {
                for (int i = index; i < Count - 1; i++)
                    _contents[i] = _contents[i + 1];

                _count--;
            }
        }
        public string this[int index]
        {
            get
            {
                return _contents[index];
            }
            internal set
            {
                _contents[index] = value;
            }
        }

        #endregion

        #region ICollection Members

        // Копирует элементы ICollection в Array, начиная с конкретного индекса Array.
        public void CopyTo(Array array, int index)
        {
            int j = index;
            for (int i = 0; i < Count; i++)
            {
                array.SetValue(_contents[i], j);
                j++;
            }
        }

        // Возвращает число элементов, содержащихся в коллекции ICollection.
        public int Count => _count;

        // Получает значение, позволяющее определить, является ли доступ к коллекции ICollection синхронизированным (потокобезопасным).
        public bool IsSynchronized => false;

        // Получает объект, который можно использовать для синхронизации доступа к ICollection.
        public object SyncRoot => default(object);

        #endregion

        #region IEnumerable Members


        /// <summary>
        /// Возвращает перечислитель, выполняющий перебор элементов в коллекции.  (Унаследовано от IEnumerable<T>.)
        /// </summary>
        /// <returns></returns>
        public IEnumerator<string> GetEnumerator()
        {
            return ((IEnumerable<string>)_contents).GetEnumerator();
        }

        /// <summary>
        /// Возвращает перечислитель, который выполняет итерацию по элементам коллекции. (Унаследовано от IEnumerable)
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<string>).GetEnumerator();
        }


        #endregion

        /// <summary>
        /// конструктор непосредственного ресурса в виде стринговой строки
        /// </summary>
        public string AsLine => string.Join(";", _contents);


        public override bool Equals(object obj)
        {
            ResourceConstructor input = (ResourceConstructor) obj;
            if (input != null)
                return (AsLine.Equals(input.AsLine));
            return false;
        }

        public override int GetHashCode()
        {
            return base.MainLog3Net.GetHashCode();
        }

        public override string ToString()
        {
            return AsLine;
        }

    }
}
