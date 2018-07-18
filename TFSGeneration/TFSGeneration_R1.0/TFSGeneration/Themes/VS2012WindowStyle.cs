using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TFSAssist.Themes
{
	public partial class VS2012WindowStyle
	{
	    //public VS2012WindowStyle()
	    //{
     //       ResourceDictionary myResourceDictionary1 = new ResourceDictionary();
	    //    myResourceDictionary1.Source = new Uri("../Themes/Brushes.xaml", UriKind.Relative);
     //       MergedDictionaries.Add(myResourceDictionary1);

     //       ResourceDictionary myResourceDictionary2 = new ResourceDictionary();
	    //    myResourceDictionary2.Source = new Uri("../Themes/Core.xaml", UriKind.Relative);
	    //    MergedDictionaries.Add(myResourceDictionary2);
     //   }

		public bool DragMovePass { get; private set; } = false;
		void OnSizeSouth(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.South); }
		void OnSizeNorth(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.North); }
		void OnSizeEast(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.East); }
		void OnSizeWest(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.West); }
		void OnSizeNorthWest(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.NorthWest); }
		void OnSizeNorthEast(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.NorthEast); }
		void OnSizeSouthEast(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.SouthEast); }
		void OnSizeSouthWest(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.SouthWest); }
		void OnSize(object sender, SizingAction action)
		{
            if (Mouse.LeftButton == MouseButtonState.Pressed)
			{
				DragMovePass = true;
				sender.ForWindowFromTemplate(w =>
				{
					if (w.ResizeMode == ResizeMode.NoResize)
					{
						return;
					}
					if (w.WindowState == WindowState.Normal)
					{
						UIControls32.DragSize(w.GetWindowHandle(), action);
					}
					else if (w.WindowState == WindowState.Maximized)
					{
						MaximisedWindow(w);
					}
				});
			}
		}


     //   bool ResizeInProcess = false;

	    //private void Resize_Init(object sender, MouseButtonEventArgs e)
	    //{

	    //    Border senderRect = sender as Border;
	    //    if (senderRect != null)
	    //    {
	    //        ResizeInProcess = true;

	    //        senderRect.CaptureMouse();
	    //    }
	    //}

	    //private void Resize_End(object sender, MouseButtonEventArgs e)
	    //{
	    //    Border senderRect = sender as Border;
	    //    if (senderRect != null)
	    //    {
	    //        ResizeInProcess = false;

	    //        senderRect.ReleaseMouseCapture();
	    //    }
	    //}

	    //private void Resizeing_Form(object sender, MouseEventArgs e)
	    //{
	    //    if (ResizeInProcess)
	    //    {
	    //        Border senderRect = sender as Border;
	    //        Window mainWindow = senderRect.Tag as Window;
	    //        if (senderRect != null)
	    //        {
	    //            double width = e.GetPosition(mainWindow).X;
	    //            double height = e.GetPosition(mainWindow).Y;
	    //            senderRect.CaptureMouse();
	    //            if (senderRect.Name.Equals("Right", StringComparison.CurrentCultureIgnoreCase))
	    //            {
	    //                width += 5;
	    //                if (width > 0)
	    //                    mainWindow.Width = width;
	    //            }
	    //            if (senderRect.Name.Equals("Left", StringComparison.CurrentCultureIgnoreCase))
	    //            {
	    //                width -= 5;
	    //                mainWindow.Left += width;
	    //                width = mainWindow.Width - width;
	    //                if (width > 0)
	    //                {
	    //                    mainWindow.Width = width;
	    //                }
	    //            }
	    //            if (senderRect.Name.Equals("Bottom", StringComparison.CurrentCultureIgnoreCase))
	    //            {
	    //                height += 5;
	    //                if (height > 0)
	    //                    mainWindow.Height = height;
	    //            }
	    //            if (senderRect.Name.Equals("Top", StringComparison.CurrentCultureIgnoreCase))
	    //            {
	    //                height -= 5;
	    //                mainWindow.Top += height;
	    //                height = mainWindow.Height - height;
	    //                if (height > 0)
	    //                {
	    //                    mainWindow.Height = height;
	    //                }
	    //            }
	    //        }
	    //    }
	    //}


	    void MaximisedWindow(Window w)
		{
			w.WindowState = WindowState.Maximized;
			UIControls32.MoveWindow(w.GetWindowHandle(), //, (IntPtr)(-7), (IntPtr)(-7),
				(IntPtr)System.Windows.Forms.SystemInformation.WorkingArea.Width + 7,
				(IntPtr)System.Windows.Forms.SystemInformation.WorkingArea.Height + 7,
				(IntPtr)System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Width + 7,
				(IntPtr)System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Height + 7, 
				false);
		}

		void IconMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount > 1)
				sender.ForWindowFromTemplate(w => SystemCommands.CloseWindow(w));
		}

		void IconMouseUp(object sender, MouseButtonEventArgs e)
		{
			var element = sender as FrameworkElement;
			var point = element.PointToScreen(new Point(element.ActualWidth / 2, element.ActualHeight));
			sender.ForWindowFromTemplate(w => SystemCommands.ShowSystemMenu(w, point));
		}

		void MinButtonClick(object sender, RoutedEventArgs e)
		{
			sender.ForWindowFromTemplate(w => SystemCommands.MinimizeWindow(w));
		}

		void MaxButtonClick(object sender, RoutedEventArgs e)
		{
			sender.ForWindowFromTemplate(w =>
			{
				if (w.WindowState == WindowState.Maximized)
					SystemCommands.RestoreWindow(w);
				else SystemCommands.MaximizeWindow(w);
			});
		}
		void CloseButtonClick(object sender, RoutedEventArgs e)
		{
			sender.ForWindowFromTemplate(w => w.Close());
		}

		private void WindowLoaded(object sender, RoutedEventArgs e)
		{
			WindowNew mainWindow = (WindowNew)sender;

			//чтобы можно было перемещать окно по нажатию клавиши в любой точки окна, а не только через верхнюю панель
			Grid parentGrid = (Grid)mainWindow.Template.FindName("LayoutRoot", mainWindow);

            parentGrid.MouseLeftButtonDown += (o, args) =>
            {
                //если менялся размер окна то не перемещаем окно
                if (!DragMovePass)
                    mainWindow.DragMove();
                else
                    DragMovePass = false;
                args.Handled = true;
            };
        }

		private void Information_OnClick(object sender, RoutedEventArgs e)
		{
			WindowNew mainWindow = Application.Current.MainWindow as WindowNew;

		    Style style = mainWindow?.FindResource("VS2012WindowStyle") as Style;
		    if (style == null)
		        return;


            WindowNew windowInfo = new WindowNew(false, false);
		    windowInfo.Title = "Vladimir Khovanskiy";
            windowInfo.ResizeMode = ResizeMode.NoResize;
			windowInfo.Style = style;
            windowInfo.FontFamily = new FontFamily("Segoe UI");
		    windowInfo.FontSize = 13;

            //windowInfo.Icon = mainWindow.Icon;
            //new BitmapImage(Properties.Resources.Overwolf);
            //Properties.Resources.Overwolf
            windowInfo.Icon = new BitmapImage(new Uri(@"pack://application:,,,/Images/overwolf.ico"));
            //windowInfo.Icon = BitmapFrame.Create(Application.GetResourceStream(new Uri("LiveJewel.png", UriKind.RelativeOrAbsolute)).Stream);
		    windowInfo.Background = (Brush)new BrushConverter().ConvertFrom("#333");
            windowInfo.Foreground = (Brush)new BrushConverter().ConvertFrom("#FF32EBFB");


            TextBlock text = new TextBlock();
			text.Padding = new Thickness(0,0,7,7);
			text.Foreground = Brushes.White;
			text.TextWrapping = TextWrapping.Wrap;
			text.FontSize = 12;
			text.HorizontalAlignment = HorizontalAlignment.Center;
			text.VerticalAlignment = VerticalAlignment.Center;
			text.TextAlignment = TextAlignment.Center;
			text.Text = "Hello! Thanks for choosing my application!";
		    text.Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF7F7F7");

            windowInfo.Content = text;

			windowInfo.MinWidth = 350;
			windowInfo.MinHeight = 100;
			windowInfo.MaxWidth = 350;
			windowInfo.MaxHeight = 100;
			windowInfo.Owner = mainWindow;
			windowInfo.SizeToContent = SizeToContent.WidthAndHeight;
			windowInfo.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			windowInfo.Loaded += WindowInfo_Loaded;

			mainWindow.IsBlured = true;
			windowInfo.ShowDialog();
			windowInfo.Loaded -= WindowInfo_Loaded;
			mainWindow.IsBlured = false;
		}

		private void WindowInfo_Loaded(object sender, RoutedEventArgs e)
		{
			WindowNew mainWindow = (WindowNew) sender;
			Border infButtom = (Border)mainWindow.Template.FindName("TitleBar", mainWindow);
			TextBlock infText = (TextBlock)mainWindow.Template.FindName("Caption", mainWindow);
			infButtom.Background = (Brush)mainWindow.FindResource("AuthorWindowTopBorderBrush");
			infText.Foreground = (Brush)mainWindow.FindResource("AuthorWindowTopBorderTextBrush");
			infText.Opacity = 1;
		}
	}
}