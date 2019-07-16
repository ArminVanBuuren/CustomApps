using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA.Components
{
    public class Command : ObjectTemplate
    {
        readonly ServiceInstance _parent;

        [DGVColumn(ColumnPosition.Before, "HostType")]
        public string HostTypeName => _parent.HostTypeName;

        [DGVColumn(ColumnPosition.After, "Command")]
        public override string Name { get; protected set; }

        public Command(ServiceInstance parent, string path) : base(path)
        {
            _parent = parent;
        }
    }
}
