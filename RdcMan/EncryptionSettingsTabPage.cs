using System;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace RdcMan {
	public class EncryptionSettingsTabPage : SettingsTabPage<EncryptionSettings> {
		public InheritanceControl InheritEncryptionSettings;

		protected ValueComboBox<EncryptionMethod> _passwordEncryptionMethodCombo;

		protected Label _passwordEncryptionDataLabel;

		protected Button _passwordEncryptionDataButton;

		protected Label _passwordEncryptionDataInfoLabel;

		private EncryptionMethod _passwordEncryptionMethodPrevious;

		public EncryptionSettingsTabPage(TabbedSettingsDialog dialog, EncryptionSettings settings)
			: base(dialog, settings) {
			int num = 0;
			int rowIndex = 0;
			CreateInheritanceControl(ref rowIndex, ref num);
			if (base.InheritanceControl != null) {
				base.InheritanceControl.EnabledChanged += delegate {
					PasswordEncryptionMethodCombo_Changed(null, null);
				};
			}
			_passwordEncryptionMethodCombo = FormTools.AddLabeledEnumDropDown(this, "密码加密：", base.Settings.EncryptionMethod, ref rowIndex, ref num, Encryption.EncryptionMethodToString);
			_passwordEncryptionMethodCombo.Enter += PasswordEncryptionMethodCombo_Enter;
			_passwordEncryptionMethodCombo.SelectedIndexChanged += PasswordEncryptionMethodCombo_Changed;
			_passwordEncryptionDataLabel = FormTools.NewLabel(string.Empty, 0, rowIndex);
			_passwordEncryptionDataButton = new Button {
				Enabled = false,
				Location = FormTools.NewLocation(1, rowIndex++),
				Width = 340,
				TabIndex = num++,
				TextAlign = ContentAlignment.MiddleLeft
			};
			_passwordEncryptionDataButton.Click += PasswordEncryptionMethodButton_Click;
			_passwordEncryptionDataInfoLabel = FormTools.NewLabel(string.Empty, 1, rowIndex++);
			_passwordEncryptionDataInfoLabel.Width = FormTools.TextBoxWidth;// 340;
			base.Controls.Add(_passwordEncryptionDataLabel, _passwordEncryptionDataButton, _passwordEncryptionDataInfoLabel);
		}

		protected override void UpdateControls() {
			if (base.Settings.EncryptionMethod.Value == EncryptionMethod.Certificate) {
				X509Certificate2 certificate = Encryption.GetCertificate(base.Settings.CredentialData.Value);
				_passwordEncryptionDataButton.Tag = certificate;
			}
			_passwordEncryptionMethodCombo.SelectedValue = base.Settings.EncryptionMethod.Value;
			base.UpdateControls();
			PasswordEncryptionMethodCombo_Changed(null, null);
		}

		protected override void UpdateSettings() {
			base.UpdateSettings();
			X509Certificate2 x509Certificate = (X509Certificate2)_passwordEncryptionDataButton.Tag;
			base.Settings.CredentialData.Value = ((x509Certificate != null) ? x509Certificate.Thumbprint : string.Empty);
			base.Settings.CredentialName.Value = _passwordEncryptionDataButton.Text;
		}

		private void PasswordEncryptionMethodCombo_Enter(object sender, EventArgs e) {
			_passwordEncryptionMethodPrevious = _passwordEncryptionMethodCombo.SelectedValue;
		}

		private void PasswordEncryptionMethodCombo_Changed(object sender, EventArgs e) {
			switch (_passwordEncryptionMethodCombo.SelectedValue) {
				case EncryptionMethod.LogonCredentials:
					_passwordEncryptionDataLabel.Text = "用户名：";
					_passwordEncryptionDataButton.Text = CredentialsUI.GetLoggedInUser();
					_passwordEncryptionDataButton.Tag = null;
					_passwordEncryptionDataButton.Enabled = false;
					_passwordEncryptionDataInfoLabel.Text = string.Empty;
					break;
				case EncryptionMethod.Certificate: {
					X509Certificate2 x509Certificate = _passwordEncryptionDataButton.Tag as X509Certificate2;
					if (x509Certificate == null) {
						try {
							base.Enabled = false;
							x509Certificate = Encryption.SelectCertificate();
						}
						finally {
							base.Enabled = true;
						}
					}
					if (x509Certificate != null)
						SetSelectedCertificate(x509Certificate);
					else
						_passwordEncryptionMethodCombo.SelectedValue = _passwordEncryptionMethodPrevious;
					break;
				}
				default:
					throw new NotImplementedException("意外的加密方法“{0}”".InvariantFormat(_passwordEncryptionMethodCombo.SelectedValue.ToString()));
			}
			_passwordEncryptionMethodPrevious = _passwordEncryptionMethodCombo.SelectedValue;
		}

		protected void SetSelectedCertificate(X509Certificate2 cert) {
			if (cert != null) {
				_passwordEncryptionDataButton.Text = cert.SimpleName();
				_passwordEncryptionDataButton.Tag = cert;
				_passwordEncryptionDataButton.Enabled = _passwordEncryptionMethodCombo.Enabled;
				_passwordEncryptionDataLabel.Text = "证书：";
				_passwordEncryptionDataInfoLabel.Text = "从 {0} 到 {1} 有效".InvariantFormat(cert.NotBefore.ToUniversalTime().ToShortDateString(), cert.NotAfter.ToUniversalTime().ToShortDateString());
			}
		}

		private void PasswordEncryptionMethodButton_Click(object sender, EventArgs e) {
			X509Certificate2 selectedCertificate;
			try {
				base.Enabled = false;
				selectedCertificate = Encryption.SelectCertificate();
			}
			finally {
				base.Enabled = true;
			}
			SetSelectedCertificate(selectedCertificate);
		}
	}
}
