using System;
using System.Collections.Generic;
using Protas.Components.PerformanceLog;

namespace Protas.Control.Resource
{
    internal class ResourceSegmentCollection : List<ResourceSegment>, IDisposable
    {
        ShellLog3Net _log3;
        public ResourceSegmentCollection(ILog3NetMain log)
        {
            _log3 = new ShellLog3Net(log);
        }
        public bool UseCoreMode
        {
            get
            {
                try
                {
                    foreach (ResourceSegment segment in this)
                    {
                        if (IsCoreMode(segment))
                            return true;
                    }
                }
                catch (Exception ex)
                {
                    _log3.AddLogForm(Log3NetSeverity.Error, "Exception:{0}\r\n{1}", ex.Message, ex.StackTrace);
                }
                return false;
            }
        }
        bool IsCoreMode(ResourceSegment parentSegment)
        {
            if (parentSegment.ChildSegemnets == null)
            {
                if (parentSegment.UseCoreMode)
                    return true;
                else
                    return false;
            }
            foreach (ResourceSegment segment in parentSegment.ChildSegemnets)
            {
                if (IsCoreMode(segment))
                    return true;
            }
            return false;
        }

        public bool IsAnyUniqueType
        {
            get
            {
                try
                {
                    foreach (ResourceSegment segment in this)
                        if (segment.IsCurrentUniqueType)
                            return true;
                }
                catch (Exception ex)
                {
                    _log3.AddLogForm(Log3NetSeverity.Error, "Exception:{0}\r\n{1}", ex.Message, ex.StackTrace);
                }
                return false;
            }
        }
        public string Source
        {
            get
            {
                string source = string.Empty;
                try
                {
                    foreach (ResourceSegment segment in this)
                        source = string.Format("{0}{1}", source, segment.Source);
                }
                catch (Exception ex)
                {
                    _log3.AddLogForm(Log3NetSeverity.Error, "Exception:{0}\r\n{1}", ex.Message, ex.StackTrace);
                }
                return source;
            }
        }

        public string Result
        {
            get
            {
                
                try
                {
                    string result = string.Empty;

                    foreach (ResourceSegment segment in this)
                        result = string.Format("{0}{1}", result, segment.Result);

                    return result;
                }
                catch (Exception ex)
                {
                    _log3.AddLogForm(Log3NetSeverity.Error, "Exception:{0}\r\n{1}", ex.Message, ex.StackTrace);
                    return string.Empty;
                }
            }
        }

        public bool IsResource
        {
            get
            {
                try
                {
                    foreach (ResourceSegment segment in this)
                        if (segment.IsResource || (segment.ChildSegemnets != null && segment.ChildSegemnets.IsResource))
                            return true;
                }
                catch (Exception ex)
                {
                    _log3.AddLogForm(Log3NetSeverity.Error, "Exception:{0}\r\n{1}", ex.Message, ex.StackTrace);
                }
                return false;
            }
        }
        public void InitializeHandlers()
        {
            try
            {
                foreach (ResourceSegment segment in this)
                {
                    if (segment.ChildSegemnets != null)
                    {
                        segment.AddLogForm(Log3NetSeverity.Debug, "Initialize Childs Segments, Count:{0}", segment.ChildSegemnets.Count);
                        segment.ChildSegemnets.InitializeHandlers();
                    }
                    segment.PrimaryInitializer();
                }
            }
            catch (Exception ex)
            {
                _log3.AddLogForm(Log3NetSeverity.Error, "Exception:{0}\r\n{1}", ex.Message, ex.StackTrace);
            }
        }

        public override string ToString()
        {
            return Source;
        }

        public void Dispose()
        {

        }

    }
}
