using System;
using System.Windows;
using System.Windows.Interop;

namespace Utils.UIControls.Main
{
    internal static class LocalExtensions
    {
        public static void ForWindowFromTemplate(this object templateFrameworkElement, Action<UIWindow> action)
        {
            if (((FrameworkElement)templateFrameworkElement).TemplatedParent is UIWindow window)
                action(window);
        }

        public static IntPtr GetWindowHandle(this Window window)
        {
            var helper = new WindowInteropHelper(window);
            return helper.Handle;
        }
    }
}
