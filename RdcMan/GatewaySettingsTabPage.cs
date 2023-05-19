using System;
using System.Windows.Forms;

namespace RdcMan
{
	public class GatewaySettingsTabPage : CredentialsTabPage<GatewaySettings>
	{
		private CheckBox _useGatewayServerCheckBox;

		private ValueComboBox<RdpClient.GatewayLogonMethod> _gatewayLogonMethodCombo;

		private CheckBox _gatewayLocalBypassCheckBox;

		private RdcTextBox _gatewayHostNameTextBox;

		private CheckBox _gatewayCredSharingCheckBox;

		public GatewaySettingsTabPage(TabbedSettingsDialog dialog, GatewaySettings settings)
			: base(dialog, settings)
		{
		}

		public void CreateControls(LogonCredentialsDialogOptions options)
		{
			int tabIndex = 0;
			int rowIndex = 0;
			CreateInheritanceControl(ref rowIndex, ref tabIndex);
			if (base.InheritanceControl != null) {
				base.InheritanceControl.EnabledChanged += delegate (bool enabled) {
					_useGatewayServerCheckBox.Enabled = enabled;
					UseGatewayServerCheckBox_CheckedChanged(null, null);
				};
			}
			_useGatewayServerCheckBox = FormTools.AddCheckBox(this, "使用 TS 网关服务器(&U)", base.Settings.UseGatewayServer, 1, ref rowIndex, ref tabIndex);
			_useGatewayServerCheckBox.CheckedChanged += UseGatewayServerCheckBox_CheckedChanged;
			_gatewayHostNameTextBox = FormTools.AddLabeledTextBox(this, "服务器名称(&S)：", base.Settings.HostName, ref rowIndex, ref tabIndex);
			_gatewayHostNameTextBox.Validate = delegate {
				if (_gatewayHostNameTextBox.Enabled) {
					_gatewayHostNameTextBox.Text = _gatewayHostNameTextBox.Text.Trim();
					string text = _gatewayHostNameTextBox.Text;
					if (text.Length == 0)
						return "请输入服务器名称";
				}
				return null;
			};
			_gatewayLocalBypassCheckBox = FormTools.AddCheckBox(this, "绕过本地地址(&B)", base.Settings.BypassGatewayForLocalAddresses, 1, ref rowIndex, ref tabIndex);
			_gatewayLogonMethodCombo = FormTools.AddLabeledEnumDropDown(this, "登录方式(&L)：", base.Settings.LogonMethod, ref rowIndex, ref tabIndex, RdpClient.GatewayLogonMethodToString);
			_gatewayLogonMethodCombo.SelectedValueChanged += GatewayLogonMethodComboBox_SelectedValueChanged;
			if (RdpClient.SupportsGatewayCredentials) {
				_gatewayCredSharingCheckBox = FormTools.AddCheckBox(this, "与远程计算机共享网关凭证(&C)", base.Settings.CredentialSharing, 1, ref rowIndex, ref tabIndex);
				_gatewayCredSharingCheckBox.CheckedChanged += GatewayCredSharingCheckBox_CheckedChanged;
				_credentialsUI = new CredentialsUI(base.InheritanceControl);
				_credentialsUI.AddControlsToParent(this, options, ref rowIndex, ref tabIndex);
			}
		}

		protected override void UpdateControls()
		{
			base.UpdateControls();
			if (base.InheritanceControl == null)
			{
				UseGatewayServerCheckBox_CheckedChanged(null, null);
			}
		}

		private void GatewayCredSharingCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			GatewayLogonMethodComboBox_SelectedValueChanged(null, null);
		}

		private void UseGatewayServerCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			bool enabled = _useGatewayServerCheckBox.Enabled && _useGatewayServerCheckBox.Checked;
			_gatewayHostNameTextBox.Enabled = enabled;
			_gatewayLogonMethodCombo.Enabled = enabled;
			_gatewayLocalBypassCheckBox.Enabled = enabled;
			if (RdpClient.SupportsGatewayCredentials)
			{
				_gatewayCredSharingCheckBox.Enabled = enabled;
			}
			GatewayLogonMethodComboBox_SelectedValueChanged(null, null);
		}

		private void GatewayLogonMethodComboBox_SelectedValueChanged(object sender, EventArgs e)
		{
			bool enable = _gatewayLogonMethodCombo.Enabled && _gatewayLogonMethodCombo.SelectedValue == RdpClient.GatewayLogonMethod.NTLM;
			if (RdpClient.SupportsGatewayCredentials)
			{
				_credentialsUI.EnableDisableControls(enable);
			}
		}
	}
}
