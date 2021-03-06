namespace TFSAssist.Remoter
{
    public enum RemoteControlCommands
    {
        COMMANDS,

        PING,
        INFO,        
        DRIVE,
        CPU,
        LOC,
        CLEAR,


        MEDIA,
        RECSETT,
        ACAM,
        ECAM,
        BROADCAST,
        SCAM,
        NCAM,
        RECSTOP,


        SCREEN,
        PHOTO,



        //!!!!!!!!!!!!!!!!!!!!!!!! Данные команды относятся к MainWindow !!!!!!!!!!!!!!!!!!!!!!!!
        /// <summary>
        /// Получить логи родительской и текущей обработки
        /// </summary>
        LOG,
        /// <summary>
        /// Проверить на обновления, если есть то обновить
        /// </summary>
        UPDATE,
        /// <summary>
        /// Перезапустить приложение
        /// </summary>
        RESTART,

        /// <summary>
        /// Очистить логи
        /// </summary>
        CLEARLOG,
        /// <summary>
        /// Start или Stop приложения TFSControl
        /// </summary>
        TFSCOMM,
        /// <summary>
        /// Получить основные настройки TFSControl
        /// </summary>
        TFSETT,
        /// <summary>
        /// Минимизировать окно десктопа
        /// </summary>
        WINSTATE
    }
}
