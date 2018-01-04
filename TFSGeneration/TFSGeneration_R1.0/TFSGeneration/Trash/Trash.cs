namespace TFSGeneration.Trash
{
	class Trash
	{
		//https://stackoverflow.com/questions/4330012/how-to-add-events-to-templated-control-in-silverlight
		//[TemplatePart(Name = DatePicker.ElementStartDate, Type = typeof(RadDatePicker))]
		//[TemplatePart(Name = DatePicker.ElementEndDate, Type = typeof(RadDatePicker))]
		//public class DatePicker : Control
		//{
		//	public DatePicker()
		//	{
		//		this.DefaultStyleKey = typeof(DatePicker);
		//	}


		//	#region Template Part Names
		//	private const string ElementStartDate = "startDate";
		//	private const string ElementEndDate = "endDate";
		//	#endregion

		//	#region Template Parts
		//	private RadDatePicker _StartDate;

		//	internal RadDatePicker StartDate
		//	{
		//		get { return _StartDate; }
		//		private set
		//		{
		//			if (_StartDate != null)
		//			{
		//				_StartDate.SelectionChanged -= StartDate_SelectionChanged;
		//			}

		//			_StartDate = value;

		//			if (_StartDate != null)
		//			{
		//				_StartDate.SelectionChanged += StartDate_SelectionChanged;
		//			}
		//		}
		//	}

		//	private RadDatePicker _EndDate;

		//	internal RadDatePicker EndDate
		//	{
		//		get { return _EndDate; }
		//		private set
		//		{
		//			if (_EndDate != null)
		//			{
		//				_EndDate.SelectionChanged -= EndDate_SelectionChanged;
		//			}

		//			_EndDate = value;

		//			if (_EndDate != null)
		//			{
		//				_EndDate.SelectionChanged += EndDate_SelectionChanged;
		//			}
		//		}
		//	}

		//	#endregion

		//	public static readonly DependencyProperty StartDateSelectedDateProperty =
		//		DependencyProperty.Register(
		//			"StartDateSelectedDateProperty",
		//			 typeof(DateTime?),
		//			 typeof(DatePicker),
		//			 new PropertyMetaData(new DateTime(2010, 01, 01)));

		//	public DateTime? StartDateSelectedDate
		//	{
		//		get { return (DateTime?)GetValue(StartDateSelectedDateProperty); }
		//		set { SetValue(StartDateSelectedDateProperty)}
		//	}

		//	public static readonly DependencyProperty EndDateSelectedDateProperty =
		//		DependencyProperty.Register(
		//			"EndDateSelectedDateProperty",
		//			 typeof(DateTime?),
		//			 typeof(DatePicker),
		//			 new PropertyMetaData(new DateTime(2010, 01, 01)));

		//	public DateTime? EndDateSelectedDate
		//	{
		//		get { return (DateTime?)GetValue(EndDateSelectedDateProperty); }
		//		set { SetValue(EndDateSelectedDateProperty)}
		//	}

		//	public override void OnApplyTemplate()
		//	{
		//		base.OnApplyTemplate();

		//		StartDate = GetTemplateChild(ElementStartDate) as RadDatePicker;
		//		EndDate = GetTemplateChild(ElementEndDate) as RadDatePicker;
		//	}

		//	void StartDate_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
		//	{
		//		// Do stuff with StartDate here
		//	}

		//	void EndDate_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
		//	{
		//		// Do stuff with EndDate here
		//	}
		//}
	}
}
