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
    public partial class Presenter
    {
        public Presenter(bool canDragMove = true, bool panelItemIsVisible = true) :base(canDragMove, panelItemIsVisible)
        {
            InitializeComponent();
        }
    }
}
