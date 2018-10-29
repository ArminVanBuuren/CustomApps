using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utils.WinForm.DataGridViewHelper;
using static Utils.WinForm.DataGridViewHelper.DGVEnhancer;

namespace ProcessFilter.SPA
{
    public class CollectionNetworkElements
    {
        public NetworkElementCollection Elements { get; } = new NetworkElementCollection();

        public CollectionNetworkElements(string opsPath)
        {
            List<string> dirs = Directory.GetDirectories(opsPath).ToList();
            dirs.Sort(StringComparer.CurrentCulture);

            int i = 0;
            foreach (string dirNetElem in dirs)
            {
                Elements.Add(new NetworkElement(dirNetElem, ++i));
            }
        }
    }

    public class NetworkElementCollection : List<NetworkElement>
    {
        public List<string> AllNetworkElements => this.Select(p => p.Name).ToList();

        public List<string> AllOperationsName
        {
            get
            {
                List<string> allOps = new List<string>();
                foreach (NetworkElement netEl in this)
                {
                    allOps.AddRange(netEl.Operations.Select(p => p.Name));
                }
                return allOps;
            }
        }

        public List<NetworkElementOpartion> AllOperations
        {
            get
            {
                List<NetworkElementOpartion> allOps = new List<NetworkElementOpartion>();
                foreach (NetworkElement netEl in this)
                {
                    allOps.AddRange(netEl.Operations);
                }
                return allOps;
            }
        }

        public NetworkElementCollection Clone()
        {
            NetworkElementCollection currentClone = new NetworkElementCollection();
            foreach (NetworkElement netElement in this)
            {
                currentClone.Add(netElement.Clone());
            }
            return currentClone;
        }
    }


    

    public class NetworkElement : ObjectTempalte
    {
        public List<NetworkElementOpartion> Operations { get; private set; } = new List<NetworkElementOpartion>();

        public NetworkElement(string path, int id, List<NetworkElementOpartion> ops) :base(path, id)
        {
            Operations.AddRange(ops);
        }

        public NetworkElement(string path, int id) : base(path, id)
        {
            List<string> files = Directory.GetFiles(path).ToList();
            files.Sort(StringComparer.CurrentCulture);

            int i = 0;
            foreach (string operation in files)
            {
                Operations.Add(new NetworkElementOpartion(operation, ++i, this));
            }
        }

        public NetworkElement Clone()
        {
            NetworkElement cloneElement = MemberwiseClone() as NetworkElement;
            cloneElement.Operations = new List<NetworkElementOpartion>();
            cloneElement.Operations.AddRange(Operations);
            return cloneElement;
        }
    }

    public class NetworkElementOpartion : ObjectTempalte
    {
        private NetworkElement parent;

        [DGVColumn(ColumnPosition.After, "Operation")]
        public override string Name { get; protected set; }

        [DGVColumn(ColumnPosition.Before, "Network Element")]
        public string NetworkElement => parent.Name;

        public NetworkElementOpartion(string path, int id, NetworkElement parentElement) :base(path, id)
        {
            parent = parentElement;
        }
    }
}
