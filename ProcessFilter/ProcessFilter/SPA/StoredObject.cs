using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Utils;
using Utils.Handles;

namespace SPAFilter.SPA
{
    #region Implementation of StoredObject class

    /// <summary>
    /// Represents information about initilizing file, content, object and its location on disk.
    /// </summary>
    [Serializable]
    public class StoredObject
    {
        #region Implementation of Namer class

        /// <summary>
        /// Defines the rules of objects' naming
        /// </summary>
        [Serializable]
        public class Namer
        {
            private const string S_FMT_FILE_NOT_FOUND = "FILE NOT FOUND ( {0:D4} )";
            private const string S_FMT_INVALID = "INVALID XML CONTENT ( {0:D4} )";
            private const string S_FMT_DUPLICATED = "DUPLICATED \"{0}\" ( {1:D4} )";

            private int m_cntInvalid = 0;
            private int m_cntFileNotFound = 0;
            private int m_cntDuplicated = 0;

            #region Public methods

            /// <summary>
            /// Reset namer to initial state
            /// </summary>
            public void Reset()
            {
                m_cntInvalid = 0;
                m_cntFileNotFound = 0;
                m_cntDuplicated = 0;
            }

            /// <summary>
            /// Generates name for invalid object (invalid initializing file)
            /// </summary>
            /// <returns>Name of object</returns>
            public string GetInvalid()
            {
                return string.Format(S_FMT_INVALID, m_cntInvalid++);
            }

            /// <summary>
            /// Generates name for object if initializing file was not found
            /// </summary>
            /// <returns>Name of object</returns>
            public string GetFileNotFound()
            {
                return string.Format(S_FMT_FILE_NOT_FOUND, m_cntFileNotFound++);
            }

            /// <summary>
            /// Generates name of duplicated object
            /// </summary>
            /// <param name="name">Configured name of and object</param>
            /// <returns>Name of object</returns>
            public string GetDuplicated(string name)
            {
                return string.Format(S_FMT_DUPLICATED, name, m_cntDuplicated++);
            }

            #endregion
        }

        #endregion
        /// <summary>
        /// The enumeration represents state of an object
        /// </summary>
        public enum Status
        {
            /// <summary>
            /// Initial state of object
            /// </summary>
            NotSpecified,
            /// <summary>
            /// Invalid file content
            /// </summary>
            InvalidContent,
            /// <summary>
            /// Wrong path to initializing file
            /// </summary>
            FileNotFound,
            /// <summary>
            /// Dulicated name of object (source files may be different)
            /// </summary>
            Duplicated,
            /// <summary>
            /// Initialization failed for unknown reason (e.g. duplicated parameters)
            /// </summary>
            InitializationFailed,
            /// <summary>
            /// Content of configuration file accepted but not validated yet
            /// </summary>
            ConfigurationAccepted,
            /// <summary>
            /// Object has been initialized, i.e. all internal object are valid
            /// </summary>
            Initialized,
            /// <summary>
            /// Object has been successfully validated against dictionary
            /// </summary>
            Valid,
            /// <summary>
            /// Validation against dictionary failed
            /// </summary>
            Invalid
        }

        /// <summary>
        /// The enumeration represents syntax of path which was specified in 
        /// dictionaries configuration
        /// </summary>
        public enum PathType
        {
            /// <summary>
            /// Initial state of path
            /// </summary>
            Undefined,
            /// <summary>
            /// Absolute path, e.g. "c:\foris\spa\sa\config\dict\cmd\abc.xml"
            /// </summary>
            Absolute,
            /// <summary>
            /// Relative to application path, e.g. "..\..\..\dict\cmd\abc.xml"
            /// </summary>
            Relative,
            /// <summary>
            /// File name which must be concatenated to root directory of commands (or other objects), e.g. "abc.xml"
            /// will be transformed to ".\dict\cmd\abc.xml"
            /// </summary>
            FileName
        }

