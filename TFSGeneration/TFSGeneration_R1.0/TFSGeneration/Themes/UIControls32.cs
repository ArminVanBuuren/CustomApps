using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace TFSAssist.Themes
{
    public class WOSVersion
    {
        string _name = string.Empty;
        WOSNames _version = WOSNames.None;
        public WOSVersion()
        {
            System.OperatingSystem osInfo = System.Environment.OSVersion;
            switch (osInfo.Platform)
            {
                case System.PlatformID.Win32Windows:

                    switch (osInfo.Version.Minor)
                    {
                        case 0:
                            {
                                _version = WOSNames.Win95;
                                _name = "Windows 95";
                                break;
                            }
                        case 10:
                            {
                                if (osInfo.Version.Revision.ToString() == "2222A")
                                {
                                    _version = WOSNames.Win98SecondEdition;
                                    _name = "Windows 98 Second Edition";
                                }
                                else
                                {
                                    _version = WOSNames.Win98;
                                    _name = "Windows 98";
                                }
                                break;
                            }
                        case 90:
                            {
                                _version = WOSNames.WinMe;
                                _name = "Windows Me";
                                break;
                            }
                    } break;

                // Platform is Windows NT 3.51, Windows NT 4.0, Windows 2000,
                // or Windows XP.
                case System.PlatformID.Win32NT:

                    switch (osInfo.Version.Major)
                    {
                        case 3:
                            {
                                _version = WOSNames.WinNT351;
                                _name = "Windows NT 3.51";
                                break;
                            }
                        case 4:
                            {
                                _version = WOSNames.WinNT40;
                                _name = "Windows NT 4.0";
                                break;
                            }
                        case 5:
                            {
                                if (osInfo.Version.Minor == 0)
                                {
                                    _version = WOSNames.Win2000;
                                    _name = "Windows 2000";
                                }
                                else
                                {
                                    _version = WOSNames.WinXP;
                                    _name = "Windows XP";
                                }
                                break;
                            }
                        case 6:
                            {
                                _version = WOSNames.Win8;
                                _name = "Windows 8";
                                break;
                            }
                    } break;
            }
        }

        public string Name { get { return _name; } set { _name = value; } }
        public WOSNames Version { get { return _version; } set { _version = value; } }
    }

    public enum WOSNames : int
    {
        Win95 = 0,
        Win98 = 1,
        Win98SecondEdition = 2,
        WinNT351 = 3,
        WinNT40 = 4,
        Win2000 = 5,
        WinXP = 6,
        Win7 = 7,
        Win8 = 8,
        WinMe = 9,
        None = 10
    }

    public enum TaskBarLocation { TOP, BOTTOM, LEFT, RIGHT } 

    public class UIControls32
    {
        public UIControls32()
        {
            WOSVersion wm = new WOSVersion();
            _currentVersion = wm;
        }




	    const int WM_SYSCOMMAND = 0x0112;
	    const int SC_MOVE = 0xF010;

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
        public static extern IntPtr OpenProcess(  ProcessAccess desiredAccess, bool inheritHandle, int processId);

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
            Bitmap bmp = new Bitmap(0,0);
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
        public enum SizingAction
        {
            North = 3,
            South = 6,
            East = 2,
            West = 1,
            NorthEast = 5,
            NorthWest = 4,
            SouthEast = 8,
            SouthWest = 7
        }


        public  enum TernaryRasterOperations : uint
        {
            /// <summary>dest = source</summary>
            SRCCOPY = 0x00CC0020,
            /// <summary>dest = source OR dest</summary>
            SRCPAINT = 0x00EE0086,
            /// <summary>dest = source AND dest</summary>
            SRCAND = 0x008800C6,
            /// <summary>dest = source XOR dest</summary>
            SRCINVERT = 0x00660046,
            /// <summary>dest = source AND (NOT dest)</summary>
            SRCERASE = 0x00440328,
            /// <summary>dest = (NOT source)</summary>
            NOTSRCCOPY = 0x00330008,
            /// <summary>dest = (NOT src) AND (NOT dest)</summary>
            NOTSRCERASE = 0x001100A6,
            /// <summary>dest = (source AND pattern)</summary>
            MERGECOPY = 0x00C000CA,
            /// <summary>dest = (NOT source) OR dest</summary>
            MERGEPAINT = 0x00BB0226,
            /// <summary>dest = pattern</summary>
            PATCOPY = 0x00F00021,
            /// <summary>dest = DPSnoo</summary>
            PATPAINT = 0x00FB0A09,
            /// <summary>dest = pattern XOR dest</summary>
            PATINVERT = 0x005A0049,
            /// <summary>dest = (NOT dest)</summary>
            DSTINVERT = 0x00550009,
            /// <summary>dest = BLACK</summary>
            BLACKNESS = 0x00000042,
            /// <summary>dest = WHITE</summary>
            WHITENESS = 0x00FF0062
        }

        public enum UI32SC : int
        {
            /// <summary>
            /// Закрывает окно.
            /// </summary>
            SC_CLOSE = 0xF060,
            /// <summary>
            /// Изменяет курсор на вопросительный знак с указателем. Если пользователь щелкает элемент 
            /// управления в диалоговом окне, элемент управления получает сообщение WM_HELP.
            /// </summary>
            SC_CONTEXTHELP = 0xF180,
            /// <summary>
            /// Выбирает элемент по умолчанию, пользователь дважды щелкнул в меню окна.
            /// </summary>
            SC_DEFAULT = 0xF160,
            /// <summary>
            /// Активизирует окно, связанное с приложением указанной горячей клавиши. LPARAM определяет окно активации.
            /// </summary>
            SC_HOTKEY = 0xF150,
            /// <summary>
            /// Горизонтальная прокрутка
            /// </summary>
            SC_HSCROLL = 0xF080,
            /// <summary>
            /// Указывает что экранная заставка является безопасной.
            /// </summary>
            SCF_ISSECURE = 0x00000001,
            /// <summary>
            /// Получает окно меню в результате нажатия клавиши.
            /// </summary>
            SC_KEYMENU = 0xF100,
            /// <summary>
            /// Разворачивает окно.
            /// </summary>
            SC_MAXIMIZE = 0xF030,
            /// <summary>
            /// Сворачивает окно.
            /// </summary>
            SC_MINIMIZE = 0xF020,
            /// <summary>
            /// Устанавливает состояние дисплея. Эта команда поддерживает устройства, которые имеют функции 
            /// энергосбережения, такие как батарейным питанием персональный компьютер.
            /// LPARAM может принимать следующие значения:
            /// -1 (Дисплей включением)
            /// 1 (на дисплее будет малой мощности)
            /// 2 (дисплей отключаться)
            /// </summary>
            SC_MONITORPOWER = 0xF170,
            /// <summary>
            /// Получает окно меню в результате щелчка мышью.
            /// </summary>
            SC_MOUSEMENU = 0xF090,
            /// <summary>
            /// Перемещает окно.
            /// </summary>
            SC_MOVE = 0xF010,
            /// <summary>
            /// Переход к следующему окну.
            /// </summary>
            SC_NEXTWINDOW = 0xF040,
            /// <summary>
            /// Переход к предыдущему окну.
            /// </summary>
            SC_PREVWINDOW = 0xF050,
            /// <summary>
            /// Восстанавливает окно в нормальное положение и размер.
            /// </summary>
            SC_RESTORE = 0xF120,
            /// <summary>
            /// Выполняет заставку экрана указано в [Boot] раздел файла System.ini.
            /// </summary>
            SC_SCREENSAVE = 0xF140,
            /// <summary>
            /// Размеры окна.
            /// </summary>
            SC_SIZE = 0xF000,
            /// <summary>
            /// Активизирует меню Пуск.
            /// </summary>
            SC_TASKLIST = 0xF130,
            /// <summary>
            /// Веритикалый скролл
            /// </summary>
            SC_VSCROLL = 0xF070
        }


        public enum UI32WM : int
        {
            /// <summary>
            /// Окно получает это сообщение, когда пользователь выбирает команду из меню 
            /// Окно (ранее известная как система меню или управления) или когда пользователь 
            /// выбирает кнопку максимизации, минимизации кнопку, кнопку восстановления, или кнопку Закрыть.
            /// </summary>
            WM_SYSCOMMAND = 0x112,
            WM_MOVE = 0x0003
        }

        public enum MouseEvent
        {
            MOUSEEVENTF_LEFTDOWN = 0x02,
            MOUSEEVENTF_LEFTUP = 0x04,
            MOUSEEVENTF_RIGHTDOWN = 0x08,
            MOUSEEVENTF_RIGHTUP = 0x10,
        }

        public enum ProcessAccess : uint
        {
            /// <summary>
            /// Required to terminate a process using TerminateProcess.
            /// </summary>
            Terminate = 0x1,

            /// <summary>
            /// Required to create a thread.
            /// </summary>
            CreateThread = 0x2,

            /// <summary>
            /// Undocumented.
            /// </summary>
            SetSessionId = 0x4,

            /// <summary>
            /// Required to perform an operation on the address space of a process (see VirtualProtectEx and WriteProcessMemory).
            /// </summary>
            VmOperation = 0x8,

            /// <summary>
            /// Required to read memory in a process using ReadProcessMemory.
            /// </summary>
            VmRead = 0x10,

            /// <summary>
            /// Required to write to memory in a process using WriteProcessMemory.
            /// </summary>
            VmWrite = 0x20,

            /// <summary>
            /// Required to duplicate a handle using DuplicateHandle.
            /// </summary>
            DupHandle = 0x40,

            /// <summary>
            /// Required to create a process.
            /// </summary>
            CreateProcess = 0x80,

            /// <summary>
            /// Required to set memory limits using SetProcessWorkingSetSize.
            /// </summary>
            SetQuota = 0x100,

            /// <summary>
            /// Required to set certain information about a process, such as its priority class (see SetPriorityClass).
            /// </summary>
            SetInformation = 0x200,

            /// <summary>
            /// Required to retrieve certain information about a process, such as its token, exit code, and priority class (see OpenProcessToken, GetExitCodeProcess, GetPriorityClass, and IsProcessInJob).
            /// </summary>
            QueryInformation = 0x400,

            /// <summary>
            /// Undocumented.
            /// </summary>
            SetPort = 0x800,

            /// <summary>
            /// Required to suspend or resume a process.
            /// </summary>
            SuspendResume = 0x800,

            /// <summary>
            /// Required to retrieve certain information about a process (see QueryFullProcessImageName). A handle that has the PROCESS_QUERY_INFORMATION access right is automatically granted PROCESS_QUERY_LIMITED_INFORMATION.
            /// </summary>
            QueryLimitedInformation = 0x1000,

            /// <summary>
            /// Required to wait for the process to terminate using the wait functions.
            /// </summary>
            Synchronize = 0x100000
        }

 
 

}
