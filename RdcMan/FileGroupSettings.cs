using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace RdcMan
{
	public class FileGroupSettings : GroupSettings
	{
		//internal new const string TabName = "File Settings";

		private static Dictionary<string, SettingProperty> _settingProperties;

		protected override Dictionary<string, SettingProperty> SettingProperties => _settingProperties;

		static FileGroupSettings()
		{
			typeof(FileGroupSettings).GetSettingProperties(out _settingProperties);
		}

		public FileGroupSettings()
			: base("Œƒº˛…Ë÷√")
		{
		}

		protected FileGroupSettings(string name)
			: base(name)
		{
		}

		public override TabPage CreateTabPage(TabbedSettingsDialog dialog)
		{
			return new FileGroupPropertiesTabPage(dialog, this);
		}

		protected override void WriteSettings(XmlTextWriter tw, RdcTreeNode node)
		{
			HashSet<ISetting> hashSet = new HashSet<ISetting>();
			if (string.IsNullOrEmpty(base.Comment.Value))
			{
				hashSet.Add(base.Comment);
			}
			base.WriteSettings(tw, node, hashSet);
		}

		protected override void Copy(RdcTreeNode node) { }
	}
}
