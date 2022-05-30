using System;
using System.Windows.Forms;

namespace RdcMan {
	internal class AddCredentialsDialog : LogonSettingsDialog {
		private FileGroup _file;

		private RdcTextBox _profileNameTextBox;

		private bool _profileNameUserCreated;

		public new string ProfileName => _profileNameTextBox.Text;

		public new ProfileScope ProfileScope {
			get {
				if (_file != null) {
					return ProfileScope.File;
				}
				return ProfileScope.Global;
			}
		}

		public AddCredentialsDialog(RdcTreeNode node)
			: base("���ƾ֤", "����") {
			_file = node.FileGroup;
			int rowIndex = 0;
			int num = 0;
			_profileNameTextBox = FormTools.AddLabeledTextBox(this, "�����ļ�����(&N)��", ref rowIndex, ref num);
			_profileNameTextBox.Enabled = true;
			_profileNameTextBox.TextChanged += delegate {
				_profileNameUserCreated = true;
			};
			_profileNameTextBox.Validate = ValidateProfileName;
			_logonCredentialsUI.AddControlsToParent(this, LogonCredentialsDialogOptions.None, ref rowIndex, ref num);
			_logonCredentialsUI.UserNameTextBox.TextChanged += CredentialsChanged;
			_logonCredentialsUI.DomainTextBox.TextChanged += CredentialsChanged;
			_logonCredentialsUI.EnableDisableControls(enable: true);
			FinalizeLayout(rowIndex, num);
		}

		private string ValidateProfileName() {
			_profileNameTextBox.Text = _profileNameTextBox.Text.Trim();
			if (string.IsNullOrEmpty(_profileNameTextBox.Text)) {
				return "�����������ļ�����";
			}
			if (LogonCredentials.IsCustomProfile(ProfileName)) {
				return "��{0}�� �Ǳ����������ļ�����".InvariantFormat("Custom");
			}
			CredentialsStore credentialsProfiles = Program.CredentialsProfiles;
			string text = "ȫ��";//Global
			if (ProfileScope == ProfileScope.File) {
				credentialsProfiles = _file.CredentialsProfiles;
				text = _file.Text;
			}
			if (credentialsProfiles.Contains(ProfileName)) {
				DialogResult dialogResult = FormTools.YesNoDialog(ProfileName + " �Ѿ������� " + text + Environment.NewLine + "�Ƿ���£�", MessageBoxDefaultButton.Button2);
				if (dialogResult != DialogResult.Yes) {
					return "�����ļ�����";
				}
			}
			return null;
		}

		private void CredentialsChanged(object sender, EventArgs e) {
			if (!_profileNameUserCreated) {
				_profileNameTextBox.Text = CredentialsUI.GetQualifiedUserName(_logonCredentialsUI.UserNameTextBox.Text, _logonCredentialsUI.DomainTextBox.Text);
				_profileNameUserCreated = false;
			}
		}
	}
}
