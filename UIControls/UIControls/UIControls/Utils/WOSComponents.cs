namespace UIControls.Utils
{
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

    public enum TernaryRasterOperations : uint
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
