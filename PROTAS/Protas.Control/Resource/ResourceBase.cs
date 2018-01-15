using System;
using System.Collections.Generic;
using Protas.Components.PerformanceLog;
using Protas.Control.Resource.Base;
using Protas.Control.Resource.SysResource;

namespace Protas.Control.Resource
{

    public class ResourceBase : ShellLog3Net, IObjectStatistic, IDisposable
    {
        static int _uniqueIdResourcePack = -1;
        public int UniqueId => _uniqueIdResourcePack;
        /// <summary>
        /// Колличество созданных оболочек ресурса
        /// </summary>
        public int CountResourceKernels { get; private set; } = 0;
        
        internal StandartContext BaseContext { get; private set; }
        public List<ResourceKernel> Resources { get; } = new List<ResourceKernel>();
        internal ResourceHandlerCollection HandlerCollection { get; } = new ResourceHandlerCollection();
        internal List<ResourceSignature> SignatureCollection { get; set; } = new List<ResourceSignature>();
        public ResourceBase()
        {
            Initialize();
        }

        public ResourceBase(ShellLog3Net log) : base(log)
        {
            Initialize();


            //для записи лога
            AddMessagePrefix(string.Format("PCK:{0}", _uniqueIdResourcePack));
            InitInstanceOnStatistic(this);
        }
        void Initialize()
        {
            _uniqueIdResourcePack++;
            BaseContext = new StandartContext();
        }

        public ResourceKernel GetResource(string input, ResourceContextPack contexts)
        {
            if (string.IsNullOrEmpty(input))
            {
                AddLogForm(Log3NetSeverity.Error, "Input String Expression For Resource Is Empty");
                return null;
            }
            CountResourceKernels++;
            if (contexts == null)
                return null;
            ResourceKernel resource = new ResourceKernel(this, CountResourceKernels, input, contexts);
            Resources.Add(resource);
            AddLogForm(Log3NetSeverity.Debug, "Expression:\"{0}\" | Contexts:{1}", input, contexts);
            return resource;
        }
        internal ResourceComplex GetComlex(ResourceContextPack contexts)
        {
            if (contexts == null)
                return null;
            return new ResourceComplex(this, contexts);
        }
        internal ResourceKernel GetInnerResource(string input, List<ResourceShell> blackList, ResourceContextPack contexts)
        {
            if (string.IsNullOrEmpty(input))
                return null;
            if (contexts == null)
                return null;
            ResourceKernel resource = new ResourceKernel(this, -1, input, blackList, contexts);
            AddLogForm(Log3NetSeverity.Debug, "Expression:\"{0}\" | Contexts:{1}", input, contexts);
            return resource;
        }


        public void GetStatistics()
        {
            AddLog(Log3NetSeverity.Report, "Start");
            foreach (ResourceKernel resourceBs in Resources)
                resourceBs.GetStatistic();
            AddLog(Log3NetSeverity.Report, "End");
        }

        public void Dispose()
        {
            foreach (ResourceKernel resObj in Resources)
                resObj.Dispose();
            foreach (ResourceSignature signature in SignatureCollection)
                signature.Dispose();
            HandlerCollection.Dispose();
        }
    }
}