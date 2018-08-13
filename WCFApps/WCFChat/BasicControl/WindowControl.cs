using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using UIControls.MainControl;
using UIControls.Utils;
using Utils;
using WCFChat.Service;
using Timer = System.Timers.Timer;

namespace WCFChat.Client.BasicControl
{
    class WindowControl
    {
        internal static readonly Brush AdminBackground = (Brush)new BrushConverter().ConvertFrom("#FF009EAE"); // обычный фон ячейки админа 
        internal static readonly Brush UserBackground = (Brush)new BrushConverter().ConvertFrom("#FF555555"); // обычный фон ячейки юзера
        internal static readonly Brush UserWritingBackground = (Brush)new BrushConverter().ConvertFrom("#FF07C1C1"); // когда какой то юзер начал что то писать
        internal static readonly Brush UserIsWaiterStatus = (Brush)new BrushConverter().ConvertFrom("#FF555555");
        internal static readonly Brush UserIsNotLogOnYetStatus = (Brush)new BrushConverter().ConvertFrom("#FFFFDC00"); // ждем его непосредственного коннекта Желтый цвет
        internal static readonly Brush UserIsActiveStatus = (Brush)new BrushConverter().ConvertFrom("#FF90EE90"); // когда юзер непосредственно подсоединился к чату Зеленый цвет

        
        public event EventHandler Unbind;
        public bool IsUnbinded { get; private set; } = false;
        private bool isAdmin = false;

        //private MainWindow window;

        private ListBox Users;
        private RichTextBox DialogHistory;
        private RichTextBox DialogWindow;
        private UIWindow mainWindow;

        internal Dictionary<string, UserBindings> AllUsers { get; } = new Dictionary<string, UserBindings>(StringComparer.CurrentCulture);

        public string TransactionID { get; private set; }
        public UserBindings Initiator { get; private set; }
        public Cloud CurrentCloud { get; private set; }

        internal List<Message> Messages()
        {
            List<Message> allMesages = new List<Message>();
            foreach (var block in DialogHistory.Document.Blocks)
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
            DialogHistory = new RichTextBox();
            DialogWindow = new RichTextBox();
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

        public virtual void IncomingRequestForAccess(User user, string address)
        {

        }

        protected void AddWaiter(User newUser, IChatCallback callback, string address, string port)
        {
            UserBindings userBind = new UserBindings(newUser, callback, address, port);
            AddUser(userBind, $"{address}:{port}");
        }

        protected void RemoveUser(UserBindings userBind)
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

            bool isExist = AllUsers.TryGetValue(guid, out var userBind);
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

        void AddUser(UserBindings user, string address, string gridName, string textblockUser, string textblockStatus, string borderName)
        {
            ListBoxItem newUserItem = new ListBoxItem();
            newUserItem.Style = (Style)mainWindow.FindResource("ListBoxItemUser");
            Grid cloneExist = UICustomCommands.XamlClone((Grid)mainWindow.FindResource(gridName));
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
                        //result.Text = user.Name;
                        //result.ToolTip = user.GUID;
                    }
                    else if (resultText.Name == textblockStatus)
                    {
                        userStatus = resultText;
                        //result.ToolTip = address;
                    }
                }
                else if (item is Border resultBorder)
                {
                    userBackgound = resultBorder;
                    userBackgound.Background = user.Status == UserStatus.Admin ? AdminBackground : UserBackground;
                }
                else if (item is Button resultButton)
                {
                    resultButton.Click += ButtonAccessRejectClick;
                }
            }

            newUserItem.Content = cloneExist;
            user.AddUIControl(newUserItem, userBackgound, userName, userStatus);

            Users.Items.Add(newUserItem);
            AllUsers.Add(user.GUID.Value, user);
        }


        object sync = new object();
        private void ButtonAccessRejectClick(object sender, RoutedEventArgs e)
        {
            lock (sync)
            {
                Button button = (Button) sender;
                Grid parentGrid = (Grid) button.Parent;
                TextBlock userName = parentGrid.Children.OfType<TextBlock>().FirstOrDefault(p => p.Name == "UserNameAdmin");

                if (userName == null)
                    return;

                bool isExist = AllUsers.TryGetValue(userName.ToolTip.ToString(), out UserBindings userBind);

                if (button.Name.Equals("AcceptAdmin"))
                {
                    if (isExist)
                        userBind.UIControls.Status.Foreground = UserIsNotLogOnYetStatus; // ждем его непосредственного коннекта Желтый цвет
                    AccessOrRemoveUser(userBind, false);
                }
                else
                {
                    AccessOrRemoveUser(userBind, true);
                }
            }
        }

        protected virtual void AccessOrRemoveUser(UserBindings userBind, bool isRemove)
        {
            if (isRemove)
            {
                RemoveUser(userBind);
            }
        }

        protected void ChangeUserStatusIsActive(UserBindings user)
        {
            user.Status = UserStatus.User;
            user.UIControls.Status.Foreground = UserIsActiveStatus; // когда юзер непосредственно подсоединился к чату Зеленый цвет
        }

        protected void ChangeUserName(UserBindings userBind, User userNewName)
        {
            userBind.Name.Value = userNewName.Name;
        }

        protected void UpdateUserList(List<User> allUsers)
        {
            if (allUsers == null)
                return;

            foreach (User user in allUsers)
            {
                UserBindings userBind;
                bool isExist = AllUsers.TryGetValue(user.GUID, out userBind);
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
                List<string> forRemove = new List<string>();
                foreach (KeyValuePair<string, UserBindings> userBind in AllUsers)
                {
                    if (allUsers.All(p => p.GUID != userBind.Key))
                    {
                        forRemove.Add(userBind.Key);
                    }
                }
                foreach (string guid in forRemove)
                {
                    RemoveUser(guid);
                }
            }
        }

        protected void SomeoneUserIsWriting(UserBindings userBind, bool isWriting)
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
            public List<Message> Messages { get; } = new List<Message>();

            internal MyParagraph(Message msg)
            {
                LineHeight = 1;
                GUID = msg.Sender.GUID;
                AddMessage(msg, true);
            }

            public void AddMessage(Message msg, bool newParagraph = false)
            {
                Messages.Add(msg);
                if (newParagraph)
                {
                    Run userName = new Run($"{msg.Sender.Name:G}:");
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

        protected void SomeoneUserReceveMessage(Message msg)
        {
            if (DialogHistory.Document.Blocks.LastBlock is MyParagraph exist)
            {
                if (exist.GUID == msg.Sender.GUID)
                {
                    exist.AddMessage(msg);
                    return;
                }
            }

            MyParagraph par = new MyParagraph(msg);
            DialogHistory.Document.Blocks.Add(par);
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


        protected virtual void CurrentUserIsWriting(bool isWriting)
        {
            
        }

        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            string richText = new TextRange(DialogWindow.Document.ContentStart, DialogWindow.Document.ContentEnd).Text;
            CurrentUserIsSaying(richText);
            DialogWindow.Document.Blocks.Clear();
        }

        protected virtual void CurrentUserIsSaying(string msg)
        {

        }

        protected virtual bool GetUserBinding(User user, out UserBindings userBind)
        {
            userBind = null;
            if (user == null || string.IsNullOrEmpty(user.GUID) || string.IsNullOrEmpty(user.Name))
                return false;
            
            return AllUsers.TryGetValue(user.GUID, out userBind);
        }
    }
}
