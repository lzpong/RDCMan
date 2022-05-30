using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RdcMan {
	public class RemoteDesktopTabPage : SettingsTabPage<RemoteDesktopSettings> {
		private readonly GroupBox _rdsSizeGroup;

		private readonly RadioButton _rdsCustomRadio;

		private readonly Button _rdsCustomButton;

		public RemoteDesktopTabPage(TabbedSettingsDialog dialog, RemoteDesktopSettings settings)
			: base(dialog, settings) {
			int num = 0;
			int rowIndex = 0;
			CreateInheritanceControl(ref rowIndex, ref num);
			ValueComboBox<int> previousGroupBox = FormTools.AddLabeledValueDropDown(this, "颜色深度：", settings.ColorDepth, ref rowIndex, ref num, (int v) => v.ToString(), new int[5] { 8, 15, 16, 24, 32 });
			RadioButton value = new RdcRadioButton {
				Setting = settings.DesktopSizeSameAsClientAreaSize,
				Size = new Size(140, 24),
				Text = "适应客户区(&S)"
			};
			RadioButton value2 = new RdcRadioButton {
				Setting = settings.DesktopSizeFullScreen,
				Size = new Size(140, 24),
				Text = "全屏(&F)"
			};
			_rdsCustomRadio = new RadioButton();
			_rdsCustomButton = new Button();
			_rdsCustomRadio.Size = new Size(96, 24);
			_rdsCustomRadio.Text = "自定义(&C)";
			_rdsSizeGroup = new GroupBox();
			_rdsSizeGroup.Controls.AddRange(FormTools.NewSizeRadios());
			_rdsSizeGroup.Controls.Add(value);
			_rdsSizeGroup.Controls.Add(value2);
			_rdsSizeGroup.Controls.Add(_rdsCustomRadio);
			_rdsSizeGroup.Text = "远程桌面大小";
			FormTools.LayoutGroupBox(_rdsSizeGroup, 2, previousGroupBox);
			_rdsCustomButton.Location = new Point(_rdsCustomRadio.Right + 10, _rdsCustomRadio.Location.Y);
			_rdsCustomButton.TabIndex = _rdsCustomRadio.TabIndex + 1;
			_rdsCustomButton.Click += CustomSizeClick;
			_rdsCustomButton.Text = Program.TheForm.GetClientSize().ToFormattedString();
			_rdsSizeGroup.Controls.Add(_rdsCustomButton);
			base.Controls.Add(_rdsSizeGroup);
		}

		protected override void UpdateControls() {
			base.UpdateControls();
			Size size = base.Settings.DesktopSize.Value;
			if (!base.Settings.DesktopSizeSameAsClientAreaSize.Value && !base.Settings.DesktopSizeFullScreen.Value) {
				RadioButton radioButton = _rdsSizeGroup.Controls.OfType<RadioButton>().Where(delegate (RadioButton r) {
					Size? size2 = (Size?)r.Tag;
					Size size3 = size;
					if (!size2.HasValue)
						return false;

					return !size2.HasValue || size2.GetValueOrDefault() == size3;
				}).FirstOrDefault();
				if (radioButton != null)
					radioButton.Checked = true;
				else
					_rdsCustomRadio.Checked = true;
			}
			_rdsCustomButton.Text = size.ToFormattedString();
		}

		protected override void UpdateSettings() {
			base.UpdateSettings();
			if (base.Settings.DesktopSizeSameAsClientAreaSize.Value || base.Settings.DesktopSizeFullScreen.Value)
				return;

			string dim = _rdsCustomButton.Text;
			if (!_rdsCustomRadio.Checked) {
				dim = (from r in _rdsSizeGroup.Controls.OfType<RadioButton>()
					   where r.Checked
					   select r).First().Text;
			}
			base.Settings.DesktopSize.Value = SizeHelper.Parse(dim);
		}

		private void CustomSizeClick(object sender, EventArgs e) {
			Button button = sender as Button;
			RadioButton radioButton = button.Parent.GetNextControl(button, forward: false) as RadioButton;
			radioButton.Checked = true;
			Size size = SizeHelper.Parse(button.Text);
			using CustomSizeDialog customSizeDialog = new CustomSizeDialog(size);
			if (customSizeDialog.ShowDialog() == DialogResult.OK)
				button.Text = customSizeDialog.WidthText + SizeHelper.Separator + customSizeDialog.HeightText;
		}
	}
}
