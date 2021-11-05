using System.Windows;

namespace TFSAssist
{
	/// <summary>
	/// Логика взаимодействия для WindowWarning.xaml
	/// </summary>
	public partial class WindowWarning
	{
		public WindowWarning(double width, string title, string message)
		{
			InitializeComponent();
			WindowStartupLocation = WindowStartupLocation.CenterOwner;
			Title = title;
			ErrorMessage.Text = message;
			Width = width > MaxWidth ? MaxWidth : width;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
