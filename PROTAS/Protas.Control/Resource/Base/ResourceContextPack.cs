using Protas.Components.XPackage;
using System;
using System.Collections.Generic;
using System.Linq;
using Protas.Components.Functions;

namespace Protas.Control.Resource.Base
{

    /// <summary>
    /// Оболочка которая содержит в себе необходимые контексты
    /// </summary>
    public class ResourceContextPack
    {
        bool _initByItSelf;
        Dictionary<string, List<IResourceContext>> Primary { get; }
        Dictionary<string, List<IResourceContext>> Secondary { get; } = new Dictionary<string, List<IResourceContext>>(StringComparer.CurrentCultureIgnoreCase);

        public ResourceContextPack()
        {
            _initByItSelf = true;
            Primary = new Dictionary<string, List<IResourceContext>>(StringComparer.CurrentCultureIgnoreCase);
        }

        public ResourceContextPack(ResourceContextPack prepared)
        {
            _initByItSelf = false;
            Primary = prepared.Primary;
        }

        internal void AddPrimary(string name, IResourceContext context)
        {
            if(!_initByItSelf)
                return;
            AddNewContext(Primary, name, context);
        }

        internal void AddSecondary(string name, IResourceContext context)
        {
            AddNewContext(Secondary, name, context);
        }

        void AddNewContext(Dictionary<string, List<IResourceContext>> contextPack, string name, IResourceContext newContext)
        {
            List<IResourceContext> findedItem;
            if (contextPack.TryGetValue(name, out findedItem))
                findedItem.Add(newContext);
            else
                contextPack.Add(name, new List<IResourceContext> { newContext });
        }
        internal void ClearSecondary()
        {
            Secondary.Clear();
        }

        public IHandler GetHandler(string contextName, XPack handlerPack)
        {
            List<IResourceContext> findedContextList;
            if (!Primary.TryGetValue(contextName, out findedContextList))
                return null;

            foreach (IResourceContext context in findedContextList)
            {
                IHandler resultHandler = context.GetHandler(handlerPack);
                if (resultHandler != null)
                    return resultHandler;
            }

            return null;
        }


        internal ResourceShell GetResource(string contextName, string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName))
                return null;

            ResourceShell result;

            if (TryGetAnyResourceShell(contextName, resourceName, Primary, out result))
                return result;

            if (TryGetResourceShell(contextName, resourceName, Secondary, out result))
                return result;

            return null;
        }

        /// <summary>
        /// Находим базовые ресурсы, в данном случае не обязательно чтобы было имя конетекста, может найтись первый попавшийся по названию ресусра
        /// </summary>
        /// <param name="contextName"></param>
        /// <param name="resourceName"></param>
        /// <param name="contexts"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        bool TryGetAnyResourceShell(string contextName, string resourceName, Dictionary<string, List<IResourceContext>> contexts, out ResourceShell result)
        {
            result = null;

            //находим ресурс полагая что имя контекста известно
            ResourceShell correctResult;
            if (TryGetResourceShell(contextName, resourceName, contexts, out correctResult))
            {
                result = correctResult;
                return true;
            }


            //находим первый попавшийся контекст если имя контекста неизвестно при совпадении по имени ресурса в контексте
            foreach (KeyValuePair<string, List<IResourceContext>> context in contexts)
            {
                foreach (IResourceContext innerContext in context.Value)
                {
                    ResourceEntity possibleEntity = innerContext.EntityCollection?[resourceName];
                    if (possibleEntity == null)
                        continue;

                    result = new ResourceShell(innerContext, context.Key, possibleEntity);
                    return true;
                }
            }


            return false;
        }

        bool TryGetResourceShell(string contextName, string resourceName, Dictionary<string, List<IResourceContext>> contexts, out ResourceShell result)
        {
            result = null;
            //имя контекста должно быть обязательно
            if (string.IsNullOrEmpty(contextName))
                return false;


            //находим непосредственный контекст по имени контекста из строкового выражения далее по имени ресурса
            foreach (KeyValuePair<string, List<IResourceContext>> context in contexts)
            {
                if (!context.Key.Equals(contextName, StringComparison.CurrentCultureIgnoreCase))
                    continue;

                foreach (IResourceContext innerContext in context.Value)
                {
                    ResourceEntity possibleEntity = innerContext.EntityCollection?[resourceName];
                    if (possibleEntity == null)
                        continue;

                    result = new ResourceShell(innerContext, context.Key, possibleEntity);
                    return true;
                }
            }
            return false;
        }


        public override string ToString()
        {
            return XString.Format(new[] { "Primary", "Secondary"}, 
                string.Join(";", Primary.Select(x => x.Key + "=" + x.Value).ToArray()),
                string.Join(";", Secondary.Select(x => x.Key + "=" + x.Value).ToArray()));
        }

    }
}
