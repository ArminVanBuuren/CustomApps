using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Utils;
using Utils.UIControls.Main;

namespace SPAMessageSaloon.About
{
    /// <inheritdoc cref="" />
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class AboutWindow
    {
        private static AboutWindow About;
        public AboutWindow()
        {
            InitializeComponent();
        }

        public static void ShowOwner()
        {
            Presenter.ShowOwner();
            STREAM.GarbageCollect();
        }
    }
}
