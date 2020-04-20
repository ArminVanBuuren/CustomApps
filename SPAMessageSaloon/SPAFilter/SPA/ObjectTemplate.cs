using System;
using System.Runtime.CompilerServices;
using Utils;
using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA
{
    public sealed class BlankTemplate : ObjectTemplate
    {
        public override string UniqueName { get; protected set; }
        public BlankTemplate(string uniqueId)
        {
            UniqueName = uniqueId;
        }
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

        public override bool Equals(object obj)
        {
            if (!(obj is ObjectTemplate objRes))
                return false;

            if (objRes.UniqueName.Like(UniqueName))
                return true;

            return RuntimeHelpers.Equals(this, obj);
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            if (!(obj is ObjectTemplate objRes))
                return 1;

            if (ReferenceEquals(this, objRes))
                return 0;

            return Equals(objRes) ? 0 : 1;
        }

        public override int GetHashCode()
        {
            return !UniqueName.IsNullOrEmptyTrim() ? UniqueName.ToLower().GetHashCode() : RuntimeHelpers.GetHashCode(this);
        }

        public override string ToString()
        {
            return UniqueName;
        }
    }
}