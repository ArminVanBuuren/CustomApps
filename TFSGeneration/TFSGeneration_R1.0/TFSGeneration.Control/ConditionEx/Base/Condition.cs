using System;
using TFSAssist.Control.ConditionEx.Collections;
using TFSAssist.Control.ConditionEx.Utils;

namespace TFSAssist.Control.ConditionEx.Base
{
    internal class Condition
    {
        /// <summary>
        /// Родительское выражение
        /// </summary>
        internal ConditionBlock Parent { get; }
        public Condition(ConditionBlock parent, string firstParam, string secondParam, ConditionOperator _operator)
        {
            Parent = parent;
            ParamFirst = new ConditionParameter(firstParam);
            ParamSecond = new ConditionParameter(secondParam);
            Operator = _operator;
        }

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

        /// <summary>
        /// Определяем результат выражения текущих параметров
        /// </summary>
        bool EvaluateCurrentParams()
        {
            Parameter pRes1 = ParamFirst.DynamicParam;
            Parameter pRes2 = ParamSecond.DynamicParam;
            string pStr1 = pRes1.Value;
            string pStr2 = pRes2.Value;
            TypeParam pType1 = pRes1.Type;
            TypeParam pType2 = pRes2.Type;
            //проверяем если параметры типа number
            if (pType1 == TypeParam.Number && pType2 == TypeParam.Number && Operator != ConditionOperator.Like && Operator != ConditionOperator.NotLike)
            {
                ConditionOperator temp = EqualsNumbers(pStr1, pStr2);
                if (Operator == ConditionOperator.EqualOrGreaterThan && (temp == ConditionOperator.Equal || temp == ConditionOperator.GreaterThan))
                {
                    return true;
                }
                if (Operator == ConditionOperator.EqualOrLessThan && (temp == ConditionOperator.Equal || temp == ConditionOperator.LessThan))
                {
                    return true;
                }
                if (Operator == ConditionOperator.NotEqual)
                {
                    if (temp != ConditionOperator.Equal)
                    {
                        return true;
                    }
                    return false;
                }
                if (Operator == temp)
                {
                    return true;
                }
                return false;
            }

            //проверяем если параметры типа string или bool
            if (Operator == ConditionOperator.Equal)
            {
                if (pStr1 == pStr2 && pType1 == pType2)
                {
                    return true;
                }
            }
            if (Operator == ConditionOperator.NotEqual)
            {
                if (pStr1 != pStr2 || pType1 != pType2)
                {
                    return true;
                }
            }
            if (!ParamsIsNotNull(pType1, pType2) || !ParamsIsNotNull(pType2, pType1))
            {
                return false;
            }
            if (Operator == ConditionOperator.Like)
            {
                return IsLikesParams(pStr1, pStr2);
            }
            if (Operator == ConditionOperator.NotLike)
            {
                return !IsLikesParams(pStr1, pStr2);
            }
            return false;
        }

	    static bool IsLikesParams(string param1, string param2)
        {
            return StaffFunk.IsObjectsSimilar(param1, param2);
        }

	    static bool ParamsIsNotNull(TypeParam pType1, TypeParam pType2)
        {
            if (pType1 == TypeParam.Null && pType2 != TypeParam.Null) return false;
            return true;
        }
        /// <summary>
        /// Выполняем вырежение если оба параметра типа int
        /// </summary>
        static ConditionOperator EqualsNumbers(string param1, string param2)
        {
            ConditionOperator sResult = ConditionOperator.NotEqual;
            double firstp = double.Parse(param1);
            double secondp = double.Parse(param2);

            if (Math.Abs(firstp - secondp) > 0)
            {
                if (firstp > secondp)
                {
                    sResult = ConditionOperator.GreaterThan;
                }
                else if (firstp < secondp)
                {
                    sResult = ConditionOperator.LessThan;
                }
            }
            else
            {
                sResult = ConditionOperator.Equal;
            }
            return sResult;
        }

    }
}
