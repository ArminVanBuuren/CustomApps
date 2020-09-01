using System;

namespace SPAFilter.SPA.SC
{
    [Flags]
    internal enum LinkType
    {
        Add = 0,
        Remove = 1
    }

    internal class CFS_RFS
    {
        private HostOperation _parent;
        protected internal CFS_RFS(CFS cfs, LinkType link, HostOperation parent)
        {
            ParentCFS = cfs;
            Link = link;
            _parent = parent;
        }

        protected internal void ChangeLinkType(LinkType link)
        {
            Link = Link & link;
        }

        protected internal CFS ParentCFS { get; }
        protected internal LinkType Link { get; private set; }
        public override string ToString() => $"{ParentCFS.Name} = {Link:G}";
    }
}
