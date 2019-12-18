namespace RdcMan
{
	internal class GroupBasePropertiesTabPage<TSettingsGroup> : NodePropertiesPage<TSettingsGroup> where TSettingsGroup : GroupSettings
	{
		private RdcTextBox _groupNameTextBox;

		protected GroupBasePropertiesTabPage(TabbedSettingsDialog dialog, TSettingsGroup settings, string name)
			: base(dialog, settings, name)
		{
		}

		protected void AddGroupName(ref int rowIndex, ref int tabIndex)
		{
			TSettingsGroup settings = base.Settings;
			_groupNameTextBox = FormTools.AddLabeledTextBox(this, "组名:", settings.GroupName, ref rowIndex, ref tabIndex);
			_groupNameTextBox.Enabled = true;
			_groupNameTextBox.Validate = delegate
			{
				_groupNameTextBox.Text = _groupNameTextBox.Text.Trim();
				string text = _groupNameTextBox.Text;
				if (text.Length == 0)
				{
					return "请输入组名";
				}
				string pathSeparator = ServerTree.Instance.PathSeparator;
				return (text.IndexOf(pathSeparator) != -1) ? ("组名可能不包含路径分隔符 \"" + pathSeparator + "\"") : null;
			};
			base.FocusControl = _groupNameTextBox;
		}

		protected override bool CanBeParent(GroupBase group)
		{
			return group.CanAddGroups();
		}
	}
}
