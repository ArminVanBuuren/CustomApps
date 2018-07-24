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
        public string Password { get; set; }
        [DataMember]
        public DateTime Time { get; set; }
    }
}
