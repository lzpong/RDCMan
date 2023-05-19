using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace RdcMan {
	internal class About : Form {
		private void InitializeComponent(bool isLarge) {
			int num = (isLarge ? 450 : 400);
			int num2 = num - 26;
			Label label = new Label {
				Location = new Point(53, 13),
				AutoSize = true,
				Text = "{0} v{1}".InvariantFormat(Program.TheForm.DescriptionText, Program.TheForm.VersionText)
			};
			Label value = label;
			Label value2 = new Label {
				Location = new Point(53, 31),
				Width = num2 - FormTools.LabelWidth,
				AutoSize = true,
				Text = "Copyright © 2023 Microsoft. By Julian Burger"
			};
			LinkLabel linkLabel = new LinkLabel {
				Location = new Point(53, 48),
				AutoSize = true,
				Text = "Sysinternals - www.sysinternals.com"
			};
			linkLabel.LinkClicked += delegate {
				Process.Start("https://www.sysinternals.com");
			};
			LinkLabel linkLabel2 = new LinkLabel {
				Location = new Point(53, 66),
				AutoSize = true,
				Text = "lzpong/RDCMan - github.com/lzpong/RDCMan"
			};
			linkLabel2.LinkClicked += delegate {
				Process.Start("https://github.com/lzpong/RDCMan");
			};
			Button button = new Button {
				TabIndex = 1,
				Text = "确定"
			};
			button.Location = new Point(num2 - button.Width + FormTools.TopMargin, 60);
			base.AutoScaleDimensions = new SizeF(96f, 96f);
			base.AutoScaleMode = AutoScaleMode.Dpi;
			base.FormBorderStyle = FormBorderStyle.FixedDialog;
			base.ClientSize = new Size(num, 90);
			base.Controls.Add(value);
			base.Controls.Add(value2);
			base.Controls.Add(linkLabel);
			base.Controls.Add(linkLabel2);
			base.Controls.Add(button);
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.SizeGripStyle = SizeGripStyle.Hide;
			base.AcceptButton = button;
			base.CancelButton = button;
			this.Text = "关于远程桌面连接管理器";
			base.StartPosition = FormStartPosition.CenterParent;
			this.ScaleAndLayout();
			base.ActiveControl = button;
		}

		public About(bool isLarge) {
			InitializeComponent(isLarge);
		}

		protected override void OnPaint(PaintEventArgs e) {
			e.Graphics.DrawIcon(Program.TheForm.Icon, 10, 10);
			base.OnPaint(e);
		}
	}
}
