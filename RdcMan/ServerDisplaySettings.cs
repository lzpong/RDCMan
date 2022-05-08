using System.Collections.Generic;
using System.Windows.Forms;

namespace RdcMan {
	public class ServerDisplaySettings : CommonDisplaySettings {
		private static Dictionary<string, SettingProperty> _settingProperties;

		protected override Dictionary<string, SettingProperty> SettingProperties => _settingProperties;

		static ServerDisplaySettings() {
			typeof(ServerDisplaySettings).GetSettingProperties(out _settingProperties);
		}

		public override TabPage CreateTabPage(TabbedSettingsDialog dialog) {
			return new ServerDisplaySettingsTabPage(dialog, this);
		}
	}
}
