using System;
using System.Drawing;
using System.Windows.Forms;

namespace RdcMan {
	public class CredentialsUI {
		public const char DomainSeparator = '\\';

		//private const string DummyPassword = "Placeholder1234567890";

		private FileGroup _file;

		private bool _usingCustomCredentials;

		private Button _saveProfileButton;

		private InheritanceControl _inheritSettings;

		private int _globalStoreChangeId;

		private int _fileStoreChangeId;

		public ValueComboBox<CredentialsProfile> ProfileComboBox { get; private set; }

		public RdcTextBox UserNameTextBox { get; private set; }

		public RdcTextBox DomainTextBox { get; private set; }

		public RdcTextBox PasswordTextBox { get; private set; }

		public bool PasswordChanged { get; private set; }

		private static bool HasDomainUser(string userName) {
			return userName.IndexOf('\\') != -1;
		}

		public static string GetUserName(string userName) {
			int num = userName.IndexOf('\\');
			if (num == -1) {
				return userName;
			}
			return userName.Substring(num + 1);
		}

		public static string GetQualifiedUserName(string userName, string domain) {
			string text = userName;
			if (!HasDomainUser(text) && !string.IsNullOrEmpty(domain)) {
				text = domain + "\\" + text;
			}
			return text;
		}

		public static string GetLoggedInUser() {
			return GetQualifiedUserName(Environment.UserName, Environment.UserDomainName);
		}

		public CredentialsUI(InheritanceControl inheritSettings) {
			_inheritSettings = inheritSettings;
			PasswordChanged = true;
		}

		public void AddControlsToParent(Control parent, LogonCredentialsDialogOptions options, ref int rowIndex, ref int tabIndex) {
			if ((options & LogonCredentialsDialogOptions.ShowProfiles) != 0) {
				ProfileComboBox = FormTools.AddLabeledValueDropDown<CredentialsProfile>(parent, "配置文件", ref rowIndex, ref tabIndex, null, null);
				ProfileComboBox.SelectedIndexChanged += OnProfileChanged;
				ProfileComboBox.VisibleChanged += OnProfileVisible;
				_saveProfileButton = new Button {
					TabIndex = tabIndex++,
					Text = "保存(&S)"
				};
				_saveProfileButton.Location = new Point(ProfileComboBox.Right - _saveProfileButton.Width, ProfileComboBox.Location.Y - 1);
				_saveProfileButton.Click += SaveProfileButton_Click;
				parent.Controls.Add(_saveProfileButton);
				ProfileComboBox.Width -= _saveProfileButton.Width;
			}
			UserNameTextBox = FormTools.AddLabeledTextBox(parent, "用户名(&U)：", ref rowIndex, ref tabIndex);
			UserNameTextBox.TextChanged += OnUserNameChanged;
			PasswordTextBox = FormTools.AddLabeledTextBox(parent, "密码(&P)：", ref rowIndex, ref tabIndex);
			PasswordTextBox.PasswordChar = '●';
			PasswordTextBox.TextChanged += OnPasswordChanged;
			DomainTextBox = FormTools.AddLabeledTextBox(parent, "域名(&D)：", ref rowIndex, ref tabIndex);
		}

		public void PopulateCredentialsProfiles(FileGroup file) {
			if (file == null || _file != file) {
				_file = file;
				PopulateCredentialsProfilesWorker();
			}
		}

		public void InitFromCredentials(ILogonCredentials credentials) {
			if (ProfileComboBox != null) {
				_usingCustomCredentials = LogonCredentials.IsCustomProfile(credentials.ProfileName);
				ProfileComboBox.SelectedIndex = ProfileComboBox.FindItem(LogonCredentials.ConstructQualifiedName(credentials));
			}
			UserNameTextBox.Text = credentials.UserName;
			InitPassword(credentials.Password);
			DomainTextBox.Text = credentials.Domain;
		}

		public void InitPassword(PasswordSetting password) {
			if (password != null && password.IsDecrypted && !string.IsNullOrEmpty(password.Value)) {
				PasswordTextBox.Text = password.Value;// DummyPassword; 直接放置原密码
				PasswordChanged = false;
			}
			else
				PasswordTextBox.Text = string.Empty;
		}

		public void EnableDisableControls(bool enable) {
			enable &= _inheritSettings == null || !_inheritSettings.FromParentCheck.Checked;
			if (ProfileComboBox != null) {
				ProfileComboBox.Enabled = enable;
				enable &= _usingCustomCredentials;
			}
			if (_saveProfileButton != null)
				_saveProfileButton.Enabled = enable;

			UserNameTextBox.Enabled = enable;
			DomainTextBox.Enabled = enable;
			PasswordTextBox.Enabled = enable;
			OnUserNameChanged(null, null);
		}

