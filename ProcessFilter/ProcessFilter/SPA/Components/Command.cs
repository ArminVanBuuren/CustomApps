using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA.Components
{
    public class Command : DriveTemplate, ISAComponent
    {
        readonly ServiceInstance _parent;

        [DGVColumn(ColumnPosition.After, "HostType")]
        public string HostTypeName => _parent.HostTypeName;

        [DGVColumn(ColumnPosition.After, "Command")]
        public override string Name { get; set; }

        public Command(ServiceInstance parent, string path) : base(path)
        {
            _parent = parent;
        }
    }
}
