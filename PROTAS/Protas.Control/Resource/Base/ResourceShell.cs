using System;
using Protas.Components.Functions;

namespace Protas.Control.Resource.Base
{
    /// <summary>
    /// Оболочка ресурса который содержит в себе точную ссылку на контекст и сущность ресурса
    /// </summary>
    internal class ResourceShell
    {
        public string ContextName { get; }
        public string ResourceName => Entity.ResourceName;
        /// <summary>
        /// Непосредственный контекст ресурса
        /// </summary>
        public IResourceContext Context { get; }
        /// <summary>
        /// Сущность ресурса
        /// </summary>
        public ResourceEntity Entity { get; }
        public ResourceShell(IResourceContext context, string contextName, ResourceEntity entity)
        {
            Context = context;
            ContextName = contextName;
            Entity = entity;
        }

        public override bool Equals(object obj)
        {
            ResourceShell input = (ResourceShell)obj;
            if (input == null)
                return false;
            return ToString().Equals(input.ToString(), StringComparison.CurrentCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            return Context.GetHashCode();
        }

        public override string ToString()
        {
            return XString.Format(new[] { ContextName + "[" + Context.GetType().FullName + "]", typeof(ResourceEntity).Name }, ContextName, Entity.ToString());
        }
    }
}
