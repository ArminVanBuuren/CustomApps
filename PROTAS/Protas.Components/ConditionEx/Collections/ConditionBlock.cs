using System.Collections.Generic;
using Protas.Components.ConditionEx.Base;
using Protas.Components.PerformanceLog;
using Protas.Components.Types;

namespace Protas.Components.ConditionEx.Collections
{
    internal class ConditionBlock : List<Condition>
    {
        internal IfTarget Parent { get; set; }
        ShellLog3Net _log3;
        public ConditionBlock(ShellLog3Net log)
        {
            _log3 = log;
        }

        public void AddChild(string firstParam, string secondParam, ConditionOperator _operator)
        {
            if (Count == 0)
                Add(new Condition(this, firstParam, secondParam, _operator, _log3));
            else
                Add(new Condition(this, this[Count - 1].ParamSecond.SourceParam, secondParam, _operator, _log3));
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
                foreach (Condition p in this)
                {
                    if (!p.ResultCondition)
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
                foreach (Condition p in this)
                {
                    if (p.Operator == ConditionOperator.Unknown)
                    {
                        continue;
                    }
                    Parameter pRes1 = p.ParamFirst.DynamicParam;
                    Parameter pRes2 = p.ParamSecond.DynamicParam;

                    i++;
                    if (i > 1)
                    {
                        result = result + string.Format(@"{0}{1}[{2}]",
                            ComponentCondition.GetOperator(p.Operator),
                            GetTypeString(pRes2),
                            pRes2.Value);
                        continue;
                    }
                    result = result + string.Format(@"{0}[{1}]{2}{3}[{4}]",
                        GetTypeString(pRes1),
                        pRes1.Value,
                        ComponentCondition.GetOperator(p.Operator),
                        GetTypeString(pRes2),
                        pRes2.Value);
                }
                return result;
            }
        }

        string GetTypeString(Parameter pt)
        {
            switch (pt.Type)
             {
                 case TypeParam.Number: return "Num";
                 case TypeParam.Bool: return "Bool";
                 case TypeParam.String: return "String";
                 case TypeParam.MathEx: return "MathEx";
                 default: return "Null";
             }
        }



    }
}
