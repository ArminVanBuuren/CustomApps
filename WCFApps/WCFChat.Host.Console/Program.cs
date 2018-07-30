using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace WCFChat.Host.Console
{
    class Program
    {

        static void Main(string[] args)
        {
            ServiceHost host = null;
            try
            {
                //Uri httpAdrs = new Uri("http://localhost:8040/WPFHost/");
                host = new ServiceHost(typeof(ChatService));
                //WSDualHttpBinding dualBinding = new WSDualHttpBinding();
                //dualBinding.MaxBufferPoolSize = (int) 67108864;
                //dualBinding.MaxReceivedMessageSize = (int) 67108864;
                //dualBinding.ReaderQuotas.MaxArrayLength = 67108864;
                //dualBinding.ReaderQuotas.MaxBytesPerRead = 67108864;
                //dualBinding.ReaderQuotas.MaxStringContentLength = 67108864;

                //ServiceThrottlingBehavior throttle;
                //throttle = host.Description.Behaviors.Find<ServiceThrottlingBehavior>();
                //if (throttle == null)
                //{
                //    throttle = new ServiceThrottlingBehavior();
                //    throttle.MaxConcurrentCalls = 100;
                //    throttle.MaxConcurrentSessions = 100;
                //    host.Description.Behaviors.Add(throttle);
                //}
                //dualBinding.ReceiveTimeout = new TimeSpan(20, 0, 0);
                //dualBinding.ReliableSession.InactivityTimeout = new TimeSpan(20, 0, 10);


                //host.AddServiceEndpoint(typeof(IChat), dualBinding, "http");
                //ServiceMetadataBehavior mBehave = new ServiceMetadataBehavior();
                //host.Description.Behaviors.Add(mBehave);

                
                //host.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexTcpBinding(), "http://localhost:8040/WPFHost/mex");


                host.Open();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
            finally
            {
                if (host != null && host.State == CommunicationState.Opened)
                {
                    System.Console.WriteLine("Host Opened!\r\nProcessing...");
                }
                else
                {
                    System.Console.WriteLine("Host Not Opened.");
                }
            }

            System.Console.ReadLine();
        }
    }
}
