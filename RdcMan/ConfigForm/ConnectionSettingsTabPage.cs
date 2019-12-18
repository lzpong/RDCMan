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
			FormTools.AddCheckBox(this, "连接到控制台", settings.ConnectToConsole, 1, ref rowIndex, ref tabIndex);
			FormTools.AddLabeledTextBox(this, "启动程序:", settings.StartProgram, ref rowIndex, ref tabIndex);
			FormTools.AddLabeledTextBox(this, "工作目录:", settings.WorkingDir, ref rowIndex, ref tabIndex);
			Label label = FormTools.NewLabel("端口:", 0, rowIndex);
			_portUpDown = new NumericUpDown();
			((ISupportInitialize)_portUpDown).BeginInit();
			_portUpDown.Location = FormTools.NewLocation(1, rowIndex++);
			_portUpDown.Size = new Size(160, 20);
			_portUpDown.Minimum = 1m;
			_portUpDown.Maximum = 65535m;
			NumericUpDown portUpDown = _portUpDown;
			KeyEventHandler value = delegate
			{
				UpdatePortDefaultLabel();
			};
			portUpDown.KeyUp += value;
			_portUpDown.ValueChanged += delegate
			{
				UpdatePortDefaultLabel();
			};
			_portUpDown.TabIndex = tabIndex++;
			((ISupportInitialize)_portUpDown).EndInit();
			_portDefaultLabel = new Label();
			_portDefaultLabel.Location = new Point(_portUpDown.Location.X + _portUpDown.Width, _portUpDown.Location.Y - 1);
			_portDefaultLabel.Size = new Size(140, 20);
			_portDefaultLabel.TextAlign = ContentAlignment.MiddleLeft;
			FormTools.AddLabeledTextBox(this, "加载负载平衡配置:", settings.LoadBalanceInfo, ref rowIndex, ref tabIndex);
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

		private void UpdatePortDefaultLabel()
		{
			_portDefaultLabel.Text = ((_portUpDown.Value == 3389) ? "(默认)" : string.Empty);
		}
	}
}
