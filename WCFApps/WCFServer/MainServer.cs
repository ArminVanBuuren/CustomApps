using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace WCFChat.Service
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

    [ServiceContract(Namespace = "http://localhost/services/server",
        CallbackContract = typeof(IMainCallback), SessionMode = SessionMode.Allowed)]
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
        /// <param name="transactionID"></param>
        [OperationContract(IsOneWay = true)]
        void CreateCloudResult(CloudResult result, string transactionID);

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

}