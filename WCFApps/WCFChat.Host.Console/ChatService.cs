using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;

namespace WCFChat.Host.Console
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class ChatService : IChat
    {
        object syncObj = new object();
        Dictionary<User, IChatCallback> clients = new Dictionary<User, IChatCallback>();

        public IChatCallback CurrentCallback => OperationContext.Current.GetCallbackChannel<IChatCallback>();

        private User SearchClientsByGuid(string guid)
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

        private User SearchClientsBySameName(string name)
        {
            foreach (User user in clients.Keys)
            {
                if (user.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase) )
                {
                    return user;
                }
            }
            return null;
        }

        public bool Login(User newUser)
        {
            try
            {
                lock (syncObj)
                {
                    RemoteEndpointMessageProperty prop = (RemoteEndpointMessageProperty)OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name];
                    System.Console.WriteLine("Try To Connect {0}:{1}", prop.Address, prop.Port);

                    // Если в системе уже есть другой юзер с тем же именем
                    User findedUserByName = SearchClientsBySameName(newUser.Name);
                    if (findedUserByName != null && findedUserByName.GUID != newUser.GUID)
                        return false;

                    User userInBase = SearchClientsByGuid(newUser.GUID);
                    if (userInBase != null)
                    {
                        // грохаем юзера который уже есть в чате но с другого приложения, т.е. грохаем его на старом приложении
                        clients[userInBase].Terminate();
                        clients.Remove(userInBase);
                    }


                    clients.Add(newUser, CurrentCallback);


                    User getHistoryFromClient = null;
                    DateTime earlyDatamessage = DateTime.Now;
                    List<Client> clientList = clients.Keys.Cast<Client>().ToList();

                    //Task.Run(() =>
                    //         {
                    //             Thread.Sleep(10 * 1000);
                    //         }).ContinueWith((antecedent) =>
                    //                         {
                    //                             CurrentCallback.RefreshClientsAndGetEarlyDataMessage(clientList, false);
                    //                         });

                    
                    // DateTime res;
                    CurrentCallback.RefreshClientsAndGetEarlyDataMessage(clientList, false);

                    

                    foreach (KeyValuePair<User, IChatCallback> client in clients)
                    {
                        if (client.Key == newUser)
                            continue;

                        //DateTime earlyData = client.Value.RefreshClientsAndGetEarlyDataMessage(clientList, true);
                        //if (earlyData < earlyDatamessage)
                        //{
                        //    getHistoryFromClient = client.Key;
                        //    earlyDatamessage = earlyData;
                        //}
                    }



                    if (getHistoryFromClient != null)
                    {
                        CurrentCallback.RefreshContentHistory(clients[getHistoryFromClient].GetAllContentHistory());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(false);
                System.Console.WriteLine(ex);
                return false;
            }
            System.Console.WriteLine(true);
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
            User user = SearchClientsByGuid(client.GUID);
            lock (syncObj)
            {
                clients.Remove(user);
                List<Client> clientList = clients.Keys.Cast<Client>().ToList();
                foreach (IChatCallback callback in clients.Values)
                {
                    //callback.RefreshClientsAndGetEarlyDataMessage(clientList, false);
                }
            }
        }
    }
}
