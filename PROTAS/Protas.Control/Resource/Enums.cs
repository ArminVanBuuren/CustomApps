namespace Protas.Control.Resource
{
    public enum FinalResultMode
    {
        All = 0,
        Unique = 1
    }
    /// <summary>
    /// Режим работы ресурса
    /// </summary>
    public enum ResourceMode
    {
        /// <summary>
        /// Вызывать результат непосредтвенно из вне класса ResourceKernel. Самостоятельное вызывание результата непосредственного ресурса
        /// </summary>
        External = 0,
        /// <summary>
        /// Запуск ресурса в режиме ядра. Автоматическое инициирования эвента, при изменении результата непосредственного ресурса
        /// </summary>
        Core = 1
    }
    public enum HintResourceType
    {
        None = 0,
        /// <summary>
        /// если мы должны восоздать ресурс согласно пределенному конструктору
        /// </summary>
        IsBoundProperty = 1,
        /// <summary>
        /// если для работы ресурса не нужен входной конструктор, то создается конструктор сразу
        /// </summary>
        IsIResource = 2
    }

 
}