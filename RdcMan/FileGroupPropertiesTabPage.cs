using System.Windows.Forms;

namespace RdcMan
{
	internal class FileGroupPropertiesTabPage : GroupBasePropertiesTabPage<FileGroupSettings>
	{
		private readonly TextBox _pathnameTextBox;

		public FileGroupPropertiesTabPage(TabbedSettingsDialog dialog, FileGroupSettings settings)
			: base(dialog, settings, settings.Name)
		{
			int rowIndex = 0;
			int tabIndex = 0;
			AddGroupName(ref rowIndex, ref tabIndex);
			_pathnameTextBox = FormTools.AddLabeledTextBox(this, "Â·¾¶Ãû³Æ£º", ref rowIndex, ref tabIndex);
			AddComment(ref rowIndex, ref tabIndex).Setting = base.Settings.Comment;
		}

		protected override void UpdateControls()
		{
			base.UpdateControls();
			_pathnameTextBox.Enabled = false;
			_pathnameTextBox.Text = ((base.Dialog as FileGroupPropertiesDialog).AssociatedNode as FileGroup).Pathname;
		}
	}
}
