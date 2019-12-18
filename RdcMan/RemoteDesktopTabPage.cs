using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RdcMan
{
	public class RemoteDesktopTabPage : SettingsTabPage<RemoteDesktopSettings>
	{
		private readonly GroupBox _rdsSizeGroup;

		private readonly RadioButton _rdsCustomRadio;

		private readonly Button _rdsCustomButton;

		public RemoteDesktopTabPage(TabbedSettingsDialog dialog, RemoteDesktopSettings settings)
			: base(dialog, settings)
		{
			int tabIndex = 0;
			int rowIndex = 0;
			CreateInheritanceControl(ref rowIndex, ref tabIndex);
			ValueComboBox<int> previousGroupBox = FormTools.AddLabeledValueDropDown(this, "Color Depth", settings.ColorDepth, ref rowIndex, ref tabIndex, (int v) => v.ToString(), new int[5]
			{
				8,
				15,
				16,
				24,
				32
			});
			RadioButton value = new RdcRadioButton
			{
				Setting = settings.DesktopSizeSameAsClientAreaSize,
				Size = new Size(140, 24),
				Text = "&Same as client area"
			};
			RadioButton value2 = new RdcRadioButton
			{
				Setting = settings.DesktopSizeFullScreen,
				Size = new Size(140, 24),
				Text = "&Full screen"
			};
			_rdsCustomRadio = new RadioButton();
			_rdsCustomButton = new Button();
			_rdsCustomRadio.Size = new Size(72, 24);
			_rdsCustomRadio.Text = "&Custom";
			_rdsSizeGroup = new GroupBox();
			_rdsSizeGroup.Controls.AddRange(FormTools.NewSizeRadios());
			_rdsSizeGroup.Controls.Add(value);
			_rdsSizeGroup.Controls.Add(value2);
			_rdsSizeGroup.Controls.Add(_rdsCustomRadio);
			_rdsSizeGroup.Text = "Remote Desktop Size";
			FormTools.LayoutGroupBox(_rdsSizeGroup, 2, previousGroupBox);
			_rdsCustomButton.Location = new Point(_rdsCustomRadio.Right + 10, _rdsCustomRadio.Location.Y);
			_rdsCustomButton.TabIndex = _rdsCustomRadio.TabIndex + 1;
			_rdsCustomButton.Click += CustomSizeClick;
			_rdsCustomButton.Text = Program.TheForm.GetClientSize().ToFormattedString();
			_rdsSizeGroup.Controls.Add(_rdsCustomButton);
			base.Controls.Add(_rdsSizeGroup);
		}

		protected override void UpdateControls()
		{
			base.UpdateControls();
			Size size = base.Settings.DesktopSize.Value;
			if (!base.Settings.DesktopSizeSameAsClientAreaSize.Value && !base.Settings.DesktopSizeFullScreen.Value)
			{
				RadioButton radioButton = (from r in _rdsSizeGroup.Controls.OfType<RadioButton>()
					where (Size?)r.Tag == size
					select r).FirstOrDefault();
				if (radioButton != null)
				{
					radioButton.Checked = true;
				}
				else
				{
					_rdsCustomRadio.Checked = true;
				}
			}
			_rdsCustomButton.Text = size.ToFormattedString();
		}

		protected override void UpdateSettings()
		{
			base.UpdateSettings();
			if (!base.Settings.DesktopSizeSameAsClientAreaSize.Value && !base.Settings.DesktopSizeFullScreen.Value)
			{
				string text = _rdsCustomButton.Text;
				if (!_rdsCustomRadio.Checked)
				{
					text = (from r in _rdsSizeGroup.Controls.OfType<RadioButton>()
						where r.Checked
						select r).First().Text;
				}
				base.Settings.DesktopSize.Value = SizeHelper.Parse(text);
			}
		}

		private void CustomSizeClick(object sender, EventArgs e)
		{
			Button button = sender as Button;
			RadioButton radioButton = button.Parent.GetNextControl(button, forward: false) as RadioButton;
			radioButton.Checked = true;
			Size size = SizeHelper.Parse(button.Text);
			using (CustomSizeDialog customSizeDialog = new CustomSizeDialog(size))
			{
				if (customSizeDialog.ShowDialog() == DialogResult.OK)
				{
					button.Text = customSizeDialog.WidthText + SizeHelper.Separator + customSizeDialog.HeightText;
				}
			}
		}
	}
}
