using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using Utils;
using Utils.Crypto;
using WCFChat.Client.ServiceReference1;

namespace WCFChat.Client
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
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
            this.ShowDialog();
        }

        public DateTime RefreshClientsAndGetEarlyDataMessage(ServiceReference1.Client[] clients, bool isGetEarlyMessage)
        {
            throw new NotImplementedException();
        }

        public Message[] GetAllContentHistory()
        {
            throw new NotImplementedException();
        }

        public void RefreshContentHistory(Message[] messages)
        {
            throw new NotImplementedException();
        }

        public void Receive(Message msg)
        {
            throw new NotImplementedException();
        }

        public void IsWritingCallback(ServiceReference1.Client client)
        {
            throw new NotImplementedException();
        }

        public void Terminate()
        {
            throw new NotImplementedException();
        }
    }
}