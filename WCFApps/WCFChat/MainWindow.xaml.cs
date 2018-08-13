using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
using System.Reflection;
using System.Windows.Markup;
using System.Xml;
using System.Resources;
using System.Windows.Data;
using WCFChat.Service;
using Message = WCFChat.Service.Message;
using Utils;

namespace WCFChat.Client
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            //simpleUser = WCFChat.Client.Properties.Resources.UserSimple;
            InitializeComponent();
        }

        private void AddCloud_OnClick(object sender, RoutedEventArgs e)
        {
            
        }

        private void RemoveCloud_OnClick(object sender, RoutedEventArgs e)
        {
            
        }

        private StackPanel curentCloud;
        public virtual void CreateCloud(User initiator, Cloud cloud, string transactionId, AccessResult removeOrAcceptUser)
        {
            curentCloud = new StackPanel();
            curentCloud.Orientation = Orientation.Horizontal;
            CheckBox unbind = new CheckBox();
            unbind.VerticalAlignment = VerticalAlignment.Center;
            unbind.Margin = new Thickness(0, 0, 0, -4);
            unbind.ToolTip = "Unbind chat from main server";
            unbind.Checked += Unbind_Checked;
            curentCloud.Children.Add(new Label()
                                     {
                                         Content = cloud.Name.IsNullOrEmpty() ? cloud.Address : cloud.Name
                                     });
            curentCloud.Children.Add(unbind);

            NameOfCloud.Items.Add(curentCloud);
        }

        private void Unbind_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox unbindChk = (CheckBox)sender;
            unbindChk.IsEnabled = false;
        }

        public virtual void JoinToCloud(User initiator, Cloud cloud)
        {
            curentCloud = new StackPanel();
            curentCloud.Children.Add(new Label()
                                     {
                                         Content = cloud.Name.IsNullOrEmpty() ? cloud.Address : cloud.Name
                                     });

            NameOfCloud.Items.Add(curentCloud);
        }
    }
}