using System;
using System.Collections.Generic;
using System.Windows;

namespace WCFChat.Client
{
	/// <summary>
	/// Логика взаимодействия для WindowWarning.xaml
	/// </summary>
	public partial class WindowWarning
	{
	    private bool isWarning = false;
	    public event EventHandler CheckAuthorization;
		public WindowWarning(double width, string message):base(false, false)
		{
		    isWarning = true;
            InitializeComponent();
		    ErrorMessage.Visibility = Visibility.Visible;
		    Authorization.Visibility = Visibility.Collapsed;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
			ErrorMessage.Text = message;
			Width = width > MaxWidth ? MaxWidth : width;
		}

	    public WindowWarning() : base(false, false)
	    {

	    }

	    private void Button_Click(object sender, RoutedEventArgs e)
	    {
	        if (!isWarning)
	            CheckAuthorization?.Invoke(new KeyValuePair<string, string>(UserName.Text, Password.Text), e);
	        else
	            Close();
	    }

	    public void WaitOwner()
	    {
	        if (isWarning)
	            return;

	        Progress.IsIndeterminate = true;
	        UserName.IsEnabled = false;
	        Password.IsEnabled = false;
	        ButtonOK.IsEnabled = false;
	    }
	    public void WakeUp()
	    {
	        if (isWarning)
	            return;

	        Progress.IsIndeterminate = false;
	        UserName.IsEnabled = true;
	        Password.IsEnabled = true;
	        ButtonOK.IsEnabled = true;
	    }
    }
}
