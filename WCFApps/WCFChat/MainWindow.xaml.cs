using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;

namespace WCFChat.Client
{
    public delegate void AccessOrRemoveUser(string name, string address, bool isRemove);
    public partial class MainWindow
    {
        public event AccessOrRemoveUser OnAccessOrRemoveUser;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void AddCloud(string name)
        {
            NameOfCloud.Items.Add(new Label() {
                                                  Content = name
                                              });
        }
        public void AddUser(string user, string address)
        {
            StackPanel panel2 = (StackPanel)FindResource("TemplateForUser");
            foreach (var item in panel2.Children)
            {
                Label label = (Label)item;
                label.Content = user;
                label.ToolTip = address;
            }
        }

        public void AdminAddUser(string user, string address)
        {
            StackPanel panel2 = (StackPanel) FindResource("TemplateForUserAdmin");
            foreach (var item in panel2.Children)
            {
                switch (item)
                {
                    case Button button:
                        button.Click += ButtonAccessRejectClick;
                        button.ToolTip = address;
                        break;
                    case Label _:
                        Label userName = (Label) item;
                        userName.Content = user;
                        userName.ToolTip = address;
                        break;
                }
            }
            Users.Items.Add(panel2);
        }

        private void ButtonAccessRejectClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button) sender;
            StackPanel parent = (StackPanel)button.Parent;

            Label user = parent.Children.OfType<Label>().FirstOrDefault();

            if (user == null)
                return;


            if (button.Name.Equals("ButtonAccept"))
            {
                parent.Background = (Brush)new BrushConverter().ConvertFrom("#FF00FF0C");
                OnAccessOrRemoveUser?.BeginInvoke(user.Name, user.ToolTip.ToString(), false, null, null);
            }
            else
            {
                Users.Items.Remove(parent);
                OnAccessOrRemoveUser?.BeginInvoke(user.Name, user.ToolTip.ToString(), true, null, null);
            }

        }

    }
}