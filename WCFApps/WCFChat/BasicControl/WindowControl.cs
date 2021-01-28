using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Utils.UIControls.Main;
using Utils.UIControls.Tools;
using Utils;
using WCFChat.Client.ServiceReference1;
using Timer = System.Timers.Timer;

namespace WCFChat.Client.BasicControl
{
    public enum UserChanges
    {
        Access = 0,
        Remove = 1
    }

    internal delegate void UserCloudChanges(UserBindings userBind, WindowControl cloud, UserChanges changes);

    internal class WindowControl
    {
        internal static readonly Brush AdminBackground = (Brush)new BrushConverter().ConvertFrom("#FF009EAE"); // обычный фон ячейки админа 
        internal static readonly Brush UserBackground = (Brush)new BrushConverter().ConvertFrom("#FF555555"); // обычный фон ячейки юзера
        internal static readonly Brush UserWritingBackground = (Brush)new BrushConverter().ConvertFrom("#FF07C1C1"); // когда какой то юзер начал что то писать
        internal static readonly Brush UserIsWaiterStatus = (Brush)new BrushConverter().ConvertFrom("#FF555555");
        internal static readonly Brush UserIsNotLogOnYetStatus = (Brush)new BrushConverter().ConvertFrom("#FFFFDC00"); // ждем его непосредственного коннекта Желтый цвет
        internal static readonly Brush UserIsActiveStatus = (Brush)new BrushConverter().ConvertFrom("#FF90EE90"); // когда юзер непосредственно подсоединился к чату Зеленый цвет

        public event UserCloudChanges AdminChangedUserList;
        public bool IsUnbinded { get; private set; } = false;
        private bool isAdmin = false;

        //private MainWindow window;

        public ListBox Users;
        public FlowDocument DialogHistory;
        public FlowDocument DialogWindow;
        private UIWindow mainWindow;

        internal Dictionary<string, UserBindings> AllUsers { get; } = new Dictionary<string, UserBindings>(StringComparer.CurrentCulture);

        public string TransactionID { get; private set; }
        internal UserBindings Initiator { get; private set; }
        public Cloud CurrentCloud { get; private set; }

        internal List<ChatMessage> Messages()
        {
            var allMesages = new List<ChatMessage>();
            foreach (var block in DialogHistory.Blocks)
            {
                if (block is MyParagraph mypar)
                {
                    allMesages.AddRange(mypar.Messages);
                }
            }
            return allMesages;
        }

        private Timer _timerWritingUser;

        public WindowControl(UIWindow mainWindow, User initiator, Cloud cloud, string transactionId, bool isAdmin = false)
        {
            Users = new ListBox();
            DialogHistory = new FlowDocument();
            DialogWindow = new FlowDocument();
            this.mainWindow = mainWindow;

            TransactionID = transactionId;
            Initiator = new UserBindings(initiator, true);
            AddUser(Initiator, ".$my_localhost::");
            CurrentCloud = cloud;
            this.isAdmin = isAdmin;

            _timerWritingUser = new Timer();
            _timerWritingUser.Interval = 5000;
            _timerWritingUser.Elapsed += UserNotWriting;
            _timerWritingUser.AutoReset = false;
        }

        public bool IsUniqueNames(User newUser)
        {
            var currentNameAlreadyExist = AllUsers.Values.Any(p => p.User.Name.Equals(newUser.Name, StringComparison.CurrentCultureIgnoreCase));
            return !currentNameAlreadyExist;
        }

        public virtual void IncomingRequestForAccess(User user, string address)
        {

        }

        public void AddWaiter(User newUser, IChatServiceCallback callback, string address, string port)
        {
            var userBind = new UserBindings(newUser, callback, address, port);
            AddUser(userBind, $"{address}:{port}");
        }

        internal void RemoveUser(UserBindings userBind)
        {
            if (userBind == null)
                return;

            if (AllUsers.ContainsKey(userBind.GUID.Value))
            {
                AllUsers.Remove(userBind.GUID.Value);
                Users.Items.Remove(userBind.UIControls.ParentListBox);
            }
        }

