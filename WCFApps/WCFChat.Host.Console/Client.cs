using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WCFChat.Host.Console
{
    [DataContract]
    //[KnownType(typeof(User))] - в случа апкаста чтобы свойство Password из наследника передавалось в запрос то нужен этот аттрибут иначе серализация не сработает
    public class WCFChatClient
    {
        [DataMember]
        public string GUID { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public DateTime Time { get; set; }
    }

    [DataContract]
    public class User
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
