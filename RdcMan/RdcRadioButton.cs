using System.Windows.Forms;

namespace RdcMan {
	public class RdcRadioButton : RadioButton, ISettingControl {
		public BoolSetting Setting { get; set; }

		void ISettingControl.UpdateControl() {
			if (Setting != null)
				base.Checked = Setting.Value;
		}

		void ISettingControl.UpdateSetting() {
			if (Setting != null)
				Setting.Value = base.Checked;
		}

		string ISettingControl.Validate() {
			return null;
		}
	}
}
