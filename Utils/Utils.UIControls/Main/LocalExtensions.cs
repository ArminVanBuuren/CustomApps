using System;
using System.Windows;
using System.Windows.Interop;

namespace Utils.UIControls.Main
{
    internal static class LocalExtensions
    {
        public static void ForWindowFromTemplate(this object templateFrameworkElement, Action<UIWindow> action)
        {
            UIWindow window = ((FrameworkElement)templateFrameworkElement).TemplatedParent as UIWindow;
            if (window != null)
                action(window);
        }

        public static IntPtr GetWindowHandle(this Window window)
        {
            WindowInteropHelper helper = new WindowInteropHelper(window);
            return helper.Handle;
        }
    }
}
