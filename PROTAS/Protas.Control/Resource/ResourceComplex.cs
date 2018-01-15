using System.Collections.Generic;
using Protas.Control.Resource.Base;
using Protas.Components.XPackage;

namespace Protas.Control.Resource
{
    /// <summary>
    /// Класс который переопределяет каждый стринговый объект согласно ресурсным контекстам
    /// </summary>
    public class ResourceComplex
    {
        ResourceKernel Resource { get; }
        internal ResourceComplex(ResourceBase main, ResourceContextPack contexts)
        {
            Resource = new ResourceKernel(main, -1, string.Empty, contexts);
        }

        /// <summary>
        /// Переопределяет каждый стринговый объект из списка возможных ресурсных выражений, в соотсветвии с текущем количеством контекстов
        /// </summary>
        /// <param name="collectionAttributes"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetResourceValues(XPackAttributes collectionAttributes)
        {
            return GetResourceValues((Dictionary<string, string>)collectionAttributes);
        }
        /// <summary>
        /// Переопределяет каждый стринговый объект из списка возможных ресурсных выражений, в соотсветвии с текущем количеством контекстов
        /// </summary>
        /// <param name="collectionAttributes"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetResourceValues(Dictionary<string, string> collectionAttributes)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> input in collectionAttributes)
                result.Add(input.Key, GetResourceValues(input.Value));
            return result;
        }
        /// <summary>
        /// Получение только обработанных значений из стрингового ресурсного выражения, в соотсветвии с текущем количеством контекстов
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string GetResourceValues(string input)
        {
            ResourceSegmentCollection segments = Resource.Parcer.GetByExpression(input);
            segments.InitializeHandlers();
            if (segments.IsResource)
                return segments.Result;
            return input;
        }
    }
}
