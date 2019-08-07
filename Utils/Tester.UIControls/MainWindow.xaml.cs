using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
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
using Utils.UIControls.Tools;
using Utils.WinForm.Notepad;

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

            //Directory.CreateDirectory("temp");
            //CreateImage(ImageFormat.Gif);
            //CreateImage(ImageFormat.Bmp);
            //CreateImage(ImageFormat.Emf);
            //CreateImage(ImageFormat.Exif);
            //CreateImage(ImageFormat.Icon);
            //CreateImage(ImageFormat.Jpeg);
            //CreateImage(ImageFormat.MemoryBmp);
            //CreateImage(ImageFormat.Png);
            //CreateImage(ImageFormat.Tiff);
            //CreateImage(ImageFormat.Wmf);
            //CreateImage(ImageFormat.Wmf);

            //while (true)
            try
            {
                var notepad = new Notepad(@"C:\!Builds\Git\1\versions.xml");
                notepad.AddFileDocument(@"C:\!Builds\Git\2\versions.xml");
                notepad.AddFileDocument(@"C:\!Builds\Git\3\versions.xml");
                //notepad.AddDocument(@"C:\!Builds\Git\1\111.xml");
                //notepad.AddDocument(@"C:\!Builds\Git\1\222.xml");
                //notepad.AddDocument(@"C:\!Builds\Git\1\333.xml");
                notepad.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                Process.GetCurrentProcess().Kill();
            }
        }

        void CreateImage(ImageFormat imageFormat)
        {
            string imagePath5 = System.IO.Path.Combine("temp", $"{STRING.RandomString(15)}.{imageFormat}");
            //ScreenCapture.Capture(imagePath5, imageFormat);
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
