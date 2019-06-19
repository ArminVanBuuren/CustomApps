using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA.Components
{
    public class Command : ObjectTemplate
    {
        public Command(string path, int id) : base(path, id)
        {

        }

        [DGVColumn(ColumnPosition.Before, "Command")]
        public override string Name { get; protected set; }
    }
}
