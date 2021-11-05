using System;
using System.Linq;
using Utils.ConditionEx.Collections;

namespace Utils.ConditionEx
{
    public class Expression : IExpression, ICondition, IDisposable
    {
        internal event EventHandler ExpressionResultChanged;
        /// <summary>
        /// Родительское выражение
        /// </summary>
        public Expression Parent { get; internal set; }

        public bool IsValid
        {
            get
            {
                if ((ExpressionList.Count > 0 && (ConditionList.Count > 0 || LogicalGroup == LogicalGroupType.NaN)) || (ExpressionList.Count == 0 && ConditionList.Count == 0))
                    return false;

                foreach (var bc in ExpressionList)
                {
                    if (!bc.IsValid)
                        return false;
                }
                return true;
            }
        }

        public bool ConditionResult
        {
            get
            {
                if (!IsValid)
                    return false;

                if (ExpressionList.Count > 0 && ConditionList.Count == 0)
                {
                    if (LogicalGroup == LogicalGroupType.NaN)
                        return false;

                    var i = 0;
                    var iOrs = 0;
                    foreach (var bc in ExpressionList)
                    {
                        if (!bc.ConditionResult)
                        {
                            if (LogicalGroup == LogicalGroupType.And)
                                return false;
                            else
                                iOrs++;
                        }
                        i++;
                    }
                    return iOrs != i;
                }

                //если нет подблоков то проверяем на выполнения всех условий по параметрам
                return ConditionList.ConditionResult;
            }
        }

        public string StringResult
        {
            get
            {
                var i = 0;
                var result = string.Empty;

                switch (LogicalGroup)
                {
                    case LogicalGroupType.Or:
                    case LogicalGroupType.And:
                        if (ExpressionList.Count == 1)
                            return ExpressionList.First().StringResult;

                        foreach (ICondition conditionEx in ExpressionList)
                        {
                            i++;
                            result = $"{result}( {conditionEx.StringResult} ){((i < ExpressionList.Count) ? $" {LogicalGroup:G} " : string.Empty)}";
                        }
                        return result;
                }

                return ConditionList.StringResult;
            }
        }

        public LogicalGroupType LogicalGroup { get; set; } = LogicalGroupType.NaN;

        /// <summary>
        /// Группы выражений, где дочерние в выражении были в скобочках
        /// например в данном примере будут 3 группы (('2'>'1'>'0' and '1'='1') or '2'='2')
        /// '1='2'>'1'>'0' and '1'='1'       '2='2'='2'       ('2'>'1'>'0' and '1'='1') or ('2'='2')
        /// </summary>
        public ExpressionCollection ExpressionList { get; internal set; }

        /// <summary>
        /// Заполнена релизация данной группы если не заполненны подгруппы то в данной группе выполняется реализация операции AND
        /// </summary>
        public ConditionCollection ConditionList { get; internal set; }


        internal DynamicObject DynamicObject { get; private set; }

        internal Expression(DynamicObject dynamicObj)
        {
            DynamicObject = dynamicObj;
            DynamicObject.ObjectChanged += DynamicObject_ObjectChanged;
            ExpressionList = new ExpressionCollection(this);
            ConditionList = new ConditionCollection(this);
        }

        internal Expression()
        {
            ExpressionList = new ExpressionCollection(this);
            ConditionList = new ConditionCollection(this);
        }

        private void DynamicObject_ObjectChanged(object sender, EventArgs e)
        {
            if (ExpressionResultChanged == null)
                return;

            var temp = StringResult;
            ExpressionResultChanged.Invoke(this, EventArgs.Empty);
        }

        internal static LogicalGroupType GetLogicalGroup(string str)
        {
            switch (str)
            {
                case "&&": return LogicalGroupType.And;
                case "||": return LogicalGroupType.Or;
                case "(": return LogicalGroupType.BracketIsOpen;
                case ")": return LogicalGroupType.BracketIsClose;
            }

            if(str.Equals("and", StringComparison.InvariantCultureIgnoreCase))
                return LogicalGroupType.And;
            else if(str.Equals("or", StringComparison.InvariantCultureIgnoreCase))
                return LogicalGroupType.Or;

            return LogicalGroupType.NaN;
        }

        internal void AddCondition(ConditionBlock conditionBlock, string firstParam, string parameter, ConditionOperatorType @operator)
        {
            conditionBlock.AddChild(firstParam, parameter, @operator, this);
        }

        public override string ToString()
        {
            return StringResult;
        }

        public void Dispose()
        {
            if (DynamicObject != null)
            {
                DynamicObject.ObjectChanged -= DynamicObject_ObjectChanged;
                DynamicObject = null;
            }
        }
    }
}