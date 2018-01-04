using System.Collections.Generic;

namespace TFSGeneration.Control.ConditionEx.Collections
{
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
            foreach (ConditionBlock c in this)
            {
                if (!c.ResultCondition)
                {
                    return false;
                }
            }
            return true;
        }

    }
}
