namespace RdcMan
{
	internal class GroupPropertiesTabPage : GroupBasePropertiesTabPage<GroupSettings>
	{
		public GroupPropertiesTabPage(TabbedSettingsDialog dialog, GroupSettings settings)
			: base(dialog, settings, settings.Name)
		{
			int rowIndex = 0;
			int tabIndex = 0;
			AddGroupName(ref rowIndex, ref tabIndex);
			AddParentCombo(ref rowIndex, ref tabIndex);
			AddComment(ref rowIndex, ref tabIndex).Setting = base.Settings.Comment;
		}
	}
}
