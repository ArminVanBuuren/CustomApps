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
    public interface IXWindow
    {
        bool CanFullWindowDragMove { get; set; }
        bool ShowInfoButton { get; set; }
        bool ShowMinButton { get; set; }
        bool ShowMaxButton { get; set; }
    }

    public class XWindow : Window, IXWindow
    {
        private bool _isBlured, _isBlured2;

        private bool _canDragMove = true;
        private bool _canFullWindowDragMove = false;
        private bool _showInfoButton = false;
        private bool _showMinButton = false;
        private bool _showMaxButton = false;


        public XWindow(Style style)
        {
            //base.Style = FindResource("XWindowStyle") as Style;

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            
            
            //Loaded += WindowLoaded;
            //Activated += WindowNew_Activated;
            //Closing += Window_Closing;
            //StateChanged += WindowStateChanged;
        }

        public new Style Style
        {
            get; private set;
        }

        void WindowLoaded(object sender, RoutedEventArgs e)
        {
            if (Style == null)
                return;

            ShowInfoButton = true;
            ShowMinButton = true;
            ShowMaxButton = true;
            CanFullWindowDragMove = true;
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

        private void DisableWindowMoving(object sender, EventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source?.AddHook(Win32Controls.WndProc);
        }

        /// <summary>
        /// чтобы можно было перемещать окно по нажатию клавиши в любой точки окна, а не только через верхнюю панель
        /// </summary>
        public bool CanFullWindowDragMove
        {
            get { return _canFullWindowDragMove; }
            set
            {
                if (_canFullWindowDragMove == value)
                    return;

                Grid LayoutRoot = (Grid)Template.FindName("LayoutRoot", this);

                if (LayoutRoot == null)
                    return;

                if (_canFullWindowDragMove)
                    LayoutRoot.MouseLeftButtonDown += ParentGrid_MouseLeftButtonDown;
                else
                    LayoutRoot.MouseLeftButtonDown -= ParentGrid_MouseLeftButtonDown;

                _canFullWindowDragMove = value;
            }
        }

        private void ParentGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs args)
        {
            //если менялся размер окна то не перемещаем окно
            DragMove();
            args.Handled = true;
        }

        public bool ShowInfoButton
        {
            get { return _showInfoButton; }
            set
            {
                if (_showInfoButton == value)
                    return;

                if (ChangePanelButtons("Information", value, this))
                    _showInfoButton = value;
            }
        }

        public bool ShowMinButton
        {
            get { return _showMinButton; }
            set
            {
                if (_showMinButton == value)
                    return;

                if (ChangePanelButtons("MinButton", value, this))
                    _showMinButton = value;
            }
        }

        public bool ShowMaxButton
        {
            get { return _showMaxButton; }
            set
            {
                if (_showMaxButton == value)
                    return;

                if (ChangePanelButtons("MaxButton", value, this))
                    _showMaxButton = value;
            }
        }

        static bool ChangePanelButtons(string name, bool value, XWindow window)
        {
            Button panelButton = (Button)window.Template.FindName(name, window);
            if (panelButton == null)
                return false;

            if (value)
                panelButton.Visibility = Visibility.Visible;
            else
                panelButton.Visibility = Visibility.Collapsed;

            return true;
        }
    }
}
