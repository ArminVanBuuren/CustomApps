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
using WCFChat.Client.CS;

namespace WCFChat.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : CS.IChatCallback, IDisposable
    {
        internal static string AccountStorePath { get; }
        private CS.ChatClient proxy = null;
        private GeneratedUser user = null;
        private AuthorizationWindow authorizationWindow;
        static MainWindow()
        {
            AccountStorePath = Customs.AccountFilePath + ".dat";
        }

        public MainWindow(GeneratedUser user, CS.ChatClient proxy)
        {
            this.user = user;
            this.proxy = proxy;
            InitializeComponent();
        }


        public Message[] GetAllContentHistory()
        {
            
            throw new NotImplementedException();
        }

        public void IsWritingCallback(CS.Client client)
        {
            throw new NotImplementedException();
        }

        public void Receive(Message msg)
        {
            throw new NotImplementedException();
        }

        public DateTime RefreshClientsAndGetEarlyDataMessage(CS.Client[] clients, bool isGetEarlyMessage)
        {
            throw new NotImplementedException();
        }

        public void RefreshContentHistory(Message[] messages)
        {
            throw new NotImplementedException();
        }

        public void Terminate()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}