namespace Utils.ConditionEx
{
    public enum LogicalGroupType
    {
        /// <summary>
        /// ничего
        /// </summary>
        NaN = 0,
        /// <summary>
        /// условие И между условными операторами
        /// </summary>
        And = 1,
        /// <summary>
        /// условие ИЛИ между условными операторами
        /// </summary>
        Or = 2,
        /// <summary>
        /// начало групповово условия (скобка открыта)
        /// </summary>
        BracketIsOpen = 4,
        /// <summary>
        /// конец групповово условия (скобка закрыта)
        /// </summary>
        BracketIsClose = 8
    }
}
