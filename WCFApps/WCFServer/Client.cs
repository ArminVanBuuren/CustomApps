using System;
using System.Runtime.Serialization;

namespace WCFChat.Service
{
    [DataContract]
    public class Client
    {
        [DataMember]
        public string GUID { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public DateTime Time { get; set; }
    }

    [DataContract]
    public class User : Client
    {
        [DataMember]
        public string Password { get; set; }
    }

    [DataContract]
    public class Message
    {
        [DataMember]
        public string GUID { get; set; }

        [DataMember]
        public string Content { get; set; }

        [DataMember]
        public DateTime Time { get; set; }
    }
}
