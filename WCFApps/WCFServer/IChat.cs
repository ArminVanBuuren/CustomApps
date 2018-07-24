using System.Runtime.Serialization;
using System.ServiceModel;

namespace WCFChat.Service
{
    [ServiceContract(Namespace = "http://My")]
    interface IChat
    {
        [OperationContract]
        string Login(string name, string password);

        [OperationContract]
        void Say(string guid, string input);

        [OperationContract]
        void Logoff(string guid);
    }


    interface ICustomer
    {
        string GuidId();
        string Name();
        string Password();
    }

    enum Status
    {
        Success = 0,
        InvalidPassword = 1,
        NameAlreadyUsed = 2
    }

    [DataContract(Namespace = "OtherNamespace")]
    public class Point
    {
        [DataMember]
        public double x;
        [DataMember]
        public double y;

        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
