namespace RdcMan
{
	public abstract class CommonNodeSettings : SettingsGroup
	{
		[Setting("name")]
		protected StringSetting NodeName { get; set; }

		[Setting("comment")]
		public StringSetting Comment { get; protected set; }

		protected CommonNodeSettings(string name)
			: base(name, "properties")
		{
			base.InheritSettingsType.Mode = InheritanceMode.Disabled;
		}

		protected override void Copy(RdcTreeNode node)
		{
			Copy(node.Properties);
		}
	}
}
