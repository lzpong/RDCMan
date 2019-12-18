using System;
using System.Drawing;
using System.Windows.Forms;

namespace RdcMan
{
	public class CustomSizeDialog : RdcDialog
	{
		private NumericTextBox _widthTextBox;

		private NumericTextBox _heightTextBox;

		private Button _fourThreeButton;

		private Button _sixteenNineButton;

		private Button _sixteenTenButton;

		public string WidthText => _widthTextBox.Text;

		public string HeightText => _heightTextBox.Text;

		public CustomSizeDialog(Size size)
			: base("自定义大小", "OK")
		{
			InitComp();
			_widthTextBox.Text = size.Width.ToString();
			_heightTextBox.Text = size.Height.ToString();
		}

		private void InitComp()
		{
			Label label = new Label();
			Label label2 = new Label();
			_fourThreeButton = new Button();
			_sixteenNineButton = new Button();
			_sixteenTenButton = new Button();
			int num = 0;
			label.Location = new Point(8, 8);
			label.Size = new Size(50, 23);
			label.Text = "宽度:";
			label.TextAlign = ContentAlignment.MiddleLeft;
			label2.Location = new Point(8, 40);
			label2.Size = new Size(50, 23);
			label2.Text = "高度:";
			label2.TextAlign = ContentAlignment.MiddleLeft;
			_widthTextBox = new NumericTextBox(1, int.MaxValue, "宽度必须至少为1像素");
			_widthTextBox.Location = new Point(72, 8);
			_widthTextBox.Size = new Size(75, 20);
			_widthTextBox.TabIndex = num++;
			_heightTextBox = new NumericTextBox(1, int.MaxValue, "高度必须至少为1像素");
			_heightTextBox.Location = new Point(72, 40);
			_heightTextBox.Size = new Size(75, 20);
			_heightTextBox.TabIndex = num++;
			_fourThreeButton.Location = new Point(160, 8);
			_fourThreeButton.TabIndex = num++;
			_fourThreeButton.Text = "4 x 3";
			_fourThreeButton.Click += fourThreeButton_Click;
			_sixteenNineButton.Location = new Point(160, 40);
			_sixteenNineButton.TabIndex = num++;
			_sixteenNineButton.Text = "16 x 9";
			_sixteenNineButton.Click += sixteenNineButton_Click;
			_sixteenTenButton.Location = new Point(160, 72);
			_sixteenTenButton.TabIndex = num++;
			_sixteenTenButton.Text = "16 x 10";
			_sixteenTenButton.Click += sixteenTenButton_Click;
			base.ClientSize = new Size(238, 143);
			base.Controls.Add(_sixteenTenButton, _sixteenNineButton, _fourThreeButton, label, _widthTextBox, label2, _heightTextBox);
			InitButtons();
			this.ScaleAndLayout();
		}

		private void fourThreeButton_Click(object sender, EventArgs e)
		{
			int num = int.Parse(_widthTextBox.Text);
			int num2 = num * 3 / 4;
			_heightTextBox.Text = num2.ToString();
		}

		private void sixteenNineButton_Click(object sender, EventArgs e)
		{
			int num = int.Parse(_widthTextBox.Text);
			int num2 = num * 9 / 16;
			_heightTextBox.Text = num2.ToString();
		}

		private void sixteenTenButton_Click(object sender, EventArgs e)
		{
			int num = int.Parse(_widthTextBox.Text);
			int num2 = num * 10 / 16;
			_heightTextBox.Text = num2.ToString();
		}
	}
}
