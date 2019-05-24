using System.Collections.Generic;
using Utils.ConditionEx.Base;
using static Utils.TYPES;

namespace Utils.ConditionEx.Collections
{
    internal class ConditionBlock : List<Condition>
    {
        internal IfTarget Parent { get; set; }

        public void AddChild(string firstParam, string secondParam, ConditionOperator _operator)
        {
            if (Count == 0)
                Add(new Condition(this, firstParam, secondParam, _operator));
            else
                Add(new Condition(this, this[Count - 1].ParamSecond.SourceParam, secondParam, _operator));
        }

        /// <summary>
        /// функция работает правильно. НЕ МЕНЯТЬ!!! условие должно быть от большего к меньшему или наоборот (период например как between)
        /// например: '2' больше чем '1' больше чем '0' или '1' меньше чем '2' меньше чем '3'
        /// каждый из блоков должен быть true
        /// </summary>
        /// <returns></returns>
        public bool ResultCondition
        {
            get
            {
                foreach (var condition in this)
                {
                    if (!condition.ResultCondition)
                        return false;
                }
                return true;
            }
        }

        public string StringConstructor
        {
            get
            {
                string result = string.Empty;
                int i = 0;
                foreach (var condition in this)
                {
                    if (condition.Operator == ConditionOperator.Unknown)
                    {
                        continue;
                    }
                    var first = condition.ParamFirst.DynamicParam;
                    var second = condition.ParamSecond.DynamicParam;

                    i++;
                    if (i > 1)
                    {
                        result = result + $@"{ComponentCondition.GetOperator(condition.Operator)}{second.Type:G}[{second.Value}]";
                        continue;
                    }

                    result = result + $@"{first.Type:G}[{first.Value}]{ComponentCondition.GetOperator(condition.Operator)}{second.Type:G}[{second.Value}]";
                }
                return result;
            }
        }

    }
}
