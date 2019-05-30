using static Utils.IO;
using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA
{

    public class ObjectTemplate
    {
        public ObjectTemplate(string path, int id)
        {
            ID = id;
            Name = path.GetLastNameInPath(true);
            FilePath = path;
        }

        [DGVEnhancer.DGVColumnAttribute(DGVEnhancer.ColumnPosition.First, "ID")]
        public int ID { get; protected set; }

        [DGVEnhancer.DGVColumnAttribute(DGVEnhancer.ColumnPosition.After, "Name")]
        public virtual string Name { get; protected set; }

        [DGVEnhancer.DGVColumnAttribute(DGVEnhancer.ColumnPosition.Last, "File Path")]
        public virtual string FilePath { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}
