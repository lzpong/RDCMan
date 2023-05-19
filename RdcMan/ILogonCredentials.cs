namespace RdcMan
{
	public interface ILogonCredentials
	{
		string ProfileName { get; }

		ProfileScope ProfileScope { get; }

		string UserName { get; }

		PasswordSetting Password { get; }

		string Domain { get; }
	}
}
