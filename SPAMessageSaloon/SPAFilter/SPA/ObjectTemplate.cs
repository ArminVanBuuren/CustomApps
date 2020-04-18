using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA
{
    public abstract class ObjectTemplate : IObjectTemplate
    {
        [DGVColumn(ColumnPosition.First, "PrivateID", false)]
        public int PrivateID { get; set; }

        [DGVColumn(ColumnPosition.First, "ID")]
        public int ID { get; set; } = 0;

        // Свойство Name должно быть виртуальным, т.к. мы переопределяем место в датагриде по атрибутам
        [DGVColumn(ColumnPosition.After, "Name")]
        public virtual string Name { get; set; }

        protected ObjectTemplate()
        {

        }
    }
}