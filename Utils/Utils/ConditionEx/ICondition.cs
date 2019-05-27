using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.ConditionEx
{
    public interface ICondition
    {
        bool ConditionResult { get; }
        string StringResult { get; }
    }
}
