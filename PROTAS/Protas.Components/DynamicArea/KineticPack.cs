using System;
using System.Collections.Generic;

namespace Protas.Components.DynamicArea
{
    public class KineticPack : Dictionary<string, RegularAttribute>
    {
        public KineticPack Parent { get; internal set; }
        public KineticPack(string name) :base(StringComparer.OrdinalIgnoreCase)
        {
            Add(name, new RegularAttribute());
        }
        public KineticPack(string name, string value) : base(StringComparer.OrdinalIgnoreCase)
        {
            Add(name, new RegularAttribute(value));
        }
        public KineticPack(string name, Func<object> value) : base(StringComparer.OrdinalIgnoreCase)
        {
            Add(name, new RegularAttribute(value));
        }
        /// <summary>
        /// Индекс дочернего пакета. Если текущий пакет дочерний, то он больше -1
        /// </summary>
        public int CurrentUniqueIndex
        {
            get
            {
                int myIndex = -1;
                if (Parent != null)
                {
                    foreach (KeyValuePair<string, RegularAttribute> xp in Parent)
                    {
                        myIndex++;
                        if (ReferenceEquals(xp, this))
                        {
                            break;
                        }
                    }
                }
                return myIndex;
            }
        }
    }
}
