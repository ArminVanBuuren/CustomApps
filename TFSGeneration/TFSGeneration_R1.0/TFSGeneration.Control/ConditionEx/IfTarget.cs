using TFSAssist.Control.ConditionEx.Base;
using TFSAssist.Control.ConditionEx.Collections;

namespace TFSAssist.Control.ConditionEx
{
    public class IfTarget
    {
        public IfTarget()
        {
            ChildGroups = new IfTargetCollection(this);
            BlockConditions = new ConditionBlockCollection(this);
        }

        /// <summary>
        /// Родительское выражение
        /// </summary>
        internal IfTarget Parent { get; set; }

        internal LogicalGroup LogicalGroup { get; set; } = LogicalGroup.NaN;

        /// <summary>
        /// Группы выражений, дочерние IfTarget то что в выражении были в скобочках
        /// например в данном примере будут 3 группы (('2'>'1'>'0' and '1'='1') or '2'='2')
        /// 1='2'>'1'>'0' and '1'='1'       2='2'='2'       3=('2'>'1'>'0' and '1'='1') or ('2'='2')
        /// </summary>
        internal IfTargetCollection ChildGroups { get; set; }

        /// <summary>
        /// Заполнена релизация данной группы если не заполненны подгруппы (SubBlocks)
        /// то в данной группе выполняется реализация операции AND
        /// </summary>
        internal ConditionBlockCollection BlockConditions { get; set; }

        public string StringConstructor
        {
            get
            {
                switch (LogicalGroup)
                {
                    case LogicalGroup.Or:
                        return GetStringGroups(" OR ");
                    case LogicalGroup.And:
                        return GetStringGroups(" AND ");
                }

                int i = 0;
                int countBlk = BlockConditions.Count;
                string result = string.Empty;
                foreach (ConditionBlock block in BlockConditions)
                {
                    i++;
                    if (i == countBlk)
                    {
                        result = result + block.StringConstructor;
                        break;
                    }
                    result = string.Format("{0}{1} AND ", result, block.StringConstructor);
                }
                return result;
            }
        }

        string GetStringGroups(string operatorStr)
        {
            string result = string.Empty;
            if (ChildGroups.Count == 1)
                result = ChildGroups[0].StringConstructor;
            else
            {
                int i = 0;
                foreach (IfTarget bc in ChildGroups)
                {
                    i++;
                    result = string.Format("{0}( {1} ){2}", result, bc.StringConstructor, (i < ChildGroups.Count) ? operatorStr : string.Empty);
                }
            }
            return result;
        }

        public bool ResultCondition
        {
            get
            {
                if (!ValidationStructure)
                    return false;
                if (ChildGroups.Count > 0 && BlockConditions.Count == 0)
                {
                    if (LogicalGroup == LogicalGroup.NaN)
                        return false;
                    return GetBoolByBlock(LogicalGroup);
                }
                //если нет подблоков то проверяем на выполнения всех условий по параметрам
                return BlockConditions.GetResultCondition();
            }
        }

        bool GetBoolByBlock(LogicalGroup conditGroup)
        {
            int i = 0;
            int iOrs = 0;
            foreach (IfTarget bc in ChildGroups)
            {
                if (!bc.ResultCondition)
                {
                    if (conditGroup == LogicalGroup.And)
                        return false;
                    else
                        iOrs++;
                }
                i++;
            }
            return iOrs != i;
        }

        public bool ValidationStructure
        {
            get
            {
                if ((ChildGroups.Count > 0 && BlockConditions.Count > 0)
                    || (ChildGroups.Count > 0 && LogicalGroup == LogicalGroup.NaN))
                {
                    return false;
                }
                foreach (IfTarget bc in ChildGroups)
                {
                    if (bc.ValidationStructure == false)
                        return false;
                }
                return true;
            }
        }
    }
}