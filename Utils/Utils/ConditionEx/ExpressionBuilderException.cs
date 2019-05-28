using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.ConditionEx
{
    public class ExpressionBuilderException : Exception
    {
        internal ExpressionBuilderException(string message) : base(message)
        {

        }
    }
}
