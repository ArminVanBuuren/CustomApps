using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Protas.Components.ConditionEx;
using Protas.Control.Resource;
using Protas.Control.Resource.Base;
using Protas.Components.XPackage;

namespace Protas.Control.ProcessFrame.Components
{
    internal class DynamicParams
    {
        public ResourceEntityCollection EntityCollection { get; }
        public XPack Result { get; }
        public DynamicParams(ResourceEntityCollection collection, XPack result)
        {
            EntityCollection = collection;
            Result = result;
        }
        public DynamicParams(XPack result)
        {
            Result = result;
        }
    }

    public class ContextDynamic : IResourceContext
    {
        List<DynamicParams> _resultList;
        public bool IsIntialized => true;
        public ResourceEntityCollection EntityCollection
        {
            get
            {
                if (_resultList.Count > 0)
                   return _resultList.Last().EntityCollection;
                return null;
            }
        }
        /// <summary>
        /// Актуальный результат выполнения Триггера
        /// </summary>
        internal XPack Result
        {
            get
            {
                if (_resultList.Count > 0)
                    return _resultList.Last().Result;
                return null;
            }
        }
        public ContextDynamic(Dictionary<string, string> attributes, XPack result)
        {
            _resultList = new List<DynamicParams>();
            Add(attributes, result);
        }


        /// <summary>
        /// Добавляем сюда новые выполненные результаты. Напрмиер после базового выполнения триггера, будут выполняться остальные Кейсы или РемоутКейсы 
        /// То результат этих кейсов добавлется в коллекцию результатов. 
        /// </summary>
        /// 
        /// <param name="attributes">список параметров</param>
        /// <param name="newResult">новый(актуальный) результат</param>
        public void Add(Dictionary<string, string> attributes, XPack newResult)
        {
            ResourceEntityCollection resourcesEnties = new ResourceEntityCollection();
            foreach (KeyValuePair<string, string> attr in attributes)
                resourcesEnties.Add(new ResourceEntity(attr.Key, new DynamicProperty(attr.Key, attr.Value, this)));

            _resultList.Add(new DynamicParams(resourcesEnties, newResult));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="newResult">новый(актуальный) результат</param>
        public void Add(XPack newResult)
        {
            _resultList.Add(new DynamicParams(newResult));
        }

        public IResource GetResource(Type resource, ResourceConstructor constructor)
        {
            return null;
        }
        public IHandler GetHandler(XPack pack)
        {
            return null;
        }
        public void Dispose()
        {

        }
    }

    public class DynamicProperty : IResource
    {
        internal DynamicXPack _result;
        public DynamicProperty(string attributeName, string attributeValue, ContextDynamic context)
        {
            _result = new DynamicXPack(attributeName, attributeValue, context);
        }

        public bool IsTrigger
        {
            get { return false; }
            set { }
        }

        public bool IsIntialized => true;

        public ResourceConstructor Constructor => null;

        public ResultType Type => ResultType.Constant;

        public RHandlerEvent ResourceChanged { get; set; }

        public XPack GetResult()
        {
            return _result;
        }
        public void Dispose()
        {
            _result.Dispose();
        }
    }

    public class DynamicXPack : XPack
    {
        delegate string GetResult(string original, ContextDynamic context);
        GetResult _getResult;
        string _newValue = string.Empty;
        ContextDynamic _context;
        public DynamicXPack(string name, string value, ContextDynamic context) : base(name, value)
        {
            _context = context;
            if (_context == null || string.IsNullOrEmpty(value))
                _getResult = ReturnOriginal;
            else if (name.Equals("conditionex", StringComparison.CurrentCultureIgnoreCase))
                _getResult = CheckCondition;
            else
                _getResult = SubstituteValue;
        }

        public override string Value
        {
            get
            {
                return _getResult.Invoke(base.Value, _context);
            }
            set
            {

            }
        }

        string ReturnOriginal(string original, ContextDynamic context)
        {
            return original;
        }

        /// <summary>
        /// Т.к. этот класс присвоен к обработке динамических параметров триггера. В данном методе мы проверяем аттрибут ConditionEx на валидность.
        /// Для этого в строке реплейсим по синтаксису {0} на результат и вызываем класс IfCondition
        /// </summary>
        /// <param name="original">стринговая строка</param>
        /// <param name="context">Контекст с текущим результатом</param>
        /// <returns></returns>
        string CheckCondition(string original, ContextDynamic context)
        {
            string condition = SubstituteValue(original, context);
            IfCondition ifCondition = new IfCondition();
            IfTarget target = ifCondition.ExpressionEx(condition);
            return target.ResultCondition.ToString();
        }

        /// <summary>
        /// В данном методе мы парсим всю строку и реплейсим значения {0} или {0$12345$123456} и тд, на результат, полученный при формировании динамичного контекста ContextDynamic
        /// </summary>
        /// <param name="original">стринговая строка</param>
        /// <param name="context">Контекст с текущим результатом</param>
        /// <returns></returns>
        string SubstituteValue(string original, ContextDynamic context)
        {
            int startPattern = -1;
            StringBuilder newValue = new StringBuilder();
            StringBuilder properties = new StringBuilder();
            foreach (char ch in original)
            {

                switch (startPattern)
                {
                    case 0:
                        {
                            if (ch == '0')
                            {
                                startPattern = 1;
                                continue;
                            }
                            else
                            {
                                newValue.Append('{');
                                startPattern = -1;
                            }
                        }
                        break;
                    case 1:
                        {
                            if (ch != '}')
                            {
                                properties.Append(ch);
                                continue;
                            }
                            break;
                        }
                }

                switch (ch)
                {
                    case '{':
                        {
                            startPattern = 0;
                            continue;
                        }
                    case '}':
                        {
                            if (startPattern == -1)
                                break;
                            if (context.Result != null)
                            {
                                if (properties.Length == 0)
                                    newValue.Append(context.Result.Value);
                                else
                                {
                                    string path = ResourceSignature.GetOutputProperties(properties.ToString());
                                    newValue.Append(ResourceSegment.GetXPackResultByPath(context.Result, path));
                                    properties.Remove(0, properties.Length);
                                }
                            }
                            startPattern = -1;
                            continue;
                        }
                }

                newValue.Append(ch);
            }


            return newValue.ToString();
        }
    }
}
