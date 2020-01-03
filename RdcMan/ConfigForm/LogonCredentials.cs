using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace RdcMan
{
	/// <summary>
	/// 登录验证信息
	/// </summary>
	public class LogonCredentials : SettingsGroup, ILogonCredentials
	{
		internal const string TabName = "登录凭证";

		public static readonly string GlobalStoreName;

		private static Dictionary<string, SettingProperty> _settingProperties;

		protected override Dictionary<string, SettingProperty> SettingProperties => _settingProperties;

		[Setting("profileName")]
		public ProfileSetting ProfileName { get; protected set; }

		[Setting("userName")]
		public StringSetting UserName { get; protected set; }

		[Setting("password")]
		internal PasswordSetting Password{ get; set; }

		[Setting("domain")]
		public StringSetting Domain { get; protected set; }

		string ILogonCredentials.ProfileName => ProfileName.Value;

		ProfileScope ILogonCredentials.ProfileScope => ProfileName.Scope;

		string ILogonCredentials.UserName => UserName.Value;

		PasswordSetting ILogonCredentials.Password => Password;

		string ILogonCredentials.Domain => Domain.Value;

		static LogonCredentials()
		{
			GlobalStoreName = ProfileScope.Global.ToString();
			typeof(LogonCredentials).GetSettingProperties(out _settingProperties);
			_settingProperties["userName"].Attribute.DefaultValue = Environment.UserName;
			_settingProperties["domain"].Attribute.DefaultValue = Environment.UserDomainName;
		}

		public LogonCredentials()
			: this("登录凭证", "logonCredentials")
		{
		}

		public LogonCredentials(string description, string xmlNodeName)
			: base(description, xmlNodeName)
		{
		}

		public override TabPage CreateTabPage(TabbedSettingsDialog dialog)
		{
			return new LogonCredentialsTabPage(dialog, this);
		}

		public bool DirectlyReferences(ILogonCredentials credentials)
		{
			if (base.InheritSettingsType.Mode == InheritanceMode.None && ProfileName.Scope == credentials.ProfileScope)
			{
				return ProfileName.Value == credentials.ProfileName;
			}
			return false;
		}

		public static bool IsCustomProfile(string profileName)
		{
			return string.Compare(profileName, "Custom", ignoreCase: true) == 0;
		}

		public static string ConstructQualifiedName(ILogonCredentials credentials)
		{
			if (IsCustomProfile(credentials.ProfileName))
			{
				return credentials.ProfileName;
			}
			return $"{credentials.ProfileName} ({credentials.ProfileScope})";
		}

		protected override void WriteSettings(XmlTextWriter tw, RdcTreeNode node)
		{
			HashSet<ISetting> hashSet = new HashSet<ISetting>();
			if (ProfileName.Scope != ProfileScope.Local)
			{
				hashSet.Add(UserName);
				hashSet.Add(Password);
				hashSet.Add(Domain);
			}
			base.WriteSettings(tw, node, hashSet);
		}

		protected override void Copy(RdcTreeNode node)
		{
			Copy(node.LogonCredentials);
		}

		public void SetPassword(string clearTextPassword)
		{
			Password.SetPlainText(clearTextPassword);
		}
	}
}
