using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Utils.UIControls.Tools;

namespace Utils.UIControls.Main
{
    public class UIWindow : Window
    {
        private bool _canDragMove = true;
        private bool _visibleResizeMode = true;
        private bool _isBlured, _isBlured2;

        public bool CanDragMove
        {
            get => _canDragMove;
            set
            {
                if(_canDragMove == value)
                    return;

                _canDragMove = value;
                if (!_canDragMove)
                    SourceInitialized += DisableUIWindowMoving;
                else
                    SourceInitialized -= DisableUIWindowMoving;
            }
        }

        public bool PanelItemsIsVisible { get; private set; } = true;

        public new string Title
        {
            get => base.Title;
            set
            {
                base.Title = value;
                CheckTitle(value);
            }
        }

        public string PresenterTitleContent { get; protected set; }

        public Button Information { get; private set; }
        public Button MinButton { get; private set; }
        public Button MaxButton { get; private set; }
        public Button CloseButton { get; private set; }

        public bool VisibleResizeMode
        {
            get => _visibleResizeMode;
            set
            {
                _visibleResizeMode = value;
                if (this.Style == null)
                    return;

                SwitchResizeMode(this.Template.FindName("Left", this), _visibleResizeMode ? Cursors.SizeWE : null);
                SwitchResizeMode(this.Template.FindName("Right", this), _visibleResizeMode ? Cursors.SizeWE : null);
                SwitchResizeMode(this.Template.FindName("Bottom", this), _visibleResizeMode ? Cursors.SizeNS : null);
                SwitchResizeMode(this.Template.FindName("rectSizeNorthWest", this), _visibleResizeMode ? Cursors.SizeNESW : null);
                SwitchResizeMode(this.Template.FindName("rectSizeNorthEast", this), _visibleResizeMode ? Cursors.SizeNWSE : null);
                SwitchResizeMode(this.Template.FindName("rectSizeSouthWest", this), _visibleResizeMode ? Cursors.SizeNESW : null);
                SwitchResizeMode(this.Template.FindName("rectSizeSouthEast", this), _visibleResizeMode ? Cursors.SizeNWSE : null);
            }
        }

        public UIWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Loaded += UIWindowLoaded;
            Activated += UIWindowActivated;
            Closing += UIWindowClosing;
            StateChanged += UIWindowStateChanged;
        }

        void UIWindowLoaded(object sender, RoutedEventArgs e)
        {
            Information = this.Style != null ? (Button)this.Template.FindName("Information", this) : null;
            MinButton = this.Style != null ? (Button)this.Template.FindName("MinButton", this) : null;
            MaxButton = this.Style != null ? (Button)this.Template.FindName("MaxButton", this) : null;
            CloseButton = this.Style != null ? (Button)this.Template.FindName("CloseButton", this) : null;

            CheckTitle(Title);
            OpacityActivate(this);
        }

        private void UIWindowActivated(object sender, EventArgs e)
        {

        }

