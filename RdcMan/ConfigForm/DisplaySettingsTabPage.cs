using System.Drawing;
using System.Windows.Forms;

namespace RdcMan
{
	public abstract class DisplaySettingsTabPage<TSettingsGroup> : SettingsTabPage<TSettingsGroup> where TSettingsGroup : CommonDisplaySettings
	{
		protected DisplaySettingsTabPage(TabbedSettingsDialog dialog, TSettingsGroup settings)
			: base(dialog, settings)
		{
		}

		protected void Create(out int rowIndex, out int tabIndex)
		{
			tabIndex = 0;
			rowIndex = 0;
			CreateInheritanceControl(ref rowIndex, ref tabIndex);
			Label label = FormTools.NewLabel("缩略图比例:", 0, rowIndex);
			label.Size = new Size(140, 20);
			RdcNumericUpDown rdcNumericUpDown = new RdcNumericUpDown();
			rdcNumericUpDown.Location = FormTools.NewLocation(1, rowIndex++);
			rdcNumericUpDown.Location = new Point(rdcNumericUpDown.Location.X, rdcNumericUpDown.Location.Y + 2);
			rdcNumericUpDown.Minimum = 1m;
			rdcNumericUpDown.Maximum = 9m;
			TSettingsGroup settings = base.Settings;
			rdcNumericUpDown.Setting = settings.ThumbnailScale;
			rdcNumericUpDown.Size = new Size(40, 20);
			rdcNumericUpDown.TabIndex = tabIndex++;
			RdcCheckBox rdcCheckBox = FormTools.NewCheckBox("缩放停靠的远程桌面以适合窗口", 0, rowIndex++, tabIndex++);
			TSettingsGroup settings2 = base.Settings;
			rdcCheckBox.Setting = settings2.SmartSizeDockedWindow;
			RdcCheckBox rdcCheckBox2 = FormTools.NewCheckBox("缩放未停靠的远程桌面以适合窗口", 0, rowIndex++, tabIndex++);
			TSettingsGroup settings3 = base.Settings;
			rdcCheckBox2.Setting = settings3.SmartSizeUndockedWindow;
			base.Controls.Add(label, rdcNumericUpDown, rdcCheckBox, rdcCheckBox2);
		}
	}
}
