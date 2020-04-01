using System;
using System.Collections.Generic;

namespace LogsReader.Reader
{
    public class DataTemplatesDuplicateComparer : IComparer<DataTemplate>
    {
        public int Compare(DataTemplate x, DataTemplate y)
        {
            var xDate = x.DateOfTrace ?? DateTime.MinValue;
            var yDate = y.DateOfTrace ?? DateTime.MinValue;

            int result = xDate.CompareTo(yDate);

            if (result == 0)
            {
                if (x.ParentReader.FilePath.Equals(y.ParentReader.FilePath))
                {
                    return x.FoundLineID.CompareTo(y.FoundLineID);
                }
                else
                {
                    if (x.ParentReader.FileNamePartial.Equals(y.ParentReader.FileNamePartial))
                    {
                        return DateTime.Compare(x.ParentReader.File.CreationTime, y.ParentReader.File.CreationTime);
                    }
                    return string.CompareOrdinal(x.ParentReader.FileNamePartial, y.ParentReader.FileNamePartial);
                }
            }
            else
            {
                return result;
            }
        }
    }
}
