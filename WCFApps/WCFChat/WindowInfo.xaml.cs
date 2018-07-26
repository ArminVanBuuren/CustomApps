using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WCFChat.Client
{
	/// <summary>
	/// Логика взаимодействия для WindowWarning.xaml
	/// </summary>
	public partial class WindowInfo
	{
		public WindowInfo(double width, string message):base(false, false)
		{
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
			ErrorMessage.Text = message;
			Width = width > MaxWidth ? MaxWidth : width;
		}

	}
}
