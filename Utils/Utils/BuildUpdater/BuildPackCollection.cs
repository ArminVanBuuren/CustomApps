using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utils.BuildUpdater
{
    [Serializable]
    public class BuildPackCollection : UploadProgress, IUploadProgress, IEnumerable<BuildPack>, IEnumerator<BuildPack>, IDisposable
    {
        public event UploadBuildHandler OnFetchComplete;
        int index = -1;
        private List<BuildPack> _collection = new List<BuildPack>();
        private Assembly _runningApp;
        private object _lock = new object();
        private int _completedFetchCount = 0;
        private int _waitingFetchCount = 0;
        private bool _inProgress = false;

        public override bool IsUploaded
        {
            get
            {
                foreach (BuildPack build in _collection)
                {
                    if (!build.IsUploaded)
                        return false;
                }

                return true;
            }
        }

        public BuildPackCollection(Assembly runningApp)
        {
            _runningApp = runningApp;
        }

        internal void Add(LocalAssemblyInfo currentFile, ServerAssemblyInfo serverFile)
        {
            BuildPack build = new BuildPack(currentFile, serverFile);
            build.OnFetchComplete += Build_OnFetchComplete;
            _collection.Add(build);
        }

        BuildUpdaterProcessingArgs fetchArgs;

        public override bool Fetch()
        {
            lock (_lock)
            {
                if (!_inProgress)
                {
                    fetchArgs = new BuildUpdaterProcessingArgs(this);
                    try
                    {
                        _inProgress = true;
                        _completedFetchCount = 0;
                        _waitingFetchCount = 0;
                        foreach (BuildPack build in _collection)
                        {
                            if (build.Fetch())
                            {
                                _waitingFetchCount++;
                            }
                        }

                        return true;
                    }
                    catch (Exception ex)
                    {
                        _inProgress = false;
                        fetchArgs.Error = ex;
                        OnFetchComplete.Invoke(this, fetchArgs);
                    }
                }
                return false;
            }
        }

        private void Build_OnFetchComplete(object sender, BuildUpdaterProcessingArgs e)
        {
            lock (_lock)
            {
                if (!_inProgress)
                {
                    if (sender != null && sender is BuildPack)
                    {
                        ((BuildPack)sender).RemoveTempFiles();
                    }
                    return;
                }

                _completedFetchCount++;

                if (e.Error != null)
                    fetchArgs.InnerException.Add(e.Error);

                if (_waitingFetchCount > _completedFetchCount)
                    return;

                if(fetchArgs.InnerException.Count > 0)
                    fetchArgs.Error = new Exception("Catched exception when download files from server!");

                _inProgress = false;
                OnFetchComplete.Invoke(this, fetchArgs);
            }
        }

        public override void Commit()
        {
            if(!IsUploaded)
                return;

            BuildPack runningApp = null;
            foreach (BuildPack build in _collection)
            {
                if (build.CurrentFile != null && build.CurrentFile.IsExecutingFile)
                {
                    runningApp = build;
                    continue;
                }

                build.Commit();
            }

            if (runningApp != null)
                BuildPack.EndOfCommit(runningApp);
            else
                BuildPack.EndOfCommit(_runningApp);

            Process.GetCurrentProcess().Kill();
        }

        public override void RemoveTempFiles()
        {
            foreach (BuildPack build in _collection)
            {
                build.RemoveTempFiles();
            }
        }

        public override int ProgressPercent
        {
            get
            {
                int allFileProgress = 0;
                foreach (BuildPack build in _collection)
                {
                    allFileProgress += build.ProgressPercent;
                }

                return allFileProgress / _collection.Count;
            }
        }

        public override long UploadedBytes
        {
            get
            {
                long allFileUploads = 0l;
                foreach (BuildPack build in _collection)
                {
                    allFileUploads += build.UploadedBytes;
                }

                return allFileUploads;
            }
        }

        public override long TotalBytes
        {
            get
            {
                long allFileTotla = 0l;
                foreach (BuildPack build in _collection)
                {
                    allFileTotla += build.TotalBytes;
                }

                return allFileTotla;
            }
        }

        
        public int Count => _collection.Count;

        object IEnumerator.Current => _collection[index];

        public BuildPack Current => _collection[index];

        bool IEnumerator.MoveNext()
        {
            if (index < Count - 1)
            {
                index++;
                return true;
            }
            ((IEnumerator)this).Reset();
            return false;
        }

        void IEnumerator.Reset()
        {
            index = -1;
        }

        public IEnumerator<BuildPack> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        void IDisposable.Dispose()
        {
            ((IDisposable)_collection).Dispose();
        }

        public override string ToString()
        {
            return $"Count=[{Count}]";
        }
    }
}