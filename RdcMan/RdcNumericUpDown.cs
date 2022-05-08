using System;
using System.Windows.Forms;

namespace RdcMan {
	public class RdcNumericUpDown : NumericUpDown, ISettingControl {
		public IntSetting Setting { get; set; }

		public new Func<string> Validate { private get; set; }

		void ISettingControl.UpdateControl() {
			if (Setting != null)
				base.Value = Setting.Value;
		}

		void ISettingControl.UpdateSetting() {
			if (Setting != null)
				Setting.Value = (int)base.Value;
		}

		string ISettingControl.Validate() {
			if (Validate != null)
				return Validate();
			return null;
		}
	}
}
