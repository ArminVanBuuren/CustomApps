using System;
using Protas.Components.XPackage;

namespace Protas.Control.Resource.Base
{
    public class ResourceEvent : IDisposable
    {
        public delegate void CallExternal(ResourceKernel segment, XPack result);
        public CallExternal del;
        internal void AddCallback(CallExternal m)
        {
            del += m;
        }

        internal void RemoveCallback(CallExternal m)
        {
            del -= m;
        }

        //or
        public static ResourceEvent operator +(ResourceEvent x, CallExternal m)
        {
            x.AddCallback(m);
            return x;
        }
        public static ResourceEvent operator -(ResourceEvent x, CallExternal m)
        {
            x.RemoveCallback(m);
            return x;
        }

        public void Invoke(ResourceKernel segment, XPack result)
        {
            if (del == null)
                return;
            foreach (CallExternal d in del.GetInvocationList())
            {
                d.Invoke(segment, result);
            }
        }

        public void Dispose()
        {
            del = null;
        }
    }
    public class RHandlerEvent : IDisposable
    {
        public delegate void CallRHandler(IResource resource, XPack result);
        public CallRHandler del;
        internal void AddCallback(CallRHandler m)
        {
            del += m;
        }

        internal void RemoveCallback(CallRHandler m)
        {
            del -= m;
        }

        //or
        public static RHandlerEvent operator +(RHandlerEvent x, CallRHandler m)
        {
            x.AddCallback(m);
            return x;
        }
        public static RHandlerEvent operator -(RHandlerEvent x, CallRHandler m)
        {
            x.RemoveCallback(m);
            return x;
        }

        public void Invoke(IResource resource, XPack result)
        {
            if (del == null)
                return;
            foreach (CallRHandler d in del.GetInvocationList())
            {
                d.Invoke(resource, result);
            }
        }

        public void Dispose()
        {
            del = null;
        }
    }

    internal class RSegmentEvent : IDisposable
    {
        public delegate void CallRSegment(ResourceHandler handler, XPack result);
        public CallRSegment del;
        internal void AddCallback(CallRSegment m)
        {
            del += m;
        }

        internal void RemoveCallback(CallRSegment m)
        {
            del -= m;
        }

        //or
        public static RSegmentEvent operator +(RSegmentEvent x, CallRSegment m)
        {
            x.AddCallback(m);
            return x;
        }
        public static RSegmentEvent operator -(RSegmentEvent x, CallRSegment m)
        {
            x.RemoveCallback(m);
            return x;
        }

        public void Invoke(ResourceHandler handler, XPack result)
        {
            if (del == null)
                return;
            foreach (CallRSegment d in del.GetInvocationList())
            {
                d.Invoke(handler, result);
            }
        }

        public void Dispose()
        {
            del = null;
        }
    }
}
