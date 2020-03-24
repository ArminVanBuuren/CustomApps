using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LogsReader.Data
{
    public class DataTemplateCollection : IEnumerable<DataTemplate>, IDisposable
    {
        int _seqPrivateID = 0;
        int _seqID = 0;
        private readonly Dictionary<int, DataTemplate> _values;

        public DataTemplateCollection(IEnumerable<DataTemplate> list)
        {
            _values = new Dictionary<int, DataTemplate>(list.Count());
            AddRange(list.OrderByDescending(p => p.Date).ThenByDescending(p => p.FileName).ThenBy(p => p.ID));
        }

        public void AddRange(IEnumerable<DataTemplate> list)
        {
            foreach (var template in list)
            {
                template.PrivateID = ++_seqPrivateID;
                if (template.ID == -1)
                    template.ID = ++_seqID;

                _values.Add(template.PrivateID, template);
            }
        }

        public int Count => _values.Count;

        public DataTemplate this[int privateID] => _values[privateID];

        public void Clear()
        {
            _values.Clear();
        }

        public IEnumerator<DataTemplate> GetEnumerator()
        {
            return _values.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.Values.GetEnumerator();
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
