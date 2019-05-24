using System.Collections.Generic;

namespace Utils.ConditionEx.Collections
{
    internal class IfTargetCollection : List<IfTarget>
    {
        private readonly IfTarget _parent;
        public IfTargetCollection(IfTarget bc)
        {
            _parent = bc;
        }

        public void AddChild(IfTarget bc)
        {
            if (bc == null)
                return;

            bc.Parent = _parent;
            Insert(Count, bc);
        }

        public bool GetResultCondition()
        {
            foreach (var ifTarget in this)
            {
                if (!ifTarget.ResultCondition)
                {
                    return false;
                }
            }
            return true;
        }

        public void RemoveChild(IfTarget bc)
        {
            Remove(bc);
            bc.Parent = null;
        }
    }
}
