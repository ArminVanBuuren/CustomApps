using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace WCFChat.Host.Console
{
    [DataContract]
    public class User
    {
        [DataMember]
        public string GUID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string CloudName { get; set; }
    }

    [DataContract]
    public class Cloud
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Address { get; set; }
    }

    [DataContract]
    public enum CloudResult
    {
        [EnumMember]
        SUCCESS = 0,

        [EnumMember]
        FAILURE = 1,

        [EnumMember]
        CloudNotFound = 2,

        [EnumMember]
        CloudIsBusy = 3,

        [EnumMember]
        YourRequestInProgress = 4,

        [EnumMember]
        NotFound = 5
    }

    [ServiceContract(CallbackContract = typeof(IMainCallback), SessionMode = SessionMode.Allowed)]
    public interface IMainContract
    {
        /// <summary>
        /// Создать облако
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cloud"></param>
        /// <param name="transactionID"></param>
        [OperationContract(IsOneWay = true)]
        void CreateCloud(User user, Cloud cloud, string transactionID);

        /// <summary>
        /// Стать самостоятельным сервером и отвязаться от сервера
        /// </summary>
        /// <param name="transactionID"></param>
        [OperationContract(IsOneWay = true)]
        void Unbind(string transactionID);

        /// <summary>
        /// Получить облако
        /// </summary>
        /// <param name="user"></param>
        [OperationContract(IsOneWay = true)]
        void GetCloud(User user);

        [OperationContract(IsOneWay = true)]
        void RequestForAccessResult(CloudResult result, User user);
    }

    public interface IMainCallback
    {
        /// <summary>
        /// Результат создания облака на основном сервере, через который можно подключаться к серверу чата
        /// </summary>
        /// <param name="result"></param>
        /// <param name="cloud"></param>
        [OperationContract(IsOneWay = true)]
        void CreateCloudResult(CloudResult result, Cloud cloud);

        /// <summary>
        /// Результат отвязки от основного сервера, чтобы больше никто не смог подконнектиться к облаку
        /// </summary>
        /// <param name="result"></param>
        /// <param name="transactionID"></param>
        [OperationContract(IsOneWay = true)]
        void UnbindResult(CloudResult result, string transactionID);

        /// <summary>
        /// Получить адрес облака к которому хочет подконнектиться юзер
        /// </summary>
        /// <param name="user"></param>
        /// <param name="address"></param>
        [OperationContract(IsOneWay = true)]
        void RequestForAccess(User user, string address);

        /// <summary>
        /// Вернукть результат рецепиенту который запросил войти в облако
        /// </summary>
        /// <param name="result"></param>
        /// <param name="cloud"></param>
        [OperationContract(IsOneWay = true)]
        void GetCloudResult(CloudResult result, Cloud cloud);
    }


    class CloudArgs
    {
        public CloudArgs(Cloud cloud, IMainCallback authorCallBack, bool localServer):this(cloud, authorCallBack)
        {
            IsLocalServer = localServer;
        }
        public CloudArgs(Cloud cloud, IMainCallback authorCallBack)
        {
            CloudConfig = cloud;
            AuthorCallBack = authorCallBack;
        }
        public Cloud CloudConfig { get; }
        public IMainCallback AuthorCallBack { get; }
        public bool IsAvailable { get; set; } = false;
        public bool IsLocalServer { get; }
    }

    class CloudCollection : Dictionary<string, CloudArgs>
    {
        public CloudArgs GetCloud(string transactionID)
        {
            CloudArgs result = this[transactionID];
            if (result == null || !result.IsAvailable)
                return null;

            return result;
        }

        public CloudArgs GetCloud(User user)
        {
            return Values.FirstOrDefault(exs => exs.CloudConfig.Name.Equals(user.CloudName, StringComparison.CurrentCultureIgnoreCase));
        }

        public CloudArgs GetCloudArgs(string transactionID)
        {
            CloudArgs result = this[transactionID];
            if (result == null || !result.IsAvailable)
                return null;

            return result;
        }

        public bool IsExist(Cloud cloud)
        {
            return Values.Any(exs => exs.CloudConfig.Name.Equals(cloud.Name, StringComparison.CurrentCultureIgnoreCase));
        }

    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class MainServer : IMainContract
    {
        object syncServer = new object();
        object syncOperation = new object();
        internal static CloudCollection Clouds { get; } = new CloudCollection();
        Dictionary<User, IMainCallback> waitForAccessToCloud = new Dictionary<User, IMainCallback>();

        public IMainCallback CurrentCallback => OperationContext.Current.GetCallbackChannel<IMainCallback>();

        public bool CurrentIsOpened => ((IChannel) CurrentCallback).State == CommunicationState.Opened;


        ServiceHost host = null;
        Dictionary<string, KeyValuePair<Cloud, IMainCallback>> cloudInMainServer = new Dictionary<string, KeyValuePair<Cloud, IMainCallback>>();


        public void CreateCloud(User user, Cloud cloud, string transactionID)
        {
            lock (syncServer)
            {
                try
                {
                    bool exist = Clouds.IsExist(cloud);
                    if (!exist)
                    {
                        bool isOpenned = true;
                        if (string.IsNullOrEmpty(cloud.Address))
                        {
                            try
                            {
                                if (host == null)
                                {
                                    host = new ServiceHost(typeof(ClientServer));
                                    host.Open();
                                }
                                cloud.Address = host.BaseAddresses.ToString();
                                Clouds.Add(transactionID, new CloudArgs(cloud, CurrentCallback, true));
                            }
                            catch (Exception e)
                            {
                                isOpenned = false;
                            }
                        }
                        else
                        {
                            Clouds.Add(transactionID, new CloudArgs(cloud, CurrentCallback));
                        }

                        if (isOpenned)
                        {
                            if (CurrentIsOpened)
                                CurrentCallback.CreateCloudResult(CloudResult.SUCCESS, cloud);
                        }
                        else
                        {
                            if (CurrentIsOpened)
                                CurrentCallback.CreateCloudResult(CloudResult.FAILURE, cloud);
                        }
                    }
                    else
                    {
                        if (CurrentIsOpened)
                            CurrentCallback.CreateCloudResult(CloudResult.CloudIsBusy, null);
                    }
                }
                catch (Exception e)
                {
                    if (CurrentIsOpened)
                        CurrentCallback.CreateCloudResult(CloudResult.FAILURE, null);
                }
            }
        }

        public void Unbind(string transactionID)
        {
            lock (syncServer)
            {
                try
                {
                    CloudArgs exist = Clouds.GetCloud(transactionID);
                    if (exist != null)
                    {
                        if (exist.IsLocalServer)
                            exist.IsAvailable = false;
                        else
                            Clouds.Remove(transactionID);

                        if (CurrentIsOpened)
                            CurrentCallback.UnbindResult(CloudResult.SUCCESS, transactionID);
                    }
                    else if (CurrentIsOpened)
                    {
                        CurrentCallback.UnbindResult(CloudResult.NotFound, transactionID);
                    }
                }
                catch (Exception ex)
                {
                    if (CurrentIsOpened)
                        CurrentCallback.UnbindResult(CloudResult.FAILURE, null);
                }
            }
        }

        public void GetCloud(User user)
        {
            lock (syncServer)
            {
                try
                {
                    if (waitForAccessToCloud.ContainsKey(user))
                    {
                        if (CurrentIsOpened)
                            CurrentCallback.GetCloudResult(CloudResult.YourRequestInProgress, null);
                        return;
                    }

                    RemoteEndpointMessageProperty prop = (RemoteEndpointMessageProperty) OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name];

                    string[] cloudTrnIDs = Clouds.Keys.ToArray();
                    for (int i = 0; i < cloudTrnIDs.Length; i++)
                    {
                        string transactionID = cloudTrnIDs[i];
                        CloudArgs exist = Clouds.GetCloudArgs(transactionID);
                        if (exist != null && exist.CloudConfig.Name.Equals(user.CloudName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            IMainCallback cloudCreator = exist.AuthorCallBack;
                            if (((IChannel) cloudCreator).State == CommunicationState.Opened)
                            {
                                cloudCreator.RequestForAccess(user, $"{prop.Address}:{prop.Port}");
                                waitForAccessToCloud.Add(user, CurrentCallback);
                                return;
                            }
                            else
                            {
                                Clouds.Remove(transactionID);
                                if (CurrentIsOpened)
                                    CurrentCallback.GetCloudResult(CloudResult.CloudNotFound, null);
                                return;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (CurrentIsOpened)
                        CurrentCallback.GetCloudResult(CloudResult.FAILURE, null);
                }
            }
        }

        public void RequestForAccessResult(CloudResult result, User user)
        {
            lock (syncServer)
            {
                try
                {
                    if (!waitForAccessToCloud.ContainsKey(user))
                        return;

                    IMainCallback callBackWaiter = waitForAccessToCloud[user];
                    if (result == CloudResult.SUCCESS)
                    {
                        CloudArgs cloudRes = Clouds.GetCloud(user);
                        if (((IChannel) callBackWaiter).State == CommunicationState.Opened)
                            callBackWaiter.GetCloudResult(result, cloudRes.CloudConfig);
                    }
                    else
                    {
                        if (((IChannel) callBackWaiter).State == CommunicationState.Opened)
                            callBackWaiter.GetCloudResult(result, null);
                    }
                    waitForAccessToCloud.Remove(user);

                }
                catch (Exception ex)
                {
                    // ignored
                }
            }
        }


        //IAsyncResult asyncResult = new Action<User, Cloud, IChatContractCallback>(CreateCloud).BeginInvoke(user, cloud, CurrentCallback, null, null);
        //void CreateCloud(User user, Cloud cloud, IChatContractCallback callBack)
        //{
        //    InstanceContext context = new InstanceContext(mainWindow);
        //    proxy = new ChatClient(context);
        //    proxy.Open();
        //}
    }
}