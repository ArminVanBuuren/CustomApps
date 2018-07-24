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
        const string ICON_PATH = @"C:\@MyRepos\CustomApp\UIControls\UIControls\UIControls\Images\overwolf.ico";
        public Presenter(bool canDragMove = true, bool panelItemIsVisible = true) :base(canDragMove, panelItemIsVisible)
        {
            InitializeComponent();

            if (File.Exists(ICON_PATH))
            {
                Uri uriIcon = new Uri(ICON_PATH, UriKind.RelativeOrAbsolute);
                this.Icon = new BitmapImage(uriIcon);
            }
        }
    }
}
