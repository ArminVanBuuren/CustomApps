using Utils.ConditionEx.Utils;

namespace Utils.ConditionEx.Base
{
    internal class ComponentCondition
    {
        public static ConditionOperator GetOperator(string str)
        {
            switch (StaffFunk.ReplaceXmlSpecSymbls(str, 0))
            {
                case ">=": return ConditionOperator.EqualOrGreaterThan;
                case "<=": return ConditionOperator.EqualOrLessThan;
                case "=":
                case "==": return ConditionOperator.Equal;
                case "<>":
                case "!=": return ConditionOperator.NotEqual;
                case ">": return ConditionOperator.GreaterThan;
                case "<": return ConditionOperator.LessThan;
                case "^=": return ConditionOperator.Like;
                case "!^=": return ConditionOperator.NotLike;
                default: return ConditionOperator.Unknown;
            }
        }

        public static string GetOperator(ConditionOperator s)
        {
            switch (s)
            {
                case ConditionOperator.EqualOrGreaterThan:
                    return ">=";
                case ConditionOperator.EqualOrLessThan:
                    return "<=";
                case ConditionOperator.Equal:
                    return "=";
                case ConditionOperator.NotEqual:
                    return "!=";
                case ConditionOperator.GreaterThan:
                    return ">";
                case ConditionOperator.LessThan:
                    return "<";
                case ConditionOperator.Like:
                    return "^=";
                case ConditionOperator.NotLike:
                    return "!^=";
                default:
                    return string.Empty;
            }
        }

        public static LogicalGroup GetLogicalGroup(string str)
        {
            switch (StaffFunk.ReplaceXmlSpecSymbls(str.ToLower(), 0))
            {
                case "&&":
                case "and": return LogicalGroup.And;
                case "||":
                case "or": return LogicalGroup.Or;
                case "(": return LogicalGroup.OpenBkt;
                case ")": return LogicalGroup.CloseBkt;
            }
            return LogicalGroup.NaN;
        }
    }

    internal enum LogicalGroup
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
        OpenBkt = 3,
        /// <summary>
        /// конец групповово условия (скобка закрыта)
        /// </summary>
        CloseBkt = 4
    }
    /// <summary>
    /// Значение условий
    /// </summary>
    internal enum ConditionOperator
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