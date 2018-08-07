using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using WCFChat.Client.ServiceReference1;
using WCFChat.Client.ServiceReference2;
using WCFChat.Service;

namespace WCFChat.Client
{

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    class MainWindowChatServer : WCFChat.Service.IChat
    {
        public event EventHandler Closing;
        public event EventHandler Unbind;
        public string TransactionID { get; private set; }
        public bool OnClose { get; private set; } = false;
        public bool IsUnbinded { get; set; } = false;
        private MainWindow window;
        public void Show(string transactionId)
        {
            TransactionID = transactionId;
            window = new MainWindow();
            window.Show();
            window.Closing += Window_Closing;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            OnClose = true;
            Closing?.Invoke(this, EventArgs.Empty);
        }

        public void Close()
        {
            if (!OnClose)
                window?.Close();
        }

        public void IncomingRequestForAccess(Service.User user)
        {
            
        }

        public void UnbindCurrentServerFromMain()
        {
            Unbind?.Invoke(this, EventArgs.Empty);
        }


        public Service.ServerResult Connect(Service.User user)
        {
            throw new NotImplementedException();
        }

        public void Disconnect(Service.User user)
        {
            throw new NotImplementedException();
        }

        public void IsWriting(Service.User user, bool isWriting)
        {
            throw new NotImplementedException();
        }

        public void Say(Service.Message message)
        {
            throw new NotImplementedException();
        }
    }
}
