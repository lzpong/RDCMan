using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace RdcMan {
	public class ConnectionSettingsTabPage : SettingsTabPage<ConnectionSettings> {
		private Label _portDefaultLabel;

		private NumericUpDown _portUpDown;

		public ConnectionSettingsTabPage(TabbedSettingsDialog dialog, ConnectionSettings settings)
			: base(dialog, settings) {
			int num = 0;
			int rowIndex = 0;
			CreateInheritanceControl(ref rowIndex, ref num);
			FormTools.AddCheckBox(this, "连接到控制台(&C)", settings.ConnectToConsole, 1, ref rowIndex, ref num);
			FormTools.AddLabeledTextBox(this, "启动程序(&S)：", settings.StartProgram, ref rowIndex, ref num);
			FormTools.AddLabeledTextBox(this, "工作目录(&W)：", settings.WorkingDir, ref rowIndex, ref num);
			Label label = FormTools.NewLabel("端口(&P)：", 0, rowIndex);
			_portUpDown = new NumericUpDown {
				Location = FormTools.NewLocation(1, rowIndex++),
				Size = new Size(160, FormTools.ControlHeight),
				Minimum = 1m,
				Maximum = 65535m,
				TabIndex = num++
			};
			_portUpDown.KeyUp += delegate {
				UpdatePortDefaultLabel();
			};
			_portUpDown.ValueChanged += delegate {
				UpdatePortDefaultLabel();
			};
			((ISupportInitialize)_portUpDown).EndInit();
			_portDefaultLabel = new Label {
				Location = new Point(_portUpDown.Location.X + _portUpDown.Width, _portUpDown.Location.Y - 1),
				Size = new Size(FormTools.LabelWidth, FormTools.ControlHeight),
				TextAlign = ContentAlignment.MiddleLeft
			};
			FormTools.AddLabeledTextBox(this, "负载均衡配置(&L)：", settings.LoadBalanceInfo, ref rowIndex, ref num);
			base.Controls.Add(label, _portUpDown, _portDefaultLabel);
		}

		protected override void UpdateControls() {
			base.UpdateControls();
			_portUpDown.Text = base.Settings.Port.Value.ToString();
		}

		protected override void UpdateSettings() {
			base.UpdateSettings();
			base.Settings.Port.Value = (int)_portUpDown.Value;
		}

		private void UpdatePortDefaultLabel() {
			_portDefaultLabel.Text = ((_portUpDown.Value == 3389m) ? "(default)" : string.Empty);
		}
	}
}
