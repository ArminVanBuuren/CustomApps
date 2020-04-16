using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Utils.UIControls.Main
{
    public partial class Presenter
    {
        public Presenter(string textPresenter = null, bool canDragMove = true, bool panelItemIsVisible = true) : base(canDragMove, panelItemIsVisible)
        {
            
            InitializeComponent();
            //this.Icon = UIControls.Properties.Resources.overwolf.ToImageSource();
            
            if (!string.IsNullOrEmpty(textPresenter))
            {
                X_title.Text = X_title.Text + "\r\n" + textPresenter;
            }

            //Uri uriIcon = new Uri(@"C:\@MyRepos\CustomApp\UIControls\UIControls\UIControls\Images\overwolf.ico", UriKind.RelativeOrAbsolute);
            //this.Icon = new BitmapImage(uriIcon);
        }

        public static void ShowOwner()
        {
            var vkhovanskiy = new Presenter(string.Empty, false, false)
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
