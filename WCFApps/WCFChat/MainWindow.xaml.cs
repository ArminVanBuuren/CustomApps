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
using WCFChat.Service;
using Message = WCFChat.Service.Message;

namespace WCFChat.Client
{
    public delegate bool AccessOrRemoveUser(string guid, bool isRemove);
    public delegate void UserIsWriting(bool isWriting);
    public delegate void UserSaying(string message);
    public partial class MainWindow
    {
        public event AccessOrRemoveUser OnAccessOrRemoveUser;
        public event UserIsWriting OnUserWriting;
        public event UserSaying OnUserSay;
        private Dictionary<string, KeyValuePair<TextBlock, TextBlock>> users = new Dictionary<string, KeyValuePair<TextBlock, TextBlock>>();
        public MainWindow()
        {
            //simpleUser = WCFChat.Client.Properties.Resources.UserSimple;
            InitializeComponent();
        }

        public void AddCloud(string name)
        {
            NameOfCloud.Items.Add(new Label() {
                                                  Content = name
                                              });
        }

        /// <summary>
        /// Клонимрует уже существующие объекты, очень важная хрень!!!!!!!!! Потому что в ебанном wpf нельзя просто так клонировать объекты
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public T XamlClone<T>(T source)
        {
            string savedObject = System.Windows.Markup.XamlWriter.Save(source);

            // Load the XamlObject
            StringReader stringReader = new StringReader(savedObject);
            System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(stringReader);
            T target = (T)System.Windows.Markup.XamlReader.Load(xmlReader);

            return target;
        }

        public void AddUser(User user, string address)
        {
            ListBoxItem newUserItem = new ListBoxItem();
            newUserItem.Style = (Style)FindResource("ListBoxItemUser");
            Grid cloneExist = XamlClone((Grid)FindResource("UserTemplateSimple"));
            TextBlock userName = null;
            TextBlock userStatus = null;
            foreach (var item in cloneExist.Children)
            {
                if (item is TextBlock result)
                {
                    if (result.Name == "UserNameSimple")
                    {
                        userName = result;
                        result.Text = user.Name;
                        result.ToolTip = user.GUID;
                    }
                    else if (result.Name == "StatusSimple")
                    {
                        userStatus = result;
                        result.ToolTip = address;
                    }
                }
            }

            users.Add(user.GUID, new KeyValuePair<TextBlock, TextBlock>(userName, userStatus));

            newUserItem.Content = cloneExist;
            Users.Items.Add(newUserItem);
        }

        public void Admin_AddUser(User user, string address)
        {
            ListBoxItem newUserItem = new ListBoxItem();
            newUserItem.Style = (Style)FindResource("ListBoxItemUser");
            Grid cloneExist = XamlClone((Grid)FindResource("UserTemplateForAdmin"));
            TextBlock userName = null;
            TextBlock userStatus = null;

            foreach (var item in cloneExist.Children)
            {
                if (item is TextBlock result)
                {
                    if (result.Name == "UserNameAdmin")
                    {
                        userName = result;
                        result.Text = user.Name;
                        result.ToolTip = user.GUID;
                    }
                    else if (result.Name == "StatusAdmin")
                    {
                        userStatus = result;
                        result.ToolTip = address;
                    }
                }
                else if (item is Button resultBut)
                {
                    resultBut.Click += ButtonAccessRejectClick;
                }
            }

            users.Add(user.GUID, new KeyValuePair<TextBlock, TextBlock>(userName, userStatus));

            newUserItem.Content = cloneExist;
            Users.Items.Add(newUserItem);
        }
        private void ButtonAccessRejectClick(object sender, RoutedEventArgs e)
        {
            if(OnAccessOrRemoveUser == null)
                return;

            Button button = (Button)sender;
            Grid parentGrid = (Grid)button.Parent;
            TextBlock userName = parentGrid.Children.OfType<TextBlock>().FirstOrDefault(p => p.Name == "UserNameAdmin");

            if (button.Name.Equals("AcceptAdmin"))
            {
                TextBlock userStatus = parentGrid.Children.OfType<TextBlock>().FirstOrDefault(p => p.Name == "StatusAdmin");
               
                bool result = OnAccessOrRemoveUser.Invoke(userName.ToolTip.ToString(), true);
                if (result)
                    userStatus.Foreground = (Brush) new BrushConverter().ConvertFrom("#FF90EE90"); // уже подсоединился к чату
                else
                    userStatus.Foreground = (Brush)new BrushConverter().ConvertFrom("#FFFFDC00"); // ждем его непосредственного коннекта
            }
            else
            {
                users.Remove(userName.ToolTip.ToString());
                Users.Items.Remove((ListBoxItem)parentGrid.Parent);
                OnAccessOrRemoveUser.BeginInvoke(userName.ToolTip.ToString(), true, null, null);
            }
        }

        public void RemoveUser(User user)
        {
            foreach (var item in Users.Items)
            {
                ListBoxItem findedPanel = (ListBoxItem) item;
                Grid childGrid = (Grid)findedPanel.Content;
                foreach (var item2 in childGrid.Children)
                {
                    if (item2 is TextBlock result && (result.Name == "UserNameSimple" || result.Name == "UserNameAdmin") && result.Text.Equals(user.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (users.ContainsKey(user.GUID))
                            users.Remove(user.GUID);
                        Users.Items.Remove(findedPanel);
                        return;
                    }
                }
            }
        }

        public void ChangeUserName(User user)
        {
            KeyValuePair<TextBlock, TextBlock> userTextBlock;
            bool exist = users.TryGetValue(user.GUID, out userTextBlock);
            if (exist)
                userTextBlock.Key.Text = user.Name;
        }

        public void ChangeUserStatusIsActive(User user)
        {
            KeyValuePair<TextBlock, TextBlock> userTextBlock;
            bool exist = users.TryGetValue(user.GUID, out userTextBlock);
            if (exist)
            {
                userTextBlock.Value.Foreground = (Brush) new BrushConverter().ConvertFrom("#FF90EE90");
            }
        }



        public void IncomingIsWriting(User user, bool isWriting)
        {
            
        }

        public void IncomingReceve(Message msg)
        {
            
        }

    }
}