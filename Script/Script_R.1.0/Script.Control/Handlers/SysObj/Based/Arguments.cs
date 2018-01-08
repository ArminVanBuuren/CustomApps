using System;

namespace Script.Control.Handlers.SysObj.Based
{
    /// <summary>
    /// тип выполнения операции с файлами или папками
    /// </summary>
    [Flags]
    public enum ProcessingOptions
    {
        None = 0,
        Copy = 1,
        Replace = 2,
        Move = 4,
        Delete = 8
    }
    public enum FindType
    {
        All = 0,
        Directories = 1,
        Files = 2
    }


}
