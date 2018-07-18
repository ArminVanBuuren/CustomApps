using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace TFSAssist.Trash
{
	class Temp1
	{
		public void method_1()
		{
			//var style = StartDate.Style;

			//var _textDate = StartDate.Template.TargetType.CustomAttributes.Select(s => s.NamedArguments).ToList(); //.FindName("PART_Root", StartDate);
			int i = 0;

			//TextBox menu = FindVisualParent<TextBox>(StartDate);
			//List<TextBox> textBoxes = GetVisualChildCollection<TextBox>(StartDate.Template.TargetType);
			object tt;
			//foreach (CustomAttributeData attr in StartDate.Template.TargetType.CustomAttributes)
			//{
			//	foreach (CustomAttributeNamedArgument arg in attr.NamedArguments)
			//	{
			//		if (arg.MemberName == "Name")
			//		{
			//			if (arg.TypedValue.Value.Equals("PART_TextBox"))
			//			{
			//				i++;
			//			}
			//		}
			//		if (arg.MemberName == "Type" && i > 0)
			//		{
			//			tt = arg.TypedValue.Value;
			//		}

			//	}
			//}

			//if (_textDate != null)
			//	_textDate.TextChanged += _textDate_TextChanged;
		}
		public static T FindVisualParent<T>(UIElement element) where T : UIElement
		{
			UIElement parent = element;
			while (parent != null)
			{
				T correctlyTyped = parent as T;
				if (correctlyTyped != null)
				{
					return correctlyTyped;
				}
				parent = VisualTreeHelper.GetParent(parent) as UIElement;
			}
			return null;
		}
		public static List<T> GetVisualChildCollection<T>(object parent) where T : Visual
		{
			List<T> visualCollection = new List<T>();
			GetVisualChildCollection(parent as DependencyObject, visualCollection);
			return visualCollection;
		}
		private static void GetVisualChildCollection<T>(DependencyObject parent, List<T> visualCollection) where T : Visual
		{
			int count = VisualTreeHelper.GetChildrenCount(parent);
			for (int i = 0; i < count; i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(parent, i);
				if (child is T)
				{
					visualCollection.Add(child as T);
				}
				else if (child != null)
				{
					GetVisualChildCollection(child, visualCollection);
				}
			}
		}
	}
}
