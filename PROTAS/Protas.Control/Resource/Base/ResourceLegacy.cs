using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Timers;
using Protas.Components.Functions;
using Protas.Components.PerformanceLog;
using Protas.Components.XPackage;

namespace Protas.Control.Resource.Base
{
    public abstract class BaseFrame : ShellLog3Net, IDisposable
    {
        XPack _prevResult;
        /// <summary>
        /// количество шагов для получения значений во внутрь каждых их возможных свойств
        /// </summary>
        int Unders { get; set; } = 1;
        /// <summary>
        /// колличество свойств которые нашлись при первичной инициализации
        /// </summary>
        int CountDynamicFields { get; set; } = 0;

        protected BaseFrame(ILog3NetMain log) : base(log)
        {
            AddMessagePrefix("Resource");
        }

        protected BaseFrame(int unders, ILog3NetMain log) : base(log)
        {
            Unders = unders;
            AddMessagePrefix("Resource");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent">XPack объект который нужно вернуть в результате</param>
        /// <param name="obj">Сам ресурсный объект из которого мы получаем значения всех свойств</param>
        public void InitializeOrUpdateObjectFields(XPack parent, object obj)
        {
            if (parent == null || obj == null)
                return;
            if (_prevResult != parent)
            {
                try
                {
                    CountDynamicFields = 0;
                    InitializeObjects(parent, obj, -1);
                    _prevResult = parent;
                }
                catch (Exception ex)
                {
                    CountDynamicFields = 0;
                    AddLogForm(Log3NetSeverity.Error, "Exception:{0}\r\n{1}", ex.Message, ex.Data);
                }
            }
            else if (CountDynamicFields > 0)
            {
                UpdateObjectFields(parent, obj, -1);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent">XPack объект который нужно вернуть в результате</param>
        /// <param name="obj">Сам ресурсный объект из которого мы получаем все значения свойств</param>
        /// <param name="under">Задает условие на количество шагов для получения значений во внутрь каждых их возможных свойств</param>
        /// <returns></returns>
        void InitializeObjects(XPack parent, object obj, int under)
        {
            under++;
            if (under >= Unders)
                return;
            //получаем динамичные свойства
            foreach (PropertyInfo prop in obj.GetType().GetProperties())
            {
                CountDynamicFields++;
                object underObj = prop.GetValue(obj, null);
                XPack child = new XPack(prop.Name, underObj.ToString());
                parent.ChildPacks.Add(child);
                InitializeObjects(child, underObj, under);
            }
            //получаем статичные свойства
            foreach (KeyValuePair<string, string> prop in GetStaticFieldValues(obj))
            {
                parent.ChildPacks.Add(new XPack(prop.Key, prop.Value));
            }
        }
        static Dictionary<string, string> GetStaticFieldValues(object obj)
        {
            return obj.GetType()
                      .GetFields(BindingFlags.Public | BindingFlags.Static)
                      //.Where(f => f.FieldType == typeof(string))
                      .ToDictionary(f => f.Name, f => f.GetValue(null).ToString());
        }
        void UpdateObjectFields(XPack parent, object obj, int under)
        {
            under++;
            if (under >= Unders)
                return;
            foreach (PropertyInfo prop in obj.GetType().GetProperties())
            {
                foreach (XPack child in parent.ChildPacks)
                {
                    if (child.Name == prop.Name)
                    {
                        object underObj = prop.GetValue(obj, null);
                        child.Value = underObj.ToString();
                        UpdateObjectFields(child, underObj, under);
                        break;
                    }
                }
            }
        }

        public override string ToString()
        {
            return XString.Format(new[] { "Unders" }, Unders);
        }

        public void Dispose()
        {
            _prevResult = null;
            Unders = 1;
            CountDynamicFields = 0;
        }
    }

    public abstract class ResourceConstantFrame : BaseFrame, IResource
    {
        public RHandlerEvent ResourceChanged { get; set; }
        public ResourceConstructor Constructor { get; }

        public bool IsTrigger
        {
            get { return false; }
            set { }
        }

        public ResultType Type => ResultType.Constant;
        public bool IsIntialized { get; set; } = true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceConstructor">конструктор текущего ресурса который используется непосредственно внитри инициализации или в других оспектах</param>
        protected ResourceConstantFrame(ResourceConstructor resourceConstructor) : base(resourceConstructor)
        {
            Constructor = resourceConstructor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceConstructor">конструктор текущего ресурса который используется непосредственно внитри инициализации или в других оспектах</param>
        /// <param name="unders">рекурсивно получить значения свойств из количество ступеней внутрь объекта чтобы узнать что внутри объекта</param>
        protected ResourceConstantFrame(ResourceConstructor resourceConstructor, int unders) : base(unders, resourceConstructor)
        {
            Constructor = resourceConstructor;
        }
        public virtual XPack GetResult()
        {
            return null;
        }

        public new virtual void Dispose()
        {
            base.Dispose();
        }
        public override string ToString()
        {
            return XString.Format(new[] { "IsIntialized", "Type", "IsTrigger", "Constructor", "" }, IsIntialized, Type.ToString("g"), IsTrigger, string.Join(";", Constructor), base.ToString());
        }
    }
    public abstract class ResourceInfinityUniqueTimerFrame : BaseFrame, IResource
    {
        Timer TimerUpdateResource { get; } = new Timer { Interval = 5000 };
        public RHandlerEvent ResourceChanged { get; set; }
        public ResourceConstructor Constructor { get; }

        public bool IsTrigger
        {
            get { return TimerUpdateResource.Enabled; }
            set
            {
                TimerUpdateResource.Enabled = value;
                if (value)
                    ResourceChanged?.Invoke(this, GetResult());
            }
        }
        public double Interval
        {
            get
            {
                return TimerUpdateResource.Interval;
            }
            set
            {
                TimerUpdateResource.Interval = value;
            }
        }
        public ResultType Type => ResultType.InfinityUnique;
        public bool IsIntialized { get; set; } = true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceConstructor">конструктор текущего ресурса который используется непосредственно внитри инициализации или в других оспектах</param>
        protected ResourceInfinityUniqueTimerFrame(ResourceConstructor resourceConstructor) : base(resourceConstructor)
        {
            TimerUpdateResource.Elapsed += (TimerElapsed);
            TimerUpdateResource.Enabled = false;
            Constructor = resourceConstructor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceConstructor">конструктор текущего ресурса который используется непосредственно внитри инициализации или в других оспектах</param>
        /// <param name="unders">рекурсивно получить значения свойств из количество ступеней внутрь объекта чтобы узнать что внутри объекта</param>
        protected ResourceInfinityUniqueTimerFrame(ResourceConstructor resourceConstructor, int unders) : base(unders, resourceConstructor)
        {
            TimerUpdateResource.Elapsed += (TimerElapsed);
            TimerUpdateResource.Enabled = false;
            Constructor = resourceConstructor;
        }
        public virtual XPack GetResult()
        {
            return null;
        }
        void TimerElapsed(object sender, EventArgs e)
        {
            //AddLog(Log3NetSeverity.Error, "Start" + DateTime.Now.ToString("HH:mm:ss.fff"));
            ResourceChanged.Invoke(this, GetResult());
        }
        public new virtual void Dispose()
        {
            TimerUpdateResource?.Dispose();
            ResourceChanged.Dispose();
            base.Dispose();
        }
        public override string ToString()
        {
            return XString.Format(new[] { "IsIntialized", "Type", "IsTrigger", "Constructor", "" }, IsIntialized, Type.ToString("g"), IsTrigger, string.Join(";", Constructor), base.ToString());
        }
    }
    public abstract class ResourceSpecificFrame : BaseFrame, IResource
    {
        public RHandlerEvent ResourceChanged { get; set; }
        public ResourceConstructor Constructor { get; }
        public virtual bool IsTrigger { get; set; }
        public ResultType Type => ResultType.Specific;
        public bool IsIntialized { get; set; } = true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceConstructor">конструктор текущего ресурса который используется непосредственно внитри инициализации или в других оспектах</param>
        protected ResourceSpecificFrame(ResourceConstructor resourceConstructor) : base(resourceConstructor)
        {
            Constructor = resourceConstructor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceConstructor">конструктор текущего ресурса который используется непосредственно внитри инициализации или в других оспектах</param>
        /// <param name="unders">рекурсивно получить значения свойств из количество ступеней внутрь объекта чтобы узнать что внутри объекта</param>
        protected ResourceSpecificFrame(ResourceConstructor resourceConstructor, int unders) : base(unders, resourceConstructor)
        {
            Constructor = resourceConstructor;
        }
        public virtual XPack GetResult()
        {
            return null;
        }

        public new virtual void Dispose()
        {
            ResourceChanged.Dispose();
            base.Dispose();
        }
        public override string ToString()
        {
            return XString.Format(new[] { "IsIntialized", "Type", "IsTrigger", "Constructor", "" }, IsIntialized, Type.ToString("g"), IsTrigger, string.Join(";", Constructor), base.ToString());
        }
    }
}
