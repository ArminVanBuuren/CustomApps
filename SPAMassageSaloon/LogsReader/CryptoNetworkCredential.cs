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

				var securedPassword = new SecureString();
				foreach (var ch in AES.DecryptStringAES(Password, nameof(CryptoNetworkCredential)))
					securedPassword.AppendChar(ch);

				// сначала приоритетнее без домена, должен быть первым в списке
				var listCredential = new List<NetworkCredential> { new NetworkCredential(UserName, securedPassword) };
				var domain_Username = UserName.Split('\\');

				if (domain_Username.Length > 1)
				{
					listCredential.Add(new NetworkCredential(domain_Username[1], securedPassword, domain_Username[0]));
					listCredential.Add(new NetworkCredential(UserName, securedPassword, domain_Username[0]));
				}

				_value = new ReadOnlyCollection<NetworkCredential>(listCredential);
				return _value;
			}
		}

		public CryptoNetworkCredential(string userName, string password)
		{
			UserName = userName;
			Password = AES.EncryptStringAES(password, nameof(CryptoNetworkCredential));
		}
	}
}