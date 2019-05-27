using System.Collections.Generic;

namespace Utils.ConditionEx.Collections
{
    public class ExpressionCollection : List<Expression>
    {
        private readonly Expression _parent;
        internal ExpressionCollection(Expression bc)
        {
            _parent = bc;
        }

        internal void AddChild(Expression bc)
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
                if (!ifTarget.ConditionResult)
                {
                    return false;
                }
            }
            return true;
        }

        internal void RemoveChild(Expression bc)
        {
            Remove(bc);
            bc.Parent = null;
        }
    }
}
