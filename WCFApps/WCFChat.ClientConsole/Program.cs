using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using WCFChat.ClientConsole.ServiceReference1;

namespace WCFChat.ClientConsole
{
	class Program
	{
		private static MainContractClient mainProxy;
		private static MainCallback mainCallBack;

		static readonly object sync = new object();
		static bool _isClosed;
		private static bool isClosed
		{
			get
			{
				lock (sync)
					return _isClosed;
			}
			set
			{
				lock (sync)
					_isClosed = value;
			}
		}
		

		static void Main(string[] args)
		{
			mainCallBack = new MainCallback();
			ConnectToMainServer();
			ConnectionCompleted();

			try
			{
				var isExit = false;
				while (!isExit)
				{
					var command = Console.ReadLine();
					isExit = command != null && command.IndexOf("exit", StringComparison.CurrentCultureIgnoreCase) != -1;

					if (command.IndexOf("test", StringComparison.CurrentCultureIgnoreCase) != -1)
					{
						var contact = new EdmContact
						{
							Guid = new Guid(),
							FnsId = "1111-2222-3333"
						};
						
						mainProxy.Test(contact);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				Thread.Sleep(5000);
			}
			finally
			{
				isClosed = true;
				Closing();
			}
		}

		static bool ConnectToMainServer()
		{
			try
			{
				OpenOrReopenConnection();
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				((ICommunicationObject)mainProxy)?.Abort();
				mainProxy = null;
				//InfoWarningMessage(e.InnerException != null ? string.Format("Exception when connect to Server!\r\n{0}", e.InnerException.Message) : string.Format("Exception when connect to Server!\r\n{0}", e.Message));
				return false;
			}
		}

		static void OpenOrReopenConnection()
		{
			Console.WriteLine("Try to connect..");
			mainProxy?.Abort();
			var context = new InstanceContext(mainCallBack);
			mainProxy = new MainContractClient(context);
			mainProxy.Open();
			mainProxy.InnerDuplexChannel.Faulted += InnerDuplexChannel_Faulted;
			mainProxy.InnerDuplexChannel.Opened += InnerDuplexChannel_Opened;
			mainProxy.InnerDuplexChannel.Closed += InnerDuplexChannel_Closed;
		}

		public static void ConnectionCompleted()
		{
			Console.WriteLine("Connection completed!");
		}

        private static void Closing()
		{
			mainProxy?.Abort();
		}


        static void InnerDuplexChannel_Closed(object sender, EventArgs e)
        {
	        HandleProxy();
        }

        static void InnerDuplexChannel_Opened(object sender, EventArgs e)
        {
	        HandleProxy();
        }

        static void InnerDuplexChannel_Faulted(object sender, EventArgs e)
        {
	        HandleProxy();
        }

        private static void HandleProxy()
        {
	        if (mainProxy != null)
	        {
		        Console.WriteLine($"Status changed: {mainProxy.State}");
				switch (mainProxy.State)
		        {
			        case CommunicationState.Faulted:
				        if (!isClosed)
					        OpenOrReopenConnection();
				        break;
		        }
	        }
        }
	}

	public class MainCallback : IMainContractCallback
	{
		public void CreateCloudResult(CloudResult result, string transactionID)
		{
			Console.WriteLine(result);
		}

		public void GetCloudResult(ServerResult result, Cloud cloud, string transactionID)
		{
			Console.WriteLine(result);
		}

		public void RequestForAccess(User user, string address)
		{
			Console.WriteLine($"User:{user} Address:{address}");
		}
	}
}
