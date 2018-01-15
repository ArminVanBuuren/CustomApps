using Protas.Components.PerformanceLog;
using System;
using System.Collections.Generic;
using System.Timers;

namespace Protas.Control.Resource
{
    internal class ResourceHandlerCollection : List<ResourceHandler>, IDisposable
    {
        public int uniqueIdHandler = -1;
        public ResourceHandlerCollection()
        {
            Timer _timer = new Timer { Interval = 60000 };
            _timer.Elapsed += RemoveNotUsedHandlers;
            _timer.Enabled = true;
        }

        void RemoveNotUsedHandlers(object sender, ElapsedEventArgs e)
        {
            lock(this)
            {
                List<ResourceHandler> handlersForRemove = new List<ResourceHandler>();
                foreach (ResourceHandler handler in this)
                {
                    if (DateTime.Now.Subtract(handler.TimeForLastAppeal).TotalSeconds >= 60)
                        handlersForRemove.Add(handler);
                }
                foreach (ResourceHandler handler in handlersForRemove)
                {
                    base.Remove(handler);
                }
            }
        }

        public ResourceHandler GetExistingHandler(ResourceSegment segment, ResourceSignature signature)
        {
            if (signature == null)
                return null;
            //лочить обязательно, т.к. меняется при добавлении и удаении из класса ResourceSignature
            lock (this)
            {
                foreach (ResourceHandler existing in this)
                {
                    if (existing == null)
                        continue;
                    if (existing.Shell.Equals(signature.Shell) && existing.Constructor.Equals(signature.Constructor))
                    {
                        return existing;
                    }
                }
            }
            return null;
        }
        public new void Add(ResourceHandler signature)
        {
            //никаких добавлений готовых сигнатур
        }
        public ResourceHandler Add(ResourceSignature link, Log3Net log)
        {
            lock (this)
            {
                uniqueIdHandler++;
                ResourceHandler newSignature = new ResourceHandler(uniqueIdHandler, link.Shell, link.Constructor, log);
                base.Add(newSignature);
                return newSignature;
            }
        }
        public new void AddRange(IEnumerable<ResourceHandler> collection)
        {
            //никаких добавлений коллекций cигнатур
        }
        public new void Remove(ResourceHandler item)
        {
            //никаких удалений из вне
        }
        public new void RemoveAll(Predicate<ResourceHandler> match)
        {
            //никаких удалений из вне
        }
        public new void RemoveAt(int index)
        {
            //никаких удалений из вне
        }
        public new void RemoveRange(int index, int count)
        {
            //никаких удалений из вне
        }
        public override string ToString()
        {
            lock (this)
            {
                int i = -1;
                string result = string.Empty;
                foreach (ResourceHandler handler in this)
                {
                    i++;
                    result = string.Format("{3}{0}{1}.\"{2}\"", result, i, handler, (i > 0) ? "\r\n" : string.Empty);
                }
                return result;
            }
        }

        public void Dispose()
        {
            lock(this)
            {
                foreach (ResourceHandler handler in this)
                    handler.Dispose();
            }
        }
    }
}