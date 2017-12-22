using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using UIPresentationControls.Styles;
using UIPresentationControls.Utils;

namespace UIPresentationControls
{
    public partial class XWindowStyle : IDisposable
    {
        private XWindow _mainXWindow;

        public XWindowStyle(XWindow baseXWindow, ResourceDictionary controlDictionary, ResourceDictionary brushDictionary)
        {
            //ResourceDictionary myControlDictionary = new ResourceDictionary();
            //myControlDictionary.Source = new Uri(controlsUri, UriKind.RelativeOrAbsolute);

            //ResourceDictionary myBrushDictionary = new ResourceDictionary();
            //myBrushDictionary.Source = new Uri(brushUri, UriKind.RelativeOrAbsolute);

            MergedDictionaries.Add(controlDictionary);
            MergedDictionaries.Add(brushDictionary);
            

            _mainXWindow = baseXWindow;
            _mainXWindow.Loaded += BegginingActivate;
            _mainXWindow.Closing += BegginingDeactivate;
        }

        internal virtual void BegginingActivate(object sender, RoutedEventArgs e)
        {
            Window mainWindow = (Window)sender;
            //эффект перехода из прозрачного в нормальный режим и из размытого в четкий
            Storyboard activate = mainWindow.FindResource("BegginingActivate") as Storyboard;
            if (activate == null)
                return;

            Border borderOpacity = (Border)mainWindow.Template.FindName("PART_Container", mainWindow);
            BlurEffect blurEffect = (BlurEffect)mainWindow.Template.FindName("MyBlurEffect", mainWindow);
            borderOpacity.Opacity = 0;
            blurEffect.Radius = 15;
            activate.Begin(borderOpacity);
        }

        internal virtual void BegginingDeactivate(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Window mainWindow = (Window)sender;
            mainWindow.Closing -= BegginingDeactivate;
            e.Cancel = true;
            var anim = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(0.2));
            //после завершения эффекта закрывает окно
            anim.Completed += (s, _) => mainWindow.Close();
            mainWindow.BeginAnimation(UIElement.OpacityProperty, anim);
        }

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
            bool _prevCanFullWindowDragMove = _mainXWindow.CanFullWindowDragMove;

            // пропускает DragMove окна, т.к. возникает конфликт между DragMove и Resize Window
            if (_prevCanFullWindowDragMove)
                _mainXWindow.CanFullWindowDragMove = false;

            CustomOnSize(sender, action);

            if (_prevCanFullWindowDragMove)
                _mainXWindow.CanFullWindowDragMove = true;
        }
        /// <summary>
        /// Изменяем размер окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="action"></param>
        internal virtual void CustomOnSize(object sender, SizingAction action)
        {
            try
            {
                if (Mouse.LeftButton != MouseButtonState.Pressed)
                    return;

                sender.ForWindowFromTemplate(w =>
                                             {
                                                 switch (w.WindowState)
                                                 {
                                                     case WindowState.Normal:
                                                         Win32Controls.DragSize(w.GetWindowHandle(), action);
                                                         break;
                                                     case WindowState.Maximized:
                                                         MaximisedWindow(w);
                                                         break;
                                                     case WindowState.Minimized:
                                                         return;
                                                     default:
                                                         return;
                                                 }
                                             });
            }
            catch (Exception)
            {
                // ignored
            }
        }

        static void MaximisedWindow(Window w)
        {
            w.WindowState = WindowState.Maximized;
            Win32Controls.MoveWindow(w.GetWindowHandle(),
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

        private void Information_OnClick(object sender, RoutedEventArgs e)
        {
           
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
                                             else
                                                 SystemCommands.MaximizeWindow(w);
                                         });
        }
        void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(w => w.Close());
        }

        public void Dispose()
        {
            MergedDictionaries.Clear();
            _mainXWindow.Loaded -= BegginingActivate;
            _mainXWindow.Closing -= BegginingDeactivate;
        }
    }
}
