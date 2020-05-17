using System;
using System.Runtime.InteropServices;

namespace Utils
{
	public static class NetworkShare
	{
		/// <summary>
		/// Connects to the remote share
		/// </summary>
		/// <returns>Null if successful, otherwise error message.</returns>
		public static string ConnectToShare(string uri, string username, string password)
		{
			//Create netresource and point it at the share
			var nr = new NETRESOURCE
			{
				dwType = RESOURCETYPE_DISK,
				lpRemoteName = uri
			};

			//Create the share
			var ret = WNetUseConnection(IntPtr.Zero, nr, password, username, 0, null, null, null);

			//Check for errors
			if (ret == NO_ERROR)
				return null;
			else
				return GetError(ret);
		}

		/// <summary>
		/// Remove the share from cache.
		/// </summary>
		/// <returns>Null if successful, otherwise error message.</returns>
		public static string DisconnectFromShare(string uri, bool force)
		{
			//remove the share
			var ret = WNetCancelConnection(uri, force);

			//Check for errors
			if (ret == NO_ERROR)
				return null;
			else
				return GetError(ret);
		}

		#region P/Invoke Stuff

		[DllImport("Mpr.dll")]
		private static extern int WNetUseConnection(
			IntPtr hwndOwner,
			NETRESOURCE lpNetResource,
			string lpPassword,
			string lpUserID,
			int dwFlags,
			string lpAccessName,
			string lpBufferSize,
			string lpResult
		);

		[DllImport("Mpr.dll")]
		private static extern int WNetCancelConnection(
			string lpName,
			bool fForce
		);

		[StructLayout(LayoutKind.Sequential)]
		private class NETRESOURCE
		{
			public int dwScope = 0;
			public int dwType = 0;
			public int dwDisplayType = 0;
			public int dwUsage = 0;
			public string lpLocalName = "";
			public string lpRemoteName = "";
			public string lpComment = "";
			public string lpProvider = "";
		}

		#region Consts

		const int RESOURCETYPE_DISK = 0x00000001;
		const int CONNECT_UPDATE_PROFILE = 0x00000001;

		#endregion

		#region Errors

		const int NO_ERROR = 0;

		const int ERROR_ACCESS_DENIED = 5;
		const int ERROR_ALREADY_ASSIGNED = 85;
		const int ERROR_BAD_DEVICE = 1200;
		const int ERROR_BAD_NET_NAME = 67;
		const int ERROR_BAD_PROVIDER = 1204;
		const int ERROR_CANCELLED = 1223;
		const int ERROR_EXTENDED_ERROR = 1208;
		const int ERROR_INVALID_ADDRESS = 487;
		const int ERROR_INVALID_PARAMETER = 87;
		const int ERROR_INVALID_PASSWORD = 1216;
		const int ERROR_MORE_DATA = 234;
		const int ERROR_NO_MORE_ITEMS = 259;
		const int ERROR_NO_NET_OR_BAD_PATH = 1203;
		const int ERROR_NO_NETWORK = 1222;
		const int ERROR_SESSION_CREDENTIAL_CONFLICT = 1219;

		const int ERROR_BAD_PROFILE = 1206;
		const int ERROR_CANNOT_OPEN_PROFILE = 1205;
		const int ERROR_DEVICE_IN_USE = 2404;
		const int ERROR_NOT_CONNECTED = 2250;
		const int ERROR_OPEN_FILES = 2401;

		private static string GetError(int errNum)
		{
			switch (errNum)
			{
				case ERROR_ACCESS_DENIED: return "Error: Access Denied";
				case ERROR_ALREADY_ASSIGNED: return "Error: Already Assigned";
				case ERROR_BAD_DEVICE: return "Error: Bad Device";
				case ERROR_BAD_NET_NAME: return "Error: Bad Net Name";
				case ERROR_BAD_PROVIDER: return "Error: Bad Provider";
				case ERROR_CANCELLED: return "Error: Cancelled";
				case ERROR_INVALID_ADDRESS: return "Error: Invalid Address";
				case ERROR_INVALID_PARAMETER: return "Error: Invalid Parameter";
				case ERROR_INVALID_PASSWORD: return "Error: Invalid Password";
				case ERROR_MORE_DATA: return "Error: More Data";
				case ERROR_NO_MORE_ITEMS: return "Error: No More Items";
				case ERROR_NO_NET_OR_BAD_PATH: return "Error: No Net Or Bad Path";
				case ERROR_NO_NETWORK: return "Error: No Network";
				case ERROR_BAD_PROFILE: return "Error: Bad Profile";
				case ERROR_CANNOT_OPEN_PROFILE: return "Error: Cannot Open Profile";
				case ERROR_DEVICE_IN_USE: return "Error: Device In Use";
				case ERROR_EXTENDED_ERROR: return "Error: Extended Error";
				case ERROR_NOT_CONNECTED: return "Error: Not Connected";
				case ERROR_OPEN_FILES: return "Error: Open Files";
				case ERROR_SESSION_CREDENTIAL_CONFLICT: return "Error: Credential Conflict";
				default: return "Error: Unknown, " + errNum;
			}
		}

		#endregion

		#endregion
	}
}
