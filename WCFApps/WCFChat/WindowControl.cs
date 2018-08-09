using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using UIControls.Utils;
using WCFChat.Service;
using Timer = System.Timers.Timer;

namespace WCFChat.Client
{
    internal enum UserStatus
    {
        Admin = 0,
        User = 1,
        Waiter = 2
    }
    internal class UserControlUI
    {
        public TextBlock Name { get; }
        public TextBlock Status { get; }
        public ListBoxItem ParentListBox { get; }

        public UserControlUI(ListBoxItem parent, TextBlock userName, TextBlock userStatus)
        {
            ParentListBox = parent;
            Name = userName;
            Status = userStatus;
        }
    }

    internal class UserBindings
    {
        internal const string GUID_ADMIN = "ADMIN";
        internal UserBindings(User user)
        {
            InitBase(user);
            if (user.GUID == GUID_ADMIN)
                Status = UserStatus.Admin;
            else
                Status = UserStatus.User;
            CallBack = null;
            Address = new UIPropertyValue<string> {
                                                      Value = string.Empty
                                                  };
            Port = string.Empty;
        }

        internal UserBindings(User user, IChatCallback callback, string address, string port)
        {
            InitBase(user);
            CallBack = callback;
            Address = new UIPropertyValue<string> {
                                                      Value = address
                                                  };
            Port = port;
            Status = UserStatus.Waiter;
        }

        void InitBase(User user)
        {
            GUID = new UIPropertyValue<string> {
                                                   Value = user.GUID
                                               };
            Name = new UIPropertyValue<string> {
                                                   Value = user.Name
                                               };
            User = user;
        }

        public void AddUIControl(ListBoxItem parent, TextBlock userName, TextBlock userStatus)
        {
            UIControls = new UserControlUI(parent, userName, userStatus);
            UICustomCommands.DefaultBinding(userName, TextBlock.TextProperty, Name);
            UICustomCommands.DefaultBinding(userName, FrameworkElement.ToolTipProperty, Name);
            UICustomCommands.DefaultBinding(userStatus, FrameworkElement.ToolTipProperty, Address);
        }

        public UIPropertyValue<string> GUID { get; private set; }
        public UIPropertyValue<string> Name { get; private set; }
        public User User { get; private set; }

        internal IChatCallback CallBack { get; set; }
        public UIPropertyValue<string> Address { get; }
        public string Port { get; }
        internal UserControlUI UIControls { get; private set; }
        public UserStatus Status { get; internal set; }

        //public override bool Equals(object obj)
        //{
        //    if (!(obj is User))
        //        return false;
        //    User input = (User) obj;
        //    if (input.GUID == base.GUID)
        //        return true;
        //    return false;
        //}

        //protected bool Equals(UserBindings other)
        //{
        //    return Equals(CallBack, other.CallBack) && string.Equals(Address, other.Address) && string.Equals(Port, other.Port) && Equals(UIControls, other.UIControls);
        //}

        //public override int GetHashCode()
        //{
        //    unchecked
        //    {
        //        var hashCode = (CallBack != null ? CallBack.GetHashCode() : 0);
        //        hashCode = (hashCode * 397) ^ (Address != null ? Address.GetHashCode() : 0);
        //        hashCode = (hashCode * 397) ^ (Port != null ? Port.GetHashCode() : 0);
        //        hashCode = (hashCode * 397) ^ (UIControls != null ? UIControls.GetHashCode() : 0);
        //        return hashCode;
        //    }
        //}
    }
    
    class WindowControl
    {
        protected object sync = new object();
        public event EventHandler Unbind;
        public event EventHandler Closing;
        public bool OnClose { get; private set; } = false;
        public bool IsUnbinded { get; private set; } = false;

        private bool isAdmin = false;
        private MainWindow window;
        internal Dictionary<string, UserBindings> AllUsers { get; } = new Dictionary<string, UserBindings>(StringComparer.CurrentCulture);
        internal List<Message> Messages { get; } = new List<Message>();
        private Timer _timerActivateWindow;

        public WindowControl(bool isAdmin = false)
        {
            window = new MainWindow();
            window.Closing += Window_Closing;
            window.DialogWindow.TextChanged += DialogWindow_TextChanged;
            window.SendMessage.Click += SendMessage_Click;
            this.isAdmin = isAdmin;

            _timerActivateWindow = new Timer();
            _timerActivateWindow.Interval = 5000;
            _timerActivateWindow.Elapsed += UserNotWriting;
            _timerActivateWindow.AutoReset = false;
            _timerActivateWindow.Enabled = false;
        }



        public void Close()
        {
            if (!OnClose)
                window?.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            OnClose = true;
            Closing?.Invoke(this, EventArgs.Empty);
        }

