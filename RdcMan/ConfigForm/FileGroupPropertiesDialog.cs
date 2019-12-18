using System.Windows.Forms;

namespace RdcMan
{
	internal class FileGroupPropertiesDialog : GroupBasePropertiesDialog
	{
		protected FileGroupPropertiesDialog(FileGroup group, string dialogTitle, string acceptButtonText, Form parentForm)
			: base(group, dialogTitle, acceptButtonText, parentForm)
		{
		}

		public static FileGroupPropertiesDialog NewPropertiesDialog(FileGroup group, Form parentForm)
		{
			FileGroupPropertiesDialog fileGroupPropertiesDialog = new FileGroupPropertiesDialog(group, group.Text + " File Properties", "OK", parentForm);
			fileGroupPropertiesDialog.CreateControls(group);
			return fileGroupPropertiesDialog;
		}

		public override void CreateControls(RdcTreeNode settings)
		{
			FileGroupPropertiesTabPage page = (FileGroupPropertiesTabPage)(base.PropertiesPage = (settings.Properties.CreateTabPage(this) as FileGroupPropertiesTabPage));
			AddTabPage(page);
			base.PropertiesPage.ParentGroupChanged += base.PopulateCredentialsProfiles;
			base.CreateControls(settings);
			AddTabPage(settings.EncryptionSettings.CreateTabPage(this));
			CreateProfileManagementTabPage();
			PopulateCredentialsProfiles(base.AssociatedNode as FileGroup);
			PopulateCredentialsManagementTab((base.AssociatedNode as FileGroup).CredentialsProfiles);
		}
	}
}
