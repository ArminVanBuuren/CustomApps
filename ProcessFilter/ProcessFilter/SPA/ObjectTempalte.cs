﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FormUtils.DataGridViewHelper;
using Utils.IOExploitation;
using static FormUtils.DataGridViewHelper.DGVEnhancer;

namespace ProcessFilter.SPA
{

    public class ObjectTempalte
    {
        public ObjectTempalte(string path, int id)
        {
            ID = id;
            Name = path.GetLastNameInPath(true);
            FilePath = path;
        }

        [DGVColumn(ColumnPosition.First, "ID")]
        public int ID { get; protected set; }

        [DGVColumn(ColumnPosition.After, "Name")]
        public virtual string Name { get; protected set; }

        [DGVColumn(ColumnPosition.Last, "File Path")]
        public virtual string FilePath { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}