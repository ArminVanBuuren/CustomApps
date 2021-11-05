using System.Collections.Generic;

namespace Utils.ConditionEx.Collections
{
    /// <summary>
    /// колличество условных блоков в связке and или or
    /// </summary>
    public class ConditionCollection : List<ConditionBlock>, IExpression, ICondition
    {
        public Expression Parent { get; }

        public bool IsValid
        {
            get
            {
                foreach (IExpression condition in this)
                    if (!condition.IsValid)
                        return false;

                return true;
            }
        }

        public bool ConditionResult
        {
            get
            {
                foreach (var condition in this)
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
                var i = 0;
                var result = string.Empty;
                foreach (ICondition condition in this)
                {
                    i++;
                    if (i == Count)
                    {
                        result = result + condition.StringResult;
                        break;
                    }
                    result = $"{result}{condition.StringResult} AND ";
                }

                return result;
            }
        }

        internal ConditionCollection(Expression parent)
        {
            Parent = parent;
        }

        internal void AddChild(ConditionBlock condition)
        {
            if (condition == null)
                return;

            condition.Parent = Parent;
            Insert(Count, condition);
        }

        public override string ToString()
        {
            return StringResult;
        }
    }
}
