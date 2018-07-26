using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WCFChat.Client
{
    /// <summary>
    /// Interaction logic for AuthorizationWindow.xaml
    /// </summary>
    public partial class AuthorizationWindow
    {
        public event EventHandler CheckAuthorization;
        public AuthorizationWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Progress.IsIndeterminate = true;
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CheckAuthorization?.Invoke(new KeyValuePair<string, string>(UserName.Text, Password.Text), e);
        }

        public void OwnerIsLoaded()
        {
            Authorization.Visibility = Visibility.Visible;
            ButtonOK.Visibility = Visibility.Visible;
            Progress.IsIndeterminate = false;
        }

        public void WaitWhenConnect()
        {
            Progress.IsIndeterminate = true;
            UserName.IsEnabled = false;
            Password.IsEnabled = false;
            ButtonOK.IsEnabled = false;
        }

        public void WakeUp()
        {
            Progress.IsIndeterminate = false;
            UserName.IsEnabled = true;
            Password.IsEnabled = true;
            ButtonOK.IsEnabled = true;
        }

        public void InfoWarningMessage(string msg)
        {
            ErrorMessage.Visibility = Visibility.Visible;
            ErrorMessage.Text = msg;
            ErrorMessage.Foreground = (Brush)new BrushConverter().ConvertFrom("#FFCB0000");
        }

        private void UserName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (ErrorMessage.Visibility == Visibility.Visible)
                ErrorMessage.Visibility = Visibility.Collapsed;
        }

        private void Password_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (ErrorMessage.Visibility == Visibility.Visible)
                ErrorMessage.Visibility = Visibility.Collapsed;
        }
    }
}
