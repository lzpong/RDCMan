using System.Collections.Generic;
using System.Windows.Forms;

namespace RdcMan
{
	public class LocalResourcesSettings : SettingsGroup
	{
		internal const string TabName = "Local Resources";

		private static Dictionary<string, SettingProperty> _settingProperties;

		protected override Dictionary<string, SettingProperty> SettingProperties => _settingProperties;

		[Setting("audioRedirection", DefaultValue = RdpClient.AudioCaptureRedirectionMode.DoNotRecord)]
		public EnumSetting<RdpClient.AudioRedirectionMode> AudioRedirectionMode
		{
			get;
			private set;
		}

		[Setting("audioRedirectionQuality", DefaultValue = RdpClient.AudioRedirectionQuality.Dynamic)]
		public EnumSetting<RdpClient.AudioRedirectionQuality> AudioRedirectionQuality
		{
			get;
			private set;
		}

		[Setting("audioCaptureRedirection", DefaultValue = RdpClient.AudioCaptureRedirectionMode.DoNotRecord)]
		public EnumSetting<RdpClient.AudioCaptureRedirectionMode> AudioCaptureRedirectionMode
		{
			get;
			private set;
		}

		[Setting("keyboardHook", DefaultValue = RdpClient.KeyboardHookMode.FullScreenClient)]
		public EnumSetting<RdpClient.KeyboardHookMode> KeyboardHookMode
		{
			get;
			private set;
		}

		[Setting("redirectClipboard", DefaultValue = true)]
		public BoolSetting RedirectClipboard
		{
			get;
			private set;
		}

		[Setting("redirectDrives")]
		public BoolSetting RedirectDrives
		{
			get;
			private set;
		}

		[Setting("redirectDrivesList")]
		public ListSetting<string> RedirectDrivesList
		{
			get;
			private set;
		}

		[Setting("redirectPrinters")]
		public BoolSetting RedirectPrinters
		{
			get;
			private set;
		}

		[Setting("redirectPorts")]
		public BoolSetting RedirectPorts
		{
			get;
			private set;
		}

		[Setting("redirectSmartCards", DefaultValue = true)]
		public BoolSetting RedirectSmartCards
		{
			get;
			private set;
		}

		[Setting("redirectPnpDevices")]
		public BoolSetting RedirectPnpDevices
		{
			get;
			private set;
		}

		static LocalResourcesSettings()
		{
			typeof(LocalResourcesSettings).GetSettingProperties(out _settingProperties);
		}

		public LocalResourcesSettings()
			: base("Local Resources", "localResources")
		{
		}

		public override TabPage CreateTabPage(TabbedSettingsDialog dialog)
		{
			return new LocalResourcesTabPage(dialog, this);
		}

		protected override void Copy(RdcTreeNode node)
		{
			Copy(node.LocalResourceSettings);
		}
	}
}
