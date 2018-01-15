using Protas.Components.Functions;
using System;
using System.Text.RegularExpressions;

namespace Protas.Control.Resource.Base
{
    /// <summary>
    /// Сущность ресурса которая содержит в себе ссылку на непосредственный ресурс или уже готовый непосредственный ресурс
    /// </summary>
    public class ResourceEntity
    {
        /// <summary>
        /// Обязательный паттерн на имя ресурса
        /// </summary>
        public const string RESOURCE_NNAME_PATTERN = "^[A-z0-9_]{3,}$";

        string _resourceName = string.Empty;
        int _minComstrParams = 0;
        IResource _directIResource;
        Type _directTypeIResource;


        /// <summary>
        /// имя ресурса
        /// </summary>
        public string ResourceName
        {
            get { return _resourceName; }
            private set
            {
                if (value == null || !Regex.IsMatch(value, RESOURCE_NNAME_PATTERN))
                    throw new ArgumentException($"ResourceName=\"{value}\" Is Incorrect. Success Pattern=\"{RESOURCE_NNAME_PATTERN}\"");
                _resourceName = value;
            }
        }

        /// <summary>
        /// Минимальное колличество параметров конструктора для того чтобы ресурс смог быть проиннициализирован
        /// </summary>
        public int MinConstructorParams
        {
            get { return _minComstrParams; }
            private set { _minComstrParams = _minComstrParams < 0 ? 0 : value; }
        }

        /// <summary>
        /// Тип сущности 
        /// </summary>
        public EntityMode Mode { get; private set; } = EntityMode.None;

        /// <summary>
        /// Тип ресурса
        /// </summary>
        public Type ResourceLink
        {
            get { return _directTypeIResource; }
            private set
            {
                if (value == null)
                    throw new ArgumentException(string.Format("Type Inherit Of {0} Is NUll. When Initialize {1} By ResourceName=\"{2}\" With {3}=\"{4}\"", typeof(IResource).Name, typeof(ResourceEntity).Name, ResourceName, typeof(EntityMode).Name, Mode.ToString("g")));

                if (value != typeof(IResource))
                    throw new ArgumentException(string.Format("Input Type Is Not {0}! When Initialize {1} By ResourceName=\"{2}\" With {3}=\"{4}\"", typeof(IResource).Name, typeof(ResourceEntity).Name, ResourceName, typeof(EntityMode).Name, Mode.ToString("g")));

                _directTypeIResource = value;
            }
        }

        /// <summary>
        /// Непосредственный ресурс, если он был проинициализирован как готовый ресурс - например как макрос
        /// </summary>
        public IResource ReadyResource
        {
            get { return _directIResource; }
            private set
            {
                if (value == null)
                    throw new ArgumentException(string.Format("{0} Is NUll! When Initialize {1} By ResourceName=\"{2}\" With {3}=\"{4}\"", typeof(IResource).Name, typeof(ResourceEntity).Name, ResourceName, typeof(EntityMode).Name, Mode.ToString("g")));
                _directIResource = value;
            }
        }

        public ResourceEntity(string name, Type resource, EntityMode viaMode, int minStrConstructorParams)
        {
            Initialize(name, resource, viaMode, minStrConstructorParams);
        }

        public ResourceEntity(string name, Type resource, int minStrConstructorParams)
        {
            Initialize(name, resource, EntityMode.None, minStrConstructorParams);
        }

        public ResourceEntity(string name, Type resource, EntityMode viaMode)
        {
            Initialize(name, resource, viaMode, 0);
        }

        public ResourceEntity(string name, Type resource)
        {
            Initialize(name, resource, EntityMode.None, 0);
        }

        void Initialize(string name, Type resource, EntityMode viaMode, int minStrConstructorParams)
        {
            ResourceName = name;
            Mode = viaMode;
            ResourceLink = resource;
            MinConstructorParams = minStrConstructorParams;
        }

        public ResourceEntity(string name, IResource readyResource)
        {
            ResourceName = name;
            Mode = EntityMode.Ready;
            ReadyResource = readyResource;
            ResourceLink = readyResource.GetType();
        }

        public override bool Equals(object obj)
        {
            ResourceEntity input = (ResourceEntity)obj;

            if (input.Mode != Mode)
                return false;

            if (MinConstructorParams != input.MinConstructorParams)
                return false;

            if (!ResourceName.Equals(input.ResourceName, StringComparison.CurrentCultureIgnoreCase))
                return false;

            if ((Mode == EntityMode.None || Mode == EntityMode.Unusual) && ResourceLink != input.ResourceLink)
                return false;

            if (Mode == EntityMode.Ready && !ReadyResource.Equals(input.ReadyResource))
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return ResourceLink.GetHashCode();
        }

        public override string ToString()
        {
            return XString.Format(new[] { "ResourceName", typeof(EntityMode).Name, "Type" }, ResourceName, Mode.ToString("g"), ResourceLink.Name);
        }
    }
}