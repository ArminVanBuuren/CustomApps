using System;
using Utils;

namespace TFSAssist
{
    public class BottomNotification : IDisposable
    {
        private readonly System.Windows.Forms.NotifyIcon notification;
        private readonly MainWindow _mainWindow;
        uint _countNotWatchedNotifications = 0;
        private readonly string _header;
        public bool isDisposed { get; private set; } = false;

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
            isDisposed = true;
        }
    }
}
