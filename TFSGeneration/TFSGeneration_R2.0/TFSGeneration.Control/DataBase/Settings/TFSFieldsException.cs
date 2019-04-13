using System;

namespace TFSAssist.Control.DataBase.Settings
{
	[Serializable]
	public class TFSFieldsException : Exception
	{
		public TFSFieldsException()
		{

		}
		public TFSFieldsException(string message) : base(message)
		{

		}
	    public TFSFieldsException(string message, Exception innerException) : base(message, innerException)
	    {

	    }
	}
}
