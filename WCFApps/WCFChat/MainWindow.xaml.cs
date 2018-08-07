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
using System.Windows.Threading;
using Utils;
using Utils.Crypto;
using WCFChat.Client.ServiceReference1;
using WCFChat.Client.ServiceReference2;
using Message = WCFChat.Client.ServiceReference2.Message;
using User = WCFChat.Client.ServiceReference2.User;
using MainServerUser = WCFChat.Client.ServiceReference1.User;

namespace WCFChat.Client
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

    }
}