        private void UIWindowClosing(object sender, CancelEventArgs e)
        {
            var mainWindow = (Window)sender;
            mainWindow.Closing -= UIWindowClosing;
            e.Cancel = true;
            var anim = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(0.2));
            //после завершения эффекта закрывает окно
            anim.Completed += (s, _) =>
            {
                mainWindow.Close(); // после завершения анимации закрываем приложение
            };
            mainWindow.BeginAnimation(UIElement.OpacityProperty, anim); // приводим Opacity к 0, т.е. к полной прозрачности, а затем закрываем 
        }

        void UIWindowStateChanged(object sender, EventArgs e)
        {
            var w = (Window)sender;

            if (w.Style == null)
                return;

            var handle = w.GetWindowHandle();
            var containerBorder = (Border)w.Template.FindName("PART_Container", w);
            var containerBorderRadius = (Border)w.Template.FindName("PART_Border", w);

            if (w.WindowState == WindowState.Maximized)
            {
                // Make sure window doesn't overlap with the taskbar.
                var screen = System.Windows.Forms.Screen.FromHandle(handle);
                //if (screen.Primary)
                {
                    containerBorder.Padding = new Thickness(
                        SystemParameters.WorkArea.Left + 7,
                        SystemParameters.WorkArea.Top + 7,
                        SystemParameters.PrimaryScreenWidth - SystemParameters.WorkArea.Right + 7,
                        SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Bottom + 7);
                    //containerBorderRadius.CornerRadius = new CornerRadius(0);
                }
            }
            else
            {
                containerBorder.Padding = new Thickness(20, 20, 20, 20);
                //containerBorderRadius.CornerRadius = new CornerRadius(6);
            }
        }


        void CheckTitle(string value)
        {
            var topBorderButt = Template.FindName("TopBorderTitButt", this);
            if (!(topBorderButt is Grid))
                return;

            var res = (Grid)topBorderButt;
            if (string.IsNullOrEmpty(value))
                res.Visibility = Visibility.Collapsed;
            else
                res.Visibility = Visibility.Visible;
        }

        private void DisableUIWindowMoving(object sender, EventArgs e)
        {
            var helper = new WindowInteropHelper(this);
            var source = HwndSource.FromHwnd(helper.Handle);
            source?.AddHook(UIControls32.WndProc);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                MaxWindowImage(this);
            else if (WindowState == WindowState.Minimized || WindowState == WindowState.Normal)
                MinWindowImage(this);
            base.OnStateChanged(e);
        }

        void MaxWindowImage(UIWindow window)
        {
            var imageIcon = (Image)window.Template.FindName("Icon", window);
            imageIcon.Margin = new Thickness(4, -2, 0, 2);
        }

        void MinWindowImage(UIWindow window)
        {
            var imageIcon = (Image)window.Template.FindName("Icon", window);
            imageIcon.Margin = new Thickness(4, -10, 0, 7);
        }

        private void UIWindowClosed(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        static void SwitchResizeMode(object frame, Cursor cursor)
        {
            if (frame is FrameworkElement element)
                element.Cursor = cursor;
        }

        static void OpacityActivate(UIWindow mainWindow)
        {
            //эффект перехода из прозрачного в нормальный режим и из размытого в четкий
            if (!(mainWindow.FindResource("HoverOn") is Storyboard sb))
                return;

            var containerBorder = (Border)mainWindow.Template.FindName("PART_Container", mainWindow);
            var containerBorder2 = (BlurEffect)mainWindow.Template.FindName("MyBlurEffect", mainWindow);
            containerBorder.Opacity = 0;
            containerBorder2.Radius = 15; // блурим
            sb.Begin(containerBorder); // выводит из блура в нормальный вид
        }

        void OpacityDeactivate()
        {

        }

        

        public void Blur()
        {
            Blur("IsDisabled");
        }

        internal void Blur(string storyBoardKey)
        {
            if (Style == null)
                return;

            if(!(FindResource(storyBoardKey) is Storyboard sb))
                return;

            var containerBorder = (Border)Template.FindName("PART_Container", this);
            sb.Begin(containerBorder);
        }

        public void UnBlur()
        {
            UnBlur("IsEnabled");
        }

        internal void UnBlur(string storyBoardKey)
        {
            if (Style == null)
                return;

            var sb = FindResource(storyBoardKey) as Storyboard;
            if (sb == null)
                return;

            var containerBorder = (Border)Template.FindName("PART_Container", this);
            sb.Begin(containerBorder);
        }


        internal bool IsBlured2
        {
            get => _isBlured2;
            set
            {
                _isBlured2 = value;
                if (_isBlured2)
                {
                    //Blur
                    var effect = Effect as BlurEffect;
                    effect?.BeginAnimation(BlurEffect.RadiusProperty, new DoubleAnimation(20, TimeSpan.FromSeconds(0.5)));
                }
                else
                {
                    var effect = Effect as BlurEffect;
                    effect?.BeginAnimation(BlurEffect.RadiusProperty, new DoubleAnimation(0, TimeSpan.FromSeconds(0.5)));
                }
            }
        }


        //public bool IsBlured
        //{
        //    get
        //    {
        //        return _isBlured;
        //    }
        //    set
        //    {
        //        if (Style == null)
        //            return;

        //        _isBlured = value;

        //        if (_isBlured)
        //        {
        //            Storyboard sb = this.FindResource("IsDisabled") as Storyboard;
        //            Border containerBorder = (Border)Template.FindName("PART_Container", this);
        //            sb?.Begin(containerBorder);
        //        }
        //        else
        //        {
        //            Storyboard sb = this.FindResource("IsEnabled") as Storyboard;
        //            Border containerBorder = (Border)Template.FindName("PART_Container", this);
        //            sb?.Begin(containerBorder);
        //        }
        //    }
        //}

        //private bool closeStoryBoardCompleted = false;
        //private void Window_Closing(object sender, CancelEventArgs e)
        //{
        //	Window mainWindow = (Window)sender;
        //	if (!closeStoryBoardCompleted)
        //	{
        //		Storyboard sb = mainWindow.FindResource("HoverOff") as Storyboard;
        //		Border containerBorder = (Border)mainWindow.Template.FindName("PART_Container", mainWindow);
        //		e.Cancel = true;
        //		sb.Begin(containerBorder);
        //		//sb.Completed += Timeline_OnCompleted;
        //	}
        //}

        //private void EventSetter_OnHandler(object sender, RoutedEventArgs e)
        //{
        //	Window mainWindow = (Window)sender;
        //	Storyboard sb = mainWindow.FindResource("HoverOff") as Storyboard;
        //	Border containerBorder = (Border)mainWindow.Template.FindName("PART_Container", mainWindow);
        //	sb.Begin(containerBorder);
        //}

        //private void Timeline_OnCompleted(object sender, EventArgs e)
        //{
        //	Window mainWindow = (Window)sender;
        //	closeStoryBoardCompleted = true;
        //	mainWindow.Close();
        //}

    }
}