        protected void RemoveUser(User user)
        {
            if (user == null)
                return;

            RemoveUser(user.GUID);
        }

        protected void RemoveUser(string guid)
        {
            if (guid == null)
                return;

            var isExist = AllUsers.TryGetValue(guid, out var userBind);
            if (isExist)
            {
                AllUsers.Remove(userBind.GUID.Value);
                Users.Items.Remove(userBind.UIControls.ParentListBox);
            }
        }


        internal void AddUser(UserBindings userBind, string address)
        {
            if (isAdmin && userBind.Status != UserStatus.Admin)
            {
                AddUser(userBind, address, "UserTemplateForAdmin", "UserNameAdmin", "StatusAdmin", "BackgroundAdmin");
            }
            else
            {
                AddUser(userBind, address, "UserTemplateSimple", "UserNameSimple", "StatusSimple", "BackgroundSimple");
            }
        }

        void AddUser(UserBindings userBind, string address, string gridName, string textblockUser, string textblockStatus, string borderName)
        {
            var newUserItem = new ListBoxItem();
            newUserItem.Style = (Style)mainWindow.FindResource("ListBoxItemUser");
            var cloneExist = UICustomCommands.XamlClone((Grid)mainWindow.FindResource(gridName));
            TextBlock userName = null;
            TextBlock userStatus = null;
            Border userBackgound = null;

            foreach (var item in cloneExist.Children)
            {
                if (item is TextBlock resultText)
                {
                    if (resultText.Name == textblockUser)
                    {
                        userName = resultText;
                    }
                    else if (resultText.Name == textblockStatus)
                    {
                        userStatus = resultText;
                    }
                }
                else if (item is Border resultBorder)
                {
                    userBackgound = resultBorder;
                    userBackgound.Background = userBind.Status == UserStatus.Admin ? AdminBackground : UserBackground;
                }
                else if (item is Button resultButton)
                {
                    resultButton.Click += ButtonAccessRejectClick;
                }
            }

            newUserItem.Content = cloneExist;
            userBind.AddUIControl(newUserItem, userBackgound, userName, userStatus);

            Users.Items.Add(newUserItem);
            AllUsers.Add(userBind.GUID.Value, userBind);
        }


        object sync = new object();
        private void ButtonAccessRejectClick(object sender, RoutedEventArgs e)
        {
            lock (sync)
            {
                var button = (Button)sender;
                var parentGrid = (Grid)button.Parent;
                var userName = parentGrid.Children.OfType<TextBlock>().FirstOrDefault(p => p.Name == "UserNameAdmin");

                if (userName == null)
                    return;

                var isExist = AllUsers.TryGetValue(userName.ToolTip.ToString(), out var userBind);

                if (button.Name.Equals("AcceptAdmin"))
                {
                    if (isExist)
                        userBind.UIControls.Status.Foreground = UserIsNotLogOnYetStatus; // ждем его непосредственного коннекта Желтый цвет
                    AdminChangedUserList?.Invoke(userBind, this, UserChanges.Access);
                }
                else
                {
                    AdminChangedUserList?.Invoke(userBind, this, UserChanges.Remove);
                    RemoveUser(userBind);
                }
            }
        }

        

        internal void ChangeUserStatusIsActive(UserBindings user)
        {
            user.Status = UserStatus.User;
            user.UIControls.Status.Foreground = UserIsActiveStatus; // когда юзер непосредственно подсоединился к чату Зеленый цвет
        }

        internal void ChangeUserName(UserBindings userBind, User userNewName)
        {
            userBind.Name.Value = userNewName.Name;
        }

