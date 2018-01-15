using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Protas.Control.Resource.Base
{
    /// <summary>
    /// Коллекция сущностей ресурсов
    /// </summary>
    public class ResourceEntityCollection : IEnumerable
    {
        List<ResourceEntity> _resourceEntity;
        public ResourceEntityCollection()
        {
            _resourceEntity = new List<ResourceEntity>();
        }
        /// <summary>
        /// при добавлении ресурса производится проверка, все имена рескуров должны быть уникальные
        /// в случае если ресурс с таким же имененм уже существует - то он заменятся новым
        /// </summary>
        /// <param name="inputEntity"></param>
        public void Add(ResourceEntity inputEntity)
        {
            foreach (ResourceEntity entity in _resourceEntity)
            {
                if (entity.ResourceName.Equals(inputEntity.ResourceName, StringComparison.CurrentCultureIgnoreCase))
                    throw new Exception(string.Format("Can't Add Identional Names of \"{1}\" In {0}. Only Unque Items!", typeof(ResourceEntityCollection).Name, entity.ResourceName));
            }

            _resourceEntity.Add(inputEntity);
        }

        public void AddRange(ResourceEntityCollection collection)
        {
            foreach (ResourceEntity entity in collection._resourceEntity)
            {
                Add(entity);
            }
        }

        /// <summary>
        /// Возвращает число сущностей в коллекции ResourceEntityCollection
        /// </summary>
        public int Count => _resourceEntity.Count;

        /// <summary>
        /// находим ресурс по имени, поиск в нижнем регистре
        /// </summary>
        /// <param name="inputResurceName"></param>
        /// <returns></returns>
        public ResourceEntity this[string inputResurceName]
        {
            get
            {
                return _resourceEntity.FirstOrDefault(entity => entity.ResourceName.Equals(inputResurceName, StringComparison.CurrentCultureIgnoreCase)); 
            }
        }


        /// <summary>
        /// Возвращает перечислитель, выполняющий перебор элементов в коллекции.  (Унаследовано от IEnumerable<T>.)
        /// </summary>
        /// <returns></returns>
        public IEnumerator<ResourceEntity> GetEnumerator()
        {
            return ((IEnumerable<ResourceEntity>)_resourceEntity).GetEnumerator();
        }

        /// <summary>
        /// Возвращает перечислитель, который выполняет итерацию по элементам коллекции. (Унаследовано от IEnumerable)
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (_resourceEntity as IEnumerable<ResourceEntity>).GetEnumerator();
        }
    }

}
