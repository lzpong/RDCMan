namespace RdcMan
{
	public abstract class CredentialsTabPage<TSettingGroup> : SettingsTabPage<TSettingGroup>, ICredentialsTabPage where TSettingGroup : LogonCredentials
	{
		protected CredentialsUI _credentialsUI;

		public CredentialsProfile Credentials => _credentialsUI.ProfileComboBox.SelectedValue;

		protected CredentialsTabPage(TabbedSettingsDialog dialog, TSettingGroup settings)
			: base(dialog, settings)
		{
		}

		public void PopulateCredentialsProfiles(FileGroup file)
		{
			if (_credentialsUI != null)
			{
				_credentialsUI.PopulateCredentialsProfiles(file);
			}
		}

		protected override void UpdateControls()
		{
			base.UpdateControls();
			if (_credentialsUI != null)
			{
				_credentialsUI.InitFromCredentials(base.Settings);
			}
		}

		protected override void UpdateSettings()
		{
			base.UpdateSettings();
			if (_credentialsUI != null)
			{
				TSettingGroup settings = base.Settings;
				settings.ProfileName.UpdateValue(_credentialsUI.ProfileComboBox.SelectedValue.ProfileName, _credentialsUI.ProfileComboBox.SelectedValue.ProfileScope);
				TSettingGroup settings2 = base.Settings;
				settings2.UserName.Value = _credentialsUI.UserNameTextBox.Text;
				if (_credentialsUI.PasswordChanged)
				{
					TSettingGroup settings3 = base.Settings;
					settings3.Password.SetPlainText(_credentialsUI.PasswordTextBox.Text);
				}
				TSettingGroup settings4 = base.Settings;
				settings4.Domain.Value = _credentialsUI.DomainTextBox.Text;
			}
		}
	}
}
