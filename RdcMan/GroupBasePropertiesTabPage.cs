namespace RdcMan {
	internal class GroupBasePropertiesTabPage<TSettingsGroup> : NodePropertiesPage<TSettingsGroup> where TSettingsGroup : GroupSettings {
		private RdcTextBox _groupNameTextBox;

		protected GroupBasePropertiesTabPage(TabbedSettingsDialog dialog, TSettingsGroup settings, string name)
			: base(dialog, settings, name) { }

		protected void AddGroupName(ref int rowIndex, ref int tabIndex) {
			_groupNameTextBox = FormTools.AddLabeledTextBox(this, "������(&G)��", base.Settings.GroupName, ref rowIndex, ref tabIndex);
			_groupNameTextBox.Enabled = true;
			_groupNameTextBox.Validate = delegate {
				_groupNameTextBox.Text = _groupNameTextBox.Text.Trim();
				string text = _groupNameTextBox.Text;
				if (text.Length == 0)
					return "������������";

				string pathSeparator = ServerTree.Instance.PathSeparator;
				return (text.IndexOf(pathSeparator) != -1) ? ("�����Ʋ��ܰ���·���ָ��� \"" + pathSeparator + "\"") : null;
			};
			base.FocusControl = _groupNameTextBox;
		}

		protected override bool CanBeParent(GroupBase group) {
			return group.CanAddGroups();
		}
	}
}
