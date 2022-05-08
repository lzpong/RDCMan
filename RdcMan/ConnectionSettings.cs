using System.Collections.Generic;
using System.Windows.Forms;

namespace RdcMan {
	public class ConnectionSettings : SettingsGroup {
		//internal const string TabName = "连接设置";

		private static Dictionary<string, SettingProperty> _settingProperties;

		protected override Dictionary<string, SettingProperty> SettingProperties => _settingProperties;

		[Setting("connectToConsole")]
		public BoolSetting ConnectToConsole { get; private set; }

		[Setting("startProgram")]
		public StringSetting StartProgram { get; private set; }

		[Setting("workingDir")]
		public StringSetting WorkingDir { get; private set; }

		[Setting("port", DefaultValue = 3389)]
		public IntSetting Port { get; private set; }

		[Setting("loadBalanceInfo")]
		public StringSetting LoadBalanceInfo { get; private set; }

		static ConnectionSettings() {
			typeof(ConnectionSettings).GetSettingProperties(out _settingProperties);
		}

		public ConnectionSettings()
			: base("连接设置", "connectionSettings") {
		}

		public override TabPage CreateTabPage(TabbedSettingsDialog dialog) => new ConnectionSettingsTabPage(dialog, this);

		protected override void Copy(RdcTreeNode node) => Copy(node.ConnectionSettings);
	}
}
