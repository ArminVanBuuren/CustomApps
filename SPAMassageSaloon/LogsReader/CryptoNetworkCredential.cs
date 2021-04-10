using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Security;
using Utils.Crypto;

namespace LogsReader
{
	[Serializable]
	public class CryptoNetworkCredential
	{
		[field: NonSerialized]
		private IReadOnlyList<NetworkCredential> _value;

		internal string UserName { get; }

		internal string Password { get; }

		[field: NonSerialized]
		public IReadOnlyList<NetworkCredential> Value
		{
			get
			{
				if (_value != null)
					return _value;

				var listCredential = new List<NetworkCredential>();
				var securedPassword = new SecureString();
				foreach (var ch in AES.DecryptStringAES(Password, nameof(CryptoNetworkCredential)))
					securedPassword.AppendChar(ch);

				var userName = GetUserName();
				var domain_Username = userName.Split('\\');

				// сначала приоритетнее c доменом, должен быть первым в списке
				if (domain_Username.Length > 1)
				{
					listCredential.Add(new NetworkCredential(domain_Username[1], securedPassword, domain_Username[0]));
					//listCredential.Add(new NetworkCredential(domain_Username[1], securedPassword));
				}
				else
				{
					listCredential.Add(new NetworkCredential(userName, securedPassword));
				}

				_value = new ReadOnlyCollection<NetworkCredential>(listCredential);
				return _value;
			}
		}

		public CryptoNetworkCredential(string userName, string password)
		{
			UserName = AES.EncryptStringAES(userName, nameof(CryptoNetworkCredential));
			Password = AES.EncryptStringAES(password, nameof(CryptoNetworkCredential));
		}

		public string GetUserName() => AES.DecryptStringAES(UserName, nameof(CryptoNetworkCredential));
	}
}