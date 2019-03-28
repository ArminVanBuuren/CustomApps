using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Utils.ConditionEx;
using Utils;

namespace TFSAssist.Control.DataBase.Settings
{
    [XmlRoot("CreateTFS")]
    public class CreateTFS
    {
        [XmlElement("TeamProject", IsNullable = false)]
        public TeamProjectCondition[] TeamProjects { get; set; }

        public override string ToString()
        {
            return $"TeamProject's:[{(TeamProjects?.Length ?? 0)}]";
        }
    }

    /// <summary>
    /// Поле с условиями аттрибутом Condition="#{'true'='true'}"
    /// </summary>
    public class ConditionClass
    {
        /// <summary>
        /// Исходное значение условия
        /// </summary>
        [XmlAttribute]
        public string Condition { get; set; }

        /// <summary>
        /// Выполняется замена спец параметров на их значение и получаем результат условия.
        /// </summary>
        /// <param name="getParcedValue"></param>
        /// <returns></returns>
        internal bool GetConditionResult(GetParcedValue getParcedValue, LogPerformer log)
        {
            if (Condition.IsNullOrEmpty())
                return true;

            string value = getParcedValue(Condition);

            try
            {
                IfCondition ifCond = new IfCondition();
                IfTarget target = ifCond.ExpressionEx(value);
                bool result = target.ResultCondition;

                log.OnWriteLog($"{nameof(Condition)}=[{Condition}]; Final{nameof(Condition)}=[{value}]. Result=[{result}]", true);
                return result;
            }
            catch (Exception ex)
            {
                throw new TFSFieldsException($"Final{nameof(Condition)}=[{value}] is incorrect! {nameof(Condition)}=[{Condition}];\r\n{ex.Message}", ex);
            }
        }

        [XmlAttribute]
        public string Value { get; set; }
    }


    /// <summary>
    /// Создать ТФС проект при условии
    /// </summary>
    [XmlRoot("TeamProject")]
    public class TeamProjectCondition : ConditionClass
    {
        [XmlElement("WorkItem", IsNullable = false)]
        public WorkItemCondition[] WorkItems { get; set; }

        public override string ToString()
        {
            return $"{Value} WorkItem's:[{WorkItems?.Length ?? 0}]";
        }
    }

    /// <summary>
    /// Создать в ТФС проекте рабочий элемент
    /// </summary>
    [XmlRoot("WorkItem")]
    public class WorkItemCondition : ConditionClass
    {
        [XmlElement("Field", IsNullable = false)]
        public FieldCondition[] Fields { get; set; }

        public override string ToString()
        {
            return $"{Value} Field's:[{Fields?.Length ?? 0}]";
        }
    }

    /// <summary>
    /// Поля рабочего элемента
    /// </summary>
    [XmlRoot("Field")]
    public class FieldCondition
    {
        private string _name;
        private string _value;

        /// <summary>
        /// Имя поля обязательно должно быть заполненно
        /// </summary>
        [XmlAttribute]
        public string Name
        {
            get
            {
                if (_name.IsNullOrEmpty())
                    throw new ArgumentException($"Field is incorrect! Attribute=[{nameof(Name)}] must be declared and shouldn't be empty.");
                return _name;
            }
            set
            {
                if (value.IsNullOrEmpty())
                    throw new ArgumentException($"Field is incorrect! Attribute=[{nameof(Name)}] must be declared and shouldn't be empty.");
                _name = value;
            }
        }

        /// <summary>
        /// Значение поля
        /// </summary>
        [XmlAttribute]
        public string Value
        {
            get
            {
                if (_value == null)
                    throw new TFSFieldsException($"Field=[{Name}] is incorrect! Attribute '{nameof(Value)}' must be declared.");
                return _value;
            }
            set
            {
                if (value == null)
                    throw new TFSFieldsException($"Field=[{Name}] is incorrect! Attribute '{nameof(Value)}' must be declared.");
                _value = value;
            }
        }

        /// <summary>
        /// Если существет условие для определения значения, то проверяются все условия иначе возвращается просто Value
        /// </summary>
        /// <param name="getParcedValue"></param>
        /// <returns></returns>
        internal string GetSwitchValue(GetParcedValue getParcedValue, Action<string> writeErrLog)
        {
            if (Switch.IsNullOrEmpty() && Items.Count <= 0)
                return getParcedValue(Value);


            string resultSwitch = GetParcedValueSwitch(getParcedValue, out var errLog);
            if (!errLog.IsNullOrEmpty())
                writeErrLog(errLog);

            return resultSwitch;
        }

        /// <summary>
        /// Выбрать правильный вариант функции Switch-Map
        /// </summary>
        /// <param name="getParcedValue"></param>
        /// <param name="errorLog"></param>
        /// <returns></returns>
        string GetParcedValueSwitch(GetParcedValue getParcedValue, out string errorLog)
        {
            errorLog = null;
            string switchValue = getParcedValue(Switch).Trim();
            foreach (MappingValue mapValue in Items)
            {
                string mapCasePattern = mapValue.Case.Trim();
                if(mapCasePattern.IsNullOrEmpty() && switchValue.IsNullOrEmpty())
                    return getParcedValue(mapValue.Value);

                try
                {
                    if (Regex.IsMatch(switchValue, mapCasePattern, RegexOptions.IgnoreCase))
                    {
                        return getParcedValue(mapValue.Value);
                    }
                }
                catch (Exception e)
                {
                    errorLog += $"Incorrect {nameof(mapValue.Case)} of Map collection. {nameof(mapValue.Case)}=[{mapCasePattern}]\r\nException:{e.Message}";
                    //if (mapValue.Case.Trim().Equals(switchValue, StringComparison.CurrentCultureIgnoreCase))
                    //{
                    //    return getParcedValue(mapValue.Value);
                    //}
                }
            }

            return getParcedValue(Value);
        }

        /// <summary>
        /// Задать динамическое значение в условие
        /// </summary>
        [XmlAttribute]
        public string Switch { get; set; }

        
        [XmlElement("Map")]
        public List<MappingValue> Items { get; set; } = new List<MappingValue>();

        public override string ToString()
        {
            if (Switch != null)
            {
                return $"Field=[{Name}]; {nameof(Switch)}=[{Switch}]; MapItems=[{Items.Count}]; Default{nameof(Value)}=[{Value}]; ";
            }

            return $"Field=[{Name}]; {nameof(Value)}=[{Value}]";
        }
    }

    [XmlRoot("Map")]
    public class MappingValue
    {
        private string _case;
        private string _value;

        [XmlAttribute]
        public string Case
        {
            get
            {
                if (_case == null)
                    throw new TFSFieldsException($"Map is incorrect! Attribute '{nameof(Case)}' must be declared.");
                return _case;
            }
            set
            {
                if (value == null)
                    throw new TFSFieldsException($"Map is incorrect! Attribute '{nameof(Case)}' must be declared.");
                _case = value;
            }
        }

        [XmlAttribute]
        public string Value
        {
            get
            {
                if (_value == null)
                    throw new TFSFieldsException($"Map{nameof(Case)}=[{Case}] is incorrect! Attribute '{nameof(Value)}' must be declared.");
                return _value;
            }
            set
            {
                if (value == null)
                    throw new TFSFieldsException($"Map{nameof(Case)}=[{Case}] is incorrect! Attribute '{nameof(Value)}' must be declared.");
                _value = value;
            }
        }

        public override string ToString()
        {
            return $"{nameof(Case)}=[{Case}]; Value=[{Value}]";
        }
    }
}