using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using WCFChat.Service;

namespace WCFChat.Host.Console
{
    class Program
    {

        static void Main(string[] args)
        {

            ServiceHost host = null;
            //ServiceHost host2 = null;
            try
            {
                host = new ServiceHost(typeof(MainServer));
                //host2 = new ServiceHost(typeof(ClientServer));




                //List<Binding> bindings = new List<Binding>();
                //bindings.Add(new BasicHttpBinding());        
                //bindings.Add(new WSDualHttpBinding());      
                //bindings.Add(new WSHttpBinding());           
                //bindings.Add(new WSFederationHttpBinding()); 
                //bindings.Add(new NetHttpBinding()); 
                //bindings.Add(new NetHttpsBinding()); 
                //foreach (Binding binding in bindings)
                //{
                //    System.Console.ForegroundColor = ConsoleColor.Green;
                //    System.Console.WriteLine("\nShowing Binding Elements for {0}", binding.GetType().Name);
                //    System.Console.ForegroundColor = ConsoleColor.Gray;

                //    foreach (BindingElement element in binding.CreateBindingElements()) // все элементы из который состоит кажддая привязка
                //        System.Console.WriteLine("\t{0}", element.GetType().Name);
                //}

                //Uri httpAdrs = new Uri("http://localhost:8040/WPFHost/");

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
                //host2.Open();
                
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
            host?.Close();
            System.Threading.Thread.Sleep(1000);
            //System.Console.ReadLine();
        }
    }
}
