using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;
using System.Xml;
using System.Resources;
using System.Windows.Data;
using WCFChat.Service;
using Message = WCFChat.Service.Message;
using Utils;
using WCFChat.Client.BasicControl;
using WCFChat.Client.ServiceReference1;

namespace WCFChat.Client
{
    public delegate void AccessResult(ServerResult result, User user);

    public partial class MainWindow : IMainContractCallback
    {
        static string RegeditKey => Guid.NewGuid().ToString("D");
        static MainWindow()
        {
            //RegeditKey = Customs.GetOrSetRegedit(Customs.ApplicationName, "This application create WCF chat client-server or only client to Main foreign server.");
        }

        private object sync = new object();
        private MainContractClient mainProxy;
        private MainWindowChatServer currentServer;
        private MainWindowChatClient currentClient;
        private string localAddressUri;
        private Dictionary<string, InnerWaiterCloud> listBoxClouds = new Dictionary<string, InnerWaiterCloud>();

        public MainWindow()
        {
            //simpleUser = WCFChat.Client.Properties.Resources.UserSimple;
            InitializeComponent();

            currentServer = new MainWindowChatServer(this, RequestForAccessResult);
            ServiceHost localHost = new ServiceHost(currentServer);
            localHost.Open();
            localAddressUri = localHost.Description.Endpoints[0].Address.ToString();

            currentClient = new MainWindowChatClient(this);
        }

        void IMainContractCallback.RequestForAccess(User user, string address)
        {
            if (currentServer != null)
            {
                currentServer.IncomingRequestForAccess(user, address);
            }
            else
            {
                RequestForAccessResult(ServerResult.CloudNotFound, user);
            }
        }

        void RequestForAccessResult(ServerResult result, User user)
        {
            mainProxy?.RemoveOrAccessUser(result, user);
        }

        public void UpdateProxy(MainContractClient newMainProxy)
        {
            mainProxy = newMainProxy ?? throw new ArgumentException(nameof(MainContractClient));
        }

        private void AddCloud_OnClick(object sender, RoutedEventArgs e)
        {
            Auth auth = new Auth(RegeditKey, localAddressUri);
            auth.IsCreate += Auth_IsCreate;
            auth.IsConnect += Auth_IsConnect;
            auth.Focus();
            auth.Owner = this;
            auth.ShowDialog();
        }

        private void Auth_IsCreate(User user, Cloud cloud)
        {
            string trnID = Guid.NewGuid().ToString("D");
            if (mainProxy == null)
            {
                currentServer.CreateCloud(user, cloud, trnID);
                lock (sync)
                {
                    listBoxClouds.Add(trnID, new InnerWaiterCloud(user, cloud, AddCloudToListbox(cloud, true)));
                }
            }
            else
            {
                mainProxy.CreateCloudAsync(cloud, trnID);
                lock (sync)
                {
                    listBoxClouds.Add(trnID, new InnerWaiterCloud(user, cloud, AddWaitCloudToListbox(cloud, true)));
                }
            }
        }

        class InnerWaiterCloud
        {
            public InnerWaiterCloud(User user, Cloud cloud, Grid grid)
            {
                User = user;
                Cloud = cloud;
                Grid = grid;
            }
            public User User { get; }
            public Cloud Cloud { get; }
            public Grid Grid { get; }
        }

        Grid AddWaitCloudToListbox(Cloud cloud, bool isAdmin)
        {
            Grid grd = new Grid();
            grd.Children.Add(new ProgressBar() { IsIndeterminate = true });
            AddStackText(grd, cloud, isAdmin);
            NameOfCloud.Items.Add(grd);
            return grd;
        }

        Grid AddCloudToListbox(Cloud cloud, bool isAdmin)
        {
            Grid grd = new Grid();
            AddStackText(grd, cloud, isAdmin);
            NameOfCloud.Items.Add(grd);
            return grd;
        }

        void AddStackText(Grid grd, Cloud cloud, bool isAdmin)
        {
            if (isAdmin)
            {
                StackPanel curentCloud = new StackPanel();
                curentCloud.Orientation = Orientation.Horizontal;
                CheckBox unbind = new CheckBox();
                unbind.VerticalAlignment = VerticalAlignment.Center;
                unbind.Margin = new Thickness(0, 0, 0, -4);
                unbind.ToolTip = "Unbind chat from main server";
                unbind.Checked += Unbind_Checked;
                curentCloud.Children.Add(new TextBlock() { Margin = new Thickness(5, 0, 5, 0), Text = cloud.Name ?? cloud.Address });
                curentCloud.Children.Add(unbind);
                grd.Children.Add(curentCloud);
            }
            else
            {
                grd.Children.Add(new TextBlock() { Margin = new Thickness(5, 0, 5, 0), Text = cloud.Name ?? cloud.Address });
            }
        }

        private void Unbind_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox unbindChk = (CheckBox)sender;
            unbindChk.IsEnabled = false;
        }

        void ActiveWaitCloud(Grid grid)
        {
            foreach (var item in grid.Children)
            {
                if (item is ProgressBar progressBar)
                {
                    grid.Children.Remove(progressBar);
                    return;
                }
            }
        }

        void RemoveWaitCloud(Grid grid, string trnID)
        {
            NameOfCloud.Items.Remove(grid);
            lock (sync)
            {
                listBoxClouds.Remove(trnID);
            }
        }

