using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace RdcMan
{
	public class ServerSettings : CommonNodeSettings
	{
		internal const string TabName = "Server Settings";

		private static Dictionary<string, SettingProperty> _settingProperties;

		public StringSetting ServerName => base.NodeName;

		[Setting("displayName")]
		public StringSetting DisplayName
		{
			get;
			private set;
		}

		[Setting("connectionType")]
		public EnumSetting<ConnectionType> ConnectionType
		{
			get;
			private set;
		}

		[Setting("vmId")]
		public StringSetting VirtualMachineId
		{
			get;
			private set;
		}

		protected override Dictionary<string, SettingProperty> SettingProperties => _settingProperties;

		static ServerSettings()
		{
			typeof(ServerSettings).GetSettingProperties(out _settingProperties);
		}

		public ServerSettings()
			: base("Server Settings")
		{
		}

		public override TabPage CreateTabPage(TabbedSettingsDialog dialog)
		{
			return new ServerPropertiesTabPage(dialog, this);
		}

		protected override void WriteSettings(XmlTextWriter tw, RdcTreeNode node)
		{
			HashSet<ISetting> hashSet = new HashSet<ISetting>();
			if (ConnectionType.Value == RdcMan.ConnectionType.Normal)
			{
				hashSet.Add(ConnectionType);
				hashSet.Add(VirtualMachineId);
			}
			if (ServerName.Value.Equals(DisplayName.Value))
			{
				hashSet.Add(DisplayName);
			}
			if (string.IsNullOrEmpty(base.Comment.Value))
			{
				hashSet.Add(base.Comment);
			}
			base.WriteSettings(tw, node, hashSet);
		}

		protected override void Copy(RdcTreeNode node)
		{
			ServerBase serverBase = node as ServerBase;
			if (serverBase != null)
			{
				Copy(serverBase.Properties);
			}
		}
	}
}
