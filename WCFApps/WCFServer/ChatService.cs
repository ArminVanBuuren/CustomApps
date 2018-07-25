using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace WCFChat.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class ChatService : IChat
    {
        object syncObj = new object();
        Dictionary<User, IChatCallback> clients = new Dictionary<User, IChatCallback>();


        public IChatCallback CurrentCallback
        {
            get { return OperationContext.Current.GetCallbackChannel<IChatCallback>(); }
        }

        private User SearchClientsByName(string guid)
        {
            foreach (User user in clients.Keys)
            {
                if (user.GUID == guid)
                {
                    return user;
                }
            }
            return null;
        }

        public bool Login(User newUser)
        {
            lock (syncObj)
            {
                User userInBase = SearchClientsByName(newUser.GUID);
                if (userInBase != null)
                    clients.Remove(userInBase);

                clients.Add(newUser, CurrentCallback);

                User getHistoryFromClient = null;
                DateTime earlyDatamessage = DateTime.Now;
                List<Client> clientList = clients.Keys.Cast<Client>().ToList();
                foreach (KeyValuePair<User, IChatCallback> client in clients)
                {
                    DateTime earlyData = client.Value.RefreshClientsAndGetEarlyDataMessage(clientList, true);
                    if (earlyData < earlyDatamessage)
                    {
                        getHistoryFromClient = client.Key;
                        earlyDatamessage = earlyData;
                    }
                }

                if (getHistoryFromClient != null)
                {
                    CurrentCallback.RefreshContentHistory(clients[getHistoryFromClient].GetAllContentHistory());
                }
            }
            return true;
        }

        public void Say(Message msg)
        {
            lock (syncObj)
            {
                foreach (IChatCallback callback in clients.Values)
                {
                    callback.Receive(msg);
                }
            }
        }

        public void Logoff(Client client)
        {
            User user = SearchClientsByName(client.GUID);
            lock (syncObj)
            {
                clients.Remove(user);
                List<Client> clientList = clients.Keys.Cast<Client>().ToList();
                foreach (IChatCallback callback in clients.Values)
                {
                    callback.RefreshClientsAndGetEarlyDataMessage(clientList, false);
                }
            }
        }
    }
}
