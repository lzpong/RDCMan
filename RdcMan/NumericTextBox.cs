using System;
using System.Windows.Forms;

namespace RdcMan {
	public class NumericTextBox : TextBox, ISettingControl {
		private readonly int _min;

		private readonly int _max;

		private readonly string _invalidMessage;

		public IntSetting Setting { get; set; }

		public NumericTextBox(int min, int max, string invalidMessage) {
			if (min < 0)
				throw new ArgumentOutOfRangeException("min");
			if (max < 0)
				throw new ArgumentOutOfRangeException("max");
			if (min >= max)
				throw new ArgumentException("Minimum must be less than maximum");
			if (string.IsNullOrWhiteSpace(invalidMessage))
				throw new ArgumentOutOfRangeException("invalidMessage");
			_min = min;
			_max = max;
			_invalidMessage = invalidMessage;
		}

		void ISettingControl.UpdateControl() {
			if (Setting != null)
				Text = Setting.Value.ToString();
		}

		void ISettingControl.UpdateSetting() {
			if (Setting != null)
				Setting.Value = int.Parse(Text);
		}

		string ISettingControl.Validate() {
			string result = null;
			int num = int.Parse(Text);
			if (num < _min || num > _max)
				result = _invalidMessage;
			return result;
		}

		protected override bool ProcessCmdKey(ref Message m, Keys keyData) {
			if ((keyData & Keys.Modifiers) == 0 && ((uint)(keyData - 48) <= 9u || (uint)(keyData - 96) <= 9u))
				return base.ProcessCmdKey(ref m, keyData);
			if ((keyData & (Keys.Control | Keys.Alt)) != 0)
				return base.ProcessCmdKey(ref m, keyData);
			switch (keyData & Keys.KeyCode) {
				case Keys.Back:
				case Keys.Tab:
				case Keys.Return:
				case Keys.Escape:
				case Keys.End:
				case Keys.Home:
				case Keys.Left:
				case Keys.Right:
				case Keys.Delete:
					return base.ProcessCmdKey(ref m, keyData);
				default:
					return true;
			}
		}
	}
}
