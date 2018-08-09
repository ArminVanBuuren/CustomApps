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
using WCFChat.Service;
using Message = WCFChat.Service.Message;

namespace WCFChat.Client
{

    public partial class MainWindow
    {
        public MainWindow()
        {
            //simpleUser = WCFChat.Client.Properties.Resources.UserSimple;
            InitializeComponent();
        }
    }
}