using Utils.WinForm.DataGridViewHelper;

namespace LogsReader.Reader
{
	public class TraceNameFilter
	{
		public TraceNameFilter(bool @checked, string traceName, int countMatches, int countErrors)
		{
			Checked = @checked;
			TraceName = traceName;
			CountMatches = countMatches;
			CountErrors = countErrors;
		}

		public bool Checked { get; set; } = false;

		[DGVColumn(ColumnPosition.After, nameof(DataTemplate.Tmp.TraceName))]
		public string TraceName { get; }

		/// <summary>
		/// Количество совпадений по имени трейса
		/// </summary>
		[DGVColumn(ColumnPosition.After, "Matches", true)]
		public int CountMatches { get; }

		/// <summary>
		/// Количество ошибок по имени трейса
		/// </summary>
		[DGVColumn(ColumnPosition.After, "Errors", true)]
		public int CountErrors { get; }
	}
}