        /// <summary>
        /// Name of an object
        /// </summary>
        private string m_name;
        /// <summary>
        /// Filename with extension, e.g. "abc.xml"
        /// </summary>
        public string FileName;
        /// <summary>
        /// Type of path accordingly enumeration <see cref="PathType"/>
        /// </summary>
        public PathType FilePathType;
        /// <summary>
        /// Path to a file accordingly enumeration <see cref="PathType"/>
        /// </summary>
        public string StringPath;
        /// <summary>
        /// Indicates if file is saved or not
        /// </summary>
        public bool SavedInFile = false;
        /// <summary>
        /// Indicates if and object is saved in a database
        /// </summary>
        public bool SavedInDatabase = false;
        /// <summary>
        /// Indicates current status of object
        /// </summary>
        public Status CurrentStatus = Status.NotSpecified;
        /// <summary>
        /// Indicates an intialization error
        /// </summary>
        public Exception InitializationError;
        /// <summary>
        /// Indicates identifier of an object in database
        /// </summary>
        public int? Identifier;
        /// <summary>
        /// Indicates date and time when an object was loaded from database 
        /// </summary>
        public DateTime? LoadDate;
        /// <summary>
        /// Indicates date and time when an object will be reloaded from database
        /// </summary>
        public DateTime? ReloadDate;
        /// <summary>
        /// Indicates whether an object is archived
        /// </summary>
        public bool Archived;

        #region Properties

        /// <summary>
        /// Name of an object
        /// </summary>
        public string Name
        {
            get => m_name;
            set
            {
                SetName(value);

                m_name = value;
            }
        }

        public bool InvalidContent => CurrentStatus == Status.InvalidContent;

        public bool FileNotFound => CurrentStatus == Status.FileNotFound;

        public bool Initialized => CurrentStatus == Status.Initialized;

        public bool InitializationFailed => CurrentStatus == Status.InitializationFailed;

        public bool Valid => CurrentStatus == Status.Valid;

        public bool Invalid => CurrentStatus == Status.Invalid;

        public bool NotSpecified => CurrentStatus == Status.NotSpecified;

        public bool Duplicated => CurrentStatus == Status.Duplicated;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an empty instance of StoredObject class
        /// </summary>
        public StoredObject()
        {
            FilePathType = PathType.Undefined;
            CurrentStatus = Status.NotSpecified;
        }

        /// <inheritdoc />
        /// <summary>
        /// Create an instance with specified name
        /// </summary>
        /// <param name="name">Name of an object being creating</param>
        public StoredObject(string name) : this(name, Status.NotSpecified)
        {
        }

        /// <summary>
        /// Create an instance with specified name and status
        /// </summary>
        /// <param name="name">Name of an object</param>
        /// <param name="status">Status of an object</param>
        public StoredObject(string name, Status status) : this(name, status, null)
        {
        }

        /// <summary>
        /// Create an instance with specified name, status and initialization error
        /// </summary>
        /// <param name="name">Name of an object</param>
        /// <param name="status">Status of an object</param>
        /// <param name="e">Intialization error</param>
        public StoredObject(string name, Status status, Exception e)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (name == string.Empty)
                throw new ArgumentException("name must not be empty string", nameof(name));

            Name = name;
            CurrentStatus = status;
            InitializationError = e;
        }

        /// <inheritdoc />
        /// <summary>
        /// Create an instance with specified name, status, absoulte path and initialization error
        /// </summary>
        /// <param name="name">Name of an object</param>
        /// <param name="path">Absolute path to initializing file</param>
        /// <param name="status">Status of an object</param>
        /// <param name="e">Intialization error</param>
        public StoredObject(string name, string path, Status status, Exception e) : this(name, status, e)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (path == string.Empty)
                throw new ArgumentException("path must not be empty string", nameof(path));
            if (!Path.IsPathRooted(path))
                throw new ArgumentException("path must be absolute", nameof(path));

