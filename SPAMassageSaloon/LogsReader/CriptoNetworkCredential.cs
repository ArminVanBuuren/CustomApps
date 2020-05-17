using System;
using System.Net;
using System.Security;
using Utils;
using Utils.Crypto;

namespace LogsReader
{
	[Serializable]
	public class CryptoNetworkCredential
	{
		[field: NonSerialized] private NetworkCredential _value;

		internal string Domain { get; }

		internal string UserName { get; }

		internal string Password { get; }

		[field: NonSerialized]
		public NetworkCredential Value
		{
			get
			{
				if (_value != null)
					return _value;

				var password = AES.DecryptStringAES(Password, nameof(CryptoNetworkCredential));
				var securedPassword = new SecureString();
				foreach (var ch in password)
					securedPassword.AppendChar(ch);

				if (Domain.IsNullOrEmptyTrim())
					_value = new NetworkCredential(UserName, securedPassword);
				else
					_value = new NetworkCredential(UserName, securedPassword, Domain);

				return _value;
			}
		}

		public CryptoNetworkCredential(string domain, string userName, string password)
		{
			Domain = domain;
			UserName = userName;
			Password = AES.EncryptStringAES(password, nameof(CryptoNetworkCredential));
		}
	}
}
