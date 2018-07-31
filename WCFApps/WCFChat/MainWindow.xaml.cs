using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Utils;
using Utils.Crypto;
using WCFChat.Client.ServiceReference1;

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

        public void RefreshClientsAndGetEarlyDataMessage(ServiceReference1.Client[] clients, bool isGetEarlyMessage)
        {
            MessageBox.Show("RefreshClientsAndGetEarlyDataMessage");
            //return DateTime.Now;
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

        public void Receive(Message msg)
        {
            MessageBox.Show("Receive");
        }

        public void IsWritingCallback(ServiceReference1.Client client)
        {
            MessageBox.Show("IsWritingCallback");
        }

        public void Terminate()
        {
            MessageBox.Show("Terminate");
        }
    }
}