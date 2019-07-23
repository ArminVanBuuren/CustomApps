using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA
{
    public abstract class ObjectTemplate : IObjectTemplate
    {
        [DGVColumn(ColumnPosition.First, "ID")]
        public int ID { get; set; } = 0;

        [DGVColumn(ColumnPosition.After, "Name")]
        public virtual string Name { get; set; }

        protected ObjectTemplate()
        {

        }
    }
}