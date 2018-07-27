using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace WCFChat.Host.Console
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class ChatService : Host.Console.IChat
    {
        object syncObj = new object();
        Dictionary<Host.Console.User, Host.Console.IChatCallback> clients = new Dictionary<Host.Console.User, Host.Console.IChatCallback>();

        public Host.Console.IChatCallback CurrentCallback => OperationContext.Current.GetCallbackChannel<Host.Console.IChatCallback>();

        private Host.Console.User SearchClientsByGuid(string guid)
        {
            foreach (Host.Console.User user in clients.Keys)
            {
                if (user.GUID == guid)
                {
                    return user;
                }
            }
            return null;
        }

        private Host.Console.User SearchClientsBySameName(string name)
        {
            foreach (Host.Console.User user in clients.Keys)
            {
                if (user.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase) )
                {
                    return user;
                }
            }
            return null;
        }

        public bool Login(Host.Console.User newUser)
        {
            System.Console.WriteLine("try to connect!!!!!");
            lock (syncObj)
            {
                // Если в системе уже есть другой юзер с тем же именем
                Host.Console.User findedUserByName = SearchClientsBySameName(newUser.Name);
                if (findedUserByName != null && findedUserByName.GUID != newUser.GUID)
                    return false;

                Host.Console.User userInBase = SearchClientsByGuid(newUser.GUID);
                if (userInBase != null)
                {
                    // грохаем юзера который уже есть в чате но с другого приложения, т.е. грохаем его на старом приложении
                    clients[userInBase].Terminate();
                    clients.Remove(userInBase);
                }

                clients.Add(newUser, CurrentCallback);

                Host.Console.User getHistoryFromClient = null;
                DateTime earlyDatamessage = DateTime.Now;
                List<Host.Console.Client> clientList = clients.Keys.Cast<Host.Console.Client>().ToList();
                foreach (KeyValuePair<Host.Console.User, Host.Console.IChatCallback> client in clients)
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

        public void Say(Host.Console.Message msg)
        {
            lock (syncObj)
            {
                foreach (Host.Console.IChatCallback callback in clients.Values)
                {
                    callback.Receive(msg);
                }
            }
        }

        public void Logoff(Host.Console.Client client)
        {
            Host.Console.User user = SearchClientsByGuid(client.GUID);
            lock (syncObj)
            {
                clients.Remove(user);
                List<Host.Console.Client> clientList = clients.Keys.Cast<Host.Console.Client>().ToList();
                foreach (Host.Console.IChatCallback callback in clients.Values)
                {
                    callback.RefreshClientsAndGetEarlyDataMessage(clientList, false);
                }
            }
        }
    }
}
