using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using UIControls.Utils;

namespace UIControls.MainControl
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
