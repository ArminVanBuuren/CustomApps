namespace Utils.ConditionEx
{
    /// <summary>
    /// Значение условий
    /// </summary>
    public enum ConditionOperatorType
    {
        /// <summary>
        /// Неизвестно
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Равно
        /// </summary>
        Equal = 1,
        /// <summary>
        /// Больше или равно
        /// </summary>
        EqualOrGreaterThan = 2,
        /// <summary>
        /// Меньше или равно
        /// </summary>
        EqualOrLessThan = 3,
        /// <summary>
        /// Не Равно
        /// </summary>
        NotEqual = 4,
        /// <summary>
        /// Больше чем
        /// </summary>
        GreaterThan = 5,
        /// <summary>
        /// Меньше
        /// </summary>
        LessThan = 6,
        /// <summary>
        /// в тексте присутствуют значения которые подобные
        /// </summary>
        Like = 7,
        /// <summary>
        /// в тексте присутствуют значения которые не должны быть подобными
        /// </summary>
        NotLike = 8
    }
}
