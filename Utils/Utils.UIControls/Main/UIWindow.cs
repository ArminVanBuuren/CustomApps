using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Utils.UIControls.Tools;

namespace Utils.UIControls.Main
{
    public class UIWindow : Window
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

        public UIWindow(bool canDragMove = true, bool panelItemIsVisible = true)
        {
            
            //Application.Current.Resources.MergedDictionaries.Clear();

            //ResourceDictionary myResourceDictionary1 = new ResourceDictionary();
            //myResourceDictionary1.Source = new Uri(@"../Main/UIWindowStyle.xaml", UriKind.Relative);
            //Resources.MergedDictionaries.Add(myResourceDictionary1);


            //UIElement rootElement;
            //FileStream s = new FileStream(Path.Combine(@"C:\@MyRepos\CustomApp\UIControls\UIControls\UIPresentationControls\Main\UIWindowStyle.xaml"), FileMode.Open);
            //var reader = new XamlXmlReader(@"../../UserControl1.xaml", XamlReader.GetWpfSchemaContext());
            //var userControl = XamlReader.Load(reader) as UserControl1;
            //rootElement = (UIElement)XamlReader.Load(s);
            //s.Close();

            //Resources.MergedDictionaries.Clear();
            //// Это окно необходимо чтобы скомпилировать и добавить ресурс "../Main/UIWindowStyle.xaml" иначе через код-бехайнд его скомпилировать и подклоючить невозможно
            //Resources.MergedDictionaries.Add(new LoadResource().Resources.MergedDictionaries.FirstOrDefault());
            //Style style = this.FindResource("VSUIWindowStyle") as Style;
            //Style = style;


            //ResourceDictionary myResourceDictionary2 = new ResourceDictionary();
            //myResourceDictionary2.Source = new Uri("../Themes/Brushes.xaml", UriKind.Relative);
            //Resources.MergedDictionaries.Add(myResourceDictionary2);

            //ResourceDictionary myResourceDictionary3 = new ResourceDictionary();
            //myResourceDictionary3.Source = new Uri("../Themes/VS2012WindowStyle.xaml", UriKind.Relative);
            //Resources.MergedDictionaries.Add(myResourceDictionary3);

            //Style style = this.FindResource("VSUIWindowStyle") as Style;
            //Style = style;

            //Style style = Application.Current.FindResource("VSUIWindowStyle") as Style;

            //ResourceDictionary res = (ResourceDictionary)Application.LoadComponent(new Uri(@"C:\@MyRepos\CustomApp\UIControls\UIControls\UIPresentationControls\Main\UIWindoiwStyle.xaml", UriKind.RelativeOrAbsolute));



            CanDragMove = canDragMove;
            PanelItemsIsVisible = panelItemIsVisible;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Loaded += UIWindowLoaded;
            Activated += UIWindowActivated;
            Closing += UIWindowClosing;
            StateChanged += UIWindowStateChanged;

            if (!CanDragMove)
                SourceInitialized += DisableUIWindowMoving;
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

        private void UIWindowActivated(object sender, EventArgs e)
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

        private void DisableUIWindowMoving(object sender, EventArgs e)
        {
            var helper = new WindowInteropHelper(this);
            var source = HwndSource.FromHwnd(helper.Handle);
            source?.AddHook(UIControls32.WndProc);
        }

        void UIWindowLoaded(object sender, RoutedEventArgs e)
        {
            var mainWindow = this;

            CheckTitle(Title);

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

                if (ResizeMode == ResizeMode.NoResize)
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
            if (frame is FrameworkElement element)
                element.Cursor = null;
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
    }
}