        protected void UpdateUserList(List<User> allUsers)
        {
            if (allUsers == null)
                return;

            foreach (var user in allUsers)
            {
                UserBindings userBind;
                var isExist = AllUsers.TryGetValue(user.GUID, out userBind);
                if (isExist)
                {
                    if (!userBind.Name.Value.Equals(user.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        userBind.Name.Value = user.Name;
                        continue;
                    }
                    continue;
                }
                else
                {
                    AddUser(new UserBindings(user), string.Empty);
                }
            }

            if (allUsers.Count != AllUsers.Count)
            {
                var forRemove = new List<string>();
                foreach (var userBind in AllUsers)
                {
                    if (allUsers.All(p => p.GUID != userBind.Key))
                    {
                        forRemove.Add(userBind.Key);
                    }
                }
                foreach (var guid in forRemove)
                {
                    RemoveUser(guid);
                }
            }
        }

        internal void SomeoneUserIsWriting(UserBindings userBind, bool isWriting)
        {
            if (isWriting)
            {
                userBind.UIControls.Background.Background = UserWritingBackground; // когда какой то юзер начал что то писать
            }
            else
            {
                userBind.UIControls.Background.Background = userBind.Status == UserStatus.Admin ? AdminBackground : UserBackground;
            }
        }

        internal class MyParagraph : Paragraph
        {
            public string GUID { get; }
            public List<ChatMessage> Messages { get; } = new List<ChatMessage>();

            internal MyParagraph(ChatMessage msg)
            {
                LineHeight = 1;
                GUID = msg.Sender.GUID;
                AddMessage(msg, true);
            }

            public void AddMessage(ChatMessage msg, bool newParagraph = false)
            {
                Messages.Add(msg);
                if (newParagraph)
                {
                    var userName = new Run($"{msg.Sender.Name:G}:");
                    userName.Foreground = Brushes.Aqua;
                    userName.Background = Brushes.Black;
                    //userName.ToolTip = msg.Time;
                    Inlines.Add(new Bold(userName));
                    Inlines.Add(" ");
                }
                else
                {
                    Inlines.Add(new LineBreak());
                }

                Inlines.Add($"{msg.Content.Trim()}");
                Inlines.Add($"[{msg.Time.ToString("T").Trim()}]");
            }
        }

        public void SomeoneUserReceveMessage(ChatMessage msg)
        {
            if (DialogHistory.Blocks.LastBlock is MyParagraph exist)
            {
                if (exist.GUID == msg.Sender.GUID)
                {
                    exist.AddMessage(msg);
                    return;
                }
            }

            var par = new MyParagraph(msg);
            DialogHistory.Blocks.Add(par);
        }

        private void DialogWindow_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_timerWritingUser.Enabled)
            {
                _timerWritingUser.Interval = 5000;
            }
            else
            {
                CurrentUserIsWriting(true);
                _timerWritingUser.Enabled = true;
            }
        }

        private void UserNotWriting(object sender, System.Timers.ElapsedEventArgs e)
        {
            CurrentUserIsWriting(false);
        }


        protected void CurrentUserIsWriting(bool isWriting)
        {
            lock (sync)
            {
                foreach (var existUser in AllUsers.Values)
                {
                    if (existUser.CallBack != null && ((System.ServiceModel.Channels.IChannel)existUser.CallBack).State == System.ServiceModel.CommunicationState.Opened)
                        existUser.CallBack.IsWritingCallback(Initiator.User, isWriting);
                }
            }
        }

        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            var richText = new TextRange(DialogWindow.ContentStart, DialogWindow.ContentEnd).Text;
            CurrentUserIsSaying(richText);
            DialogWindow.Blocks.Clear();
        }

        protected void CurrentUserIsSaying(string msg)
        {
            lock (sync)
            {
                var adminSay = new ChatMessage()
                {
                    Sender = Initiator.User,
                    Content = msg,
                    Time = DateTime.Now
                };

                SomeoneUserReceveMessage(adminSay);

                foreach (var existUser in AllUsers.Values)
                {
                    if (existUser.CallBack != null && ((System.ServiceModel.Channels.IChannel)existUser.CallBack).State == System.ServiceModel.CommunicationState.Opened)
                        existUser.CallBack.Receive(adminSay);
                }
            }
        }

        internal virtual bool GetUserBinding(User user, out UserBindings userBind)
        {
            userBind = null;
            if (user == null || string.IsNullOrEmpty(user.GUID) || string.IsNullOrEmpty(user.Name))
                return false;

            return AllUsers.TryGetValue(user.GUID, out userBind);
        }
    }
}
