using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.ConditionEx
{
    public enum LogicalGroupType
    {
        /// <summary>
        /// ничего
        /// </summary>
        NaN = 0,
        /// <summary>
        /// условие И между условными операторами
        /// </summary>
        And = 1,
        /// <summary>
        /// условие ИЛИ между условными операторами
        /// </summary>
        Or = 2,
        /// <summary>
        /// начало групповово условия (скобка открыта)
        /// </summary>
        BracketIsOpen = 3,
        /// <summary>
        /// конец групповово условия (скобка закрыта)
        /// </summary>
        BracketIsClose = 4
    }
}
