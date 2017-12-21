using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UIPresentationControls.Utils
{
    internal enum UI32WM : int
    {
        /// <summary>
        /// Окно получает это сообщение, когда пользователь выбирает команду из меню 
        /// Окно (ранее известная как система меню или управления) или когда пользователь 
        /// выбирает кнопку максимизации, минимизации кнопку, кнопку восстановления, или кнопку Закрыть.
        /// </summary>
        WM_SYSCOMMAND = 0x112,
        WM_MOVE = 0x0003
    }

    internal enum UI32SC : int
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

    internal class Win32Controls
    {
        const int WM_SYSCOMMAND = 0x0112;
        const int SC_MOVE = 0xF010;
        public static IntPtr WndProc (IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
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

        public static void DragSize(IntPtr handle, SizingAction sizingAction)
        {
            SendMessage(handle, (int)UI32WM.WM_SYSCOMMAND, (IntPtr)((int)UI32SC.SC_SIZE + sizingAction), IntPtr.Zero);
            SendMessage(handle, 514, IntPtr.Zero, IntPtr.Zero);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32", SetLastError = true)]
        public static extern int MoveWindow(IntPtr hwnd, IntPtr x, IntPtr y, IntPtr nWidth, IntPtr nHeight, bool bRepaint);
    }
}
