using System;

namespace Utils
{
	[Serializable]
	public class MTCallbackTimeoutException : Exception
	{
		public MTCallbackTimeoutException() : base("Exception when waiting callback")
		{

		}
		public MTCallbackTimeoutException(Exception ex) : base("Exception when waiting callback", ex)
		{

		}
	}
}
