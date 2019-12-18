namespace RdcMan
{
	public class SecuritySettingsTabPage : SettingsTabPage<SecuritySettings>
	{
		public SecuritySettingsTabPage(TabbedSettingsDialog dialog, SecuritySettings settings)
			: base(dialog, settings)
		{
			int tabIndex = 0;
			int rowIndex = 0;
			CreateInheritanceControl(ref rowIndex, ref tabIndex);
			FormTools.AddLabeledEnumDropDown(this, "&Authentication", settings.AuthenticationLevel, ref rowIndex, ref tabIndex, RdpClient.AuthenticationLevelToString);
		}
	}
}
