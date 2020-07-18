using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LogsReader.Config;
using Utils;

namespace LogsReader.Reader
{
    public class DataTemplateCollection : IEnumerable<DataTemplate>, IDisposable
    {
	    private static string[] orderByFields;

	    internal static string[] OrderByFields
	    {
		    get
		    {
			    return orderByFields ?? (orderByFields = new[]
			    {
				    nameof(DataTemplate.Tmp.FoundLineID),
				    nameof(DataTemplate.Tmp.ID),
				    nameof(DataTemplate.Tmp.Server),
				    nameof(DataTemplate.Tmp.TraceName),
				    nameof(DataTemplate.Tmp.Date),
				    nameof(DataTemplate.Tmp.ElapsedSec),
				    nameof(DataTemplate.Tmp.File),
				    DataTemplate.ReaderPriority
				});
		    }
	    }

	    private readonly Dictionary<int, DataTemplate> _values;

        private int _seqPrivateID;
        private int _seqID;

        public DataTemplateCollection(LRSettingsScheme settings, IEnumerable<DataTemplate> list)
        {
	        _values = new Dictionary<int, DataTemplate>(list.Count());
            AddRange(DoOrdering(list, settings.OrderByItems));

	        foreach (var trnTemplates in _values.Values
	            .Where(template => template.Date != null && template.TransactionValue.Any(x => !x.Trn.IsNullOrEmptyTrim()))
	            .SelectMany(x => x.TransactionValue.Select(x2 => x2.Trn), (parent, trnID) => new { parent, trnID })
	            .OrderBy(x => x.parent.Date)
	            .ThenBy(x => x.parent.FoundLineID)
				.GroupBy(x => x.trnID))
            {
	            if (trnTemplates.Count() <= 1)
					continue;

	            var i = 0;
				DataTemplate firstTemplate = null;
				foreach (var template in trnTemplates.Select(x => x.parent).OrderBy(x => x.Date).ThenBy(x => x.FoundLineID))
				{
					if (i == 0)
					{
						firstTemplate = template;
						foreach (var trn in firstTemplate.TransactionValue)
						{
							if (trn.Trn == trnTemplates.Key)
							{
								trn.IsFirst = true;
								break;
							}
						}
					}

					if (template.ElapsedSec < 0)
						template.ElapsedSec = template.Date.Value.Subtract(firstTemplate.Date.Value).TotalSeconds;
					i++;
				}
            }
        }

        public static IEnumerable<DataTemplate> DoOrdering(IEnumerable<DataTemplate> input, Dictionary<string, bool> orderBy)
        {
	        var result = input.AsQueryable();
	        var i = 0;
            foreach (var (columnName, byDescending) in orderBy)
            {
	            if (columnName.LikeAny(out var param, OrderByFields))
	            {
		            if (columnName.Equals(DataTemplate.ReaderPriority, StringComparison.InvariantCultureIgnoreCase))
		            {
			            // учитываем приоритет файла, если приоритет выше то и запись должна быть выше
			            result = byDescending
				            ? i == 0 ? result.OrderByDescending(x => x.ParentReader.Priority) : ((IOrderedQueryable<DataTemplate>)result).ThenByDescending(x => x.ParentReader.Priority)
				            : i == 0 ? result.OrderBy(x => x.ParentReader.Priority) : ((IOrderedQueryable<DataTemplate>)result).ThenBy(x => x.ParentReader.Priority);
					}
					else
					{
						result = byDescending
							? i == 0 ? result.OrderByDescending(param) : ((IOrderedQueryable<DataTemplate>) result).ThenByDescending(param)
							: i == 0 ? result.OrderBy(param) : ((IOrderedQueryable<DataTemplate>) result).ThenBy(param);
					}
					i++;
	            }
            }
            return result;
        }

        public void AddRange(IEnumerable<DataTemplate> list)
        {
			if(!list.Any())
				return;

	        var maxId = list.Max(x => x.ID);
			if (maxId > -1)
	        {
		        _seqID = maxId > _seqID ? maxId : _seqID;

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
