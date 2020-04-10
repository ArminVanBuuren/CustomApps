using System;
using System.Windows.Forms;

namespace Utils.WinForm
{
    public static class CONTROL
    {
        public static void SafeInvoke(this Control uiElement, Action action, bool forceSynchronous = true)
        {
            if (uiElement == null)
                throw new ArgumentNullException(nameof(uiElement));

            // InvokeRequired всегда вернет true, если это работает контекст чужого потока 
            if (uiElement.InvokeRequired)
            {
                if (forceSynchronous)
                    uiElement.Invoke(action);
                else
                    uiElement.BeginInvoke(action);
            }
            else
            {
                if (uiElement.IsDisposed)
                    throw new ObjectDisposedException("Control is already disposed.");

                action();
            }
        }

        public static void SafeInvoke<TSource>(this Control uiElement, Action<TSource> action, TSource arg, bool forceSynchronous = false)
        {
            if (uiElement == null)
                throw new ArgumentNullException(nameof(uiElement));

            // InvokeRequired всегда вернет true, если это работает контекст чужого потока 
            if (uiElement.InvokeRequired)
            {
                var del = new MethodInvoker(delegate { action.Invoke(arg); });
                if (forceSynchronous)
                    uiElement.Invoke(del);
                else
                    uiElement.BeginInvoke(del);
            }
            else
            {
                if (uiElement.IsDisposed)
                    throw new ObjectDisposedException("Control is already disposed.");

                action(arg);
            }
        }

        public static TSource SafeInvoke<TSource>(this Control uiElement, Func<TSource> func)
        {
            if (uiElement == null)
                throw new ArgumentNullException(nameof(uiElement));

            // InvokeRequired всегда вернет true, если это работает контекст чужого потока 
            if (uiElement.InvokeRequired)
            {
                TSource result = default(TSource);
                uiElement.Invoke(new MethodInvoker(delegate { result = func.Invoke(); }));
                return result;
            }
            else
            {
                if (uiElement.IsDisposed)
                    throw new ObjectDisposedException("Control is already disposed.");

                return func();
            }
        }

        public static TResult SafeInvoke<TSource, TResult>(this Control uiElement, Func<TSource, TResult> func, TSource arg)
        {
            if (uiElement == null)
                throw new ArgumentNullException(nameof(uiElement));

            // InvokeRequired всегда вернет true, если это работает контекст чужого потока 
            if (uiElement.InvokeRequired)
            {
                TResult result = default(TResult);
                uiElement.Invoke(new MethodInvoker(delegate { result = func.Invoke(arg); }));
                return result;
            }
            else
            {
                if (uiElement.IsDisposed)
                    throw new ObjectDisposedException("Control is already disposed.");

                return func(arg);
            }
        }
    }
}
