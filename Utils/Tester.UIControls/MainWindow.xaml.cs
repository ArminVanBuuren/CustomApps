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

namespace Tester.UIControls
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            //Closed += MainWindow_Closed;
            //text.TextChanged += Text_TextChanged;
        }
        private void Text_TextChanged(object sender, TextChangedEventArgs e)
        {
            MessageBox.Show("1111");
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            MessageBox.Show("2222");
        }
    }
}
