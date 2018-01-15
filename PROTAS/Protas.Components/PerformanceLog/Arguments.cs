
namespace Protas.Components.PerformanceLog
{
    public enum Log3NetSeverity
    {
        Disable = 0,
        /// <summary>
        /// критические ошибки
        /// </summary>
        Fatal = 1,
        /// <summary>
        /// остальные ошибки
        /// </summary>
        Error = 2,
        /// <summary>
        /// сбор статистики
        /// </summary>
        Report = 3,
        /// <summary>
        /// не значительные на процессор трассировки
        /// </summary>
        Normal = 4,
        /// <summary>
        /// Предостережение
        /// </summary>
        Warning = 5,
        /// <summary>
        /// отлаживающие трассировки включая програмные ошибки и остальные взаимосвязанные с другими платформами
        /// </summary>
        Debug = 6,
        /// <summary>
        /// ошибочные трассировки самого кода программы
        /// </summary>
        MaxErr = 7,
        /// <summary>
        /// максимальные трассировки включая кучу операций выполняющие в цикле
        /// </summary>
        Max = 8
    }
    public interface IObjectStatistic
    {
        void GetStatistics();
    }



}
