using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.ConditionEx
{
    public interface IExpression
    {
        Expression Parent { get; }
        
        /// <summary>
        /// Проверяется валидность самого вырожения
        /// </summary>
        bool IsValid { get; }
    }
}