            FileName = Path.GetFileName(path);
            FilePathType = PathType.Absolute;
            StringPath = path;
            SavedInFile = File.Exists(StringPath);
        }

        /// <inheritdoc />
        /// <summary>
        /// Create an instance with specified name, status and absoulte path
        /// </summary>
        /// <param name="name">Name of an object</param>
        /// <param name="path">Absolute path to initializing file</param>
        /// <param name="status">Status of an object</param>
        public StoredObject(string name, string path, Status status) : this(name, path, status, null)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Create an instance with specified name, status and absoulte path
        /// </summary>
        /// <param name="name">Name of an object</param>
        /// <param name="path">Absolute path to initializing file</param>
        public StoredObject(string name, string path) : this(name, path, Status.NotSpecified, null)
        {
        }

        /// <summary>
        /// Creates an instance of StoredObject class and initializea path
        /// </summary>
        /// <param name="path">File path</param>
        /// <param name="rootDirectory">Root directory of base object (base host of objects)</param>
        /// <param name="applicationDirectory">Application directory</param>
        public StoredObject(string path, string rootDirectory, string applicationDirectory)
        {
            Initialize(path, rootDirectory, applicationDirectory);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Initializes an instance of StoredObject class and initializes paths basing on 
        /// current directory
        /// </summary>
        /// <param name="path">File path</param>
        /// <param name="rootDirectory">Root directory of base object (base host of objects)</param>
        /// <param name="applicationDirectory">Application directory</param>
        public void Initialize(string path, string rootDirectory, string applicationDirectory)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (path == string.Empty) throw new ArgumentException("path must not be empty string", nameof(path));
            if (rootDirectory == null)
                throw new ArgumentNullException(nameof(rootDirectory));
            if (rootDirectory == string.Empty)
                throw new ArgumentException("root directory must not be empty string", nameof(rootDirectory));
            if (applicationDirectory == null)
                throw new ArgumentNullException(nameof(applicationDirectory));
            if (applicationDirectory == string.Empty)
                throw new ArgumentException("application directory must not be empty string", nameof(applicationDirectory));

            FileName = Path.GetFileName(path);

            if (Path.IsPathRooted(path))
            {
                // absolute path, like "c:\foris\spa\cmd\abc.xml"
                FilePathType = PathType.Absolute;
                StringPath = path;
            }
            else if (Path.GetFileName(path) == path)
            {
                // filename was given - relative to objects (files) directory path
                FilePathType = PathType.FileName;
                StringPath = Path.Combine(rootDirectory, path);
            }
            else
            {
                FilePathType = PathType.Relative;
                StringPath = Path.Combine(applicationDirectory, path);
                StringPath = Path.GetFullPath(path);
            }

            SavedInFile = File.Exists(StringPath);
        }

        /// <summary>
        /// Initializes an instance of StoredObject class and initializes a path
        /// </summary>
        /// <param name="path">File path</param>
        /// <param name="rootDirectory">Root directory of base object (base host of objects)</param>
        public void Initialize(string path, string rootDirectory)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (path == string.Empty)
                throw new ArgumentException("path must not be empty string", nameof(path));
            if (rootDirectory == null)
                throw new ArgumentNullException(nameof(rootDirectory));
            if (rootDirectory == string.Empty)
                throw new ArgumentException("root directory must not be empty string", nameof(rootDirectory));

            FileName = Path.GetFileName(path);

            if (Path.IsPathRooted(path))
            {
                // absolute path, like "c:\foris\spa\cmd\abc.xml"
                FilePathType = PathType.Absolute;
                StringPath = path;

            }
            else if (Path.GetFileName(path) == path)
            {
                // filename was given - relative to commands directory path
                FilePathType = PathType.FileName;
                StringPath = Path.Combine(rootDirectory, path);
            }
            else
            {
                FilePathType = PathType.Relative;
                StringPath = Path.GetFullPath(path);
            }

            SavedInFile = File.Exists(StringPath);
        }

        /// <summary>
        /// Evaluates and sets path and filename 
        /// </summary>
        /// <param name="path">File path</param>
        /// <param name="pathType">Requested type of path</param>
        public void SetPath(string path, string pathType)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (path == string.Empty)
                throw new ArgumentException("path must not be empty string", "path");

            FileName = Path.GetFileName(path);
            StringPath = path;
            SetPathType(pathType);

            switch (FilePathType)
            {
                case PathType.FileName:
                    StringPath = FileName;
                    break;
                case PathType.Absolute when Path.IsPathRooted(path):
                    // nothing to do - absolute path is already assigned
                    break;
                case PathType.Absolute:
                    throw new Exception("Path must be root to used absolute filepath");
                case PathType.Relative:
                    // nothing to do - path is relative to application directory  
                    break;
            }
        }

        /// <summary>
        /// Converts string to value of PathType and set it for an object
        /// </summary>
        /// <param name="pathType">string representation of type of path</param>
        public void SetPathType(string pathType)
        {
            if (pathType == null)
                throw new ArgumentNullException(nameof(pathType));
            FilePathType = (PathType)Enum.Parse(typeof(PathType), pathType);
        }

        /// <summary>
        /// Returns set of valid types of paths. Any type of an array may be assinged to an object
        /// </summary>
        /// <returns>Array of types of paths</returns>
        public static string[] GetValidPathTypes()
        {
            return new[]
                {
                    PathType.Absolute.ToString(),
                    PathType.Relative.ToString(),
                    PathType.FileName.ToString()
                };
        }

        public PathType ConvertToPathType(string pathType)
        {
            return (PathType)Enum.Parse(typeof(PathType), pathType);
        }

        /// <summary>
        /// Evaluates absolute (full) path to a file 
        /// </summary>
        /// <param name="rootDirectory">Root directory of base object (base host of objects)</param>
        /// <param name="applicationDirectory">Application directory</param>
        public string EvaluateRealPath(string rootDirectory, string applicationDirectory)
        {
            switch (FilePathType)
            {
                case PathType.Relative:
                    {
                        var path = IO.EvaluateRelativePath(applicationDirectory, StringPath);
                        path = Path.Combine(applicationDirectory, path);
                        return path;
                    }
                case PathType.FileName:
                    {
                        var path = Path.Combine(rootDirectory, FileName);
                        path = Path.Combine(applicationDirectory, path);
                        return path;
                    }
                default:
                    return StringPath;
            }
        }

        /// <summary>
        /// Evaluates absolute (full) path to a file basing on current directory
        /// </summary>
        /// <param name="obj">Object to be processed</param>
        /// <param name="rootDirectory">Root directory of base object (base host of objects)</param>
        /// <param name="applicationDirectory">Application directory</param>
        public string GetFullPath(StoredObject obj, string rootDirectory, string applicationDirectory)
        {
            switch (obj.FilePathType)
            {
                case PathType.Relative:
                    {
                        var path = IO.EvaluateRelativePath(applicationDirectory, obj.StringPath);
                        return Path.GetFullPath(path);
                    }
                case PathType.FileName:
                    {
                        var path = Path.Combine(rootDirectory, obj.FileName);
                        return Path.GetFullPath(path);
                    }
                default:
                    return obj.StringPath;
            }
        }

        /// <summary>
        /// Evaluates absolute (full) path to a file basing on current directory
        /// </summary>
        /// <param name="rootDirectory">Root directory of base object (base host of objects)</param>
        /// <param name="applicationDirectory">Application directory</param>
        public string GetFullPath(string rootDirectory, string applicationDirectory)
        {
            return GetFullPath(this, rootDirectory, applicationDirectory);
        }

        /// <summary>
        /// Evaluates absolute (full) path to a file basing on current directory
        /// </summary>
        /// <param name="obj">Object to be processed</param>
        /// <param name="rootDirectory">Root directory of base object (base host of objects)</param>
        public string GetFullPath(StoredObject obj, string rootDirectory)
        {
            switch (obj.FilePathType)
            {
                case PathType.Relative:
                    return Path.GetFullPath(obj.StringPath);
                case PathType.FileName:
                    {
                        var path = Path.Combine(rootDirectory, obj.FileName);
                        return Path.GetFullPath(path);
                    }
                default:
                    return obj.StringPath;
            }
        }

        /// <summary>
        /// Evaluates absolute (full) path to a file basing on current directory
        /// </summary>
        /// <param name="rootDirectory">Root directory of base object (base host of objects)</param>
        public string GetFullPath(string rootDirectory)
        {
            return GetFullPath(this, rootDirectory);
        }


        /// <summary>
        /// Stores a current object as XML-file. The methods does not catch exceptions
        /// </summary>
        /// <param name="rootDirectory">Root directory of base object (base host of objects)</param>
        /// <param name="applicationDirectory">Application directory</param>
        public void SaveXmlFile(string rootDirectory, string applicationDirectory)
        {
            string path = null;

            var str = ToXmlString();
            str = str.Replace("&#xD;", string.Empty);
            str = str.Replace("&#xA;", Environment.NewLine);

            switch (FilePathType)
            {
                case PathType.Absolute:
                    path = StringPath;
                    break;
                case PathType.Relative:
                    path = Path.Combine(applicationDirectory, StringPath);
                    path = Path.GetFullPath(path);
                    break;
                case PathType.FileName:
                    path = Path.Combine(rootDirectory, FileName);
                    path = Path.Combine(applicationDirectory, path);
                    path = Path.GetFullPath(path);
                    break;
            }

            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using (var stream = new StreamWriter(path, false, Encoding.UTF8))
            {
                stream.Write(str);
                stream.Close();
            }

            SavedInFile = true;
        }

        #endregion

        #region Public methods - indication of errors

        /// <summary>
        /// Modify object's data due to unexisted file
        /// </summary>
        /// <param name="applicationDirectory">Application directory for evaluation of absoultte path</param>
        /// <param name="namer">Object to generate name (optionally, may be null if name of an object
        /// must be preserved)</param>
        public void ErrorFileNotExist(string applicationDirectory, Namer namer)
        {
            if (namer != null)
            {
                Name = namer.GetFileNotFound();
            }
            CurrentStatus = Status.FileNotFound;
            InitializationError = new Exception("WrongConfiguration");
        }

        /// <summary>
        /// Modify object's data due to unexisted file
        /// </summary>
        /// <param name="namer">Object to generate name (optionally, may be null if name of an object
        /// must be preserved)</param>
        public void ErrorFileNotExist(Namer namer)
        {
            ErrorFileNotExist(null, namer);
        }

        /// <summary>
        /// Modify object's data due to unexisted file
        /// </summary>
        public void ErrorFileNotExist()
        {
            ErrorFileNotExist(null, null);
        }

        /// <summary>
        /// Modify object's data due to duplication of name
        /// </summary>
        /// <param name="namer">Object to generate name (optionally, may be null if name of an object
        /// must be preserved)</param>
        public void ErrorDuplicatedName(Namer namer)
        {
            if (namer != null)
            {
                Name = namer.GetDuplicated(Name);
            }
            CurrentStatus = Status.Duplicated;
            InitializationError = new Exception("WrongConfiguration");
        }

        /// <summary>
        /// Modify object's data due to duplication of name
        /// </summary>
        public void ErrorDuplicatedName()
        {
            ErrorDuplicatedName(null);
        }

        /// <summary>
        /// Modify object's data due to invalid content of initializing file
        /// </summary>
        /// <param name="applicationDirectory">Application directory for evaluation of absoultte path</param>
        /// <param name="namer">Object to generate name (optionally, may be null if name of an object
        /// must be preserved)</param>
        /// <param name="e">Exception indicating an error</param>
        public void ErrorInvalidContent(string applicationDirectory, Namer namer, Exception e)
        {
            if (namer != null)
            {
                Name = namer.GetInvalid();
            }

            CurrentStatus = Status.InvalidContent;

            if (e == null)
            {
                InitializationError = new Exception("WrongConfiguration");
            }
            else
            {
                InitializationError = new Exception("WrongConfiguration", e);
            }
        }

        /// <summary>
        /// Modify object's data due to invalid content of initializing file
        /// </summary>
        /// <param name="namer">Object to generate name (optionally, may be null if name of an object
        /// must be preserved)</param>
        /// <param name="e">Exception indicating an error</param>
        public void ErrorInvalidContent(Namer namer, Exception e)
        {
            ErrorInvalidContent(null, namer, e);
        }

        /// <summary>
        /// Modify object's data due to invalid content of initializing file
        /// </summary>
        /// <param name="e">Exception indicating an error</param>
        public void ErrorInvalidContent(Exception e)
        {
            ErrorInvalidContent(null, null, e);
        }

        /// <summary>
        /// Modify object's data due to invalid content of initializing file
        /// </summary>
        public void ErrorInvalidContent()
        {
            ErrorInvalidContent(null, null);
        }

        /// <summary>
        /// Indicates that requested (configured) path to a file is already used
        /// by another object
        /// </summary>
        /// <param name="path">A requested path</param>
        /// <param name="name">Name of an object which uses the same path</param>
        public void ErrorPathAlreadyInUse(string path, string name)
        {
            CurrentStatus = Status.Invalid;
            InitializationError = new Exception("WrongConfiguration");
        }

        #endregion

        #region Protected overrides

        /// <summary>
        /// This method is used for modification ascentor's internal objects 
        /// which may be linked to a name of parent object
        /// </summary>
        /// <param name="name">New name of an object</param>
        protected virtual void SetName(string name)
        {
        }

        /// <summary>
        /// Returns XML-string representation of an object
        /// </summary>
        public virtual string ToXmlString()
        {
            return null;
        }

        /// <summary>
        /// Returns XML-string representation of an object
        /// </summary>
        /// <param name="formatString">Indicates if special sequences of characters
        /// (like '&#xD') must be replaced</param>
        public virtual string ToXmlString(bool formatString)
        {
            var str = ToXmlString();

            if (str != null && formatString)
                return XML.FormatXmlString(str);

            return str;
        }

        /// <summary>
        /// Returns XML-document representing an object
        /// </summary>
        /// <param name="formatted">Indicates if special sequences of characters
        /// (like '&#xD') must be replaced</param>
        public virtual XmlDocument ToXmlDocument(bool formatted)
        {
            var doc = new XmlDocument();
            doc.LoadXml(formatted ? ToXmlString(true) : ToXmlString());
            return doc;
        }

        /// <summary>
        /// Returns XML-document representing an object
        /// </summary>
        public virtual XmlDocument ToXmlDocument()
        {
            return ToXmlDocument(true);
        }

        #endregion
    }

    #endregion
}
