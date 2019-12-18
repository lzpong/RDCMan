using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RdcMan
{
	internal class ServerPropertiesTabPage : NodePropertiesPage<ServerSettings>
	{
		private RdcTextBox _serverNameTextBox;

		private RdcCheckBox _vmConsoleConnectCheckBox;

		private Label _vmIdLabel;

		private RdcTextBox _vmIdTextBox;

		private RdcTextBox _displayNameTextBox;

		private bool _displayNameUserCreated;

		public List<string> ExpandedServerNames
		{
			get;
			private set;
		}

		public ServerPropertiesTabPage(TabbedSettingsDialog dialog, ServerSettings settings)
			: base(dialog, settings, "Server Settings")
		{
			int tabIndex = 0;
			int rowIndex = 0;
			_serverNameTextBox = FormTools.AddLabeledTextBox(this, "&Server name:", base.Settings.ServerName, ref rowIndex, ref tabIndex);
			_serverNameTextBox.Enabled = true;
			_serverNameTextBox.TextChanged += ServerNameChanged;
			_serverNameTextBox.Validate = ValidateServerName;
			_vmConsoleConnectCheckBox = FormTools.NewCheckBox("&VM console connect", 0, rowIndex, tabIndex++, 140);
			_vmConsoleConnectCheckBox.CheckedChanged += VMConsoleConnectCheckBox_CheckedChanged;
			_vmIdLabel = new Label
			{
				Location = FormTools.NewLocation(1, rowIndex++),
				Size = new Size(30, 20),
				Text = "&id:",
				TextAlign = ContentAlignment.MiddleLeft,
				Visible = false
			};
			_vmIdTextBox = new RdcTextBox
			{
				Location = new Point(_vmIdLabel.Right, _vmIdLabel.Top),
				Setting = base.Settings.VirtualMachineId,
				Size = new Size(340 - _vmIdLabel.Width, 20),
				TabIndex = tabIndex++,
				Visible = false
			};
			_displayNameTextBox = FormTools.AddLabeledTextBox(this, "&Display name:", base.Settings.DisplayName, ref rowIndex, ref tabIndex);
			_displayNameTextBox.Enabled = true;
			_displayNameTextBox.TextChanged += DisplayNameChanged;
			_displayNameTextBox.Validate = ValidateDisplayName;
			_displayNameUserCreated = !base.Settings.ServerName.Value.Equals(base.Settings.DisplayName.Value);
			AddParentCombo(ref rowIndex, ref tabIndex);
			AddComment(ref rowIndex, ref tabIndex).Setting = base.Settings.Comment;
			base.Controls.Add(_vmConsoleConnectCheckBox, _vmIdLabel, _vmIdTextBox);
			base.FocusControl = _serverNameTextBox;
		}

		protected override bool CanBeParent(GroupBase group)
		{
			return group.CanAddServers();
		}

		protected override void UpdateControls()
		{
			base.UpdateControls();
			_vmConsoleConnectCheckBox.Checked = (base.Settings.ConnectionType.Value == ConnectionType.VirtualMachineConsoleConnect);
		}

		protected override void UpdateSettings()
		{
			base.UpdateSettings();
			base.Settings.ConnectionType.Value = (_vmConsoleConnectCheckBox.Checked ? ConnectionType.VirtualMachineConsoleConnect : ConnectionType.Normal);
		}

		private string ValidateServerName()
		{
			_serverNameTextBox.Text = _serverNameTextBox.Text.Trim();
			string text = _serverNameTextBox.Text;
			if (text.Length == 0)
			{
				return "Please enter a server name";
			}
			if (text.IndexOf(' ') != -1)
			{
				return "Spaces are not permitted in a server name";
			}
			if (text.IndexOf('/') != -1 || text.IndexOf('\\') != -1)
			{
				return "Slashes are not permitted in a server name";
			}
			try
			{
				List<string> list = new List<string>(StringUtilities.ExpandPattern(text));
				if (list.Count > 1 && list.Count > 20 && FormTools.YesNoDialog("Expansion results in " + list.Count + " servers. Are you sure?") == DialogResult.No)
				{
					return "Expansion too large";
				}
				ExpandedServerNames = list;
			}
			catch (ArgumentException ex)
			{
				return ex.Message;
			}
			return null;
		}

		private void ServerNameChanged(object sender, EventArgs e)
		{
			if (!_displayNameUserCreated)
			{
				Server.SplitName(_serverNameTextBox.Text, out string serverName, out int _);
				_displayNameTextBox.Text = serverName;
				_displayNameUserCreated = false;
			}
		}

		private string ValidateDisplayName()
		{
			_displayNameTextBox.Text = _displayNameTextBox.Text.Trim();
			string text = _displayNameTextBox.Text;
			if (text.Length == 0)
			{
				return "Please enter a display name";
			}
			return null;
		}

		private void DisplayNameChanged(object sender, EventArgs e)
		{
			_displayNameUserCreated = true;
		}

		private void VMConsoleConnectCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			bool @checked = _vmConsoleConnectCheckBox.Checked;
			_vmIdLabel.Visible = @checked;
			_vmIdTextBox.Visible = @checked;
			_vmIdTextBox.Enabled = @checked;
			EnableTabsEventArgs enableTabsEventArgs = new EnableTabsEventArgs();
			enableTabsEventArgs.Enabled = !@checked;
			enableTabsEventArgs.Reason = "for virtual machine console connect";
			enableTabsEventArgs.TabNames = new string[4]
			{
				"Local Resources",
				"Remote Desktop Settings",
				"Security Settings",
				"Connection Settings"
			};
			EnableTabsEventArgs args = enableTabsEventArgs;
			NodePropertiesDialog nodePropertiesDialog = FindForm() as NodePropertiesDialog;
			nodePropertiesDialog.EnableTabs(args);
		}
	}
}
