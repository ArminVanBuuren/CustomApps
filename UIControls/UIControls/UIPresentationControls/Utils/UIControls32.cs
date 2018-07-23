using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace UIPresentationControls.Utils
{
    public class UIControls32
    {
        const int WM_SYSCOMMAND = 0x0112;
        const int SC_MOVE = 0xF010;


        public UIControls32()
        {
            WOSVersion wm = new WOSVersion();
            _currentVersion = wm;
        }

        public static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_SYSCOMMAND:
                    int command = wParam.ToInt32() & 0xfff0;
                    if (command == SC_MOVE)
                    {
                        handled = true;
                    }
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }

        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("User32.dll")]
        public static extern int FindWindow(String ClassName, String WindowName);

        [DllImport("User32.dll")]
        public static extern IntPtr SetForegroundWindow(int hWnd);

        [DllImport("user32.dll")]
        public static extern void SetCursorPos(int x, int y);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(MouseEvent dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32", SetLastError = true)]
        public static extern int MoveWindow(IntPtr hwnd, IntPtr x, IntPtr y, IntPtr nWidth, IntPtr nHeight, bool bRepaint);

        [DllImport("ntdll.dll")]
        public static extern uint NtResumeProcess([In] IntPtr processHandle);

        [DllImport("ntdll.dll")]
        public static extern uint NtSuspendProcess([In] IntPtr processHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccess desiredAccess, bool inheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle([In] IntPtr handle);

        [DllImport("kernel32.dll")]
        static extern bool SetProcessWorkingSetSize(IntPtr handle, int minimumWorkingSetSize, int maximumWorkingSetSize);


        [DllImport("gdi32.dll", ExactSpelling = true, PreserveSig = true, SetLastError = true)]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateBitmap(int nWidth, int nHeight, uint cPlanes, uint cBitsPerPel, IntPtr lpvBits);
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        //[StructLayout(LayoutKind.Sequential)]

        static WOSVersion _currentVersion;
        public static WOSVersion CurrentVersion
        {
            get { return _currentVersion; }
            set { _currentVersion = value; }
        }

        public static TaskBarLocation GetTaskBarLocation()
        {
            if (SystemParameters.WorkArea.Left > 0)
                return TaskBarLocation.LEFT;
            if (SystemParameters.WorkArea.Top > 0)
                return TaskBarLocation.TOP;
            if (SystemParameters.WorkArea.Left == 0
              && SystemParameters.WorkArea.Width < SystemParameters.PrimaryScreenWidth)
                return TaskBarLocation.RIGHT;
            return TaskBarLocation.BOTTOM;
        }

        public static Bitmap GetScreen(IntPtr handle)
        {
            Bitmap bmp = new Bitmap(0, 0);
            if (SetForegroundWindow(handle))
            {
                RECT srcRect;
                if (GetWindowRect(handle, out srcRect))
                {
                    int width = srcRect.Right - srcRect.Left;
                    int height = srcRect.Bottom - srcRect.Top;

                    //bmp = new Bitmap(width, height);
                    Graphics screenG = Graphics.FromImage(bmp);

                    try
                    {
                        screenG.CopyFromScreen(srcRect.Left, srcRect.Top,
                            0, 0, new System.Drawing.Size(width, height),
                            CopyPixelOperation.SourceCopy);

                        // bmp.Save("notepad.jpg", ImageFormat.Jpeg);
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.Message);
                    }
                    //finally
                    //{
                    //    screenG.Dispose();
                    //    bmp.Dispose();
                    //}
                }

            }
            return bmp;
        }

        public static BitmapSource Capture(Rect area)
        {
            IntPtr screenDC = GetDC(IntPtr.Zero);
            IntPtr memDC = CreateCompatibleDC(screenDC);
            IntPtr hBitmap = CreateCompatibleBitmap(screenDC, (int)SystemParameters.VirtualScreenWidth, (int)SystemParameters.VirtualScreenHeight);
            SelectObject(memDC, hBitmap); // Select bitmap from compatible bitmap to memDC

            if ((area.X != 0) && (area.Y != 0))
            {
                BitBlt(memDC, 0, 0, (int)area.Width - 15, (int)area.Height - 49, screenDC, (int)area.X + 9, (int)area.Y + 41, TernaryRasterOperations.SRCCOPY);
            }
            else
            {
                BitBlt(memDC, 0, 0, (int)area.Width, (int)area.Height, screenDC, (int)area.X, (int)area.Y, TernaryRasterOperations.SRCCOPY);
            }
            BitmapSource bsource = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            DeleteObject(hBitmap);
            //ReleaseDC(IntPtr.Zero, screenDC);
            //ReleaseDC(IntPtr.Zero, memDC);
            return bsource;
        }

        public static void StartProcess(string ProgName)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = ProgName;
            proc.Start();
            proc.WaitForInputIdle();
        }

        static IntPtr MakeLParam(int LoWord, int HiWord)
        {
            int i = (HiWord << 16) | (LoWord & 0xffff);
            return new IntPtr(i);
        }

        public static void FindByName(string ClassName, string WindowName)
        {
            IntPtr handle = (IntPtr)FindWindow(ClassName, WindowName);
            SendMessage(handle, (int)UI32WM.WM_MOVE, (IntPtr)(long)0x0, MakeLParam(50, 100));
        }

        public static void SuspendProcess(int processId)
        {
            IntPtr hProc = IntPtr.Zero;
            try
            {
                // Gets the handle to the Process
                hProc = OpenProcess(ProcessAccess.SuspendResume, false, processId);
                if (hProc != IntPtr.Zero)
                {
                    NtSuspendProcess(hProc);
                }
            }
            finally
            {
                // Don't forget to close handle you created.
                if (hProc != IntPtr.Zero)
                {
                    CloseHandle(hProc);
                }
            }
        }

        /// <summary>
        ///очистка памяти после создания нового экземпляра, это не обходимо т.к .NET самостоятельно
        ///конечно же вычищает пространсто но это происходит через большой период и немного
        ///данный метод позволяет сразу же освободить место и на больщой порядок очищается весь мусор давнишних экземпляров
        ///Однако, если вам действительно нужно, чтобы приложение в памяти было «незаметно», есть возможность 
        ///урезать свое рабочее множество вручную. К сожалению, сделать это можно только прибегнув к Win32 API, к конкретно – к функции SetProcessWorkingSetSize:
        ///Первый параметр – это хендл процесса, который можно получить из System.Diagnostics.Process.GetCurrentProcess().Han­dle, а второй и третий параметр – 
        ///это соответственно минимальный и максимальный размер рабочего множества. 
        ///Понятно, что рассчитывать нужные размеры – задача та еще, поэтому функция имеет один приятный сервис: если в качестве второго и третьего параметра указать 
        ///-1 (минус единицу), операционная система сама посчитает, какие размеры нужны вашему приложению и установит рабочее множество по минимуму.
        /// </summary>
        public static void Dispose()
        {
            SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
        }

        static bool Check(string apps)
        {
            //поиск окна по заголовку
            int hWnd = FindWindow(null, apps);
            if (hWnd > 0) //нашли
            {
                SetForegroundWindow(hWnd); //активировали
            }
            else//не нашли
            {
                return false;
            }
            return true;
        }

        public static void ChangeCorsorByProgrammName(string ProgrammName)
        {
            Process[] p = Process.GetProcessesByName(ProgrammName);

            foreach (Process s in p)
            {
                Check(s.MainWindowTitle);
            }

            SetCursorPos(500, 500);
            mouse_event(MouseEvent.MOUSEEVENTF_LEFTDOWN,
                System.Windows.Forms.Cursor.Position.X,
                System.Windows.Forms.Cursor.Position.Y,
                0,
                0);
            mouse_event(MouseEvent.MOUSEEVENTF_LEFTUP,
                 System.Windows.Forms.Cursor.Position.X,
                 System.Windows.Forms.Cursor.Position.Y,
                 0,
                 0);
        }

        public static void DragSize(IntPtr handle, SizingAction sizingAction)
        {
            SendMessage(handle, (int)UI32WM.WM_SYSCOMMAND, (IntPtr)((int)UI32SC.SC_SIZE + sizingAction), IntPtr.Zero);
            SendMessage(handle, 514, IntPtr.Zero, IntPtr.Zero);
        }

    }
}
