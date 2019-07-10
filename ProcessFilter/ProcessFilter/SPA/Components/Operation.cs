﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA.Components
{
    public class Operation : ObjectTemplate
    {
        [DGVColumn(ColumnPosition.After, "Operation")]
        public sealed override string Name { get; protected set; }

        [DGVColumn(ColumnPosition.Before, "HostType")]
        public string HostTypeName { get; protected set; }


        public Operation(string path, int id) : base(path, id)
        {

        }

        public Operation(int id) : base(id)
        {

        }
    }
}
