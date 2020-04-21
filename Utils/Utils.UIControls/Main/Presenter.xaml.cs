using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Utils.UIControls.Main
{
    public partial class Presenter
    {
        public Presenter(string textPresenter = null)
        {
            InitializeComponent();

            Loaded += (sender, args) =>
            {
                Information.Visibility = Visibility.Collapsed;
                MaxButton.Visibility = Visibility.Collapsed;
                MinButton.Visibility = Visibility.Collapsed;
                CanDragMove = false;
                VisibleResizeMode = false;
            };

            if (!string.IsNullOrEmpty(textPresenter))
                X_title.Text = X_title.Text + "\r\n" + textPresenter;

            //this.Icon = UIControls.Properties.Resources.overwolf.ToImageSource();
            //Uri uriIcon = new Uri(@"C:\@MyRepos\CustomApp\UIControls\UIControls\UIControls\Images\overwolf.ico", UriKind.RelativeOrAbsolute);
            //this.Icon = new BitmapImage(uriIcon);
        }

        public static void ShowOwner(UIWindow mainWindow)
        {
            var vkhovanskiy = new Presenter(mainWindow.PresenterTitleContent)
            {
                Owner = mainWindow
            };
            vkhovanskiy.Loaded += WindowInfo_Loaded;
            mainWindow.Blur();
            vkhovanskiy.ShowDialog();
            vkhovanskiy.Loaded -= WindowInfo_Loaded;
            mainWindow.UnBlur();
        }

        public static void ShowOwner()
        {
            var vkhovanskiy = new Presenter(string.Empty)
            {
                Width = SystemParameters.PrimaryScreenWidth,
                Height = SystemParameters.PrimaryScreenHeight,
                Topmost = true,
                Top = 0,
                Left = 0,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen
            };
            vkhovanskiy.Loaded += WindowInfo_Loaded;
            vkhovanskiy.ShowDialog();
            vkhovanskiy.Loaded -= WindowInfo_Loaded;
        }

        private static void WindowInfo_Loaded(object sender, RoutedEventArgs e)
        {
            var mainWindow = (UIWindow)sender;
            var infButtom = (Border)mainWindow.Template.FindName("TitleBar", mainWindow);
            var infText = (TextBlock)mainWindow.Template.FindName("Caption", mainWindow);
            infButtom.Background = (Brush)mainWindow.FindResource("AuthorWindowTopBorderBrush");
            infText.Foreground = (Brush)mainWindow.FindResource("AuthorWindowTopBorderTextBrush");
            infText.Opacity = 1;
        }

    }
}
