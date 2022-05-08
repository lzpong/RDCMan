using System;
using System.Windows.Forms;

namespace RdcMan {
	public class RdcTextBox : TextBox, ISettingControl {
		public StringSetting Setting { get; set; }

		public Func<string> Validate { private get; set; }

		void ISettingControl.UpdateControl() {
			if (Setting != null)
				Text = Setting.Value;
		}

		void ISettingControl.UpdateSetting() {
			if (Setting != null)
				Setting.Value = Text;
		}

		string ISettingControl.Validate() {
			if (Validate != null)
				return Validate();

			return null;
		}
	}
}
