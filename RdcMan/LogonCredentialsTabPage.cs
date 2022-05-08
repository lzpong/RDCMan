namespace RdcMan {
	public class LogonCredentialsTabPage : CredentialsTabPage<LogonCredentials> {
		public LogonCredentialsTabPage(TabbedSettingsDialog dialog, LogonCredentials settings)
			: base(dialog, settings) { }

		public void CreateControls(LogonCredentialsDialogOptions options) {
			int num = 0;
			int rowIndex = 0;
			if ((options & LogonCredentialsDialogOptions.AllowInheritance) != 0) {
				CreateInheritanceControl(ref rowIndex, ref num);
				base.InheritanceControl.EnabledChanged += delegate (bool enabled) {
					_credentialsUI.EnableDisableControls(enabled);
				};
			}
			_credentialsUI = new CredentialsUI(base.InheritanceControl);
			_credentialsUI.AddControlsToParent(this, options, ref rowIndex, ref num);
		}
	}
}
