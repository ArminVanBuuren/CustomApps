using System;
using System.Runtime.Serialization;
using XPackage;

namespace Script.Control.Handlers.Arguments
{

    [Serializable]
    public class HandlerInitializationException : Exception
    {
        /// <summary>
        /// Just create the exception
        /// </summary>
        public HandlerInitializationException()
            : base()
        {
        }

        /// <summary>
        /// Create the exception with description
        /// </summary>
        /// <param name="message">Exception description</param>
        public HandlerInitializationException(string message)
            : base(message)
        {
        }
        /// <summary>
        /// If Handler parameter Is Null Or Incorrect
        /// </summary>
        public HandlerInitializationException(IdentifierAttribute varIsNull, bool isNull)
            : base(string.Format("Parameter [{0}] {1}", varIsNull.Name, isNull ? "Cant't Be Null Or Empty" : "Is Incorrect"))
        {
        }
        /// <summary>
        /// If Parent Handler Is Incorrect
        /// </summary>
        public HandlerInitializationException(XPack current)
            : base(string.Format("Parent Object [{0}] Is Incorrect For Current Object [{1}]", current.Parent, current))
        {
        }

        /// <summary>
        /// Scpecial StringFormat Exception
        /// </summary>
        /// <param name="message">Exception description</param>
        /// <param name="obj"></param>
        public HandlerInitializationException(string message, params object[] obj)
            : base(string.Format(message, obj))
        {
        }

        /// <summary>
        /// Create the exception from serialized data.
        /// Usual scenario is when exception is occured somewhere on the remote workstation
        /// and we have to re-create/re-throw the exception on the local machine
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Serialization context</param>
        protected HandlerInitializationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
