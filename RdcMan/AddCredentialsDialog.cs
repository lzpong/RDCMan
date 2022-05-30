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
			: base("添加凭证", "保存") {
			_file = node.FileGroup;
			int rowIndex = 0;
			int num = 0;
			_profileNameTextBox = FormTools.AddLabeledTextBox(this, "配置文件名称(&N)：", ref rowIndex, ref num);
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
				return "请输入配置文件名称";
			}
			if (LogonCredentials.IsCustomProfile(ProfileName)) {
				return "“{0}” 是保留的配置文件名称".InvariantFormat("Custom");
			}
			CredentialsStore credentialsProfiles = Program.CredentialsProfiles;
			string text = "全局";//Global
			if (ProfileScope == ProfileScope.File) {
				credentialsProfiles = _file.CredentialsProfiles;
				text = _file.Text;
			}
			if (credentialsProfiles.Contains(ProfileName)) {
				DialogResult dialogResult = FormTools.YesNoDialog(ProfileName + " 已经存在于 " + text + Environment.NewLine + "是否更新？", MessageBoxDefaultButton.Button2);
				if (dialogResult != DialogResult.Yes) {
					return "配置文件存在";
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
