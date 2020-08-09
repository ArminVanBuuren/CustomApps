using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.WinForm.DataGridViewHelper;

namespace LogsReader.Reader
{
	public class TraceNameFilter
	{
		public TraceNameFilter(string traceName, int countMatches, int countErrors)
		{
			TraceName = traceName;
			CountMatches = countMatches;
			CountErrors = countErrors;
		}

		[DGVColumn(ColumnPosition.First, nameof(DataTemplate.Tmp.TraceName))]
		public string TraceName { get; }

		/// <summary>
		/// Количество совпадений по имени трейса
		/// </summary>
		[DGVColumn(ColumnPosition.After, "Matches", true)]
		public int CountMatches { get;}

		/// <summary>
		/// Количество ошибок по имени трейса
		/// </summary>
		[DGVColumn(ColumnPosition.After, "Errors", true)]
		public int CountErrors { get; }
	}
}
