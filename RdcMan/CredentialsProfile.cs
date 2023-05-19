namespace RdcMan
{
	public class CredentialsProfile : ILogonCredentials, IDeferDecryption
	{
		public const string CustomProfileName = "Custom";

		private readonly string _profileName;

		private readonly ProfileScope _profileScope;

		private string _userName;

		private PasswordSetting _password;

		private string _domain;

		public string ProfileName => _profileName;

		public ProfileScope ProfileScope => _profileScope;

		public string UserName => _userName;

		public PasswordSetting Password => _password;

		public string Domain => _domain;

		public bool IsDecrypted
		{
			get
			{
				return _password.IsDecrypted;
			}
			set
			{
				_password.IsDecrypted = value;
			}
		}

		public string QualifiedName => LogonCredentials.ConstructQualifiedName(this);

		public CredentialsProfile(string profileName, ProfileScope profileScope, string userName, string password, string domain)
		{
			_profileName = profileName;
			_profileScope = profileScope;
			_userName = userName;
			_password = new PasswordSetting(password)
			{
				IsDecrypted = true
			};
			_domain = domain;
		}

		public CredentialsProfile(string profileName, ProfileScope profileScope, string userName, PasswordSetting password, string domain)
		{
			_profileName = profileName;
			_profileScope = profileScope;
			_userName = userName;
			_password = password;
			_domain = domain;
		}

		public void Decrypt(EncryptionSettings settings)
		{
			_password.Decrypt(settings);
		}

		public override string ToString()
		{
			return ProfileName;
		}
	}
}
