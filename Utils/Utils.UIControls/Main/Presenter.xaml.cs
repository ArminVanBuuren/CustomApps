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
    }
}
