using System.Windows.Forms;

namespace RdcMan {
	internal class DefaultGroupPropertiesDialog : GroupBasePropertiesDialog {
		protected DefaultGroupPropertiesDialog(GroupBase group, Form parentForm)
			: base(group, "Ĭ������", "ȷ��", parentForm) {
		}

		public static DefaultGroupPropertiesDialog NewPropertiesDialog(GroupBase group, Form parentForm) {
			DefaultGroupPropertiesDialog defaultGroupPropertiesDialog = new DefaultGroupPropertiesDialog(group, parentForm);
			defaultGroupPropertiesDialog.CreateControls(group);
			defaultGroupPropertiesDialog.PopulateCredentialsProfiles(null);
			defaultGroupPropertiesDialog.PopulateCredentialsManagementTab(Program.CredentialsProfiles);
			return defaultGroupPropertiesDialog;
		}

		public override void CreateControls(RdcTreeNode settingsNode) {
			base.CreateControls(settingsNode);
			AddTabPage(settingsNode.EncryptionSettings.CreateTabPage(this));
			CreateProfileManagementTabPage();
		}
	}
}
