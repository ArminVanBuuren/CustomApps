using System.Collections.Generic;

namespace Utils.ConditionEx.Collections
{
    /// <inheritdoc />
    /// <summary>
    /// колличество условных блоков в связке and или or
    /// </summary>
    internal class ConditionBlockCollection : List<ConditionBlock>
    {
        readonly IfTarget _parent;
        public ConditionBlockCollection(IfTarget bc)
        {
            _parent = bc;
        }

        public void AddChild(ConditionBlock c)
        {
            if (c == null)
                return;
            c.Parent = _parent;
            Insert(Count, c);
        }

        public bool GetResultCondition()
        {
            foreach (var conditionBlock in this)
            {
                if (!conditionBlock.ResultCondition)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
