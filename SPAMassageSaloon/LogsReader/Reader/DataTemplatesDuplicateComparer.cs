using System;
using System.Collections.Generic;

namespace LogsReader.Reader
{
	public class DataTemplatesDuplicateComparer : IEqualityComparer<DataTemplate>
	{
		public int Compare(DataTemplate x, DataTemplate y)
		{
			switch (x)
			{
				// равны
				case null when y == null:
					return 0;

				// y - больше
				case null:
					return 1;
			}

			// x - больше
			if (y == null)
				return -1;

			if (x.Equals(y) && x.FoundLineID != -1)
				return 0; // означает та же строка и тот же файл

			var xDate = x.Date ?? DateTime.MinValue;
			var yDate = y.Date ?? DateTime.MinValue;
			var result = xDate.CompareTo(yDate);

			if (result == 0)
			{
				if (x.ParentReader.FilePath.Equals(y.ParentReader.FilePath, StringComparison.InvariantCultureIgnoreCase))
					return x.FoundLineID.CompareTo(y.FoundLineID);

				return x.ParentReader.FileNamePartial.Equals(y.ParentReader.FileNamePartial)
					? DateTime.Compare(x.ParentReader.File.CreationTime, y.ParentReader.File.CreationTime)
					: string.CompareOrdinal(x.ParentReader.FileNamePartial, y.ParentReader.FileNamePartial);
			}

			return result;
		}

		public bool Equals(DataTemplate x, DataTemplate y) => Compare(x, y) == 0;

		public int GetHashCode(DataTemplate obj) => obj.GetHashCode();
	}
}