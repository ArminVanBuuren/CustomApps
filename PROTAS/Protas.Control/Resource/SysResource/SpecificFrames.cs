using System;
using System.IO;
using Protas.Components.Functions;
using Protas.Control.Resource.Base;
using Protas.Components.XPackage;

namespace Protas.Control.Resource.SysResource
{
    internal class RSXpackDoc : ResourceSpecificFrame
    {
        FileSystemWatcher _watcher;
        public override bool IsTrigger
        {
            get { return _watcher.EnableRaisingEvents; }
            set { _watcher.EnableRaisingEvents = value; }
        }
        internal static int MinCountParams = 2;
        public RSXpackDoc(ResourceConstructor constructor) : base(constructor)
        {
            PathProperty path = new PathProperty(Constructor[0]);
            _watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(path.FolderPath),
                Filter = Path.GetFileName(path.FileName),
                NotifyFilter = NotifyFilters.LastWrite
            };
            _watcher.Changed += (FileOnChanged);
        }

        void FileOnChanged(object source, FileSystemEventArgs e)
        {
            if (e != null)
                ResourceChanged?.Invoke(this, GetResult());
        }
        public override XPack GetResult()
        {
            string strOut = string.Empty;
            string docPath = Path.GetFullPath(Constructor[0]);
            if (!File.Exists(docPath))
                return new XPack();
            XmlTransform xfile = new XmlTransform(docPath, null);
            if (xfile.IsCorrect)
            {
                foreach (string str in ProtasFunk.GetStringByXPath(xfile.XPathNavigator, Constructor[1]))
                {
                    strOut = strOut + str + "\r\n";
                }
            }
            return new XPack(string.Empty, strOut);
        }
        public override void Dispose()
        {
            _watcher?.Dispose();
            base.Dispose();
        }
    }
    internal class RSFileSystemWatcher : ResourceSpecificFrame
    {
        string _folderPath = string.Empty;
        string _stringFilter = string.Empty;
        FileSystemWatcher _watcher;
        WatcherChangeTypes _filter;
        PathProperty _pathProp;
        internal static int MinCountParams = 1;
        public override bool IsTrigger
        {
            get
            {
                if (_watcher != null)
                    return _watcher.EnableRaisingEvents;
                return false;
            }
            set { Initialize(value); }
        }

        public RSFileSystemWatcher(ResourceConstructor constructor) : base(constructor)
        {
            Initialize(false);
        }
        void Initialize(bool isevent)
        {
            _pathProp = new PathProperty(Constructor[0]);
            if (!_pathProp.Exists)
                return;
            if (_pathProp.Attributes == FileAttributes.Directory)
            {
                _folderPath = _pathProp.FolderPath;
                if (Constructor.Count > 1)
                    _stringFilter = Constructor[1];
            }
            else
            {
                _folderPath = _pathProp.FolderPath;
                _stringFilter = _pathProp.FileName;
            }
            if (Constructor.Count > 2)
                _filter = GetOptionsEnum(Constructor[2]);
            else
                _filter = WatcherChangeTypes.All;
            _watcher = new FileSystemWatcher
            {
                Path = _folderPath,
                Filter = _stringFilter,
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
            };
            _watcher.Changed += (FileOnChanged);
            _watcher.Created += (FileOnChanged);
            _watcher.Deleted += (FileOnChanged);
            _watcher.Renamed += (FileOnChanged);
            _watcher.EnableRaisingEvents = isevent;
        }

        static WatcherChangeTypes GetOptionsEnum(string str)
        {
            switch (str.ToLower().Trim())
            {
                case "created": return WatcherChangeTypes.Created;
                case "deleted": return WatcherChangeTypes.Deleted;
                case "changed": return WatcherChangeTypes.Changed;
                case "renamed": return WatcherChangeTypes.Renamed;
                default: return WatcherChangeTypes.All;
            }
        }
        void FileOnChanged(object source, FileSystemEventArgs e)
        {
            if (e != null)
                FileOnChanged(e.ChangeType);
        }
        void FileOnChanged(object sender, RenamedEventArgs e)
        {
            if (e != null)
                FileOnChanged(e.ChangeType);
        }
        string _eventResult = string.Empty;
        void FileOnChanged(WatcherChangeTypes type)
        {
            if (ResourceChanged != null && _filter == type)
            {
                _eventResult = (string.IsNullOrEmpty(_eventResult)) ? type.ToString("g") : _eventResult + ";" + type.ToString("g");
                ResourceChanged.Invoke(this, GetResult());
            }
        }

        public override XPack GetResult()
        {
            if (_pathProp == null)
                return null;
            XPack result;
            if (_pathProp.Exists)
                result = new XPack(string.Empty, "Exist");
            else
                result = new XPack(string.Empty, "NotExist");
            result.ChildPacks.Add(new XPack("Attributes", _pathProp.Attributes.ToString("g")));
            result.ChildPacks.Add(new XPack("FullPath", _pathProp.FullPath));
            result.ChildPacks.Add(new XPack("FolderPath", _folderPath));
            result.ChildPacks.Add(new XPack("Filter", _stringFilter));
            if (!string.IsNullOrEmpty(_eventResult) && IsTrigger)
            {
                result.ChildPacks.Add(new XPack("StatusEvent", _eventResult));
                _eventResult = string.Empty;
            }
            else
                result.ChildPacks.Add(new XPack("StatusEvent", string.Empty));
            return result;
        }
        public override void Dispose()
        {
            _watcher?.Dispose();
            base.Dispose();
        }
    }

    internal class RSLocalPath : ResourceSpecificFrame
    {
        XPack _result;
        internal static int MinCountParams = 0;
        public RSLocalPath(ResourceConstructor constructor) : base(constructor)
        {
            _result = new XPack(string.Empty, AppDomain.CurrentDomain.SetupInformation.ApplicationBase);
        }
        public override XPack GetResult()
        {
            _result.Value = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            return _result;
        }
    }

}
