using System;

namespace Utils.ConditionEx
{
    public class DynamicObject
    {
        internal event EventHandler ObjectChanged;
        public Func<string, string> GetValue { get; }

        public DynamicObject(Func<string, string> function)
        {
            GetValue = function ?? throw new ArgumentException(nameof(function));
        }

        public void Elapsed()
        {
            ObjectChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
