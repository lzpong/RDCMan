namespace RdcMan
{
	/// <summary>
	/// 登录验证信息接口
	/// </summary>
	public interface ILogonCredentials {
		string ProfileName { get; }

		ProfileScope ProfileScope { get; }

		string UserName { get; }

		PasswordSetting Password { get; }

		string Domain { get; }
	}
}
