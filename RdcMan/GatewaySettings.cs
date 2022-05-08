using System.Collections.Generic;
using System.Windows.Forms;

namespace RdcMan {
	public class GatewaySettings : LogonCredentials {
		//public new const string TabName = "网关设置";

		private static readonly Dictionary<string, SettingProperty> _settingProperties;

		protected override Dictionary<string, SettingProperty> SettingProperties => _settingProperties;

		[Setting("enabled")]
		public BoolSetting UseGatewayServer { get; private set; }

		[Setting("hostName")]
		public StringSetting HostName { get; private set; }

		[Setting("logonMethod", DefaultValue = RdpClient.GatewayLogonMethod.Any)]
		public EnumSetting<RdpClient.GatewayLogonMethod> LogonMethod { get; private set; }

		[Setting("localBypass", DefaultValue = true)]
		public BoolSetting BypassGatewayForLocalAddresses { get; private set; }

		[Setting("credSharing", DefaultValue = true)]
		public BoolSetting CredentialSharing { get; private set; }

		[Setting("enableCredSspSupport", DefaultValue = true, IsObsolete = true)]
		public BoolSetting EnableCredentialSspSupport { get; private set; }

		static GatewaySettings() {
			typeof(GatewaySettings).GetSettingProperties(out _settingProperties);
		}

		public GatewaySettings()
			: base("网关设置", "gatewaySettings") {
		}

		public override TabPage CreateTabPage(TabbedSettingsDialog dialog) {
			return new GatewaySettingsTabPage(dialog, this);
		}

		protected override void Copy(RdcTreeNode node) {
			Copy(node.GatewaySettings);
		}
	}
}
