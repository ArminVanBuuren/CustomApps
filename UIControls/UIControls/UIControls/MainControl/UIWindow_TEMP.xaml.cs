using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using UIControls.Utils;

namespace UIControls.MainControl
{
    public partial class UIWindow_TEMP
    {
        //static WindowNew()
        //{
        //       ResourceDictionary myResourceDictionary = new ResourceDictionary();

        //       myResourceDictionary.Source = new Uri("../Themes/Brushes.xaml", UriKind.Relative);
        //       Application.Current.Resources.MergedDictionaries.Add(myResourceDictionary);

        //       myResourceDictionary.Source = new Uri("../Themes/Brushes.xaml", UriKind.Relative);
        //       Application.Current.Resources.MergedDictionaries.Add(myResourceDictionary);
        //   }

        private bool _isBlured, _isBlured2;
        public bool CanDragMove { get; private set; } = true;
        public bool PanelItemsIsVisible { get; private set; } = true;
        public UIWindow_TEMP(bool canDragMove = true, bool panelItemIsVisible = true)
        {
            InitializeComponent();
            //Application.Current.Resources.MergedDictionaries.Clear();

            //ResourceDictionary myResourceDictionary1 = new ResourceDictionary();
            //myResourceDictionary1.Source = new Uri(@"../MainControl/UIWindowStyle.xaml", UriKind.Relative);
            //Resources.MergedDictionaries.Add(myResourceDictionary1);


            //string _prefix = String.Concat(typeof(UIWindow_TEMP).Namespace, ";component/");
            //// clear all ResourceDictionaries
            //this.Resources.MergedDictionaries.Clear();
            //string res = _prefix + @"../MainControl/UIWindowStyle.xaml";
            //// add ResourceDictionary
            //this.Resources.MergedDictionaries.Add
            //(
            //    new ResourceDictionary { Source = new Uri(res, UriKind.Relative) }
            //);



            //ResourceDictionary myResourceDictionary2 = new ResourceDictionary();
            //myResourceDictionary2.Source = new Uri("../Themes/Brushes.xaml", UriKind.Relative);
            //Resources.MergedDictionaries.Add(myResourceDictionary2);

            //ResourceDictionary myResourceDictionary3 = new ResourceDictionary();
            //myResourceDictionary3.Source = new Uri("../Themes/VS2012WindowStyle.xaml", UriKind.Relative);
            //Resources.MergedDictionaries.Add(myResourceDictionary3);

            //Style style = this.FindResource("VSUIWindowStyle") as Style;
            //Style = style;

            //Style style = Application.Current.FindResource("VSUIWindowStyle") as Style;

            //ResourceDictionary res = (ResourceDictionary)Application.LoadComponent(new Uri(@"C:\@MyRepos\CustomApp\UIControls\UIControls\UIPresentationControls\MainControl\UIWindoiwStyle.xaml", UriKind.RelativeOrAbsolute));



            CanDragMove = canDragMove;
            PanelItemsIsVisible = panelItemIsVisible;
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            Loaded += WindowLoaded;
            Activated += WindowNew_Activated;
            Closing += Window_Closing;
            StateChanged += WindowStateChanged;

            if (!CanDragMove)
                SourceInitialized += DisableWindowMoving;
            else
            {
                //this.PreviewMouseLeftButtonDown += (s, e) =>
                //{
                //	if (e.ChangedButton == MouseButton.Left)
                //		DragMove();
                //};
                //MouseDown += (s, e) =>
                //{
                //	var egeg = e.Handled;
                //	if (e.ChangedButton == MouseButton.Left)
                //		DragMove();
                //};
            }
        }



        private void WindowNew_Activated(object sender, EventArgs e)
        {

        }


        //protected override void OnSourceInitialized(EventArgs e)
        //{
        //	HwndSource hwndSource = (HwndSource)HwndSource.FromVisual(this);
        //	hwndSource.AddHook(WndProcHook);
        //	base.OnSourceInitialized(e);
        //}