        void IMainContractCallback.CreateCloudResult(CloudResult result, string transactionID)
        {
            InnerWaiterCloud createCloud;
            lock (sync)
            {
                bool isWaiting = listBoxClouds.TryGetValue(transactionID, out createCloud);

                if (!isWaiting)
                    return;
            }

            try
            {
                switch (result)
                {
                    case CloudResult.SUCCESS:
                        currentServer.CreateCloud(createCloud.User, createCloud.Cloud, transactionID);
                        ActiveWaitCloud(createCloud.Grid);
                        break;
                    case CloudResult.CloudIsBusy:
                        Informing($"Cloud '{createCloud.Cloud?.Name}' is busy! Choose another name.");
                        RemoveWaitCloud(createCloud.Grid, transactionID);
                        break;
                    case CloudResult.CloudNotFound:
                    case CloudResult.FAILURE:
                        Informing($"Unknown MainServer-Error when create Cloud '{createCloud.Cloud?.Name}'.");
                        RemoveWaitCloud(createCloud.Grid, transactionID);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Informing($"Exception '{ex}' when create Cloud '{createCloud.Cloud?.Name}'.");
                RemoveWaitCloud(createCloud.Grid, transactionID);
            }
        }

        private void Auth_IsConnect(User user, Cloud cloud)
        {
            string trnID = Guid.NewGuid().ToString("D");
            Grid newItemGrid;
            lock (sync)
            {
                newItemGrid = AddWaitCloudToListbox(cloud, false);
                listBoxClouds.Add(trnID, new InnerWaiterCloud(user, cloud, newItemGrid));
            }

            if (mainProxy == null)
            {
                Task.Run(() =>
                {
                    try
                    {
                        currentClient.JoinToCloud(user, cloud);
                        return string.Empty;
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                }).ContinueWith((antecedent) =>
                {
                    if (antecedent.Result.IsNullOrEmpty())
                    {
                        ActiveWaitCloud(newItemGrid);
                    }
                    else
                    {
                        RemoveWaitCloud(newItemGrid, trnID);
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                mainProxy.GetCloudAsync(user, trnID);
            }
        }

        void IMainContractCallback.GetCloudResult(ServerResult result, Cloud cloud, string transactionID)
        {
            InnerWaiterCloud createCloud;
            lock (sync)
            {
                bool isWaiting = listBoxClouds.TryGetValue(transactionID, out createCloud);

                if (!isWaiting)
                    return;
            }

            try
            {
                switch (result)
                {
                    case ServerResult.AccessGranted:
                    case ServerResult.SUCCESS:
                        currentClient.JoinToCloud(createCloud.User, cloud);
                        ActiveWaitCloud(createCloud.Grid);
                        break;
                    case ServerResult.CloudNotFound:
                        Informing($"Cloud '{createCloud.User?.CloudName}' not found on Mainserver!");
                        RemoveWaitCloud(createCloud.Grid, transactionID);
                        break;
                    case ServerResult.NameIsBusy:
                        Informing($"Nick name '{createCloud.User?.Name}' in Cloud '{createCloud.User?.CloudName}' is busy! Try again.");
                        RemoveWaitCloud(createCloud.Grid, transactionID);
                        break;
                    case ServerResult.AccessDenied:
                        Informing($"Access is denied for access to the Cloud '{createCloud.User?.CloudName}'.");
                        RemoveWaitCloud(createCloud.Grid, transactionID);
                        break;
                    case ServerResult.AwaitConfirmation:
                    case ServerResult.YourRequestInProgress:
                        Informing($"Your request for access to the Cloud '{createCloud.User?.CloudName}' in progress.");
                        RemoveWaitCloud(createCloud.Grid, transactionID);
                        return;
                    case ServerResult.FAILURE:
                        Informing($"MainServer-error when get Cloud '{createCloud.User?.CloudName}'!");
                        RemoveWaitCloud(createCloud.Grid, transactionID);
                        break;
                    default:
                        Informing($"Result:{result}!");
                        RemoveWaitCloud(createCloud.Grid, transactionID);
                        break;
                }
            }
            catch (Exception ex)
            {
                Informing($"Exception '{ex}' when get Cloud '{createCloud.User?.CloudName}'.");
                RemoveWaitCloud(createCloud.Grid, transactionID);
            }
        }

        private void RemoveCloud_OnClick(object sender, RoutedEventArgs e)
        {
            object selectedItem = NameOfCloud.SelectedItem;
            var selectedListBoxItem = NameOfCloud.ItemContainerGenerator.ContainerFromItem(selectedItem);
            lock (sync)
            {
                foreach (KeyValuePair<string, InnerWaiterCloud> item in listBoxClouds)
                {
                    if (item.Value.Grid.Equals(selectedListBoxItem))
                    {
                        listBoxClouds.Remove(item.Key);
                        break;
                    }
                }
            }
            NameOfCloud.Items.Remove(selectedListBoxItem);
        }

        public void Informing(string msg, bool isError = true)
        {
            Dispatcher?.Invoke(() =>
            {
                WindowInfo windowError = new WindowInfo(isError ? "Error" : "Warning", msg);
                windowError.Owner = this;
                windowError.Focus();
                windowError.ShowDialog();
            });
        }
    }
}