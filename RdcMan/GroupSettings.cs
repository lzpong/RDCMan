using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace RdcMan
{
	public class GroupSettings : CommonNodeSettings
	{
		internal const string TabName = "Group Settings";

		private static Dictionary<string, SettingProperty> _settingProperties;

		public StringSetting GroupName => base.NodeName;

		[Setting("expanded")]
		public BoolSetting Expanded
		{
			get;
			protected set;
		}

		protected override Dictionary<string, SettingProperty> SettingProperties => _settingProperties;

		static GroupSettings()
		{
			typeof(GroupSettings).GetSettingProperties(out _settingProperties);
		}

		public GroupSettings()
			: base("Group Settings")
		{
		}

		protected GroupSettings(string name)
			: base(name)
		{
		}

		public override TabPage CreateTabPage(TabbedSettingsDialog dialog)
		{
			return new GroupPropertiesTabPage(dialog, this);
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

		protected override void Copy(RdcTreeNode node)
		{
		}
	}
}
