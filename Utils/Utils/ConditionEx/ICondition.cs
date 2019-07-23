namespace Utils.ConditionEx
{
    public interface ICondition
    {
        /// <summary>
        /// Результат вычисления вырожения
        /// </summary>
        bool ConditionResult { get; }
        
        /// <summary>
        /// Текстовый результат логических группировок вырожения
        /// </summary>
        string StringResult { get; }
    }
}
