using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPAFilter.SPA.Collection;
using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA.Components
{
    [Flags]
    public enum CatalogOperationType
    {
        Add = 1,
        Remove = 2,
        Modify = 4
    }
    public sealed class CatalogOperation : ObjectTemplate
    {
        private readonly string _name;
        private readonly ServiceCatalog _catalog;
        private readonly ObjectTemplate _parent;

        internal string Action { get; set; }

        public override double FileSize { get; } = 0;
        public override string FilePath { get; } = null;

        [DGVColumn(ColumnPosition.Before, "NE")]
        public string NetworkElement => _parent.Name;

        [DGVColumn(ColumnPosition.After, "Name")]
        public override string Name => $"{_catalog.Prefix}.{Action}.{_name}";

        public CatalogOperation(int id, string name, string action, ObjectTemplate parentElement, ServiceCatalog catalog) :base(id)
        {
            _name = name;
            _catalog = catalog;
            _parent = parentElement;
            Action = action;
        }
    }
}