        public void UnbindCurrentServerFromMain()
        {
            IsUnbinded = true;
            Unbind?.Invoke(this, EventArgs.Empty);
        }

        public void AddCloud(string name)
        {
            window.NameOfCloud.Items.Add(new Label()
                                  {
                                      Content = name
                                  });
        }

        public void AddWaiter(User newUser, IChatCallback callback, string address, string port)
        {
            UserBindings userBind = new UserBindings(newUser, callback, address, port);
            AddUser(userBind, $"{address}:{port}");
        }

        public void RemoveUser(UserBindings userBind)
        {
            if (userBind == null)
                return;

            if (AllUsers.ContainsKey(userBind.GUID.Value))
            {
                AllUsers.Remove(userBind.GUID.Value);
                window.Users.Items.Remove(userBind.UIControls.ParentListBox);
            }
        }

        public void RemoveUser(User user)
        {
            if (user == null)
                return;

            RemoveUser(user.GUID);
        }

        public void RemoveUser(string guid)
        {
            if (guid == null)
                return;

            bool isExist = AllUsers.TryGetValue(guid, out var userBind);
            if (isExist)
            {
                AllUsers.Remove(userBind.GUID.Value);
                window.Users.Items.Remove(userBind.UIControls.ParentListBox);
            }
        }


        internal void AddUser(UserBindings userBind, string address)
        {
            if (isAdmin)
            {
                AddUser(userBind, address, "UserTemplateForAdmin", "UserNameAdmin", "StatusAdmin");
            }
            else
            {
                AddUser(userBind, address, "UserTemplateSimple", "UserNameSimple", "StatusSimple");
            }
        }

        void AddUser(UserBindings user, string address, string gridName, string textblockUser, string textblockStatus)
        {
            ListBoxItem newUserItem = new ListBoxItem();
            newUserItem.Style = (Style)window.FindResource("ListBoxItemUser");
            Grid cloneExist = UICustomCommands.XamlClone((Grid)window.FindResource(gridName));
            TextBlock userName = null;
            TextBlock userStatus = null;

            foreach (var item in cloneExist.Children)
            {
                if (item is TextBlock result)
                {
                    if (result.Name == textblockUser)
                    {
                        userName = result;
                        //result.Text = user.Name;
                        //result.ToolTip = user.GUID;
                    }
                    else if (result.Name == textblockStatus)
                    {
                        userStatus = result;
                        //result.ToolTip = address;
                    }
                }
                else if (item is Button resultBut)
                {
                    resultBut.Click += ButtonAccessRejectClick;
                }
            }

            newUserItem.Content = cloneExist;
            user.AddUIControl(newUserItem, userName, userStatus);

            window.Users.Items.Add(newUserItem);
            AllUsers.Add(user.GUID.Value, user);
        }



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
                        userBind.UIControls.Status.Foreground = (Brush) new BrushConverter().ConvertFrom("#FFFFDC00"); // ждем его непосредственного коннекта Желтый цвет
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

        public void ChangeUserStatusIsActive(UserBindings user)
        {
            user.Status = UserStatus.User;
            user.UIControls.Status.Foreground = (Brush)new BrushConverter().ConvertFrom("#FF90EE90"); // когда юзер непосредственно подсоединился к чату Зеленый цвет
        }

        public void ChangeUserName(User user)
        {
            UserBindings userBind;
            bool exist = AllUsers.TryGetValue(user.GUID, out userBind);
            if (exist)
                userBind.Name.Value = user.Name;
        }

        public void UpdateUserList(List<User> allUsers)
        {
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

        protected void SomeoneUserIsWriting(User user, bool isWriting)
        {
            UserBindings userBind;
            bool isExist = AllUsers.TryGetValue(user.GUID, out userBind);
            if (isExist)
            {
                
            }
        }

        protected void SomeoneUserReceveMessage(Message msg)
        {
            Paragraph par = new Paragraph();

            Run userName = new Run($"[{msg.Sender:G}]:");
            userName.Name = msg.Sender.GUID;
            userName.Foreground = Brushes.Aqua;
            userName.Background = Brushes.Black;
            userName.ToolTip = msg.Time;

            par.Inlines.Add(new Bold(userName));
            par.Inlines.Add(msg.Content.Trim());
        }

       

        private void DialogWindow_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_timerActivateWindow.Enabled)
            {
                _timerActivateWindow.Interval = 5000;
            }
            else
            {
                CurrentUserIsWriting(true);
                _timerActivateWindow.Enabled = true;
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
            string richText = new TextRange(window.DialogWindow.Document.ContentStart, window.DialogWindow.Document.ContentEnd).Text;
            CurrentUserIsSaying(richText);
            window.DialogWindow.Document.Blocks.Clear();
        }

        protected virtual void CurrentUserIsSaying(string msg)
        {

        }
    }
}
