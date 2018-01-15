using System;
using System.Collections.Generic;
using System.Linq;
using Protas.Components.PerformanceLog;
using Protas.Control.ProcessFrame;
using Protas.Control.ProcessFrame.Components;
using Protas.Control.ProcessFrame.Handlers;
using Protas.Control.Resource;
using Protas.Control.Resource.Base;
using Protas.Components.XPackage;

namespace Protas.Control
{
    internal sealed class Dispatcher : ShellLog3Net, IProcessor, IDisposable
    {
        public const string NODE_MACROS_LIST = @"Config/MacrosList";
        public const string NODE_PROCESS_LIST = @"Config/ProcessList";
        public const string NODE_CLUSTER_LIST = @"Config/ClusterList";
        public const string NODE_BINDING_LIST = @"Config/BindingList";
        public const string NODE_PROCESS = @"Process";
        public const string NODE_CLUSTER = @"Cluster";
        Dictionary<string, Process> CollectionProcess { get; } = new Dictionary<string, Process>(StringComparer.CurrentCultureIgnoreCase);
        Dictionary<string, IHandler> CollectionBindings { get; } = new Dictionary<string, IHandler>(StringComparer.CurrentCultureIgnoreCase);
        List<XPack> CollectionPackBindings { get; set; } = new List<XPack>();
        XPack XPackage { get;}
        public bool IsCorrect { get; private set; } = false;
        public Dispatcher(XPack xpackage, ILog3NetMain log) : base(log)
        {
            XPackage = xpackage;
            Initialize();
        }

        void Initialize()
        {
            List<XPack> collectionPck = XPackage[NODE_BINDING_LIST];
            if (collectionPck != null)
            {
                foreach (XPack bind in collectionPck)
                {
                    CollectionPackBindings.AddRange(bind.ChildPacks);
                }
            }

            CollectionXPack macrosList = XPackage[NODE_MACROS_LIST]?[0]?.ChildPacks;
            StaticContext macros = null;
            if (macrosList != null)
            {
                AddLogForm(Log3NetSeverity.Debug, "Get {0}..", NODE_MACROS_LIST);
                macros = new StaticContext(macrosList);
                AddLogForm(Log3NetSeverity.Debug, "Count Childs {0}={1}", StaticContext.NODE_MACROS_STATIC, macros.EntityCollection.Count);
            }

            AddLogForm(Log3NetSeverity.Debug, "Get {0}..", NODE_PROCESS_LIST);
            CollectionXPack processList = GetItemsByNodeName(XPackage, NODE_PROCESS_LIST, new[] { NODE_PROCESS });

            AddLogForm(Log3NetSeverity.Debug, "Get {0}..", NODE_CLUSTER_LIST);
            CollectionXPack clusterList = GetItemsByNodeName(XPackage, NODE_CLUSTER_LIST, new[] { NODE_CLUSTER });


            ResourceBase resourceBase = new ResourceBase();
            foreach (XPack proc in processList)
            {

                if (proc.Name.Equals(NODE_PROCESS, StringComparison.CurrentCultureIgnoreCase))
                {
                    AddLogForm(Log3NetSeverity.Error, "Only Items \"{0}\" Can Be In The Path=\"{1}\"", NODE_PROCESS, NODE_PROCESS_LIST);
                    continue;
                }

                string clusterName = proc.Attributes[NODE_CLUSTER];
                XPack clust = clusterList["name", clusterName][0];
                //var dwdwd = clusterList["'@name'^='"+ clusterName + "'"];

                if (clust == null)
                {
                    AddLogForm(Log3NetSeverity.Error, "{0}=\"{1}\" Not Finded In Child Of Path {2}", NODE_CLUSTER, clusterName, NODE_CLUSTER_LIST);
                    continue;
                }

                AddLogForm(Log3NetSeverity.Debug, "Initialize {0}=\"{1}\" {2}=\"{3}\"", NODE_PROCESS, proc.Attributes["name"], NODE_CLUSTER, clusterName);
                Process process = new Process(proc, clust, resourceBase, macros, this);
                CollectionProcess.Add(process.Name, process);

            }

            IsCorrect = true;
            AddLog(Log3NetSeverity.Debug, "Complete..");
        }

        CollectionXPack GetItemsByNodeName(XPack main, string path, string[] nodeNames)
        {
            CollectionXPack newObjectByNodeName = new CollectionXPack();

            int i = 0;
            foreach (XPack pack in main[path])
            {
                i++;
                foreach (XPack finded in from finded in pack.ChildPacks
                                         from nodeName in nodeNames
                                         where finded.Name.Equals(nodeName, StringComparison.CurrentCultureIgnoreCase)
                                         select finded)
                {
                    newObjectByNodeName.Add(finded);
                }
            }

            if (i == 0)
                throw new Exception(string.Format("Not Finded Any Items By Path=\"{0}\"", path));

            if (newObjectByNodeName.Count == 0)
                throw new Exception(string.Format("Not Finded Any \"{0}\"", string.Join(",", nodeNames)));

            if (!newObjectByNodeName.IsUniqueAttributeValueByKey("name"))
                throw new Exception(string.Format("In Items \"{0}\" Attribute \"Name\" Is Not Unique. Please Check Your Config File In Path \"{1}\"", string.Join(",", nodeNames), path));

            return newObjectByNodeName;
        }

        /// <summary>
        /// Возвращет уже созданный удаленный таск или создает новый и добавляет в тасков хэндлеров для текущего процесса
        /// </summary>
        /// <param name="inputHndName"></param>
        /// <returns></returns>
        internal IHandler GetRemoteBinding(string inputHndName)
        {
            IHandler existing = CollectionBindings[inputHndName];
            if (existing != null)
                return existing;

            foreach(XPack bindings in CollectionPackBindings)
            {
                string bindName;
                if (!bindings.Attributes.TryGetValue("name", out bindName))
                    continue;
                string procName;
                if (!bindings.Attributes.TryGetValue("process", out procName))
                    continue;
                if (!bindName.Equals(inputHndName, StringComparison.CurrentCultureIgnoreCase))
                    continue;
                Process existingProc;
                if (!CollectionProcess.TryGetValue(procName, out existingProc))
                    continue;

                //todo: недоработано. Непонятно что нужно делать
                RemoteBinding remote = new RemoteBinding(bindName, existingProc, bindings);
                CollectionBindings.Add(bindName, remote);
                return remote;
            }
            return null;
        }

        public bool Start()
        {
            AddLog(Log3NetSeverity.Debug, "Started");
            return false;
        }

        public bool Stop()
        {
            AddLog(Log3NetSeverity.Debug, "Stopped");
            return false;
        }

        public void Dispose()
        {

        }

    }
}