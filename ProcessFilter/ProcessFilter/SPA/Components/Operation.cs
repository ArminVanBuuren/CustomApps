using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA.Components
{
    public class Operation : ObjectTemplate
    {
        private readonly NetworkElement parent;

        [DGVColumn(ColumnPosition.After, "Operation")]
        public sealed override string Name { get; protected set; }

        [DGVColumn(ColumnPosition.Before, "Network Element")]
        public string NetworkElement => parent.Name;

        public Operation(string path, int id, NetworkElement parentElement) : base(path, id)
        {
            parent = parentElement;

            if (GetNameWithId(Name, out string newName, out int newId))
            {
                Name = newName;
                ID = newId;
            }
        }
    }
}
