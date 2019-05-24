using System;
using Utils.ConditionEx.Collections;
using static Utils.TYPES;

namespace Utils.ConditionEx.Base
{
    internal class Condition
    {
        /// <summary>
        /// Родительское выражение
        /// </summary>
        internal ConditionBlock Parent { get; }

        /// <summary>
        /// Условие выражения
        /// </summary>
        public ConditionOperator Operator { get; }

        /// <summary>
        /// Первый параметр условия выражения
        /// </summary>
        public ConditionParameter ParamFirst { get; }

        /// <summary>
        /// Второй параметр условия выражения
        /// </summary>
        public ConditionParameter ParamSecond { get; }

        /// <summary>
        /// Результат выражения
        /// </summary>
        public bool ResultCondition => EvaluateCurrentParams();


        public Condition(ConditionBlock parent, string firstParam, string secondParam, ConditionOperator @operator)
        {
            Parent = parent;
            ParamFirst = new ConditionParameter(firstParam);
            ParamSecond = new ConditionParameter(secondParam);
            Operator = @operator;
        }

        /// <summary>
        /// Определяем результат выражения текущих параметров
        /// </summary>
        bool EvaluateCurrentParams()
        {
            var firstValue = ParamFirst.DynamicParam.Value;
            var secondValue = ParamSecond.DynamicParam.Value;
            var firstType = ParamFirst.DynamicParam.Type;
            var secondType = ParamSecond.DynamicParam.Type;

            switch (firstType & secondType)
            {
                // если какой то из параметров или оба являются null
                case TypeParam.Null when firstType == secondType && (Operator == ConditionOperator.Equal || Operator == ConditionOperator.Like):
                case TypeParam.Null when firstType != secondType && (Operator == ConditionOperator.NotEqual || Operator == ConditionOperator.NotLike):
                    return true;
                case TypeParam.Null:
                    return false;
                //проверяем если оба параметра числовые
                case TypeParam.Number when Operator != ConditionOperator.Like && Operator != ConditionOperator.NotLike:
                {
                    var temp = EqualsNumbers(firstValue, secondValue);

                    switch (Operator)
                    {
                        case ConditionOperator.EqualOrGreaterThan when (temp == ConditionOperator.Equal || temp == ConditionOperator.GreaterThan):
                        case ConditionOperator.EqualOrLessThan when (temp == ConditionOperator.Equal || temp == ConditionOperator.LessThan):
                        case ConditionOperator.NotEqual when temp != ConditionOperator.Equal:
                            return true;
                        case ConditionOperator.NotEqual:
                            return false;
                    }

                    return Operator == temp;
                }
                default:
                    // проверяем остальные типы
                    switch (Operator)
                    {
                        case ConditionOperator.Equal when firstValue == secondValue && firstType == secondType:
                        case ConditionOperator.NotEqual when firstValue != secondValue || firstType != secondType:
                            return true;
                        case ConditionOperator.Like:
                            return STRING.IsObjectsSimilar(firstValue, secondValue);
                        case ConditionOperator.NotLike:
                            return !STRING.IsObjectsSimilar(firstValue, secondValue);
                        default:
                            return false;
                    }
            }
        }

        /// <summary>
        /// Выполняем вырежение если оба параметра числовые
        /// </summary>
        static ConditionOperator EqualsNumbers(string param1, string param2)
        {
            var sResult = ConditionOperator.NotEqual;
            var firstp = double.Parse(param1);
            var secondp = double.Parse(param2);

            if (Math.Abs(firstp - secondp) > 0)
            {
                if (firstp > secondp)
                    sResult = ConditionOperator.GreaterThan;
                else if (firstp < secondp)
                    sResult = ConditionOperator.LessThan;
            }
            else
            {
                sResult = ConditionOperator.Equal;
            }
            return sResult;
        }
    }
}
