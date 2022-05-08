namespace RdcMan {
	public abstract class CommonDisplaySettings : SettingsGroup {
		//public const string TabName = "显示设置";

		[Setting("thumbnailScale", DefaultValue = 1)]
		public IntSetting ThumbnailScale { get; protected set; }

		[Setting("smartSizeDockedWindows", DefaultValue = RdpClient.SmartSizeMethod.None)]
		public EnumSetting<RdpClient.SmartSizeMethod> SmartSizeDockedWindow { get; protected set; }

		[Setting("smartSizeUndockedWindows", DefaultValue = RdpClient.SmartSizeMethod.None)]
		public EnumSetting<RdpClient.SmartSizeMethod> SmartSizeUndockedWindow { get; protected set; }

		protected CommonDisplaySettings()
			: base("显示设置", "displaySettings") {
		}

		protected override void Copy(RdcTreeNode node) {
			Copy(node.DisplaySettings);
		}
	}
}
