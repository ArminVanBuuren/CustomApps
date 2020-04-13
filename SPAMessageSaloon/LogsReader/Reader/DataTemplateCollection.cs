using LogsReader.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace LogsReader.Reader
{
    public class DataTemplateCollection : IEnumerable<DataTemplate>, IDisposable
    {
        private readonly Dictionary<int, DataTemplate> _values;
        private readonly LRSettingsScheme _settings;

        int _seqPrivateID = 0;
        int _seqID = 0;

        public DataTemplateCollection(LRSettingsScheme settings, IEnumerable<DataTemplate> list)
        {
            _settings = settings;
            _values = new Dictionary<int, DataTemplate>(list.Count());
            AddRange(DoOrdering(list));
        }

        public IEnumerable<DataTemplate> DoOrdering(IEnumerable<DataTemplate> input)
        {
            IQueryable<DataTemplate> result = input.AsQueryable();
            int i = 0;
            foreach (var orderItem in _settings.OrderByItems)
            {
                if (orderItem.Key.LikeAny(out var param, "FoundLineID", "ID", "Server", "TraceName", "Date", "File"))
                {
                    result = orderItem.Value
                        ? i == 0 ? result.OrderByDescending(param) : ((IOrderedQueryable<DataTemplate>)result).ThenByDescending(param)
                        : i == 0 ? result.OrderBy(param) : ((IOrderedQueryable<DataTemplate>)result).ThenBy(param);
                    i++;
                }
            }
            return result;
        }

        public void AddRange(IEnumerable<DataTemplate> list)
        {
            if (list.Any(x => x.ID != -1))
            {
                foreach (var template in list)
                {
                    template.PrivateID = ++_seqPrivateID;
                    _values.Add(template.PrivateID, template);
                }
            }
            else
            {
                foreach (var template in list)
                {
                    template.PrivateID = ++_seqPrivateID;
                    if (template.ID == -1)
                        template.ID = ++_seqID;

                    _values.Add(template.PrivateID, template);
                }
            }
        }

        public int Count => _values.Count;

        public DataTemplate this[int privateID] => _values.TryGetValue(privateID, out var result) ? result : null;

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
