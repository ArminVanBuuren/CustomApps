using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Utils.UIControls.Tools;
using Utils;
using WCFChat.Client.ServiceReference1;

namespace WCFChat.Client.BasicControl
{
    internal enum UserStatus
    {
        Admin = 0,
        User = 1,
        Waiter = 2
    }


    internal class UserBindings
    {
        internal UserBindings(User user, bool isAdmin = false)
        {
            InitBase(user);
            if (isAdmin)
                Status = UserStatus.Admin;
            else
                Status = UserStatus.User;
            CallBack = null;
            Address = new UIPropertyValue<string>
            {
                Value = string.Empty
            };
            Port = string.Empty;
        }

        internal UserBindings(User user, IChatServiceCallback callback, string address, string port)
        {
            InitBase(user);
            CallBack = callback;
            Address = new UIPropertyValue<string>
            {
                Value = address
            };
            Port = port;
            Status = UserStatus.Waiter;
        }

        void InitBase(User user)
        {
            GUID = new UIPropertyValue<string>
            {
                Value = user.GUID
            };
            Name = new UIPropertyValue<string>
            {
                Value = user.Name
            };
            User = user;
        }

        public class UserControlUI
        {
            public ListBoxItem ParentListBox { get; }
            public Border Background { get; }
            public TextBlock Name { get; }
            public TextBlock Status { get; }

            internal UserControlUI(ListBoxItem parent, Border userBackground, TextBlock userName, TextBlock userStatus)
            {
                ParentListBox = parent;
                Background = userBackground;
                Name = userName;
                Status = userStatus;
            }
        }

        public void AddUIControl(ListBoxItem parent, Border userBackground, TextBlock userName, TextBlock userStatus)
        {
            UIControls = new UserControlUI(parent, userBackground, userName, userStatus);
            UICustomCommands.DefaultBinding(userName, TextBlock.TextProperty, Name);
            UICustomCommands.DefaultBinding(userName, FrameworkElement.ToolTipProperty, GUID);
            if (!Address.Value.IsNullOrEmpty())
                UICustomCommands.DefaultBinding(userStatus, FrameworkElement.ToolTipProperty, Address);
        }

        public UIPropertyValue<string> GUID { get; private set; }
        public UIPropertyValue<string> Name { get; private set; }
        public User User { get; private set; }

        internal IChatServiceCallback CallBack { get; set; }
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

}
