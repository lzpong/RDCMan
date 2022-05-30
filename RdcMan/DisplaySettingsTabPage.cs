using System.Drawing;
using System.Windows.Forms;

namespace RdcMan {
	public abstract class DisplaySettingsTabPage<TSettingsGroup> : SettingsTabPage<TSettingsGroup> where TSettingsGroup : CommonDisplaySettings {
		protected DisplaySettingsTabPage(TabbedSettingsDialog dialog, TSettingsGroup settings)
			: base(dialog, settings) {
		}

		protected void Create(out int rowIndex, out int tabIndex) {
			tabIndex = 0;
			rowIndex = 0;
			CreateInheritanceControl(ref rowIndex, ref tabIndex);
			Label label = FormTools.NewLabel("ËõÂÔÍ¼±ÈÀý(&T)£º", 0, rowIndex);
			label.Size = new Size(140, 20);
			RdcNumericUpDown rdcNumericUpDown = new RdcNumericUpDown {
				Location = FormTools.NewLocation(1, rowIndex++),
				Minimum = 1m,
				Maximum = 9m,
				Setting = base.Settings.ThumbnailScale,
				Size = new Size(40, 20),
				TabIndex = tabIndex++
			};
			rdcNumericUpDown.Location = new Point(rdcNumericUpDown.Location.X, rdcNumericUpDown.Location.Y + 2);
			FormTools.AddLabeledEnumDropDown(this, "Í£¿¿ RD Ëõ·Å(&D)£º", base.Settings.SmartSizeDockedWindow, ref rowIndex, ref tabIndex, RdpClient.SmartSizeMethodToString);
			FormTools.AddLabeledEnumDropDown(this, "È¡ÏûÍ£¿¿ RD Ëõ·Å(&U)£º", base.Settings.SmartSizeUndockedWindow, ref rowIndex, ref tabIndex, RdpClient.SmartSizeMethodToString);
		}
	}
}
