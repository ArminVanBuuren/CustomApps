using System;

namespace Utils
{
	[Serializable]
	public class MTCallbackException : Exception
	{
		public Exception CallbackError { get; }

		public MTCallbackException(Exception callBackError) : base("Exception when processing callback")
		{
			CallbackError = callBackError;
		}
		public MTCallbackException(Exception taskError, Exception callBackError) : base("Exception when processing callback", taskError)
		{
			CallbackError = callBackError;
		}
	}
}
