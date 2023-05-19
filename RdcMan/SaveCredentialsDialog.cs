using System;
using System.Drawing;
using System.Windows.Forms;

namespace RdcMan
{
	internal class SaveCredentialsDialog : RdcDialog
	{
		private RdcTextBox _profileNameTextBox;

		private ValueComboBox<ProfileScope> _locationComboBox;

		private FileGroup _file;

		public string ProfileName => _profileNameTextBox.Text;

		public ProfileScope ProfileScope => _locationComboBox.SelectedValue;

		public SaveCredentialsDialog(FileGroup file, string name)
			: base("���� " + name + " ������", "����")
		{
			_file = file;
			InitializeComponent(name);
		}

		private void InitializeComponent(string name)
		{
			base.Size = new System.Drawing.Size(512, 150);
			int rowIndex = 0;
			int num = 0;
			this._profileNameTextBox = RdcMan.FormTools.AddLabeledTextBox(this, "�����ļ���(&N)", ref rowIndex, ref num);
			this._profileNameTextBox.Enabled = true;
			this._profileNameTextBox.Text = name;
			this._profileNameTextBox.Validate = new System.Func<string>(ValidateProfileName);
			this._locationComboBox = RdcMan.FormTools.AddLabeledValueDropDown<RdcMan.ProfileScope>(this, "λ��(&L)��", ref rowIndex, ref num, null, null);
			this._locationComboBox.AddItem("Global", RdcMan.ProfileScope.Global);
			this._locationComboBox.SelectedIndex = 0;
			if (this._file != null)
			{
				this._locationComboBox.AddItem(this._file.Text, RdcMan.ProfileScope.File);
				this._locationComboBox.SelectedIndex = 1;
			}
			this.InitButtons();
			this.ScaleAndLayout();
		}

		private string ValidateProfileName()
		{
			_profileNameTextBox.Text = _profileNameTextBox.Text.Trim();
			if (string.IsNullOrEmpty(_profileNameTextBox.Text))
			{
				return "�����������ļ�����";
			}
			if (LogonCredentials.IsCustomProfile(ProfileName))
			{
				return "��{0}�� �Ǳ����������ļ�����".InvariantFormat("Custom");
			}
			CredentialsStore credentialsProfiles = Program.CredentialsProfiles;
			string text = "Global";
			if (ProfileScope == ProfileScope.File)
			{
				credentialsProfiles = _file.CredentialsProfiles;
				text = _file.Text;
			}
			if (credentialsProfiles.Contains(ProfileName))
			{
				DialogResult dialogResult = FormTools.YesNoDialog(ProfileName + " �Ѿ������� " + text + Environment.NewLine + " ���Ƿ���£�", MessageBoxDefaultButton.Button2);
				if (dialogResult != DialogResult.Yes)
				{
					return "�����ļ�����";
				}
			}
			return null;
		}
	}
}
