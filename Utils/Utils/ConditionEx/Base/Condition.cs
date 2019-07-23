using System;
using Utils.ConditionEx.Collections;

namespace Utils.ConditionEx.Base
{
    public class Condition : ICondition
    {
        /// <summary>
        /// Родительское выражение
        /// </summary>
        internal ConditionBlock Parent { get; }

        /// <summary>
        /// Условие выражения
        /// </summary>
        public ConditionOperatorType Operator { get; }

        /// <summary>
        /// Первый параметр условия выражения
        /// </summary>
        public DynamicParameter FirstParam { get; }

        /// <summary>
        /// Второй параметр условия выражения
        /// </summary>
        public DynamicParameter SecondParam { get; }

        /// <summary>
        /// Определяем результат выражения текущих параметров
        /// </summary>
        public bool ConditionResult
        {
            get
            {
                var firstType = FirstParam.Result.Type;
                var firstValue = FirstParam.Result.Value;

                var secondType = SecondParam.Result.Type;
                var secondValue = SecondParam.Result.Value;

                switch (firstType & secondType)
                {
                    // если какой то из параметров или оба являются null
                    case TypeParam.Null when firstType == secondType && (Operator == ConditionOperatorType.Equal || Operator == ConditionOperatorType.Like):
                    case TypeParam.Null when firstType != secondType && (Operator == ConditionOperatorType.NotEqual || Operator == ConditionOperatorType.NotLike):
                        return true;
                    case TypeParam.Null:
                        return false;
                    //проверяем если оба параметра числовые
                    case TypeParam.Number when Operator != ConditionOperatorType.Like && Operator != ConditionOperatorType.NotLike:
                        {
                            var temp = EqualsNumbers(firstValue, secondValue);

                            switch (Operator)
                            {
                                case ConditionOperatorType.EqualOrGreaterThan when (temp == ConditionOperatorType.Equal || temp == ConditionOperatorType.GreaterThan):
                                case ConditionOperatorType.EqualOrLessThan when (temp == ConditionOperatorType.Equal || temp == ConditionOperatorType.LessThan):
                                case ConditionOperatorType.NotEqual when temp != ConditionOperatorType.Equal:
                                    return true;
                                case ConditionOperatorType.NotEqual:
                                    return false;
                            }

                            return Operator == temp;
                        }
                    default:
                        // проверяем остальные типы
                        switch (Operator)
                        {
                            case ConditionOperatorType.Equal when firstValue == secondValue && firstType == secondType:
                            case ConditionOperatorType.NotEqual when firstValue != secondValue || firstType != secondType:
                                return true;
                            case ConditionOperatorType.Like:
                                return STRING.IsObjectsSimilar(firstValue, secondValue);
                            case ConditionOperatorType.NotLike:
                                return !STRING.IsObjectsSimilar(firstValue, secondValue);
                            default:
                                return false;
                        }
                }
            }
        }

        public string StringResult
        {
            get
            {
                var firstType = FirstParam.Result.Type;
                var firstValue = FirstParam.Result.Value;

                var secondType = SecondParam.Result.Type;
                var secondValue = SecondParam.Result.Value;

                return $"{firstType:G}[{firstValue}]{GetStringOperator(Operator)}{secondType:G}[{secondValue}]";
            }
        }

        internal Condition(ConditionBlock parent, string firstParam, string secondParam, ConditionOperatorType @operator, Expression @base)
        {
            Parent = parent;
            FirstParam = new DynamicParameter(firstParam, @base);
            SecondParam = new DynamicParameter(secondParam, @base);
            Operator = @operator;
        }

        internal static ConditionOperatorType GetOperator(string @operator)
        {
            switch (@operator)
            {
                case ">=": return ConditionOperatorType.EqualOrGreaterThan;
                case "<=": return ConditionOperatorType.EqualOrLessThan;
                case "=":
                case "==": return ConditionOperatorType.Equal;
                case "<>":
                case "!=": return ConditionOperatorType.NotEqual;
                case ">": return ConditionOperatorType.GreaterThan;
                case "<": return ConditionOperatorType.LessThan;
                case "^=": return ConditionOperatorType.Like;
                case "!^=": return ConditionOperatorType.NotLike;
                default: return ConditionOperatorType.Unknown;
            }
        }

        internal static string GetStringOperator(ConditionOperatorType @operator)
        {
            switch (@operator)
            {
                case ConditionOperatorType.EqualOrGreaterThan:
                    return ">=";
                case ConditionOperatorType.EqualOrLessThan:
                    return "<=";
                case ConditionOperatorType.Equal:
                    return "=";
                case ConditionOperatorType.NotEqual:
                    return "!=";
                case ConditionOperatorType.GreaterThan:
                    return ">";
                case ConditionOperatorType.LessThan:
                    return "<";
                case ConditionOperatorType.Like:
                    return "^=";
                case ConditionOperatorType.NotLike:
                    return "!^=";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Выполняем вырежение если оба параметра числовые
        /// </summary>
        static ConditionOperatorType EqualsNumbers(string param1, string param2)
        {
            var sResult = ConditionOperatorType.NotEqual;
            var firstp = double.Parse(param1);
            var secondp = double.Parse(param2);

            if (Math.Abs(firstp - secondp) > 0)
            {
                if (firstp > secondp)
                    sResult = ConditionOperatorType.GreaterThan;
                else if (firstp < secondp)
                    sResult = ConditionOperatorType.LessThan;
            }
            else
            {
                sResult = ConditionOperatorType.Equal;
            }
            return sResult;
        }

        public override string ToString()
        {
            return StringResult;
        }
    }
}
