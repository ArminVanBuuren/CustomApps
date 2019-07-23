namespace Utils.ConditionEx
{
    public interface IExpression
    {
        Expression Parent { get; }
        
        /// <summary>
        /// Проверяется валидность самого вырожения
        /// </summary>
        bool IsValid { get; }
    }
}
