namespace RdcMan
{
	public abstract class CommonDisplaySettings : SettingsGroup
	{
		public const string TabName = "œ‘ æ…Ë÷√";

		[Setting("thumbnailScale", DefaultValue = 1)]
		public IntSetting ThumbnailScale { get; protected set; }

		[Setting("smartSizeDockedWindows")]
		public BoolSetting SmartSizeDockedWindow { get; protected set; }

		[Setting("smartSizeUndockedWindows")]
		public BoolSetting SmartSizeUndockedWindow { get; protected set; }

		protected CommonDisplaySettings()
			: base("Display Settings", "displaySettings")
		{
		}

		protected override void Copy(RdcTreeNode node)
		{
			Copy(node.DisplaySettings);
		}
	}
}
