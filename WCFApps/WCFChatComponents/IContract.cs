using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace WCFChatComponents
{
    [ServiceContract]
    interface IContract
    {
        [OperationContract]
        string Login(string name);

        [OperationContract]
        void Say(string input);

        [OperationContract]
        void Logoff(string guid);
    }
}
