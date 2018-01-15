using System;
using Protas.Components.XPackage;
using Protas.Control.Resource.Base;
using Protas.Components.XPackage;

namespace Protas.Control.ProcessFrame.Components
{
    internal class StaticContext : IResourceContext
    {
        public const string NODE_MACROS_STATIC = @"Static";
        public const string NODE_MACROS_STATIC_ITEM = @"Item";
        public ResourceEntityCollection EntityCollection { get; } = new ResourceEntityCollection();
        public StaticContext(CollectionXPack macrosList)
        {
            AssignStaticItems(macrosList);
        }

        public void AddRange(StaticContext statContext)
        {
            EntityCollection.AddRange(statContext.EntityCollection);
        }

        /// <summary>
        /// Добавляем сами макросы. Пример одного &lt;Static name="monitorfile" value="C:\1\test.xml"/&gt;
        /// </summary>
        /// <param name="macrosList">коллекция макросов</param>
        void AssignStaticItems(CollectionXPack macrosList)
        {
            foreach (XPack mac in macrosList)
            {
                if (mac.Name.Equals(NODE_MACROS_STATIC, StringComparison.CurrentCultureIgnoreCase))
                {
                    string macrosName;
                    if (!mac.Attributes.TryGetValue("name", out macrosName))
                        continue;

                    //пересоздаем паки, где аттрибут name становится в названии пака вместо имени Static
                    XPack macDefine = new XPack(macrosName, mac.Attributes["value"] ?? string.Empty);

                    AssignChildItems(mac, ref macDefine);

                    EntityCollection.Add(new ResourceEntity(macrosName, new StaticResource(macDefine)));
                }
            }
        }

        ///  <summary>
        ///  Добавляем дочерние элементы бызовых макросов макросы. Пример одного 
        ///  &lt;Static name="MessagingGateway" value="a14-mg01"&gt;
        /// 	&lt;Item name = "queue" value="telcrm_output" /&gt;
        ///  &lt;/Static&gt;
        ///  </summary>
        ///  <param name="original">родительский макрос</param>
        /// <param name="product">что должно получится на выходе</param>
        void AssignChildItems(XPack original, ref XPack product)
        {
            foreach (XPack child in original.ChildPacks)
            {
                if (child.Name.Equals(NODE_MACROS_STATIC_ITEM, StringComparison.CurrentCultureIgnoreCase))
                {
                    string itemName;
                    if (!child.Attributes.TryGetValue("name", out itemName))
                        continue;

                    //пересоздаем паки, где аттрибут name становится в названии пака вместо имени Item
                    XPack macItem = new XPack(itemName, child.Attributes["value"] ?? string.Empty);

                    if (child.ChildPacks.Count > 0)
                    {
                        AssignChildItems(child, ref macItem);
                        //все имена должны быть уникальные
                        if (!macItem.ChildPacks.IsUniqueNames)
                            throw new Exception(string.Format("Child {0} Items Name's Is Not Unique. Check Your Config File.", NODE_MACROS_STATIC));
                    }

                    product.ChildPacks.Add(macItem);
                }
            }
        }

        public bool IsIntialized => true;

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

    internal class StaticResource : IResource
    {
        XPack _result;
        public StaticResource(XPack result)
        {
            _result = result;
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




}
