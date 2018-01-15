using System.Collections.Generic;

namespace Protas.Components.ConditionEx.Collections
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
            if (bc != null)
            {
                bc.Parent = _parent;
                Insert(Count, bc);
            }
        }
        public bool GetResultCondition()
        {
            foreach (IfTarget bc in this)
            {
                if (!bc.ResultCondition)
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
