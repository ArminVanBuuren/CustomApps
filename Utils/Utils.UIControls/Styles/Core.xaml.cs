using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

//using System.Globalization;

namespace Utils.UIControls.Styles
{
	public class FontSizeExtension : MarkupExtension
	{
		[TypeConverter(typeof(FontSizeConverter))]
		public double Size { get; set; }

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return Size;
		}
	}

	public class DateTimeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var dateTimeFormat = parameter as string;

            if (value is DateTime selectedDate)
			{
				return selectedDate.ToString(dateTimeFormat);
			}

			return DateTime.Now.ToString(dateTimeFormat);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			try
			{

				var valor = value as string;
				if (!string.IsNullOrEmpty(valor))
				{
					var retorno = DateTime.Parse(valor);
					return retorno;
				}

				return null;
			}
			catch
			{
				return DependencyProperty.UnsetValue;
			}
		}
	}

	public partial class Core : ResourceDictionary
	{
		private bool _lock = false;
		private const string GET_DATE_TIME = "dd.MM.yyyy HH:mm";
		private const string GET_TIME = "HH:mm";
		private const string GET_DATE = "dd.MM.yyyy";
		Regex _correctDateFormat = new Regex(@"^(\d*)\.(\d*)\.(\d*)\s*(\d*):(\d*)$");
		Regex _firstParce = new Regex(@"[0-9: .]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private Dictionary<TextBox, string> _hashTable = new Dictionary<TextBox, string>();

		public void WindowLoaded(object sender, EventArgs e)
		{
			var temp = (DatePicker) sender;
			temp.CalendarOpened += Temp_CalendarOpened;
			temp.CalendarClosed += Temp_CalendarClosed;


			//TextBox textbox = null;
			//IEnumerable<TextBox> textBoxes = FindVisualChildren<TextBox>(temp);
			//foreach (var _textbox in textBoxes)
			//{
			//	if (_textbox.Name == "PART_TextBox")
			//	{
			//		textbox = _textbox;
			//	}
			//}
		}
		public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
		{
			if (depObj != null)
			{
				for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
				{
					var child = VisualTreeHelper.GetChild(depObj, i);
					if (child != null && child is T)
					{
						yield return (T)child;
					}

					foreach (var childOfChild in FindVisualChildren<T>(child))
					{
						yield return childOfChild;
					}
				}
			}
		}
		private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			_lock = true;
		}
		private void Temp_CalendarOpened(object sender, RoutedEventArgs e)
		{
			_lock = true;
		}
		private void Temp_CalendarClosed(object sender, RoutedEventArgs e)
		{
			_lock = false; 
		}




		private void PART_TextBox_2_OnKeyUp(object sender, KeyEventArgs e)
		{
			var textox = (TextBox) sender;
			switch (e.Key)
			{
				case Key.Left:
					if (textox.CaretIndex > 0)
						KeyBoardMove(textox, -1);
					break;
				case Key.Right:
					if (textox.CaretIndex < textox.Text.Length - 1)
						KeyBoardMove(textox, 1);
					break;
				case Key.Up:
					AddOrReduceDate(textox, 1);
					break;
				case Key.Down:
					AddOrReduceDate(textox, -1);
					break;
			}
		}

		private void PART_TextBox_2_OnIsMouseCapturedChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var textox = (TextBox) sender;
			SetCorrectDateAndHighlight(textox, textox.Text);
		}

		void AddOrReduceDate(TextBox textox, int command)
		{
			var indexSelection = SetCorrectDateAndHighlight(textox, textox.Text);
			if (indexSelection.Key == -1 || indexSelection.Value == -1)
				return;

            if (!DateTime.TryParse(textox.Text, out var current))
				return;

			var index = 0;
			foreach (Group grp in _correctDateFormat.Match(textox.Text).Groups)
			{
				if (indexSelection.Key <= grp.Index && indexSelection.Value > grp.Index)
					break;
				index++;
			}

			switch (index)
			{
				case 0:
				case 1:
					current = current.AddDays(command);
					break;
				case 2:
					current = current.AddMonths(command);
					break;
				case 3:
					current = current.AddYears(command);
					break;
				case 4:
					current = current.AddHours(command);
					break;
				case 5:
					current = current.AddMinutes(command);
					break;
			}

			SetCorrectDateAndHighlight(textox, current.ToString(GET_DATE_TIME));

		}

		void KeyBoardMove(TextBox textox, int command)
		{
			textox.CaretIndex = textox.CaretIndex + command;
			SetCorrectDateAndHighlight(textox, textox.Text);
		}

		KeyValuePair<int, int> SetCorrectDateAndHighlight(TextBox textox, string newDateValue)
		{
			var _currentCaretIndex = textox.CaretIndex;
			SetCorrectDateTime(textox, newDateValue);
			return HighlightPartofDateCaret(textox, _currentCaretIndex);
		}

		private void PART_TextBox_2_OnLostFocus(object sender, RoutedEventArgs e)
		{
			var textox = (TextBox) sender;
			SetCorrectDateTime(textox, textox.Text);
		}


		KeyValuePair<int, int> HighlightPartofDateCaret(TextBox textox, int currentCaretIndex)
		{
			var start = -1;
			var end = 0;
			var temp = -1;
			var i = -1;
			foreach (var cr in textox.Text)
			{
				i++;
				if (int.TryParse(cr.ToString(), out temp))
				{
					if (start == -1)
						start = i;
					end = i;
					continue;
				}

				if (start <= currentCaretIndex && end + 1 >= currentCaretIndex)
					break;

				start = -1;
				end = -1;

			}

			if (start <= currentCaretIndex && end + 1 >= currentCaretIndex && start != -1)
				textox.Select(start, end - start + 1);
			return new KeyValuePair<int, int>(start, end + 1);
		}



	    void DatePickerChanged(object sender, RoutedEventArgs e)
	    {
	        var textox = (TextBox) sender;

	        var _result = _firstParce.Matches(textox.Text).Cast<Match>().Aggregate(string.Empty, (current, stry) => current + stry.Value);

	        if (_lock)
	        {
	            var chooseDate = ((DatePicker) textox.TemplatedParent).SelectedDate;
	            if (chooseDate == null)
	                return;

	            var getTime = DateTime.Now;
	            if (_hashTable.Count > 0)
	                DateTime.TryParse(_hashTable[textox], out getTime);

	            var _cooseDateTime = $"{((DateTime) chooseDate).ToString(GET_DATE)} {getTime.ToString(GET_TIME)}";

	            ((DatePicker) textox.TemplatedParent).SelectedDate = DateTime.Parse(_cooseDateTime);
	            textox.Text = _cooseDateTime;
	            _hashTable[textox] = _cooseDateTime;
	            return;
	        }

            if (DateTime.TryParse(_result, out _))
	        {
	            _hashTable[textox] = _result;
	        }
	    }

	    void SetCorrectDateTime(TextBox currentDateText, string _result)
		{
			if (!AccessDate(currentDateText, _result))
			{
				if (_hashTable.TryGetValue(currentDateText, out _result))
					AccessDate(currentDateText, _result);
				return;
			}
			_hashTable[currentDateText] = _result;
		}

		bool AccessDate(TextBox textox, string _dateTimeInput)
		{
            if (DateTime.TryParse(_dateTimeInput, out var _temp))
			{
				var parent = (DatePicker) textox.TemplatedParent;
				parent.SelectedDate = _temp;
				textox.Text = _dateTimeInput;
				return true;
			}
			return false;
		}
	}
}