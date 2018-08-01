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
using Message = WCFChat.Client.ServiceReference1.Message;

namespace WCFChat.Client
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public partial class MainWindow : ServiceReference1.IChatCallback
    {
        internal static string AccountStorePath { get; }
        private IChat proxy = null;
        private GeneratedUser user = null;

        static MainWindow()
        {
            AccountStorePath = Customs.AccountFilePath + ".dat";
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        public void Show(GeneratedUser user, IChat proxy)
        {
            this.user = user;
            this.proxy = proxy;
            base.Show();
        }


        public DateTime Refresh(WCFChatClient[] clients, bool isGetEarlyMessage)
        {
            foreach (WCFChatClient VARIABLE in clients)
            {
                User ss = (User) VARIABLE;
            }
            return DateTime.MaxValue;
        }

        public Message[] GetAllContentHistory()
        {
            MessageBox.Show("GetAllContentHistory");
            return new Message[]{new Message() };
        }

        public void RefreshContentHistory(Message[] messages)
        {
            MessageBox.Show("RefreshContentHistory");
        }

        public DateTime Receive(Message msg)
        {
            //MessageBox.Show("Receive");
            return DateTime.MaxValue;
        }

        public void IsWritingCallback(WCFChatClient client)
        {
            MessageBox.Show("IsWritingCallback");
        }


        public void Terminate()
        {
            RemoteEndpointMessageProperty prop = (RemoteEndpointMessageProperty)OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name];
            MessageBox.Show($"Try to Connect From {prop.Address} : {prop.Port}");
        }
    }
}