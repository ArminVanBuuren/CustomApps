using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.Threading.Tasks;
using Utils;
using Utils.Crypto;
using WCFChat.Client.CS;

namespace WCFChat.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : CS.IChatCallback, IDisposable
    {
        internal static string AccountStorePath { get; }
        private CS.ChatClient proxy = null;
        private GeneratedUser localUser = null;

        static MainWindow()
        {
            AccountStorePath = Customs.AccountFilePath + ".dat";
        }
        
        public MainWindow()
        {
            try
            {
                if (File.Exists(AccountStorePath))
                {
                    using (Stream stream = new FileStream(AccountStorePath, FileMode.Open, FileAccess.Read))
                    {
                        localUser = new BinaryFormatter().Deserialize(stream) as GeneratedUser;
                    }
                }
            }
            catch (Exception e)
            {
                
            }

            try
            {
                InstanceContext context = new InstanceContext(this);
                proxy = new CS.ChatClient(context);
                string servicePath = proxy.Endpoint.ListenUri.AbsolutePath;
                string serviceListenPort = proxy.Endpoint.Address.Uri.Port.ToString();
                proxy.Endpoint.Address = new EndpointAddress(@"http://localhost:" + serviceListenPort + servicePath);
                proxy.Open();
                //proxy.InnerDuplexChannel.Faulted += new EventHandler(InnerDuplexChannel_Faulted);
                //proxy.InnerDuplexChannel.Opened += new EventHandler(InnerDuplexChannel_Opened);
                //proxy.InnerDuplexChannel.Closed += new EventHandler(InnerDuplexChannel_Closed);
                //proxy.ConnectCompleted += new EventHandler<ConnectCompletedEventArgs>(proxy_ConnectCompleted);
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }



            WindowWarning newUser = new WindowWarning();
            newUser.Title = "Authorization";
            newUser.Topmost = true;
            newUser.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            newUser.Focus();

            
            if (localUser == null)
            {
                newUser.CheckAuthorization += (sender, args) =>
                                              {
                                                  KeyValuePair<string, string> userData = (KeyValuePair<string, string>)sender;
                                                  User newuser = new User() {
                                                                                Name = userData.Key,
                                                                                Password = userData.Value,
                                                                                GUID = Guid.NewGuid().ToString("D"),
                                                                                Time = DateTime.Now
                                                                            };

                                                  newUser.WaitOwner();
                                                  
                                              };
            }
            else
            {
                newUser.CheckAuthorization += (sender, args) =>
                                              {
                                                  KeyValuePair<string, string> userData = (KeyValuePair<string, string>)sender;
                                                  if (localUser.MyUser.Name.Equals(userData.Key) && localUser.MyUser.Password.Equals(userData.Value))
                                                  {
                                                      newUser.Close();
                                                  }
                                                  else
                                                  {
                                                      newUser.IsBlured = true;
                                                      WindowWarning authorizationError = new WindowWarning(Width, "Username or password is incorrect. Please try again.");
                                                      authorizationError.Title = "Authorization Error!";
                                                      authorizationError.ShowDialog();
                                                      newUser.IsBlured = false;
                                                  }
                                              };
            }
            newUser.Closed += (sender, args) =>
                              {
                                  
                                  this.Close();
                              };
            newUser.ShowDialog();


            InitializeComponent();
        }

        bool TryToConnect(User user)
        {
            Task<bool> result;
            result = proxy.LoginAsync(user);
            result.Start();
            return false;
        }

        //KeyValuePair<string, string> GetUserAuthorization()
        //{

        //    if(newUser.Closed)
        //    return new KeyValuePair<string, string>(newUser.UserName.Text, newUser.Password.Text);
        //}

        public Message[] GetAllContentHistory()
        {
            
            throw new NotImplementedException();
        }

        public void IsWritingCallback(CS.Client client)
        {
            throw new NotImplementedException();
        }

        public void Receive(Message msg)
        {
            throw new NotImplementedException();
        }

        public DateTime RefreshClientsAndGetEarlyDataMessage(CS.Client[] clients, bool isGetEarlyMessage)
        {
            throw new NotImplementedException();
        }

        public void RefreshContentHistory(Message[] messages)
        {
            throw new NotImplementedException();
        }

        public void Terminate()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}