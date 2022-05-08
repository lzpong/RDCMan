using System;
using System.Drawing;
using System.Windows.Forms;

namespace RdcMan {
	public class CustomSizeDialog : RdcDialog {
		private NumericTextBox _widthTextBox;

		private NumericTextBox _heightTextBox;

		private Button _fourThreeButton;

		private Button _sixteenNineButton;

		private Button _sixteenTenButton;

		public string WidthText => _widthTextBox.Text;

		public string HeightText => _heightTextBox.Text;

		public CustomSizeDialog(Size size)
			: base("自定义大小", "确定") {
			InitComp();
			_widthTextBox.Text = size.Width.ToString();
			_heightTextBox.Text = size.Height.ToString();
		}

		private void InitComp() {
			int num = 0;
			Label label = new Label {
				Location = new Point(8, 8),
				Size = new Size(50, 23),
				Text = "宽度(&W)：",
				TextAlign = ContentAlignment.MiddleLeft
			};
			Label label2 = new Label {
				Location = new Point(8, 40),
				Size = new Size(50, 23),
				Text = "高度(&H)：",
				TextAlign = ContentAlignment.MiddleLeft
			};
			_widthTextBox = new NumericTextBox(1, int.MaxValue, "宽度至少1像素") {
				Location = new Point(72, 8),
				Size = new Size(75, 20),
				TabIndex = num++
			};
			_heightTextBox = new NumericTextBox(1, int.MaxValue, "高度至少1像素") {
				Location = new Point(72, 40),
				Size = new Size(75, 20),
				TabIndex = num++
			};
			_fourThreeButton = new Button {
				Location = new Point(160, 8),
				TabIndex = num++,
				Text = "&4 x 3"
			};
			_fourThreeButton.Click += fourThreeButton_Click;
			_sixteenNineButton = new Button {
				Location = new Point(160, 40),
				TabIndex = num++,
				Text = "1&6 x 9"
			};
			_sixteenNineButton.Click += sixteenNineButton_Click;
			_sixteenTenButton = new Button {
				Location = new Point(160, 72),
				TabIndex = num++,
				Text = "16 x 1&0"
			};
			_sixteenTenButton.Click += sixteenTenButton_Click;
			base.ClientSize = new Size(238, 143);
			base.Controls.Add(_sixteenTenButton, _sixteenNineButton, _fourThreeButton, label, _widthTextBox, label2, _heightTextBox);
			InitButtons();
			this.ScaleAndLayout();
		}

		private void fourThreeButton_Click(object sender, EventArgs e) {
			int num = int.Parse(_widthTextBox.Text);
			int num2 = num * 3 / 4;
			_heightTextBox.Text = num2.ToString();
		}

		private void sixteenNineButton_Click(object sender, EventArgs e) {
			int num = int.Parse(_widthTextBox.Text);
			int num2 = num * 9 / 16;
			_heightTextBox.Text = num2.ToString();
		}

		private void sixteenTenButton_Click(object sender, EventArgs e) {
			int num = int.Parse(_widthTextBox.Text);
			int num2 = num * 10 / 16;
			_heightTextBox.Text = num2.ToString();
		}
	}
}
