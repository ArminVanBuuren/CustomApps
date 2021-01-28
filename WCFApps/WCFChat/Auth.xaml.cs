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
using Utils;
using WCFChat.Contracts;

namespace WCFChat.Client
{
    public delegate void CreateOrConnectToCloud(User user, Cloud cloud);
    public partial class Auth
    {
        public event CreateOrConnectToCloud IsCreate;
        public event CreateOrConnectToCloud IsConnect;
        private string _guid;
        private string _localAddressUri;
        private Brush defaultBorderBrush;
        public Auth(string guid, string localAddressUri)
        {
            InitializeComponent();
            defaultBorderBrush = NickName.BorderBrush;
            KeyDown += Auth_KeyDown;
            _guid = guid;
            _localAddressUri = localAddressUri;
        }

        private void Auth_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        private void Create_OnClick(object sender, RoutedEventArgs e)
        {
            if (!CheckFields())
                return;

            IsCreate?.Invoke(new User()
            {
                GUID = _guid,
                Name = NickName.Text,
                CloudName = CloudAddress.Text
            }, new Cloud()
            {
                Name = CloudAddress.Text.IsNullOrEmpty() ? null : CloudAddress.Text,
                Address = _localAddressUri
            });

            Close();
        }

        private void Connect_OnClick(object sender, RoutedEventArgs e)
        {
            if(!CheckFields())
                return;

            IsConnect?.Invoke(new User()
            {
                GUID = _guid,
                Name = NickName.Text,
                CloudName = CloudAddress.Text
            }, new Cloud()
            {
                Name = CloudAddress.Text.IsNullOrEmpty() ? null : CloudAddress.Text,
                Address = _localAddressUri
            });

            Close();
        }

        bool CheckFields()
        {
            if (NickName.Text.IsNullOrEmpty())
            {
                NickName.BorderBrush = (Brush) new BrushConverter().ConvertFrom("#FFFF7070");
                return false;
            }

            if (CloudAddress.Text.IsNullOrEmpty())
            {
                CloudAddress.BorderBrush = (Brush)new BrushConverter().ConvertFrom("#FFFF7070");
                return false;
            }

            return true;
        }

        private void NickName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Equals(NickName.BorderBrush, defaultBorderBrush))
                NickName.BorderBrush = defaultBorderBrush;
        }

        private void CloudAddress_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Equals(CloudAddress.BorderBrush, defaultBorderBrush))
                CloudAddress.BorderBrush = defaultBorderBrush;
        }
    }
}
