using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Utils.WinForm.DataGridViewHelper;
using Utils;

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