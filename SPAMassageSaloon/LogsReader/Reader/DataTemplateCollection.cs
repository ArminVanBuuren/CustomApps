﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LogsReader.Config;
using Utils;

namespace LogsReader.Reader
{
    public class DataTemplateCollection : IEnumerable<DataTemplate>, IDisposable
    {
        private readonly Dictionary<int, DataTemplate> _values;
        private readonly LRSettingsScheme _settings;

        int _seqPrivateID;
        int _seqID;

        public DataTemplateCollection(LRSettingsScheme settings, IEnumerable<DataTemplate> list)
        {
            _settings = settings;
            _values = new Dictionary<int, DataTemplate>(list.Count());
            AddRange(DoOrdering(list, _settings.OrderByItems));
        }

        public static IEnumerable<DataTemplate> DoOrdering(IEnumerable<DataTemplate> input, Dictionary<string, bool> orderBy)
        {
	        var result = input.AsQueryable();
            var i = 0;
            foreach (var (columnName, byDescending) in orderBy)
            {
	            if (columnName.LikeAny(out var param, "FoundLineID", "ID", "Server", "TraceName", "Date", "File"))
	            {
		            result = byDescending
			            ? i == 0 ? result.OrderByDescending(param) : ((IOrderedQueryable<DataTemplate>) result).ThenByDescending(param)
			            : i == 0 ? result.OrderBy(param) : ((IOrderedQueryable<DataTemplate>) result).ThenBy(param);
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
