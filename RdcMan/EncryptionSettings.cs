using System.Collections.Generic;
using System.Windows.Forms;

namespace RdcMan {
	public class EncryptionSettings : SettingsGroup {
		//public const string TabName = "加密设置";

		private static Dictionary<string, SettingProperty> _settingProperties;

		protected override Dictionary<string, SettingProperty> SettingProperties => _settingProperties;

		[Setting("encryptionMethod", DefaultValue = RdcMan.EncryptionMethod.LogonCredentials)]
		public EnumSetting<EncryptionMethod> EncryptionMethod { get; private set; }

		[Setting("credentialName")]
		public StringSetting CredentialName { get; private set; }

		[Setting("credentialData")]
		public StringSetting CredentialData { get; private set; }

		static EncryptionSettings() {
			typeof(EncryptionSettings).GetSettingProperties(out _settingProperties);
			_settingProperties["CredentialName"].Attribute.DefaultValue = CredentialsUI.GetLoggedInUser();
		}

		public EncryptionSettings()
			: base("加密设置", "encryptionSettings") {
		}

		public override TabPage CreateTabPage(TabbedSettingsDialog dialog) {
			return new EncryptionSettingsTabPage(dialog, this);
		}

		protected override void Copy(RdcTreeNode node) {
			Copy(node.EncryptionSettings);
		}
	}
}
