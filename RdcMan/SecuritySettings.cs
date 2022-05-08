using System.Collections.Generic;
using System.Windows.Forms;

namespace RdcMan {
	public class SecuritySettings : SettingsGroup {
		//internal const string TabName = "Security Settings";

		private static Dictionary<string, SettingProperty> _settingProperties;

		protected override Dictionary<string, SettingProperty> SettingProperties => _settingProperties;

		[Setting("authentication", DefaultValue = RdpClient.AuthenticationLevel.Warn)]
		public EnumSetting<RdpClient.AuthenticationLevel> AuthenticationLevel { get; private set; }

		[Setting("restrictedAdmin")]
		public BoolSetting RestrictedAdmin { get; private set; }

		[Setting("remoteGuard")]
		public BoolSetting RemoteGuard { get; private set; }

		static SecuritySettings() {
			typeof(SecuritySettings).GetSettingProperties(out _settingProperties);
		}

		public SecuritySettings()
			: base("∞≤»´…Ë÷√", "securitySettings") { }

		public override TabPage CreateTabPage(TabbedSettingsDialog dialog) {
			return new SecuritySettingsTabPage(dialog, this);
		}

		protected override void Copy(RdcTreeNode node) {
			Copy(node.SecuritySettings);
		}
	}
}
