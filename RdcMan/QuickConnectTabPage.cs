namespace RdcMan {
	public class QuickConnectTabPage : LogonCredentialsTabPage {
		public RdcTextBox ServerNameTextBox { get; private set; }

		public QuickConnectTabPage(TabbedSettingsDialog dialog, LogonCredentials settings)
			: base(dialog, settings) { }

		public void CreateControls(bool serverName, FileGroup fileGroup) {
			int num = 0;
			int rowIndex = 0;
			if (serverName) {
				ServerNameTextBox = FormTools.AddLabeledTextBox(this, "(&S)·þÎñÆ÷Ãû³Æ£º", ref rowIndex, ref num);
				ServerNameTextBox.Enabled = true;
			}
			_credentialsUI = new CredentialsUI(base.InheritanceControl);
			_credentialsUI.AddControlsToParent(this, LogonCredentialsDialogOptions.ShowProfiles, ref rowIndex, ref num);
			_credentialsUI.PopulateCredentialsProfiles(fileGroup);
		}

		public void OnShown() {
			if (ServerNameTextBox != null)
				ServerNameTextBox.Focus();
			else
				_credentialsUI.ProfileComboBox.Focus();
		}
	}
}
