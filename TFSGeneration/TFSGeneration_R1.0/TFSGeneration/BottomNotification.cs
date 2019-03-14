using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Utils;

namespace TFSAssist
{
    public class BottomNotification : IDisposable
    {
        private System.Windows.Forms.NotifyIcon notification;
        private MainWindow _mainWindow;
        uint _countNotWatchedNotifications = 0;
        private string _header;
        
        public BottomNotification(MainWindow window, string header)
        {
            _mainWindow = window;
            _header = header;

            notification = new System.Windows.Forms.NotifyIcon
            {
                BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info,
                Icon = Properties.Resources.Rick,
                Visible = true
            };
            notification.BalloonTipClicked += _mainWindow.ShowMyForm;
            notification.DoubleClick += _mainWindow.ShowMyForm;
        }

        public void Clear()
        {
            _countNotWatchedNotifications = 0;
        }

        /// <summary>
        /// Показать сообщение в уведомлениях Windows
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        public void DisplayNotify(string title, string content)
        {
            if (_mainWindow.IsActive || title.IsNullOrEmpty() || content.IsNullOrEmpty() || _countNotWatchedNotifications != 0)
                return;

            notification.Text = _header;
            notification.Visible = true;
            notification.BalloonTipTitle = title;
            notification.BalloonTipText = content;
            notification.ShowBalloonTip(100);
            _countNotWatchedNotifications++;
        }

        public void Dispose()
        {
            notification.Dispose();
        }
    }
}
