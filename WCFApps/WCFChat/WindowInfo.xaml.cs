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
	    public event EventHandler DecisionAccepted;
		public WindowInfo(string title, string message):base(false, false)
		{
		    Initialize(message);
            Title = title;
            InfoGrid.Visibility = Visibility.Visible;
		    AcceptNewUser.Visibility = Visibility.Collapsed;
        }

	    public WindowInfo(string message) : base(false, false)
	    {
	        Title = "Request";
	        InfoGrid.Visibility = Visibility.Collapsed;
	        AcceptNewUser.Visibility = Visibility.Visible;
            Initialize(message);
	    }

	    void Initialize(string message)
	    {
	        InitializeComponent();
	        Topmost = true;
	        WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ErrorMessage.Text = message;
	        //Width = width > MaxWidth ? MaxWidth : width;
        }


        private void ButtonOK_OnClick(object sender, RoutedEventArgs e)
	    {
	        Close();
	    }

	    private void Accept_OnClick(object sender, RoutedEventArgs e)
	    {
	        DecisionAccepted?.Invoke(true, EventArgs.Empty);
	        Close();
	    }

	    private void Reject_OnClick(object sender, RoutedEventArgs e)
	    {
	        DecisionAccepted?.Invoke(false, EventArgs.Empty);
	        Close();
        }
	}
}
