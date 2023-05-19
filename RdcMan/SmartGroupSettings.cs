using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace RdcMan
{
	public class SmartGroupSettings : GroupSettings
	{
		//internal new const string TabName = "Smart Group Settings";

		private static Dictionary<string, SettingProperty> _settingProperties;

		protected override Dictionary<string, SettingProperty> SettingProperties => _settingProperties;

		static SmartGroupSettings()
		{
			typeof(SmartGroupSettings).GetSettingProperties(out _settingProperties);
		}

		public SmartGroupSettings() : base("÷«ƒ‹◊È…Ë÷√") { }

		public override TabPage CreateTabPage(TabbedSettingsDialog dialog)
		{
			return new SmartGroupPropertiesTabPage(dialog, this);
		}

		protected override void WriteSettings(XmlTextWriter tw, RdcTreeNode node)
		{
			HashSet<ISetting> hashSet = new HashSet<ISetting>();
			if (string.IsNullOrEmpty(base.Comment.Value))
				hashSet.Add(base.Comment);

			base.WriteSettings(tw, node, hashSet);
		}

		protected override void Copy(RdcTreeNode node) { }
	}
}
