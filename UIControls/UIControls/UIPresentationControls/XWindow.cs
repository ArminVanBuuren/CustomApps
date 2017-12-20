using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UIPresentationControls.Utils;

namespace UIPresentationControls
{
    public class XWindow : Window
    {
        private bool _isBlured, _isBlured2;

        private bool _canDragMove = false;

        public bool PanelItemsIsVisible { get; } = true;

        public XWindow ()
        {
            Style = FindResource("XWindowStyle") as Style;

            CanDragMove = true;

            PanelItemsIsVisible = panelItemIsVisible;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;


            Loaded += WindowLoaded;
            Activated += WindowNew_Activated;
            Closing += Window_Closing;
            StateChanged += WindowStateChanged;

        }

        void WindowLoaded (object sender, RoutedEventArgs e)
        {
            OpacityActivate(this);

            if (Style == null)
                return;

            if (!PanelItemsIsVisible)
            {
                var infButtom = (Button)Template.FindName("Information", this);
                var minButtom = (Button)Template.FindName("MinButton", this);
                var maxButtom = (Button)Template.FindName("MaxButton", this);
                infButtom.Visibility = Visibility.Collapsed;
                minButtom.Visibility = Visibility.Collapsed;
                maxButtom.Visibility = Visibility.Collapsed;
            }

        }

        static void OpacityActivate (XWindow mainWindow)
        {
            if (mainWindow.Style == null)
                return;

            //эффект перехода из прозрачного в нормальный режим и из размытого в четкий
            Storyboard sb = mainWindow.FindResource("HoverOn") as Storyboard;
            if (sb == null)
                return;
            Border containerBorder = (Border)mainWindow.Template.FindName("PART_Container", mainWindow);
            BlurEffect containerBorder2 = (BlurEffect)mainWindow.Template.FindName("MyBlurEffect", mainWindow);
            containerBorder.Opacity = 0;
            containerBorder2.Radius = 15;
            sb.Begin(containerBorder);
        }

        /// <summary>
        /// Активировать или отключить перемещение окна (по умолчание перемещение можно делать)
        /// </summary>
        public bool CanDragMove
        {
            get { return _canDragMove; }
            set
            {
                if (_canDragMove == value)
                    return;

                if (!_canDragMove)
                    SourceInitialized += DisableWindowMoving;
                else
                    SourceInitialized -= DisableWindowMoving;

                _canDragMove = value;
            }
        }

        private void DisableWindowMoving (object sender, EventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source?.AddHook(Win32Controls.WndProc);
        }
    }
}
