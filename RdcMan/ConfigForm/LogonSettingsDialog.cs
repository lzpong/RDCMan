using System;

namespace RdcMan
{
	public class LogonSettingsDialog : RdcDialog
	{
		protected CredentialsUI _logonCredentialsUI;

		public bool PasswordChanged => _logonCredentialsUI.PasswordChanged;

		public string ProfileName => _logonCredentialsUI.ProfileComboBox.SelectedValue.ProfileName;

		public ProfileScope ProfileScope => _logonCredentialsUI.ProfileComboBox.SelectedValue.ProfileScope;

		public string UserName => _logonCredentialsUI.UserNameTextBox.Text;

		public PasswordSetting Password
		{
			get
			{
				PasswordSetting passwordSetting = new PasswordSetting(_logonCredentialsUI.PasswordTextBox.Text);
				passwordSetting.IsDecrypted = true;
				return passwordSetting;
			}
		}

		public string Domain => _logonCredentialsUI.DomainTextBox.Text;

		protected LogonSettingsDialog(string title, string buttonText)
			: base(title, buttonText, null)
		{
			SuspendLayout();
			_logonCredentialsUI = new CredentialsUI(null);
		}

		protected override void ShownCallback(object sender, EventArgs e)
		{
			_logonCredentialsUI.UserNameTextBox.Focus();
		}

		public static LogonSettingsDialog NewEditCredentialsDialog(CredentialsProfile credentials)
		{
			LogonSettingsDialog logonSettingsDialog = new LogonSettingsDialog("Edit Credentials", "Save");
			int rowIndex = 0;
			int tabIndex = 0;
			logonSettingsDialog._logonCredentialsUI.AddControlsToParent(logonSettingsDialog, LogonCredentialsDialogOptions.None, ref rowIndex, ref tabIndex);
			logonSettingsDialog._logonCredentialsUI.EnableDisableControls(enable: true);
			logonSettingsDialog._logonCredentialsUI.InitFromCredentials(credentials);
			logonSettingsDialog.FinalizeLayout(rowIndex, tabIndex);
			return logonSettingsDialog;
		}

		protected void FinalizeLayout(int rowIndex, int tabIndex)
		{
			int num2 = base.Height = FormTools.YPos(rowIndex + 1) + 16;
			InitButtons();
			this.ScaleAndLayout();
		}
	}
}
