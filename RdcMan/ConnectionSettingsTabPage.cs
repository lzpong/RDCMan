using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace RdcMan
{
	public class ConnectionSettingsTabPage : SettingsTabPage<ConnectionSettings>
	{
		private Label _portDefaultLabel;

		private NumericUpDown _portUpDown;

		public ConnectionSettingsTabPage(TabbedSettingsDialog dialog, ConnectionSettings settings)
			: base(dialog, settings)
		{
			int tabIndex = 0;
			int rowIndex = 0;
			CreateInheritanceControl(ref rowIndex, ref tabIndex);
			FormTools.AddCheckBox(this, "���ӵ�����̨(&C)", settings.ConnectToConsole, 1, ref rowIndex, ref tabIndex);
			FormTools.AddLabeledTextBox(this, "��������(&S)��", settings.StartProgram, ref rowIndex, ref tabIndex);
			FormTools.AddLabeledTextBox(this, "����Ŀ¼(&W)��", settings.WorkingDir, ref rowIndex, ref tabIndex);
			Label label = FormTools.NewLabel("�˿�(&P)��", 0, rowIndex);
			_portUpDown = new NumericUpDown {
				Location = FormTools.NewLocation(1, rowIndex++),
				Size = new Size(160, FormTools.ControlHeight),
				Minimum = 1,
				Maximum = 65535,
				TabIndex = tabIndex++
			};
			_portUpDown.KeyUp += delegate
			{
				UpdatePortDefaultLabel();
			};
			_portUpDown.ValueChanged += delegate
			{
				UpdatePortDefaultLabel();
			};
			((ISupportInitialize)_portUpDown).EndInit();
			_portDefaultLabel = new Label {
				Location = new Point(_portUpDown.Location.X + _portUpDown.Width, _portUpDown.Location.Y - 1),
				Size = new Size(FormTools.LabelWidth, FormTools.ControlHeight),
				TextAlign = ContentAlignment.MiddleLeft
			};
			FormTools.AddLabeledTextBox(this, "���ؾ�������(&L)��", settings.LoadBalanceInfo, ref rowIndex, ref tabIndex);
			base.Controls.Add(label, _portUpDown, _portDefaultLabel);
		}

		protected override void UpdateControls()
		{
			base.UpdateControls();
			_portUpDown.Text = base.Settings.Port.Value.ToString();
		}

		protected override void UpdateSettings()
		{
			base.UpdateSettings();
			base.Settings.Port.Value = (int)_portUpDown.Value;
		}

		private void UpdatePortDefaultLabel() {
			_portDefaultLabel.Text = ((_portUpDown.Value == 3389) ? "(Ĭ��)" : string.Empty);
		}
	}
}
