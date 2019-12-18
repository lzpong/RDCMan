using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RdcMan
{
	internal class About : Form
	{
		private void InitializeComponent(bool isLarge)
		{
			int num = isLarge ? 400 : 350;
			int num2 = num - 26;
			System.Windows.Forms.Label label = new System.Windows.Forms.Label();
			label.Location = new System.Drawing.Point(13, 13);
			label.Size = new System.Drawing.Size(num2, 28);
			label.Text = "{1}{0}{2}".InvariantFormat(System.Environment.NewLine, RdcMan.Program.TheForm.DescriptionText, "by Julian Burger");
			System.Windows.Forms.Label value = label;
			System.Windows.Forms.Panel panel = new System.Windows.Forms.Panel();
			panel.AutoScroll = true;
			panel.Location = new System.Drawing.Point(13, 55);
			panel.Size = new System.Drawing.Size(num2, 110);
			System.Text.StringBuilder versionText = new System.Text.StringBuilder();
			versionText.AppendLine("RDCMan v{0} build {1}".InvariantFormat(RdcMan.Program.TheForm.VersionText, RdcMan.Program.TheForm.BuildText)).AppendLine().AppendLine(System.Environment.OSVersion.ToString())
				.AppendLine(".NET v{0}".InvariantFormat(System.Environment.Version))
				.AppendLine("mstscax.dll v{0}".InvariantFormat(RdcMan.RdpClient.RdpControlVersion));
			bool first = true;
			RdcMan.Program.PluginAction(delegate(RdcMan.IPlugin p)
			{
				if (first)
				{
					versionText.AppendLine().AppendLine("Plugins:");
					first = false;
				}
				versionText.AppendLine(p.GetType().FullName);
			});
			System.Windows.Forms.TextBox versionLabel = new System.Windows.Forms.TextBox
			{
				ScrollBars = System.Windows.Forms.ScrollBars.Vertical,
				BackColor = BackColor,
				Enabled = true,
				ForeColor = System.Drawing.SystemColors.WindowText,
				Location = new System.Drawing.Point(13, 55),
				Multiline = true,
				ReadOnly = true,
				Size = new System.Drawing.Size(num2, 110),
				Text = versionText.ToString()
			};
			versionLabel.VisibleChanged += delegate
			{
				versionLabel.Select(0, 0);
			};
			System.Windows.Forms.Button button = new System.Windows.Forms.Button();
			button.TabIndex = 1;
			button.Text = "OK";
			System.Windows.Forms.Button button2 = button;
			button2.Location = new System.Drawing.Point(num2 - button2.Width, 167);
			base.AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.ClientSize = new System.Drawing.Size(num, 200);
			base.Controls.Add(versionLabel);
			base.Controls.Add(value);
			base.Controls.Add(button2);
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			base.AcceptButton = button2;
			base.CancelButton = button2;
			Text = "About Remote Desktop Connection Manager";
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.ScaleAndLayout();
		}

		public About(bool isLarge)
		{
			InitializeComponent(isLarge);
		}
	}
}