        //private static IntPtr WndProcHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handeled)
        //{
        //	if (msg == 0x0084) // WM_NCHITTEST
        //	{
        //		handeled = true;
        //		return (IntPtr)2; // HTCAPTION
        //	}
        //	return IntPtr.Zero;
        //}

        private void DisableWindowMoving(object sender, EventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source?.AddHook(UIControls32.WndProc);
        }

        void WindowLoaded(object sender, RoutedEventArgs e)
        {
            UIWindow_TEMP mainWindow = (UIWindow_TEMP)this;

            //Style style = this.FindResource("VSUIWindowStyle") as Style;
            //mainWindow.Style = style;

            OpacityActivate(mainWindow);

            if (!mainWindow.PanelItemsIsVisible && mainWindow.Style != null)
            {
                var infButtom = (Button)mainWindow.Template.FindName("Information", mainWindow);
                var minButtom = (Button)mainWindow.Template.FindName("MinButton", mainWindow);
                var maxButtom = (Button)mainWindow.Template.FindName("MaxButton", mainWindow);

                infButtom.Visibility = Visibility.Collapsed;
                minButtom.Visibility = Visibility.Collapsed;
                maxButtom.Visibility = Visibility.Collapsed;

                if (this.ResizeMode == ResizeMode.NoResize)
                {
                    DisableResizeMode(mainWindow.Template.FindName("Left", mainWindow));
                    DisableResizeMode(mainWindow.Template.FindName("Right", mainWindow));
                    DisableResizeMode(mainWindow.Template.FindName("Bottom", mainWindow));
                    DisableResizeMode(mainWindow.Template.FindName("rectSizeNorthWest", mainWindow));
                    DisableResizeMode(mainWindow.Template.FindName("rectSizeNorthEast", mainWindow));
                    DisableResizeMode(mainWindow.Template.FindName("rectSizeSouthWest", mainWindow));
                    DisableResizeMode(mainWindow.Template.FindName("rectSizeSouthEast", mainWindow));
                }
            }

        }

        static void DisableResizeMode(object frame)
        {
            FrameworkElement element = frame as FrameworkElement;
            if (element != null)
                element.Cursor = null;
        }

        private static void Window_Closing(object sender, CancelEventArgs e)
        {
            Window mainWindow = (Window)sender;
            mainWindow.Closing -= Window_Closing;
            e.Cancel = true;
            var anim = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(0.2));
            //после завершения эффекта закрывает окно
            anim.Completed += (s, _) => mainWindow.Close();
            mainWindow.BeginAnimation(UIElement.OpacityProperty, anim);
        }

        static void OpacityActivate(UIWindow_TEMP mainWindow)
        {
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

        void OpacityDeactivate()
        {

        }

        void WindowStateChanged(object sender, EventArgs e)
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

        public bool IsBlured
        {
            get
            {
                return _isBlured;
            }
            internal set
            {
                if (Style == null)
                    return;

                _isBlured = value;

                if (Style == null)
                    return;

                if (_isBlured)
                {
                    Storyboard sb = this.FindResource("IsDisabled") as Storyboard;
                    Border containerBorder = (Border)Template.FindName("PART_Container", this);
                    sb?.Begin(containerBorder);
                }
                else
                {
                    Storyboard sb = this.FindResource("IsEnabled") as Storyboard;
                    Border containerBorder = (Border)Template.FindName("PART_Container", this);
                    sb?.Begin(containerBorder);
                }
            }
        }

        public bool IsBlured2
        {
            get
            {
                return _isBlured2;
            }
            internal set
            {
                _isBlured2 = value;
                if (_isBlured2)
                {
                    //Blur
                    BlurEffect effect = Effect as BlurEffect;
                    effect?.BeginAnimation(BlurEffect.RadiusProperty, new DoubleAnimation(20, TimeSpan.FromSeconds(0.5)));
                }
                else
                {
                    BlurEffect effect = Effect as BlurEffect;
                    effect?.BeginAnimation(BlurEffect.RadiusProperty, new DoubleAnimation(0, TimeSpan.FromSeconds(0.5)));
                }
            }
        }
    }
}
