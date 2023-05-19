using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace RdcMan
{
	public class RemoteDesktopSettings : SettingsGroup
	{
		//internal const string TabName = "Remote Desktop Settings";

		private static Dictionary<string, SettingProperty> _settingProperties;

		protected override Dictionary<string, SettingProperty> SettingProperties => _settingProperties;

		[Setting("size")]
		public SizeSetting DesktopSize { get; private set; }

		[Setting("sameSizeAsClientArea")]
		public BoolSetting DesktopSizeSameAsClientAreaSize { get; private set; }

		[Setting("fullScreen", DefaultValue = true)]
		public BoolSetting DesktopSizeFullScreen { get; private set; }

		[Setting("colorDepth", DefaultValue = 24)]
		public IntSetting ColorDepth { get; private set; }

		static RemoteDesktopSettings()
		{
			typeof(RemoteDesktopSettings).GetSettingProperties(out _settingProperties);
			_settingProperties["size"].Attribute.DefaultValue = new Size(1024, 768);
		}

		public RemoteDesktopSettings()
			: base("‘∂≥Ã◊¿√Ê…Ë÷√", "remoteDesktop")
		{
		}

		public override TabPage CreateTabPage(TabbedSettingsDialog dialog)
		{
			return new RemoteDesktopTabPage(dialog, this);
		}

		protected override void WriteSettings(XmlTextWriter tw, RdcTreeNode node)
		{
			HashSet<ISetting> hashSet = new HashSet<ISetting>();
			if (DesktopSizeSameAsClientAreaSize.Value || DesktopSizeFullScreen.Value)
			{
				hashSet.Add(DesktopSize);
			}
			base.WriteSettings(tw, node, hashSet);
		}

		protected override void Copy(RdcTreeNode node)
		{
			Copy(node.RemoteDesktopSettings);
		}
	}
}
