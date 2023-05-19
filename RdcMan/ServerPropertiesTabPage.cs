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

		public List<string> ExpandedServerNames { get; private set; }

		public ServerPropertiesTabPage(TabbedSettingsDialog dialog, ServerSettings settings)
			: base(dialog, settings, "����������")
		{
			int tabIndex = 0;
			int rowIndex = 0;
			_displayNameTextBox = FormTools.AddLabeledTextBox(this, "��ʾ����(&D)��", base.Settings.DisplayName, ref rowIndex, ref tabIndex);
			_displayNameTextBox.Enabled = true;
			_displayNameTextBox.TextChanged += DisplayNameChanged;
			_displayNameTextBox.Validate = ValidateDisplayName;
			_serverNameTextBox = FormTools.AddLabeledTextBox(this, "��������ַ(&S)��", base.Settings.ServerName, ref rowIndex, ref tabIndex);
			_serverNameTextBox.Enabled = true;
			_serverNameTextBox.TextChanged += ServerNameChanged;
			_serverNameTextBox.Validate = ValidateServerName;
			//���������� (Windows/Linux)
			FormTools.AddLabeledValueDropDown(this, "����������(&T)��", base.Settings.ServerType, ref rowIndex, ref tabIndex, (string v)=>v.ToString(), new string[2] {
				"Windows",
				"Linux"
			});

			_vmConsoleConnectCheckBox = FormTools.NewCheckBox("&VM console connect", 0, rowIndex, tabIndex++, 140);
			_vmConsoleConnectCheckBox.CheckedChanged += VMConsoleConnectCheckBox_CheckedChanged;
			_vmIdLabel = new Label {
				Location = FormTools.NewLocation(1, rowIndex++),
				Size = new Size(30, FormTools.ControlHeight),
				Text = "&ID��",
				TextAlign = ContentAlignment.MiddleLeft,
				Visible = false
			};
			_vmIdTextBox = new RdcTextBox {
				Location = new Point(_vmIdLabel.Right, _vmIdLabel.Top),
				Setting = base.Settings.VirtualMachineId,
				Size = new Size(FormTools.TextBoxWidth - _vmIdLabel.Width, FormTools.ControlHeight),
				TabIndex = tabIndex++,
				Visible = false
			};
			_displayNameUserCreated = !base.Settings.ServerName.Value.Equals(base.Settings.DisplayName.Value);
			AddParentCombo(ref rowIndex, ref tabIndex);
			AddComment(ref rowIndex, ref tabIndex).Setting = base.Settings.Comment;
			_vmIdTextBox.Enabled = false;
			base.Controls.Add(_vmConsoleConnectCheckBox,  _vmIdLabel, _vmIdTextBox);
			base.FocusControl = _displayNameTextBox;
		}

		protected override bool CanBeParent(GroupBase group)
		{
			return group.CanAddServers();
		}

		protected override void UpdateControls()
		{
			base.UpdateControls();
			_vmIdTextBox.Enabled = false;
			_vmConsoleConnectCheckBox.Checked = base.Settings.ConnectionType.Value == ConnectionType.VirtualMachineConsoleConnect;
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
				return "���������������";
			if (text.IndexOf(' ') != -1)
				return "�����������в������пո�";
			if (text.IndexOf('/') != -1 || text.IndexOf('\\') != -1)
				return "�����������в�����ʹ��б��";
			try
			{
				List<string> list = new List<string>(StringUtilities.ExpandPattern(text));
				if (list.Count > 1 && list.Count > 20 && FormTools.YesNoDialog("չ�����Ϊ " + list.Count + "����������ȷ����") == DialogResult.No)
					return "չ������";

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
				Server.SplitName(_serverNameTextBox.Text, out var serverName, out var _);
				_displayNameTextBox.Text = serverName;
				_displayNameUserCreated = false;
			}
		}

		private string ValidateDisplayName()
		{
			_displayNameTextBox.Text = _displayNameTextBox.Text.Trim();
			string text = _displayNameTextBox.Text;
			if (text.Length == 0)
				return "��������ʾ����";

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
			EnableTabsEventArgs enableTabsEventArgs = new EnableTabsEventArgs {
				Enabled = !@checked,
				Reason = "�������������̨����",
				TabNames = new string[4] { "������Դ", "Զ����������", "��ȫ����", "��������" }
			};
			NodePropertiesDialog nodePropertiesDialog = FindForm() as NodePropertiesDialog;
			nodePropertiesDialog.EnableTabs(enableTabsEventArgs);
		}
	}
}
