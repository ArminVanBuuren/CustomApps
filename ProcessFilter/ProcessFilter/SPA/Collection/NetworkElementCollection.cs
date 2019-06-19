using System;
using System.IO;
using System.Linq;
using SPAFilter.SPA.Components;

namespace SPAFilter.SPA.Collection
{
    public class NetworkElementCollection
    {
        public CollectionNetworkElement Elements { get; } = new CollectionNetworkElement();

        public NetworkElementCollection(string opsPath)
        {
            var files = Directory.GetDirectories(opsPath).ToList();
            files.Sort(StringComparer.CurrentCulture);

            int i = 0;
            foreach (string operation in files)
            {
                Elements.Add(new NetworkElement(operation, ++i));
            }
        }
    }
}