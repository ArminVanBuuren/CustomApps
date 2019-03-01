using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.BuildUpdater
{
    public class BuildPackCollection : UploadProgress, IUploadProgress, IEnumerable<BuildPack>, IEnumerator<BuildPack>, IDisposable
    {
        int index = -1;
        private List<BuildPack> _collection = new List<BuildPack>();

        const string argument_start = "/C choice /C Y /N /D Y /T 4 & Start \"\" /D \"{0}\" \"{1}\"";
        const string argument_update = "/C choice /C Y /N /D Y /T 4 & Del /F /Q \"{0}\" & choice /C Y /N /D Y /T 2 & Move /Y \"{1}\" \"{2}\"";
        const string argument_update_start = argument_update + " & Start \"\" /D \"{3}\" \"{4}\" {5}";
        const string argument_add = "/C choice /C Y /N /D Y /T 4 & Move /Y \"{0}\" \"{1}\"";
        const string argument_remove = "/C choice /C Y /N /D Y /T 4 & Del /F /Q \"{0}\"";
        

        
        internal void Add(LocalAssemblyInfo currentFile, ServerAssemblyInfo serverFile)
        {
            _collection.Add(new BuildPack(currentFile, serverFile));
        }

        internal bool Update()
        {
            Upload();
            //UpdateUploded();
            return true;
        }


        internal override bool Upload()
        {
            foreach (BuildPack build in _collection)
            {
                
            }

            return false;
        }

        public bool UpdateUploded()
        {
            foreach (BuildPack build in _collection)
            {

            }

            return false;
        }

        void UpdateApplications(List<FileAssemblyInfo> updatesList)
        {
            string argument_complete = "";

            foreach (FileAssemblyInfo file in updatesList)
            {
                //if (file.Type == BuldPerformerType.CreateOrUpdate || file.Type == BuldPerformerType.Update || file.Type == BuldPerformerType.RollBack)
                //{
                //    //argument_complete = string.Format(argument_update, currentPaths[i], tempFilePaths[i], newPaths[i]);
                //}
                //else if ()
                //{

                //}
            }
        }

        void StartProcess(string arguments)
        {
            ProcessStartInfo cmd = new ProcessStartInfo
            {
                Arguments = arguments,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe"
            };
            Process.Start(cmd);
        }

        

        public override int GetProgressPercent()
        {
            int allFileProgress = 0;
            foreach (BuildPack build in _collection)
            {
                allFileProgress += build.GetProgressPercent();
            }

            return allFileProgress / _collection.Count;
        }

        public override long GetUploadedBytes()
        {
            long allFileUploads = 0l;
            foreach (BuildPack build in _collection)
            {
                allFileUploads += build.GetUploadedBytes();
            }

            return allFileUploads;
        }

        public override long GetTotalBytes()
        {
            long allFileTotla = 0l;
            foreach (BuildPack build in _collection)
            {
                allFileTotla += build.GetTotalBytes();
            }

            return allFileTotla;
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
    }
}
