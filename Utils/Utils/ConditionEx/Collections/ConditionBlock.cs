using System.Collections.Generic;
using Utils.ConditionEx.Base;

namespace Utils.ConditionEx.Collections
{
    public class ConditionBlock : List<Condition>, IExpression, ICondition
    {
        public Expression Parent { get; internal set; }

        public bool IsValid
        {
            get
            {
                foreach (var condition in this)
                    if (condition.Operator == ConditionOperatorType.Unknown)
                        return false;

                return true;
            }
        }

        /// <summary>
        /// функция работает правильно. НЕ МЕНЯТЬ!!! условие должно быть от большего к меньшему или наоборот (период например как between)
        /// например: '2' больше чем '1' больше чем '0' или '1' меньше чем '2' меньше чем '3'
        /// каждый из блоков должен быть true
        /// </summary>
        /// <returns></returns>
        public bool ConditionResult
        {
            get
            {
                foreach (ICondition condition in this)
                {
                    if (!condition.ConditionResult)
                        return false;
                }
                return true;
            }
        }

        public string StringResult
        {
            get
            {
                var result = string.Empty;
                var i = 0;
                foreach (var condition in this)
                {
                    if (condition.Operator == ConditionOperatorType.Unknown)
                        continue;

                    i++;
                    if (i > 1)
                    {
                        var second = condition.SecondParam.Result;
                        result = result + $@"{Condition.GetStringOperator(condition.Operator)}{second.Type:G}[{second.Value}]";
                        continue;
                    }

                    result = result + condition.StringResult;
                }
                return result;
            }
        }

        internal void AddChild(string firstParam, string secondParam, ConditionOperatorType _operator, Expression @base)
        {
            if (Count == 0)
                Add(new Condition(this, firstParam, secondParam, _operator, @base));
            else
                Add(new Condition(this, this[Count - 1].SecondParam.Source, secondParam, _operator, @base));
        }

        public override string ToString()
        {
            return StringResult;
        }
    }
}
