namespace RdcMan
{
	internal class GroupPropertiesTabPage : GroupBasePropertiesTabPage<GroupSettings>
	{
		public GroupPropertiesTabPage(TabbedSettingsDialog dialog, GroupSettings settings)
			: base(dialog, settings, settings.Name)
		{
			int rowIndex = 0;
			int num = 0;
			AddGroupName(ref rowIndex, ref num);
			AddParentCombo(ref rowIndex, ref num);
			AddComment(ref rowIndex, ref num).Setting = base.Settings.Comment;
		}
	}
}