		private void OnUserNameChanged(object sender, EventArgs e) {
			if (_inheritSettings == null || !_inheritSettings.FromParentCheck.Checked) {
				int num = UserNameTextBox.Text.IndexOf('\\');
				if (num == -1) {
					DomainTextBox.Enabled = UserNameTextBox.Enabled;
					return;
				}
				DomainTextBox.Enabled = false;
				DomainTextBox.Text = UserNameTextBox.Text.Substring(0, num);
			}
		}

		private void OnPasswordChanged(object sender, EventArgs e) {
			PasswordChanged = true;
		}

		private void OnProfileChanged(object sender, EventArgs e) {
			if (_inheritSettings == null || !_inheritSettings.FromParentCheck.Checked) {
				ILogonCredentials selectedValue = ProfileComboBox.SelectedValue;
				_usingCustomCredentials = LogonCredentials.IsCustomProfile(selectedValue.ProfileName);
				EnableDisableControls(enable: true);
				if (_usingCustomCredentials) {
					UserNameTextBox.Text = Environment.UserName;
					InitPassword(null);
					DomainTextBox.Text = Environment.UserDomainName;
				}
				else {
					UserNameTextBox.Text = selectedValue.UserName;
					InitPassword(selectedValue.Password);
					DomainTextBox.Text = selectedValue.Domain;
				}
			}
		}

		private void OnProfileVisible(object sender, EventArgs e) {
			PopulateCredentialsProfilesIfChanged();
		}

		private void PopulateCredentialsProfilesIfChanged() {
			if (_globalStoreChangeId != Program.CredentialsProfiles.ChangeId || (_file != null && _fileStoreChangeId != _file.CredentialsProfiles.ChangeId))
				PopulateCredentialsProfilesWorker();
		}

		private void PopulateCredentialsProfilesWorker() {
			CredentialsProfile selectedValue = ProfileComboBox.SelectedValue;
			ProfileComboBox.ClearItems();
			ProfileComboBox.AddItem("Custom", new CredentialsProfile("Custom", ProfileScope.Local, string.Empty, string.Empty, string.Empty));
			ProfileComboBox.SelectedIndex = 0;
			PopulateComboFromStore(Program.CredentialsProfiles);
			_globalStoreChangeId = Program.CredentialsProfiles.ChangeId;
			if (_file != null) {
				PopulateComboFromStore(_file.CredentialsProfiles);
				_fileStoreChangeId = _file.CredentialsProfiles.ChangeId;
			}
			ProfileComboBox.SelectedValue = selectedValue;
		}

		private void PopulateComboFromStore(CredentialsStore store) {
			foreach (CredentialsProfile profile in store.Profiles) {
				string qualifiedName = profile.QualifiedName;
				ProfileComboBox.AddItem(qualifiedName, profile);
			}
		}

		private void SaveProfileButton_Click(object sender, EventArgs e) {
			string qualifiedUserName = GetQualifiedUserName(UserNameTextBox.Text, DomainTextBox.Text);
			using (SaveCredentialsDialog saveCredentialsDialog = new SaveCredentialsDialog(_file, qualifiedUserName)) {
				if (saveCredentialsDialog.ShowDialog() == DialogResult.OK) {
					ProfileScope profileScope = saveCredentialsDialog.ProfileScope;
					CredentialsStore credentialsProfiles = Program.CredentialsProfiles;
					if (profileScope == ProfileScope.File)
						credentialsProfiles = _file.CredentialsProfiles;

					qualifiedUserName = saveCredentialsDialog.ProfileName;
					bool flag = !credentialsProfiles.Contains(qualifiedUserName);
					CredentialsProfile credentialsProfile2 = (credentialsProfiles[qualifiedUserName] = new CredentialsProfile(qualifiedUserName, profileScope, UserNameTextBox.Text, PasswordTextBox.Text, DomainTextBox.Text));
					string qualifiedName = credentialsProfile2.QualifiedName;
					if (flag)
						ProfileComboBox.AddItem(qualifiedName, credentialsProfile2);
					else
						ProfileComboBox.ReplaceItem(qualifiedName, credentialsProfile2);

					ProfileComboBox.SelectedValue = credentialsProfile2;
				}
			}
			ProfileComboBox.Focus();
		}
	}
}
