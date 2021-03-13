using System;
using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA
{
    public sealed class BlankTemplate : ObjectTemplate
    {
        public override string UniqueName { get; protected set; }
        public BlankTemplate(string uniqueId) => UniqueName = uniqueId;
    }

    public abstract class ObjectTemplate : IObjectTemplate, IComparable
    {
        [DGVColumn(ColumnPosition.First, "UniqueName", false)]
        public abstract string UniqueName { get; protected set; }

        [DGVColumn(ColumnPosition.First, "ID")]
        public int ID { get; set; } = 0;

        // Свойство Name должно быть виртуальным, т.к. мы переопределяем место в датагриде по атрибутам
        [DGVColumn(ColumnPosition.After, "Name")]
        public virtual string Name { get; set; }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            return Equals(obj) ? 0 : 1;
        }

        public override bool Equals(object input)
        {
            if (!(input is IObjectTemplate inputTemplate))
                return false;

            return UniqueName.Equals(inputTemplate.UniqueName);
        }

        public override int GetHashCode() => UniqueName.GetHashCode();

        public override string ToString() => UniqueName;
    }
}