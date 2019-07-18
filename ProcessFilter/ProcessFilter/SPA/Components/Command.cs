using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA.Components
{
    public class Command : DriveTemplate
    {
        readonly ServiceInstance _parent;

        [DGVColumn(ColumnPosition.After, "Command")]
        public override string Name { get; set; }

        [DGVColumn(ColumnPosition.Before, "HostType")]
        public string HostTypeName => _parent.HostTypeName;

        public Command(ServiceInstance parent, string path) : base(path)
        {
            _parent = parent;
        }
    }
}
