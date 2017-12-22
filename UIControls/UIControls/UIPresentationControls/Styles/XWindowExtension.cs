using System;
using System.Windows;
using System.Windows.Interop;

namespace UIPresentationControls.Styles
{
    internal static class XWindowExtension
    {
        public static void ForWindowFromTemplate(this object templateFrameworkElement, Action<Window> action)
        {
            Window window = ((FrameworkElement) templateFrameworkElement).TemplatedParent as Window;
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
