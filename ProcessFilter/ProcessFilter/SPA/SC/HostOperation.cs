using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessFilter.SPA.SC
{
    public class HostOperation
    {
        public string HostType { get; }
        public CFS BaseCFS { get; }

        public BindingServices BindServices { get; }
        public List<CFS> ChildCFS { get; } = new List<CFS>();

        public RFS SC_RFS { get; private set; }

        public int Index { get; } = 1;
        private string _operationName;
        public string OperationName
        {
            get
            {
                if (BaseCFS != null)
                    return _operationName + "_" + (Index < 10 ? "0" + Index : Index.ToString());
                return _operationName;
            }
        }

        public HostOperation(string opName, string hostType, BindingServices bindSrv)
        {
            _operationName = opName;
            HostType = hostType;
            BindServices = bindSrv;
        }

        public HostOperation(CFS baseCFS, string opName, string hostType, int index, BindingServices bindSrv)
        {
            _operationName = opName;
            BaseCFS = baseCFS;
            HostType = hostType;
            Index = index;
            BindServices = bindSrv;
        }

        internal RFS GenerateRFS(CFS parentCFS)
        {
            SC_RFS = new RFS(parentCFS, this);
            return SC_RFS;
        }

        public override string ToString()
        {
            return $"{OperationName}_{HostType}";
        }
    }
}